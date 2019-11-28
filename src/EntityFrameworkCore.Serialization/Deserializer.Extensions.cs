using Microsoft.EntityFrameworkCore;

namespace EntityFrameworkCore.Serialization
{
    public static partial class Deserializer
    {
        public static void Deserialize < T > ( this DbContext context, IDbContextDeserializer < T > deserializer, T readable )
        {
            context.Deserialize ( deserializer.CreateReader ( readable ) );
        }

        public static void AcceptChanges < T > ( this DbContext context, IDbContextDeserializer < T > deserializer, T readable )
        {
            context.AcceptChanges ( deserializer.CreateReader ( readable ) );
        }
    }
}