using System.Data;
using System.Linq;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace EntityFrameworkCore.Serialization.POCO
{
    public class DbContextSerializer : IDbContextSerializer < Entry >
    {
        public Entry CreateEntry ( ) => new Entry ( );

        public EntityState ReadEntityState ( Entry entry )               => entry.EntityState;
        public IEntityType ReadEntityType  ( Entry entry, IModel model ) => model.FindEntityType ( entry.EntityType );

        public object [ ]? ReadProperties         ( Entry entry, IProperty [ ] properties ) => Read ( properties, entry.Properties );
        public object [ ]? ReadModifiedProperties ( Entry entry, IEntityType entityType, out IProperty [ ] properties )
        {
            properties = entry.ModifiedProperties
                              .Select  ( property => entityType.GetProperty ( property.Name ) )
                              .ToArray ( );

            return Read ( properties, entry.ModifiedProperties );
        }

        public void ReadLoadedCollections ( Entry entry, IEntityType entityType, out INavigation [ ] collections )
        {
            collections = entry.LoadedCollections
                               .Select  ( collection => entityType.FindNavigation ( collection ) )
                               .ToArray ( );
        }

        public void WriteEntityState ( Entry entry, EntityState entityState ) => entry.EntityState = entityState;
        public void WriteEntityType  ( Entry entry, IEntityType entityType  ) => entry.EntityType  = entityType.Name;

        public void WriteProperties         ( Entry entry, IProperty [ ] properties, object [ ] values ) => entry.Properties         = Write ( properties, values );
        public void WriteModifiedProperties ( Entry entry, IProperty [ ] properties, object [ ] values ) => entry.ModifiedProperties = Write ( properties, values );

        public void WriteLoadedCollections ( Entry entry, INavigation [ ] collections ) => entry.LoadedCollections = collections.Select  ( collection => collection.Name )
                                                                                                                                         .ToArray ( );

        private static object [ ]? Read ( IProperty [ ] properties, PropertyEntry [ ] entries )
        {
            if ( entries == null )
                return null;

            return properties.Zip ( entries, (property, entry) =>
            {
                if ( entry.Name == property.Name )
                    return entry.Value;

                return entries.Single ( otherEntry => otherEntry.Name == property.Name ).Value;
            } ).ToArray ( );
        }

        private static PropertyEntry [ ] Write ( IProperty [ ] properties, object [ ] values )
        {
            return properties.Zip ( values, (property, value) => new PropertyEntry { Name = property.Name, Value = value } )
                             .ToArray ( );
        }
    }
}