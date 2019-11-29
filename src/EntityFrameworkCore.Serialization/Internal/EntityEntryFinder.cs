using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Data;
using System.Linq;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;

namespace EntityFrameworkCore.Serialization.Internal
{
    public class EntityEntryFinder
    {
        public EntityEntryFinder ( DbContext context )
        {
            Context = context ?? throw new ArgumentNullException ( nameof ( context ) );
        }

        private DbContext Context { get; }

        public EntityEntry FindOrCreate ( IEntityType entityType, IDictionary < IProperty, object? > properties )
        {
            return Find   ( entityType, properties ) ??
                   Create ( entityType, properties );
        }

        public EntityEntry Find ( IEntityType entityType, IDictionary < IProperty, object? > properties )
        {
            var primaryKey = entityType.FindPrimaryKey ( );

            #pragma warning disable EF1001 // Internal EF Core API usage.
            return Context.GetDependencies ( )
                          .StateManager
                          .TryGetEntry   ( primaryKey,
                                           primaryKey.Properties
                                                     .Select  ( property => properties [ property ] )
                                                     .ToArray ( ) )
                         ?.ToEntityEntry ( );
            #pragma warning restore EF1001 // Internal EF Core API usage.
        }

        public EntityEntry Create ( IEntityType entityType, IDictionary < IProperty, object? > properties )
        {
            // TODO: Move setting the primary key and concurrency token here...

            #pragma warning disable EF1001 // Internal EF Core API usage.
            return Context.GetDependencies ( )
                          .StateManager
                          .CreateEntry   ( ImmutableDictionary < string, object >.Empty, entityType )
                          .ToEntityEntry ( );
            #pragma warning restore EF1001 // Internal EF Core API usage.
        }
    }
}