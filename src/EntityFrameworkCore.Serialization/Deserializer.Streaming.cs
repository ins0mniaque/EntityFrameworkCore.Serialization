using System;
using System.Collections.Generic;
using System.IO;

using Microsoft.EntityFrameworkCore;

namespace EntityFrameworkCore.Serialization
{
    public static partial class Deserializer
    {
        public static IReadOnlyList < object > Deserialize ( this DbContext context, IDbContextDeserializer < Stream > deserializer, byte [ ] data )
        {
            if ( deserializer == null )
                throw new ArgumentNullException ( nameof ( deserializer ) );

            using var stream = new MemoryStream ( data );
            return context.Deserialize ( deserializer.CreateReader ( stream ) );
        }

        public static void AcceptChanges ( this DbContext context, IDbContextDeserializer < Stream > deserializer, byte [ ] data )
        {
            if ( deserializer == null )
                throw new ArgumentNullException ( nameof ( deserializer ) );

            using var stream = new MemoryStream ( data );
            context.AcceptChanges ( deserializer.CreateReader ( stream ) );
        }
    }
}