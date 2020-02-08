using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

namespace EntityFrameworkCore.Serialization
{
    public static partial class Deserializer
    {
        public static async Task < IReadOnlyList < object > > DeserializeAsync ( this DbContext context, IDbContextDeserializer < Stream > deserializer, Stream stream )
        {
            if ( deserializer == null )
                throw new ArgumentNullException ( nameof ( deserializer ) );

            using var buffer = await ReadAsync ( stream ).ConfigureAwait ( false );

            return context.Deserialize ( deserializer, buffer );
        }

        public static async Task AcceptChangesAsync ( this DbContext context, IDbContextDeserializer < Stream > deserializer, Stream stream )
        {
            if ( deserializer == null )
                throw new ArgumentNullException ( nameof ( deserializer ) );

            using var buffer = await ReadAsync ( stream ).ConfigureAwait ( false );

            context.AcceptChanges ( deserializer, buffer );
        }

        private static async Task < MemoryStream > ReadAsync ( Stream stream )
        {
            if ( stream == null )
                throw new ArgumentNullException ( nameof ( stream ) );

            var buffer = new MemoryStream ( );

            await stream.CopyToAsync    ( buffer )
                        .ConfigureAwait ( false  );

            buffer.Seek ( 0, SeekOrigin.Begin );

            return buffer;
        }
    }
}