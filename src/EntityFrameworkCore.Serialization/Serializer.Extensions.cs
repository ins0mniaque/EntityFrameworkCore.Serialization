using Microsoft.EntityFrameworkCore;

namespace EntityFrameworkCore.Serialization
{
    public static partial class Serializer
    {
        public static void Serialize < T > ( this DbContext context, IDbContextSerializer < T > serializer, T writable )
        {
            context.Serialize ( serializer.CreateWriter ( writable ) );
        }

        public static void SerializeGraph < T > ( this DbContext context, IDbContextSerializer < T > serializer, T writable, object item )
        {
            context.SerializeGraph ( serializer.CreateWriter ( writable ), item );
        }

        public static void SerializeGraph < T > ( this DbContext context, IDbContextSerializer < T > serializer, T writable, params object [ ] items )
        {
            context.SerializeGraph ( serializer.CreateWriter ( writable ), items );
        }

        public static void SerializeChanges < T > ( this DbContext context, IDbContextSerializer < T > serializer, T writable )
        {
            context.SerializeChanges ( serializer.CreateWriter ( writable ) );
        }

        public static void SerializeGraphChanges < T > ( this DbContext context, IDbContextSerializer < T > serializer, T writable, object item )
        {
            context.SerializeGraphChanges ( serializer.CreateWriter ( writable ), item );
        }

        public static void SerializeGraphChanges < T > ( this DbContext context, IDbContextSerializer < T > serializer, T writable, params object [ ] items )
        {
            context.SerializeGraphChanges ( serializer.CreateWriter ( writable ), items );
        }

        public static int SaveChanges < T > ( this DbContext context, IDbContextSerializer < T > serializer, T writable )
        {
            return context.SaveChanges ( serializer.CreateWriter ( writable ) );
        }
    }
}