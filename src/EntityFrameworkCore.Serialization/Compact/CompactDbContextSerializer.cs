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

        public object [ ]? ReadProperties ( CompactEntry entry, IEntityType entityType, out IProperty [ ] properties )
        {
            return Read ( entry.Properties, entityType, out properties );
        }

        public object [ ]? ReadModifiedProperties ( CompactEntry entry, IEntityType entityType, out IProperty [ ] properties )
        {
            return Read ( entry.ModifiedProperties, entityType, out properties );
        }

        public void ReadNavigationState ( CompactEntry entry, IEntityType entityType, out INavigation [ ] navigated )
        {
            navigated = entry.NavigationState?.Select  ( index => Find ( entityType.GetNavigations ( ), index ) )
                                              .ToArray ( );
        }

        public void WriteEntityState ( CompactEntry entry, EntityState entityState ) => entry.EntityState = entityState;
        public void WriteEntityType  ( CompactEntry entry, IEntityType entityType  ) => entry.EntityType  = entityType.ShortName ( );

        public void WriteProperties ( CompactEntry entry, IProperty [ ] properties, object [ ] values )
        {
            entry.Properties = Write ( properties, values );
        }

        public void WriteModifiedProperties ( CompactEntry entry, IProperty [ ] properties, object [ ] values )
        {
            entry.ModifiedProperties = Write ( properties, values );
        }

        public void WriteNavigationState ( CompactEntry entry, INavigation [ ] navigated )
        {
            entry.NavigationState = navigated.Select  ( collection => collection.GetIndex ( ) )
                                             .ToArray ( );
        }

        private static T Find < T > ( System.Collections.Generic.IEnumerable < T > properties, int index ) where T : IPropertyBase
        {
            return properties.FirstOrDefault ( property => index == property.GetIndex ( ) );
        }

        private static object [ ]? Read ( CompactPropertyEntry [ ] entries, IEntityType entityType, out IProperty [ ] properties )
        {
            properties = entries?.Select  ( entry => Find ( entityType.GetProperties ( ), entry.Index ) )
                                 .ToArray ( );

            if ( entries == null )
                return null;

            return entries.Select ( entry => entry.Value ).ToArray ( );
        }

        private static CompactPropertyEntry [ ] Write ( IProperty [ ] properties, object [ ] values )
        {
            return properties.Zip ( values, (property, value) => new CompactPropertyEntry { Index = property.GetIndex ( ),
                                                                                            Value = value } )
                             .ToArray ( );
        }
    }
}