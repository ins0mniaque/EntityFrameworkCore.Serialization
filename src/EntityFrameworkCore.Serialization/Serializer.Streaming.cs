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
            context.Serialize ( serializer.CreateWriter ( stream ) );
            data = stream.ToArray ( );
        }

        public static void SerializeGraph ( this DbContext context, IDbContextSerializer < Stream > serializer, out byte [ ] data, object item )
        {
            if ( serializer == null )
                throw new ArgumentNullException ( nameof ( serializer ) );

            using var stream = new MemoryStream ( );
            context.SerializeGraph ( serializer.CreateWriter ( stream ), item );
            data = stream.ToArray ( );
        }

        public static void SerializeGraph ( this DbContext context, IDbContextSerializer < Stream > serializer, out byte [ ] data, params object [ ] items )
        {
            if ( serializer == null )
                throw new ArgumentNullException ( nameof ( serializer ) );

            using var stream = new MemoryStream ( );
            context.SerializeGraph ( serializer.CreateWriter ( stream ), items );
            data = stream.ToArray ( );
        }

        public static void SerializeChanges ( this DbContext context, IDbContextSerializer < Stream > serializer, out byte [ ] data )
        {
            if ( serializer == null )
                throw new ArgumentNullException ( nameof ( serializer ) );

            using var stream = new MemoryStream ( );
            context.SerializeChanges ( serializer.CreateWriter ( stream ) );
            data = stream.ToArray ( );
        }

        public static void SerializeGraphChanges ( this DbContext context, IDbContextSerializer < Stream > serializer, out byte [ ] data, object item )
        {
            if ( serializer == null )
                throw new ArgumentNullException ( nameof ( serializer ) );

            using var stream = new MemoryStream ( );
            context.SerializeGraphChanges ( serializer.CreateWriter ( stream ), item );
            data = stream.ToArray ( );
        }

        public static void SerializeGraphChanges ( this DbContext context, IDbContextSerializer < Stream > serializer, out byte [ ] data, params object [ ] items )
        {
            if ( serializer == null )
                throw new ArgumentNullException ( nameof ( serializer ) );

            using var stream = new MemoryStream ( );
            context.SerializeGraphChanges ( serializer.CreateWriter ( stream ), items );
            data = stream.ToArray ( );
        }

        public static int SaveChanges ( this DbContext context, IDbContextSerializer < Stream > serializer, out byte [ ] data )
        {
            if ( serializer == null )
                throw new ArgumentNullException ( nameof ( serializer ) );

            using var stream = new MemoryStream ( );
            var rowCount = context.SaveChanges ( serializer.CreateWriter ( stream ) );
            data = stream.ToArray ( );
            return rowCount;
        }
    }
}