using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

using EntityFrameworkCore.Serialization.Internal;

namespace EntityFrameworkCore.Serialization
{
    public static class Serializer
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

            return context.Graph ( item ).Select ( serializer.Serialize );
        }

        public static IEnumerable < TEntry > SerializeGraph < TEntry > ( this DbContext context, IDbContextSerializer < TEntry > serializer, params object [ ] items )
        {
            if ( context    == null ) throw new ArgumentNullException ( nameof ( context    ) );
            if ( serializer == null ) throw new ArgumentNullException ( nameof ( serializer ) );

            return context.Graph ( items ).Select ( serializer.Serialize );
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

            return context.Graph ( item ).Where ( IsChanged ).Select ( serializer.SerializeChanges );
        }

        public static IEnumerable < TEntry > SerializeGraphChanges < TEntry > ( this DbContext context, IDbContextSerializer < TEntry > serializer, params object [ ] items )
        {
            if ( context    == null ) throw new ArgumentNullException ( nameof ( context    ) );
            if ( serializer == null ) throw new ArgumentNullException ( nameof ( serializer ) );

            return context.Graph ( items ).Where ( IsChanged ).Select ( serializer.SerializeChanges );
        }

        private static List < EntityEntry > Graph ( this DbContext dbContext, object item )
        {
            var entries = new List < EntityEntry > ( );
            dbContext.TraverseGraph ( item, node => entries.Add ( node.Entry ) );
            return entries;
        }

        private static List < EntityEntry > Graph ( this DbContext dbContext, IEnumerable < object > items )
        {
            var entries = new List < EntityEntry > ( );
            dbContext.TraverseGraph ( items, node => entries.Add ( node.Entry ) );
            return entries;
        }

        private static bool IsChanged ( this EntityEntry entityEntry )
        {
            return entityEntry.State == EntityState.Added    ||
                   entityEntry.State == EntityState.Modified ||
                   entityEntry.State == EntityState.Deleted;
        }
    }
}