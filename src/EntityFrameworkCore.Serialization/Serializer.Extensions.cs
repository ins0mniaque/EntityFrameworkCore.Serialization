using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

namespace EntityFrameworkCore.Serialization
{
    public static partial class Serializer
    {
        public static void Serialize < T > ( this DbContext context, IDbContextSerializer < T > serializer, T writable )
        {
            if ( serializer == null )
                throw new ArgumentNullException ( nameof ( serializer ) );

            var writer = serializer.CreateWriter ( writable );
            using ( writer as IDisposable )
                context.Serialize ( writer );
        }

        public static void SerializeGraph < T > ( this DbContext context, IDbContextSerializer < T > serializer, T writable, object item )
        {
            if ( serializer == null )
                throw new ArgumentNullException ( nameof ( serializer ) );

            var writer = serializer.CreateWriter ( writable );
            using ( writer as IDisposable )
                context.SerializeGraph ( writer, item );
        }

        public static void SerializeGraph < T > ( this DbContext context, IDbContextSerializer < T > serializer, T writable, params object [ ] items )
        {
            if ( serializer == null )
                throw new ArgumentNullException ( nameof ( serializer ) );

            var writer = serializer.CreateWriter ( writable );
            using ( writer as IDisposable )
                context.SerializeGraph ( writer, items );
        }

        public static void SerializeChanges < T > ( this DbContext context, IDbContextSerializer < T > serializer, T writable )
        {
            if ( serializer == null )
                throw new ArgumentNullException ( nameof ( serializer ) );

            var writer = serializer.CreateWriter ( writable );
            using ( writer as IDisposable )
                context.SerializeChanges ( writer );
        }

        public static void SerializeGraphChanges < T > ( this DbContext context, IDbContextSerializer < T > serializer, T writable, object item )
        {
            if ( serializer == null )
                throw new ArgumentNullException ( nameof ( serializer ) );

            var writer = serializer.CreateWriter ( writable );
            using ( writer as IDisposable )
                context.SerializeGraphChanges ( writer, item );
        }

        public static void SerializeGraphChanges < T > ( this DbContext context, IDbContextSerializer < T > serializer, T writable, params object [ ] items )
        {
            if ( serializer == null )
                throw new ArgumentNullException ( nameof ( serializer ) );

            var writer = serializer.CreateWriter ( writable );
            using ( writer as IDisposable )
                context.SerializeGraphChanges ( writer, items );
        }

        public static int SaveChanges < T > ( this DbContext context, IDbContextSerializer < T > serializer, T writable )
        {
            if ( serializer == null )
                throw new ArgumentNullException ( nameof ( serializer ) );

            var writer = serializer.CreateWriter ( writable );
            using ( writer as IDisposable )
                return context.SaveChanges ( writer );
        }

        public static async Task < int > SaveChangesAsync < T > ( this DbContext context, IDbContextSerializer < T > serializer, T writable, CancellationToken cancellationToken = default )
        {
            if ( serializer == null )
                throw new ArgumentNullException ( nameof ( serializer ) );

            var writer = serializer.CreateWriter ( writable );
            using ( writer as IDisposable )
                return await context.SaveChangesAsync ( writer, cancellationToken )
                                    .ConfigureAwait   ( false );
        }
    }
}