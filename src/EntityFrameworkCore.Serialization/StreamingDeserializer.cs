using System.IO;

using Microsoft.EntityFrameworkCore;

namespace EntityFrameworkCore.Serialization
{
    public static class StreamingDeserializer
    {
        public static void Deserialize ( this DbContext context, IDbContextDeserializer < Stream > deserializer, byte [ ] data )
        {
            using var stream = new MemoryStream ( data );
            context.Deserialize ( deserializer.CreateReader ( stream ) );
        }
    }
}