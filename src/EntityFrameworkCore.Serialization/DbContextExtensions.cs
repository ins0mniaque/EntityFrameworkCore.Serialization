using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace EntityFrameworkCore.Serialization
{
    public static class DbContextExtensions
    {
        public static IEnumerable < TEntry > Serialize < TEntry > ( this DbContext context, IDbContextSerializer < TEntry > serializer )
        {
            if ( context    == null ) throw new ArgumentNullException ( nameof ( context    ) );
            if ( serializer == null ) throw new ArgumentNullException ( nameof ( serializer ) );

            return context.ChangeTracker.Entries ( ).Select ( serializer.Serialize );
        }

        public static IEnumerable < TEntry > SerializeGraph < TEntry > ( this DbContext context, object item, IDbContextSerializer < TEntry > serializer )
        {
            if ( context    == null ) throw new ArgumentNullException ( nameof ( context    ) );
            if ( serializer == null ) throw new ArgumentNullException ( nameof ( serializer ) );

            return context.GraphEntries ( item ).Select ( serializer.Serialize );
        }

        public static IEnumerable < TEntry > SerializeChanges < TEntry > ( this DbContext context, IDbContextSerializer < TEntry > serializer )
        {
            if ( context    == null ) throw new ArgumentNullException ( nameof ( context    ) );
            if ( serializer == null ) throw new ArgumentNullException ( nameof ( serializer ) );

            return context.ChangeTracker.Entries ( ).Where ( IsChanged ).Select ( serializer.SerializeChanges );
        }

        public static IEnumerable < TEntry > SerializeGraphChanges < TEntry > ( this DbContext context, object item, IDbContextSerializer < TEntry > serializer )
        {
            if ( context    == null ) throw new ArgumentNullException ( nameof ( context    ) );
            if ( serializer == null ) throw new ArgumentNullException ( nameof ( serializer ) );

            return context.GraphEntries ( item ).Where ( IsChanged ).Select ( serializer.SerializeChanges );
        }

        public static void Deserialize < TEntry > ( this DbContext context, IEnumerable < TEntry > entries, IDbContextSerializer < TEntry > serializer )
        {
            var finder = new EntityEntryFinder < TEntry > ( context.ChangeTracker, serializer );
            var pairs  = entries.Select ( entry => new { Entry       = entry,
                                                         EntityEntry = finder.FindOrCreate ( entry ) } )
                                .ToList ( );

            foreach ( var pair in pairs )
                serializer.Deserialize ( pair.Entry, pair.EntityEntry );

            foreach ( var pair in pairs )
                SetEntityState ( pair.EntityEntry, serializer.ReadEntityState ( pair.Entry ) );

            foreach ( var pair in pairs )
                serializer.DeserializeModifiedProperties ( pair.Entry, pair.EntityEntry );

            foreach ( var pair in pairs )
                serializer.DeserializeLoadedCollections ( pair.Entry, pair.EntityEntry );
        }

        public static int SaveChanges < TEntry > ( this DbContext context, IDbContextSerializer < TEntry > serializer, out IEnumerable < TEntry > databaseGeneratedValues )
        {
            var snapshot = context.ChangeTracker.Entries ()
                                  .Where ( HasGeneratedValues )
                                  .Select ( e => new { Entry= e,
                                                       State = e.State,
                                                       PK = new { Properties = e.Properties.Where ( p => p.Metadata.IsPrimaryKey ( ) ).Select ( p => p.Metadata ).ToArray ( ),
                                                                  Values = e.Properties.Where ( p => p.Metadata.IsPrimaryKey ( ) ).Select ( p => p.CurrentValue ).ToArray ( ) },
                                                       CT = new { Properties = e.Properties.Where ( p => p.Metadata.IsConcurrencyToken ).Select ( p => p.Metadata ).ToArray ( ),
                                                                  Values = e.Properties.Where ( p => p.Metadata.IsConcurrencyToken ).Select ( p => p.CurrentValue ).ToArray ( ) } } )
                                  .ToList ( );

            var rowCount = context.SaveChanges ( );

            databaseGeneratedValues = snapshot.Select ( en =>
            {
                var x = serializer.SerializeDatabaseGeneratedValues ( en.Entry, en.State );

                serializer.WritePrimaryKey ( x, en.PK.Properties, en.PK.Values );
                serializer.WriteConcurrencyToken ( x, en.CT.Properties, en.CT.Values );

                return x;
            } );

            return rowCount;
        }

        public static void AcceptChanges < TEntry > ( this DbContext context, IEnumerable < TEntry > databaseGeneratedValues, IDbContextSerializer < TEntry > serializer )
        {
            var finder = new EntityEntryFinder < TEntry > ( context.ChangeTracker, serializer );
            var pairs  = databaseGeneratedValues.Select ( entry => new { Entry       = entry,
                                                                         EntityEntry = finder.Find ( entry ) } )
                                .ToList ( );

            foreach ( var pair in pairs )
                serializer.DeserializeGeneratedValues ( pair.Entry, pair.EntityEntry );

            context.ChangeTracker.AcceptAllChanges ( );
        }

        private static bool HasGeneratedValues ( EntityEntry entityEntry )
        {
            return entityEntry.State == EntityState.Added && entityEntry.Metadata.GetProperties().Any(p=>p.ValueGenerated.HasFlag(Microsoft.EntityFrameworkCore.Metadata.ValueGenerated.OnAdd)) ||
                entityEntry.State == EntityState.Modified && entityEntry.Metadata.GetProperties().Any(p=>p.ValueGenerated.HasFlag(Microsoft.EntityFrameworkCore.Metadata.ValueGenerated.OnUpdate));
        }

        private static List < EntityEntry > GraphEntries ( this DbContext dbContext, object item )
        {
            var entries = new List < EntityEntry > ( );
            dbContext.TraverseGraph ( item, node => entries.Add ( node.Entry ) );
            return entries;
        }

        private static bool IsChanged ( EntityEntry entityEntry )
            => entityEntry.State == EntityState.Added    ||
               entityEntry.State == EntityState.Modified ||
               entityEntry.State == EntityState.Deleted;

        private static void SetEntityState ( EntityEntry entityEntry, EntityState state )
        {
            if ( state == EntityState.Modified )
            {
                var modified = entityEntry.Properties.Where ( property => property.IsModified ).ToHashSet ( );
                entityEntry.State = state;
                foreach ( var property in entityEntry.Properties )
                    property.IsModified = modified.Contains ( property );
            }
            else
                entityEntry.State = state;
        }
    }
}