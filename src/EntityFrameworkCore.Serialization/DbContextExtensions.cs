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
        public static int SaveChanges < T > ( this DbContext context, IDbContextSerializer < T > serializer, T writable )
        {
            return context.SaveChanges ( serializer.CreateWriter ( writable ) );
        }

        public static int SaveChanges ( this DbContext context, IEntityEntryWriter writer )
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
                                                              Properties  = properties.ToDictionary ( property => property.Metadata,
                                                                                                      property => property.CurrentValue ) };
                                             } )
                                   .ToList ( );

            var rowCount = context.SaveChanges ( );

            foreach ( var snapshot in snapshots )
            {
                var mode = snapshot.State == EntityState.Added ? Serializer.SerializationMode.ValuesGeneratedOnAdd:
                                                                 Serializer.SerializationMode.ValuesGeneratedOnUpdate;

                writer.Write ( snapshot.EntityEntry, mode, snapshot.Properties );
            }

            return rowCount;
        }

        public static void AcceptChanges < T > ( this DbContext context, IDbContextDeserializer < T > deserializer, T readable )
        {
            context.AcceptChanges ( deserializer.CreateReader ( readable ) );
        }

        public static void AcceptChanges ( this DbContext context, IEntityEntryReader reader )
        {
            var finder     = new EntityEntryFinder ( context );
            var properties = new Dictionary < IProperty, object? > ( );

            while ( reader.ReadEntry ( ) )
            {
                var entityType  = reader.ReadEntityType  ( context.Model );
                var entityState = reader.ReadEntityState ( );

                properties.Clear ( );
                while ( reader.ReadProperty ( out var property, out var value ) )
                    properties [ property ] = value;

                var entityEntry = finder.Find ( entityType, properties );
                if ( entityEntry == null )
                {
                    // TODO: Log entries not found
                    continue;
                }

                while ( reader.ReadModifiedProperty ( out var property, out var value ) )
                    entityEntry.SetDatabaseGeneratedProperty ( property, value );

                while ( reader.ReadNavigationState ( out var navigation ) );
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