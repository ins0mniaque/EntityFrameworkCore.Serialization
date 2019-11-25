using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace EntityFrameworkCore.Serialization.Internal
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
        /// <param name="item">The entity to start traversing the graph from.</param>
        /// <param name="callback">The callback executed on each node in the entity graph.</param>
        public static void TraverseGraph ( this DbContext context, object item, Action < EntityEntryGraphNode > callback )
        {
            if ( context  == null ) throw new ArgumentNullException ( nameof ( context  ) );
            if ( item     == null ) throw new ArgumentNullException ( nameof ( item     ) );
            if ( callback == null ) throw new ArgumentNullException ( nameof ( callback ) );

            #pragma warning disable EF1001 // Internal EF Core API usage.
            var entry   = context.Entry ( item ).GetInfrastructure ( );
            var root    = new EntityEntryGraphNode < object > ( entry, null, null, null );
            var graph   = new EntityEntryGraphIterator ( );
            var visited = new HashSet < object > ( );

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
        /// Traverse an entity graph asynchronously executing a callback on each node.
        /// </summary>
        /// <param name="context">The entity graph context.</param>
        /// <param name="item">The entity to start traversing the graph from.</param>
        /// <param name="callback">The asynchronous callback executed on each node in the entity graph.</param>
        public static Task TraverseGraphAsync ( this DbContext context, object item, Func < EntityEntryGraphNode, CancellationToken, Task > callback )
        {
            if ( context  == null ) throw new ArgumentNullException ( nameof ( context  ) );
            if ( item     == null ) throw new ArgumentNullException ( nameof ( item     ) );
            if ( callback == null ) throw new ArgumentNullException ( nameof ( callback ) );

            #pragma warning disable EF1001 // Internal EF Core API usage.
            var entry   = context.Entry ( item ).GetInfrastructure ( );
            var root    = new EntityEntryGraphNode < object > ( entry, null, null, null );
            var graph   = new EntityEntryGraphIterator ( );
            var visited = new HashSet < object > ( );

            return graph.TraverseGraphAsync ( root, async ( node, cancellationToken ) =>
            {
                if ( ! visited.Add ( node.Entry.Entity ) )
                    return false;

                await callback ( node, cancellationToken );
                return true;
            } );
            #pragma warning restore EF1001 // Internal EF Core API usage.
        }
    }
}