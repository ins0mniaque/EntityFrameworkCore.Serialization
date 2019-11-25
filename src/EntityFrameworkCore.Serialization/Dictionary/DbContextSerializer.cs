using System.Collections.Generic;
using System.Data;
using System.Linq;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace EntityFrameworkCore.Serialization.Dictionary
{
    public class DbContextSerializer : IDbContextSerializer < Entry >
    {
        public Entry CreateEntry ( ) => new Entry ( );

        public EntityState ReadEntityState ( Entry entry )               => entry.EntityState;
        public IEntityType ReadEntityType  ( Entry entry, IModel model ) => model.FindEntityType ( entry.EntityType );

        public object [ ]? ReadProperties         ( Entry entry, IProperty [ ] properties ) => Read ( properties, entry.Properties );
        public object [ ]? ReadModifiedProperties ( Entry entry, IEntityType entityType, out IProperty [ ] properties )
        {
            properties = entry.ModifiedProperties?.Select  ( property => entityType.GetProperty ( property.Key ) )
                                                  .ToArray ( );

            return Read ( properties, entry.ModifiedProperties );
        }

        public void ReadLoadedCollections ( Entry entry, IEntityType entityType, out INavigation [ ] collections )
        {
            collections = entry.LoadedCollections?.Select  ( collection => entityType.FindNavigation ( collection ) )
                                                  .ToArray ( );
        }

        public void WriteEntityState ( Entry entry, EntityState entityState ) => entry.EntityState = entityState;
        public void WriteEntityType  ( Entry entry, IEntityType entityType  ) => entry.EntityType  = entityType.Name;

        public void WriteProperties         ( Entry entry, IProperty [ ] properties, object [ ] values ) => entry.Properties         = Write ( properties, values );
        public void WriteModifiedProperties ( Entry entry, IProperty [ ] properties, object [ ] values ) => entry.ModifiedProperties = Write ( properties, values );

        public void WriteLoadedCollections ( Entry entry, INavigation [ ] collections ) => entry.LoadedCollections = collections.Select ( collection => collection.Name )
                                                                                                                                .ToArray ( );

        private static object [ ]? Read ( IProperty [ ] properties, Dictionary < string, object > entries )
        {
            if ( entries == null )
                return null;

            return properties.Select  ( property => entries [ property.Name ] )
                             .ToArray ( );
        }

        private static Dictionary < string, object > Write ( IProperty [ ] properties, object [ ] values )
        {
            return Enumerable.Range        ( 0, properties.Length )
                             .ToDictionary ( index => properties [ index ].Name,
                                             index => values     [ index ] );
        }
    }
}