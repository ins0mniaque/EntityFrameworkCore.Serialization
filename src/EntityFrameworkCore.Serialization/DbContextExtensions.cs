using System.Collections.Generic;
using System.Data;
using System.Linq;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;

using EntityFrameworkCore.Serialization.Internal;

namespace EntityFrameworkCore.Serialization
{
    public static class DbContextExtensions
    {
        public static int SaveChanges < TEntry > ( this DbContext context, IDbContextSerializer < TEntry > serializer, out TEntry [ ] databaseGeneratedValues )
        {
            var snapshots = context.ChangeTracker
                                   .Entries ( )
                                   .Where  ( HasDatabaseGeneratedValues )
                                   .Select ( entityEntry =>
                                   {
                                       var properties = entityEntry.Properties
                                                                   .Where  ( property => property.Metadata.IsPrimaryKey ( ) ||
                                                                                         property.Metadata.IsConcurrencyToken )
                                                                   .ToList ( );

                                       return new { EntityEntry = entityEntry,
                                                    State       = entityEntry.State,
                                                    Properties  = properties.Select ( p => p.Metadata     ).ToArray ( ),
                                                    Values      = properties.Select ( p => p.CurrentValue ).ToArray ( ) };
                                   } )
                                   .ToList ( );

            var rowCount = context.SaveChanges ( );
            var index    = 0;

            databaseGeneratedValues = new TEntry [ snapshots.Count ];

            foreach ( var snapshot in snapshots )
            {
                var entry = serializer.SerializeDatabaseGeneratedValues ( snapshot.EntityEntry, snapshot.State );

                serializer.WriteProperties ( entry, snapshot.Properties, snapshot.Values );

                databaseGeneratedValues [ index++ ] = entry;
            }

            return rowCount;
        }

        public static void AcceptChanges < TEntry > ( this DbContext context, IDbContextSerializer < TEntry > serializer, IEnumerable < TEntry > databaseGeneratedValues )
        {
            var finder = new EntityEntryFinder < TEntry > ( context, serializer );

            foreach ( var entry in databaseGeneratedValues )
            {
                // TODO: Log entries not found
                var entityEntry = finder.Find ( entry );
                if ( entityEntry != null )
                    serializer.DeserializeGeneratedValues ( entry, entityEntry );
            }

            context.ChangeTracker.AcceptAllChanges ( );
        }

        private static bool HasDatabaseGeneratedValues ( this EntityEntry entityEntry )
        {
            return entityEntry.State == EntityState.Added    && entityEntry.HasValueGeneratedFlag ( ValueGenerated.OnAdd    ) ||
                   entityEntry.State == EntityState.Modified && entityEntry.HasValueGeneratedFlag ( ValueGenerated.OnUpdate );
        }

        private static bool HasValueGeneratedFlag ( this EntityEntry entityEntry, ValueGenerated valueGenerated )
        {
            return entityEntry.Metadata.GetProperties ( ).Any ( property => property.ValueGenerated.HasFlag ( valueGenerated ) );
        }
    }
}