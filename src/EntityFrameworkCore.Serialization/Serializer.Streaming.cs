using System;
using System.IO;

using Microsoft.EntityFrameworkCore;

namespace EntityFrameworkCore.Serialization
{
    public static partial class Serializer
    {
        public static void Serialize ( this DbContext context, IDbContextSerializer < Stream > serializer, out byte [ ] data )
        {
            if ( serializer == null )
                throw new ArgumentNullException ( nameof ( serializer ) );

            using var stream = new MemoryStream ( );
            context.Serialize ( serializer, stream );
            data = stream.ToArray ( );
        }

        public static void SerializeGraph ( this DbContext context, IDbContextSerializer < Stream > serializer, out byte [ ] data, object item )
        {
            if ( serializer == null )
                throw new ArgumentNullException ( nameof ( serializer ) );

            using var stream = new MemoryStream ( );
            context.SerializeGraph ( serializer, stream, item );
            data = stream.ToArray ( );
        }

        public static void SerializeGraph ( this DbContext context, IDbContextSerializer < Stream > serializer, out byte [ ] data, params object [ ] items )
        {
            if ( serializer == null )
                throw new ArgumentNullException ( nameof ( serializer ) );

            using var stream = new MemoryStream ( );
            context.SerializeGraph ( serializer, stream, items );
            data = stream.ToArray ( );
        }

        public static void SerializeChanges ( this DbContext context, IDbContextSerializer < Stream > serializer, out byte [ ] data )
        {
            if ( serializer == null )
                throw new ArgumentNullException ( nameof ( serializer ) );

            using var stream = new MemoryStream ( );
            context.SerializeChanges ( serializer, stream );
            data = stream.ToArray ( );
        }

        public static void SerializeGraphChanges ( this DbContext context, IDbContextSerializer < Stream > serializer, out byte [ ] data, object item )
        {
            if ( serializer == null )
                throw new ArgumentNullException ( nameof ( serializer ) );

            using var stream = new MemoryStream ( );
            context.SerializeGraphChanges ( serializer, stream, item );
            data = stream.ToArray ( );
        }

        public static void SerializeGraphChanges ( this DbContext context, IDbContextSerializer < Stream > serializer, out byte [ ] data, params object [ ] items )
        {
            if ( serializer == null )
                throw new ArgumentNullException ( nameof ( serializer ) );

            using var stream = new MemoryStream ( );
            context.SerializeGraphChanges ( serializer, stream, items );
            data = stream.ToArray ( );
        }

        public static int SaveChanges ( this DbContext context, IDbContextSerializer < Stream > serializer, out byte [ ] data )
        {
            if ( serializer == null )
                throw new ArgumentNullException ( nameof ( serializer ) );

            using var stream = new MemoryStream ( );
            var rowCount = context.SaveChanges ( serializer, stream );
            data = stream.ToArray ( );
            return rowCount;
        }
    }
}