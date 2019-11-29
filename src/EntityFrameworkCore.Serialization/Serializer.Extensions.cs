using System;

using Microsoft.EntityFrameworkCore;

namespace EntityFrameworkCore.Serialization
{
    public static partial class Serializer
    {
        public static void Serialize < T > ( this DbContext context, IDbContextSerializer < T > serializer, T writable )
        {
            if ( serializer == null )
                throw new ArgumentNullException ( nameof ( serializer ) );

            context.Serialize ( serializer.CreateWriter ( writable ) );
        }

        public static void SerializeGraph < T > ( this DbContext context, IDbContextSerializer < T > serializer, T writable, object item )
        {
            if ( serializer == null )
                throw new ArgumentNullException ( nameof ( serializer ) );

            context.SerializeGraph ( serializer.CreateWriter ( writable ), item );
        }

        public static void SerializeGraph < T > ( this DbContext context, IDbContextSerializer < T > serializer, T writable, params object [ ] items )
        {
            if ( serializer == null )
                throw new ArgumentNullException ( nameof ( serializer ) );

            context.SerializeGraph ( serializer.CreateWriter ( writable ), items );
        }

        public static void SerializeChanges < T > ( this DbContext context, IDbContextSerializer < T > serializer, T writable )
        {
            if ( serializer == null )
                throw new ArgumentNullException ( nameof ( serializer ) );

            context.SerializeChanges ( serializer.CreateWriter ( writable ) );
        }

        public static void SerializeGraphChanges < T > ( this DbContext context, IDbContextSerializer < T > serializer, T writable, object item )
        {
            if ( serializer == null )
                throw new ArgumentNullException ( nameof ( serializer ) );

            context.SerializeGraphChanges ( serializer.CreateWriter ( writable ), item );
        }

        public static void SerializeGraphChanges < T > ( this DbContext context, IDbContextSerializer < T > serializer, T writable, params object [ ] items )
        {
            if ( serializer == null )
                throw new ArgumentNullException ( nameof ( serializer ) );

            context.SerializeGraphChanges ( serializer.CreateWriter ( writable ), items );
        }

        public static int SaveChanges < T > ( this DbContext context, IDbContextSerializer < T > serializer, T writable )
        {
            if ( serializer == null )
                throw new ArgumentNullException ( nameof ( serializer ) );

            return context.SaveChanges ( serializer.CreateWriter ( writable ) );
        }
    }
}