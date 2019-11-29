using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace EntityFrameworkCore.Serialization.Graph
{
    /// <summary>
    /// DbContext entity graph extension methods
    /// </summary>
    public static class EntityGraphExtensions
    {
        /// <summary>
        /// Traverse an entity graph executing a callback on each node.
        /// </summary>
        /// <param name="context">The entity graph context.</param>
        /// <param name="entity">The entity to start traversing the graph from.</param>
        /// <param name="callback">The callback executed on each node in the entity graph.</param>
        public static void TraverseGraph ( this DbContext context, object entity, Action < EntityEntryGraphNode > callback )
        {
            if ( context  == null ) throw new ArgumentNullException ( nameof ( context  ) );
            if ( callback == null ) throw new ArgumentNullException ( nameof ( callback ) );

            #pragma warning disable EF1001 // Internal EF Core API usage.
            var graph   = new EntityEntryGraphIterator ( );
            var visited = new HashSet < object > ( );
            var entry   = context.Entry ( entity ).GetInfrastructure ( );
            var root    = new EntityEntryGraphNode < object? > ( entry, null, null, null );

            graph.TraverseGraph ( root, node =>
            {
                if ( ! visited.Add ( node.Entry.Entity ) )
                    return false;

                callback ( node );
                return true;
            } );
            #pragma warning restore EF1001 // Internal EF Core API usage.
        }

        /// <summary>
        /// Traverse an entity graph executing a callback on each node.
        /// </summary>
        /// <param name="context">The entity graph context.</param>
        /// <param name="entities">The entities to start traversing the graph from.</param>
        /// <param name="callback">The callback executed on each node in the entity graph.</param>
        public static void TraverseGraph ( this DbContext context, IEnumerable < object > entities, Action < EntityEntryGraphNode > callback )
        {
            if ( context  == null ) throw new ArgumentNullException ( nameof ( context  ) );
            if ( entities == null ) throw new ArgumentNullException ( nameof ( entities ) );
            if ( callback == null ) throw new ArgumentNullException ( nameof ( callback ) );

            #pragma warning disable EF1001 // Internal EF Core API usage.
            var graph   = new EntityEntryGraphIterator ( );
            var visited = new HashSet < object > ( );

            foreach ( var entity in entities )
            {
                var entry = context.Entry ( entity ).GetInfrastructure ( );
                var root  = new EntityEntryGraphNode < object? > ( entry, null, null, null );

                graph.TraverseGraph ( root, node =>
                {
                    if ( ! visited.Add ( node.Entry.Entity ) )
                        return false;

                    callback ( node );
                    return true;
                } );
            }
            #pragma warning restore EF1001 // Internal EF Core API usage.
        }

        /// <summary>
        /// Traverse an entity graph asynchronously executing a callback on each node.
        /// </summary>
        /// <param name="context">The entity graph context.</param>
        /// <param name="entity">The entity to start traversing the graph from.</param>
        /// <param name="callback">The asynchronous callback executed on each node in the entity graph.</param>
        /// <param name="cancellationToken">A System.Threading.CancellationToken to observe while waiting for the task to complete.</param>
        public static Task TraverseGraphAsync ( this DbContext context, object entity, Func < EntityEntryGraphNode, CancellationToken, Task > callback, CancellationToken cancellationToken = default )
        {
            if ( context  == null ) throw new ArgumentNullException ( nameof ( context  ) );
            if ( callback == null ) throw new ArgumentNullException ( nameof ( callback ) );

            #pragma warning disable EF1001 // Internal EF Core API usage.
            var graph   = new EntityEntryGraphIterator ( );
            var visited = new HashSet < object > ( );
            var entry   = context.Entry ( entity ).GetInfrastructure ( );
            var root    = new EntityEntryGraphNode < object? > ( entry, null, null, null );

            return graph.TraverseGraphAsync ( root,
                                              async ( node, cancellationToken ) =>
                                              {
                                                  if ( ! visited.Add ( node.Entry.Entity ) )
                                                      return false;

                                                  await callback ( node, cancellationToken ).ConfigureAwait ( false );
                                                  return true;
                                              },
                                              cancellationToken );
            #pragma warning restore EF1001 // Internal EF Core API usage.
        }

        /// <summary>
        /// Traverse an entity graph asynchronously executing a callback on each node.
        /// </summary>
        /// <param name="context">The entity graph context.</param>
        /// <param name="entities">The entities to start traversing the graph from.</param>
        /// <param name="callback">The asynchronous callback executed on each node in the entity graph.</param>
        /// <param name="cancellationToken">A System.Threading.CancellationToken to observe while waiting for the task to complete.</param>
        public static async Task TraverseGraphAsync ( this DbContext context, IEnumerable < object > entities, Func < EntityEntryGraphNode, CancellationToken, Task > callback, CancellationToken cancellationToken = default )
        {
            if ( context  == null ) throw new ArgumentNullException ( nameof ( context  ) );
            if ( entities == null ) throw new ArgumentNullException ( nameof ( entities ) );
            if ( callback == null ) throw new ArgumentNullException ( nameof ( callback ) );

            #pragma warning disable EF1001 // Internal EF Core API usage.
            var graph   = new EntityEntryGraphIterator ( );
            var visited = new HashSet < object > ( );

            foreach ( var entity in entities )
            {
                var entry = context.Entry ( entity ).GetInfrastructure ( );
                var root  = new EntityEntryGraphNode < object? > ( entry, null, null, null );

                await graph.TraverseGraphAsync ( root,
                                                 async ( node, cancellationToken ) =>
                                                 {
                                                     if ( ! visited.Add ( node.Entry.Entity ) )
                                                         return false;

                                                     await callback ( node, cancellationToken ).ConfigureAwait ( false );
                                                     return true;
                                                 },
                                                 cancellationToken )
                           .ConfigureAwait ( false );
            }
            #pragma warning restore EF1001 // Internal EF Core API usage.
        }
    }
}