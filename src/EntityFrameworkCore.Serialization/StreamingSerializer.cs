using System.IO;

using Microsoft.EntityFrameworkCore;

namespace EntityFrameworkCore.Serialization
{
    public static class StreamingSerializer
    {
        public static void Serialize ( this DbContext context, IDbContextSerializer < Stream > serializer, out byte [ ] data )
        {
            using var stream = new MemoryStream ( );
            context.Serialize ( serializer.CreateWriter ( stream ) );
            data = stream.ToArray ( );
        }

        public static void SerializeGraph ( this DbContext context, IDbContextSerializer < Stream > serializer, out byte [ ] data, object item )
        {
            using var stream = new MemoryStream ( );
            context.SerializeGraph ( serializer.CreateWriter ( stream ), item );
            data = stream.ToArray ( );
        }

        public static void SerializeGraph ( this DbContext context, IDbContextSerializer < Stream > serializer, out byte [ ] data, params object [ ] items )
        {
            using var stream = new MemoryStream ( );
            context.SerializeGraph ( serializer.CreateWriter ( stream ), items );
            data = stream.ToArray ( );
        }

        public static void SerializeChanges ( this DbContext context, IDbContextSerializer < Stream > serializer, out byte [ ] data )
        {
            using var stream = new MemoryStream ( );
            context.SerializeChanges ( serializer.CreateWriter ( stream ) );
            data = stream.ToArray ( );
        }

        public static void SerializeGraphChanges ( this DbContext context, IDbContextSerializer < Stream > serializer, out byte [ ] data, object item )
        {
            using var stream = new MemoryStream ( );
            context.SerializeGraphChanges ( serializer.CreateWriter ( stream ), item );
            data = stream.ToArray ( );
        }

        public static void SerializeGraphChanges ( this DbContext context, IDbContextSerializer < Stream > serializer, out byte [ ] data, params object [ ] items )
        {
            using var stream = new MemoryStream ( );
            context.SerializeGraphChanges ( serializer.CreateWriter ( stream ), items );
            data = stream.ToArray ( );
        }
    }
}