using System;
using System.Collections.Generic;

using Microsoft.EntityFrameworkCore;

namespace EntityFrameworkCore.Serialization
{
    public static partial class Deserializer
    {
        public static IReadOnlyList < object > Deserialize < T > ( this DbContext context, IDbContextSerializer < T > deserializer, T readable )
        {
            if ( deserializer == null )
                throw new ArgumentNullException ( nameof ( deserializer ) );

            var reader = deserializer.CreateReader ( readable );
            using ( reader as IDisposable )
                return context.Deserialize ( reader );
        }

        public static void AcceptChanges < T > ( this DbContext context, IDbContextSerializer < T > deserializer, T readable )
        {
            if ( deserializer == null )
                throw new ArgumentNullException ( nameof ( deserializer ) );

            var reader = deserializer.CreateReader ( readable );
            using ( reader as IDisposable )
                context.AcceptChanges ( reader );
        }
    }
}