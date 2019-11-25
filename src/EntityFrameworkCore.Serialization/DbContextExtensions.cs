using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;

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

        public static IEnumerable < TEntry > SerializeGraph < TEntry > ( this DbContext context, IDbContextSerializer < TEntry > serializer, object item )
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

        public static IEnumerable < TEntry > SerializeGraphChanges < TEntry > ( this DbContext context, IDbContextSerializer < TEntry > serializer, object item )
        {
            if ( context    == null ) throw new ArgumentNullException ( nameof ( context    ) );
            if ( serializer == null ) throw new ArgumentNullException ( nameof ( serializer ) );

            return context.GraphEntries ( item ).Where ( IsChanged ).Select ( serializer.SerializeChanges );
        }

        public static void Deserialize < TEntry > ( this DbContext context, IEnumerable < TEntry > entries, IDbContextSerializer < TEntry > serializer )
        {
            var finder = new Internal.EntityEntryFinder < TEntry > ( context.ChangeTracker, serializer );
            var pairs  = entries.Select ( entry => new { Entry       = entry,
                                                         EntityEntry = finder.FindOrCreate ( entry ) } )
                                .ToList ( );

            foreach ( var pair in pairs ) serializer.DeserializeProperties         ( pair.Entry, pair.EntityEntry );
            foreach ( var pair in pairs ) serializer.DeserializeEntityState        ( pair.Entry, pair.EntityEntry );
            foreach ( var pair in pairs ) serializer.DeserializeModifiedProperties ( pair.Entry, pair.EntityEntry );
            foreach ( var pair in pairs ) serializer.DeserializeLoadedCollections  ( pair.Entry, pair.EntityEntry );
        }

        public static int SaveChanges < TEntry > ( this DbContext context, IDbContextSerializer < TEntry > serializer, out IEnumerable < TEntry > databaseGeneratedValues )
        {
            var snapshot = context.ChangeTracker.Entries ( )
                                  .Where  ( HasDatabaseGeneratedValues )
                                  .Select ( entry =>
                                  {
                                      var properties = entry.Properties
                                                            .Where  ( property => property.Metadata.IsPrimaryKey ( ) ||
                                                                                  property.Metadata.IsConcurrencyToken )
                                                            .ToList ( );

                                      return new { Entry      = entry,
                                                   State      = entry.State,
                                                   Properties = properties.Select ( p => p.Metadata     ).ToArray ( ),
                                                   Values     = properties.Select ( p => p.CurrentValue ).ToArray ( ) };
                                  } )
                                  .ToList ( );

            var rowCount = context.SaveChanges ( );

            databaseGeneratedValues = snapshot.Select ( en =>
            {
                var x = serializer.SerializeDatabaseGeneratedValues ( en.Entry, en.State );

                serializer.WriteProperties ( x, en.Properties, en.Values );

                return x;
            } );

            return rowCount;
        }

        public static void AcceptChanges < TEntry > ( this DbContext context, IDbContextSerializer < TEntry > serializer, IEnumerable < TEntry > databaseGeneratedValues )
        {
            var finder = new Internal.EntityEntryFinder < TEntry > ( context.ChangeTracker, serializer );
            var pairs  = databaseGeneratedValues.Select ( entry => new { Entry       = entry,
                                                                         EntityEntry = finder.Find ( entry ) } )
                                .ToList ( );

            foreach ( var pair in pairs )
            {
                // TODO: Log entries not found
                if ( pair.EntityEntry != null )
                    serializer.DeserializeGeneratedValues ( pair.Entry, pair.EntityEntry );
            }

            context.ChangeTracker.AcceptAllChanges ( );
        }

        private static bool HasDatabaseGeneratedValues ( EntityEntry entityEntry )
        {
            return entityEntry.State == EntityState.Added    && HasValueGeneratedFlag ( entityEntry, ValueGenerated.OnAdd    ) ||
                   entityEntry.State == EntityState.Modified && HasValueGeneratedFlag ( entityEntry, ValueGenerated.OnUpdate );
        }

        private static bool HasValueGeneratedFlag ( EntityEntry entityEntry, ValueGenerated valueGenerated )
        {
            return entityEntry.Metadata.GetProperties ( ).Any ( property => property.ValueGenerated.HasFlag ( valueGenerated ) );
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
    }
}