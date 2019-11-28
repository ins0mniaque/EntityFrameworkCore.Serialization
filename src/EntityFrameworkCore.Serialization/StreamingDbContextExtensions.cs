using System.IO;

using Microsoft.EntityFrameworkCore;

namespace EntityFrameworkCore.Serialization
{
    public static class StreamingDbContextExtensions
    {
        public static int SaveChanges ( this DbContext context, IDbContextSerializer < Stream > serializer, out byte [ ] data )
        {
            using var stream = new MemoryStream ( );
            var rowCount = context.SaveChanges ( serializer.CreateWriter ( stream ) );
            data = stream.ToArray ( );
            return rowCount;
        }

        public static void AcceptChanges ( this DbContext context, IDbContextDeserializer < Stream > deserializer, byte [ ] data )
        {
            using var stream = new MemoryStream ( data );
            context.AcceptChanges ( deserializer.CreateReader ( stream ) );
        }
    }
}