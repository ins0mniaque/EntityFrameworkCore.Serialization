using System.Data;
using System.Linq;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace EntityFrameworkCore.Serialization.POCO
{
    public class DbContextEntrySerializer : IDbContextSerializer < DbContextEntry >
    {
        public DbContextEntry CreateEntry ( ) => new DbContextEntry ( );

        public EntityState ReadEntityState ( DbContextEntry entry )               => entry.EntityState;
        public IEntityType ReadEntityType  ( DbContextEntry entry, IModel model ) => model.FindEntityType ( entry.EntityType );

        public object [ ]? ReadPrimaryKey         ( DbContextEntry entry, IProperty [ ] properties ) => Read ( properties, entry.PrimaryKey       );
        public object [ ]? ReadConcurrencyToken   ( DbContextEntry entry, IProperty [ ] properties ) => Read ( properties, entry.ConcurrencyToken );
        public object [ ]? ReadProperties         ( DbContextEntry entry, IProperty [ ] properties ) => Read ( properties, entry.Properties       );
        public object [ ]? ReadModifiedProperties ( DbContextEntry entry, IEntityType entityType, out IProperty [ ] properties )
        {
            properties = entry.ModifiedProperties
                              .Select  ( property => entityType.GetProperty ( property.Name ) )
                              .ToArray ( );

            return Read ( properties, entry.ModifiedProperties );
        }

        public void WriteEntityState ( DbContextEntry entry, EntityState entityState ) => entry.EntityState = entityState;
        public void WriteEntityType  ( DbContextEntry entry, IEntityType entityType  ) => entry.EntityType  = entityType.Name;

        public void WritePrimaryKey         ( DbContextEntry entry, IProperty [ ] properties, object [ ] values ) => entry.PrimaryKey         = Write ( properties, values );
        public void WriteConcurrencyToken   ( DbContextEntry entry, IProperty [ ] properties, object [ ] values ) => entry.ConcurrencyToken   = Write ( properties, values );
        public void WriteProperties         ( DbContextEntry entry, IProperty [ ] properties, object [ ] values ) => entry.Properties         = Write ( properties, values );
        public void WriteModifiedProperties ( DbContextEntry entry, IProperty [ ] properties, object [ ] values ) => entry.ModifiedProperties = Write ( properties, values );

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