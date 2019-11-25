using System;
using System.Data;
using System.Linq;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;

namespace EntityFrameworkCore.Serialization
{
    public class EntityEntryFinder < TEntry >
    {
        public EntityEntryFinder ( ChangeTracker changeTracker, IDbContextSerializer < TEntry > serializer )
        {
            ChangeTracker = changeTracker;
            Entries       = changeTracker.Entries ( ).ToLookup ( entry => entry.Metadata );
            Serializer    = serializer;
        }

        public EntityEntry FindOrCreate ( TEntry entry ) => Find ( entry ) ?? Create ( entry );

        public EntityEntry Find ( TEntry entry )
        {
            var entityType = Serializer.ReadEntityType ( entry, ChangeTracker.Context.Model );

            // TODO: Something to cache this...
            var primaryKeyProperties = entityType.GetProperties ( )
                                                 .Where ( p => p.IsPrimaryKey ( ) )
                                                 .ToArray ( );

            var primaryKey  = Serializer.ReadPrimaryKey ( entry, primaryKeyProperties );
            var entityEntry = Entries [ entityType ].FirstOrDefault ( e =>
            {
                return primaryKey.Select ( (value, index) =>
                {
                    var property = e.Property ( primaryKeyProperties [ index ].Name );

                    return property.Metadata.AreEquals ( property.CurrentValue, value );
                } ).All ( isEqual => isEqual );
            } );

            return entityEntry;
        }

        public EntityEntry Create ( TEntry entry )
        {
            var entityType  = Serializer.ReadEntityType ( entry, ChangeTracker.Context.Model );
            var entityEntry = ChangeTracker.Context.Entry ( Activator.CreateInstance ( entityType.ClrType ) );

            // TODO: Move setting the primary key and concurrency token here...

            return entityEntry;
        }

        private ChangeTracker                        ChangeTracker { get; }
        private ILookup < IEntityType, EntityEntry > Entries       { get; }
        private IDbContextSerializer < TEntry >      Serializer    { get; }
    }
}