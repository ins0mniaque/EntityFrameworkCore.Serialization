using System.Data;
using System.Linq;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace EntityFrameworkCore.Serialization.Compact
{
    public class CompactDbContextSerializer : IDbContextSerializer < CompactEntry >
    {
        public CompactEntry CreateEntry ( ) => new CompactEntry ( );

        public EntityState ReadEntityState ( CompactEntry entry )               => entry.EntityState;
        public IEntityType ReadEntityType  ( CompactEntry entry, IModel model ) => model.GetEntityTypes ( )
                                                                                        .FirstOrDefault ( entityType => entry.EntityType == entityType.ShortName ( ) );

        public object [ ]? ReadProperties         ( CompactEntry entry, IProperty [ ] properties ) => Read ( properties, entry.Properties );
        public object [ ]? ReadModifiedProperties ( CompactEntry entry, IEntityType entityType, out IProperty [ ] properties )
        {
            properties = entry.ModifiedProperties?.Select  ( modifiedProperty => entityType.GetProperties ( ).FirstOrDefault ( property => modifiedProperty.Index == property.GetIndex ( ) ) )
                                                  .ToArray ( );

            return Read ( properties, entry.ModifiedProperties );
        }

        public void ReadLoadedCollections ( CompactEntry entry, IEntityType entityType, out INavigation [ ] collections )
        {
            collections = entry.LoadedCollections?.Select  ( collection => entityType.GetNavigations ( ).FirstOrDefault ( navigation => navigation.GetIndex ( ) == collection ) )
                                                  .ToArray ( );
        }

        public void WriteEntityState ( CompactEntry entry, EntityState entityState ) => entry.EntityState = entityState;
        public void WriteEntityType  ( CompactEntry entry, IEntityType entityType  ) => entry.EntityType  = entityType.ShortName ( );

        public void WriteProperties         ( CompactEntry entry, IProperty [ ] properties, object [ ] values ) => entry.Properties         = Write ( properties, values );
        public void WriteModifiedProperties ( CompactEntry entry, IProperty [ ] properties, object [ ] values ) => entry.ModifiedProperties = Write ( properties, values );

        public void WriteLoadedCollections ( CompactEntry entry, INavigation [ ] collections ) => entry.LoadedCollections = collections.Select  ( collection => collection.GetIndex ( ) )
                                                                                                                                       .ToArray ( );

        private static object [ ]? Read ( IProperty [ ] properties, CompactPropertyEntry [ ] entries )
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

        private static CompactPropertyEntry [ ] Write ( IProperty [ ] properties, object [ ] values )
        {
            return properties.Zip ( values, (property, value) => new CompactPropertyEntry { Index = property.GetIndex ( ),
                                                                                            Value = value } )
                             .ToArray ( );
        }
    }
}