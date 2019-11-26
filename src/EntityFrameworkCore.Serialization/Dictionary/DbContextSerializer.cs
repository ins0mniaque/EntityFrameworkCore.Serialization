using System.Collections.Generic;
using System.Data;
using System.Linq;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace EntityFrameworkCore.Serialization.Dictionary
{
    public class DbContextSerializer : IDbContextSerializer < Entry >
    {
        public Entry CreateEntry ( ) => new Entry ( );

        public EntityState ReadEntityState ( Entry entry )               => entry.EntityState;
        public IEntityType ReadEntityType  ( Entry entry, IModel model ) => model.FindEntityType ( entry.EntityType );

        public object [ ]? ReadProperties ( Entry entry, IEntityType entityType, out IProperty [ ] properties )
        {
            return Read ( entry.Properties, entityType, out properties );
        }

        public object [ ]? ReadModifiedProperties ( Entry entry, IEntityType entityType, out IProperty [ ] properties )
        {
            return Read ( entry.ModifiedProperties, entityType, out properties );
        }

        public void ReadNavigationState ( Entry entry, IEntityType entityType, out INavigation [ ] collections )
        {
            collections = entry.NavigationState?.Select  ( collection => entityType.FindNavigation ( collection ) )
                                                .ToArray ( );
        }

        public void WriteEntityState ( Entry entry, EntityState entityState ) => entry.EntityState = entityState;
        public void WriteEntityType  ( Entry entry, IEntityType entityType  ) => entry.EntityType  = entityType.Name;

        public void WriteProperties ( Entry entry, IProperty [ ] properties, object [ ] values )
        {
            entry.Properties = Write ( properties, values );
        }

        public void WriteModifiedProperties ( Entry entry, IProperty [ ] properties, object [ ] values )
        {
            entry.ModifiedProperties = Write ( properties, values );
        }

        public void WriteNavigationState ( Entry entry, INavigation [ ] navigated )
        {
            entry.NavigationState = navigated.Select  ( collection => collection.Name )
                                             .ToArray ( );
        }

        private static object [ ]? Read ( Dictionary < string, object > entries, IEntityType entityType, out IProperty [ ] properties )
        {
            if ( entries == null )
            {
                properties = null;
                return null;
            }

            properties = new IProperty [ entries.Count ];
            var values = new object    [ entries.Count ];
            var index  = 0;

            foreach ( var entry in entries )
            {
                properties [ index   ] = entityType.FindProperty ( entry.Key );
                values     [ index++ ] = entry.Value;
            }

            return values;
        }

        private static Dictionary < string, object > Write ( IProperty [ ] properties, object [ ] values )
        {
            return Enumerable.Range        ( 0, properties.Length )
                             .ToDictionary ( index => properties [ index ].Name,
                                             index => values     [ index ] );
        }
    }
}