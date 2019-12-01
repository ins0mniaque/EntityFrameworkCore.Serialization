using System;
using System.Collections.Generic;

using Microsoft.EntityFrameworkCore;

namespace EntityFrameworkCore.Serialization
{
    public static partial class Deserializer
    {
        public static IReadOnlyList < object > Deserialize < T > ( this DbContext context, IDbContextDeserializer < T > deserializer, T readable )
        {
            if ( deserializer == null )
                throw new ArgumentNullException ( nameof ( deserializer ) );

            return context.Deserialize ( deserializer.CreateReader ( readable ) );
        }

        public static void AcceptChanges < T > ( this DbContext context, IDbContextDeserializer < T > deserializer, T readable )
        {
            if ( deserializer == null )
                throw new ArgumentNullException ( nameof ( deserializer ) );

            context.AcceptChanges ( deserializer.CreateReader ( readable ) );
        }
    }
}