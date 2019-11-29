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
            if ( context == null )
                throw new ArgumentNullException ( nameof ( context ) );

            Context = context;
            Entries = context.ChangeTracker.Entries ( ).ToLookup ( entry => entry.Metadata );
        }

        private DbContext                            Context { get; }
        private ILookup < IEntityType, EntityEntry > Entries { get; }

        public EntityEntry FindOrCreate ( IEntityType entityType, IDictionary < IProperty, object? > properties )
        {
            return Find   ( entityType, properties ) ??
                   Create ( entityType, properties );
        }

        public EntityEntry Find ( IEntityType entityType, IDictionary < IProperty, object? > properties )
        {
            var primaryKeyProperties = entityType.FindPrimaryKey ( ).Properties;
            var primaryKey           = primaryKeyProperties.Select ( property => properties [ property ] ).ToArray ( );

            return Entries [ entityType ].FirstOrDefault ( entry =>
            {
                var index = 0;
                foreach ( var value in primaryKey )
                {
                    var property = entry.Property ( primaryKeyProperties [ index++ ] );
                    var comparer = property.Metadata.GetStructuralValueComparer ( );
                    var equal    = comparer != null ? comparer.Equals ( property.CurrentValue, value ) :
                                                      object  .Equals ( property.CurrentValue, value );
                    if ( ! equal )
                        return false;
                }

                return true;
            } );
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