using System;
using System.IO;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

namespace EntityFrameworkCore.Serialization
{
    public static partial class Serializer
    {
        public static async Task SerializeAsync ( this DbContext context, IDbContextSerializer < Stream > serializer, Stream stream )
        {
            if ( serializer == null )
                throw new ArgumentNullException ( nameof ( serializer ) );

            await WriteAsync ( stream, stream => context.Serialize ( serializer, stream ) ).ConfigureAwait ( false );
        }

        public static async Task SerializeGraphAsync ( this DbContext context, IDbContextSerializer < Stream > serializer, Stream stream, object item )
        {
            if ( serializer == null )
                throw new ArgumentNullException ( nameof ( serializer ) );

            await WriteAsync ( stream, stream => context.SerializeGraph ( serializer, stream, item ) ).ConfigureAwait ( false );
        }

        public static async Task SerializeGraphAsync ( this DbContext context, IDbContextSerializer < Stream > serializer, Stream stream, params object [ ] items )
        {
            if ( serializer == null )
                throw new ArgumentNullException ( nameof ( serializer ) );

            await WriteAsync ( stream, stream => context.SerializeGraph ( serializer, stream, items ) ).ConfigureAwait ( false );
        }

        public static async Task SerializeChangesAsync ( this DbContext context, IDbContextSerializer < Stream > serializer, Stream stream )
        {
            if ( serializer == null )
                throw new ArgumentNullException ( nameof ( serializer ) );

            await WriteAsync ( stream, stream => context.SerializeChanges ( serializer, stream ) ).ConfigureAwait ( false );
        }

        public static async Task SerializeGraphChangesAsync ( this DbContext context, IDbContextSerializer < Stream > serializer, Stream stream, object item )
        {
            if ( serializer == null )
                throw new ArgumentNullException ( nameof ( serializer ) );

            await WriteAsync ( stream, stream => context.SerializeGraphChanges ( serializer, stream, item ) ).ConfigureAwait ( false );
        }

        public static async Task SerializeGraphChangesAsync ( this DbContext context, IDbContextSerializer < Stream > serializer, Stream stream, params object [ ] items )
        {
            if ( serializer == null )
                throw new ArgumentNullException ( nameof ( serializer ) );

            await WriteAsync ( stream, stream => context.SerializeGraphChanges ( serializer, stream, items ) ).ConfigureAwait ( false );
        }

        public static async Task < int > SaveChangesAsync ( this DbContext context, IDbContextSerializer < Stream > serializer, Stream stream )
        {
            if ( serializer == null )
                throw new ArgumentNullException ( nameof ( serializer ) );

            return await WriteAsync ( stream, stream => context.SaveChanges ( serializer, stream ) ).ConfigureAwait ( false );
        }

        private static Task WriteAsync ( Stream stream, Action < Stream > write )
        {
            return WriteAsync ( stream, stream => { write ( stream ); return true; } );
        }

        private static async Task < T > WriteAsync < T > ( Stream stream, Func < Stream, T > write )
        {
            if ( stream == null )
                throw new ArgumentNullException ( nameof ( stream ) );

            using var buffer = new MemoryStream ( );

            var result = write ( buffer );

            buffer.Seek ( 0, SeekOrigin.Begin );

            await buffer.CopyToAsync    ( stream )
                        .ConfigureAwait ( false  );

            return result;
        }
    }
}