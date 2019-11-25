using System.Data;
using System.Linq;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace EntityFrameworkCore.Serialization.POCO
{
    public class DbContextIndexedEntrySerializer : IDbContextSerializer < DbContextIndexedEntry >
    {
        public DbContextIndexedEntry CreateEntry ( ) => new DbContextIndexedEntry ( );

        public EntityState ReadEntityState ( DbContextIndexedEntry entry )               => entry.EntityState;
        public IEntityType ReadEntityType  ( DbContextIndexedEntry entry, IModel model ) => model.GetEntityTypes ( )
                                                                                                 .FirstOrDefault ( entityType => entry.EntityType == entityType.ShortName ( ) );

        public object [ ]? ReadPrimaryKey         ( DbContextIndexedEntry entry, IProperty [ ] properties ) => Read ( properties, entry.PrimaryKey       );
        public object [ ]? ReadConcurrencyToken   ( DbContextIndexedEntry entry, IProperty [ ] properties ) => Read ( properties, entry.ConcurrencyToken );
        public object [ ]? ReadProperties         ( DbContextIndexedEntry entry, IProperty [ ] properties ) => Read ( properties, entry.Properties       );
        public object [ ]? ReadModifiedProperties ( DbContextIndexedEntry entry, IEntityType entityType, out IProperty [ ] properties )
        {
            properties = entry.ModifiedProperties
                              .Select  ( modifiedProperty => entityType.GetProperties  ( )
                                                                       .FirstOrDefault ( property => modifiedProperty.Index == property.GetIndex ( ) ) )
                              .ToArray ( );

            return Read ( properties, entry.ModifiedProperties );
        }

        public void ReadLoadedCollections ( DbContextIndexedEntry entry, IEntityType entityType, out INavigation [ ] collections )
        {
            collections = entry.LoadedCollections
                              ?.Select  ( collection => entityType.GetNavigations ( ).FirstOrDefault ( navigation => navigation.GetIndex ( ) == collection ) )
                               .ToArray ( );
        }

        public void WriteEntityState ( DbContextIndexedEntry entry, EntityState entityState ) => entry.EntityState = entityState;
        public void WriteEntityType  ( DbContextIndexedEntry entry, IEntityType entityType  ) => entry.EntityType  = entityType.ShortName ( );

        public void WritePrimaryKey         ( DbContextIndexedEntry entry, IProperty [ ] properties, object [ ] values ) => entry.PrimaryKey         = Write ( properties, values );
        public void WriteConcurrencyToken   ( DbContextIndexedEntry entry, IProperty [ ] properties, object [ ] values ) => entry.ConcurrencyToken   = Write ( properties, values );
        public void WriteProperties         ( DbContextIndexedEntry entry, IProperty [ ] properties, object [ ] values ) => entry.Properties         = Write ( properties, values );
        public void WriteModifiedProperties ( DbContextIndexedEntry entry, IProperty [ ] properties, object [ ] values ) => entry.ModifiedProperties = Write ( properties, values );

        public void WriteLoadedCollections ( DbContextIndexedEntry entry, INavigation [ ] collections ) => entry.LoadedCollections = collections.Select  ( collection => collection.GetIndex ( ) )
                                                                                                                                                .ToArray ( );

        private static object [ ]? Read ( IProperty [ ] properties, IndexedPropertyEntry [ ] entries )
        {
            if ( entries == null )
                return null;

            return properties.Zip ( entries, (property, entry) =>
            {
                var propertyIndex = property.GetIndex ( );
                if ( entry.Index == propertyIndex )
                    return entry.Value;

                return entries.Single ( otherEntry => otherEntry.Index == propertyIndex ).Value;
            } ).ToArray ( );
        }

        private static IndexedPropertyEntry [ ] Write ( IProperty [ ] properties, object [ ] values )
        {
            return properties.Zip ( values, (property, value) => new IndexedPropertyEntry { Index = property.GetIndex ( ),
                                                                                            Value = value } )
                             .ToArray ( );
        }
    }
}