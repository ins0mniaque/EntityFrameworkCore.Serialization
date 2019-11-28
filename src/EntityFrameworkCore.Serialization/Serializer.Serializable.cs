using System.Collections.Generic;

using Microsoft.EntityFrameworkCore;

namespace EntityFrameworkCore.Serialization
{
    public static partial class Serializer
    {
        public static void Serialize < T > ( this DbContext context, IDbContextSerializer < ICollection < T > > serializer, out IList < T > collection )
        {
            context.Serialize ( serializer.CreateWriter ( collection = new List < T > ( ) ) );
        }

        public static void SerializeGraph < T > ( this DbContext context, IDbContextSerializer < ICollection < T > > serializer, out IList < T > collection, object item )
        {
            context.SerializeGraph ( serializer.CreateWriter ( collection = new List < T > ( ) ), item );
        }

        public static void SerializeGraph < T > ( this DbContext context, IDbContextSerializer < ICollection < T > > serializer, out IList < T > collection, params object [ ] items )
        {
            context.SerializeGraph ( serializer.CreateWriter ( collection = new List < T > ( ) ), items );
        }

        public static void SerializeChanges < T > ( this DbContext context, IDbContextSerializer < ICollection < T > > serializer, out IList < T > collection )
        {
            context.SerializeChanges ( serializer.CreateWriter ( collection = new List < T > ( ) ) );
        }

        public static void SerializeGraphChanges < T > ( this DbContext context, IDbContextSerializer < ICollection < T > > serializer, out IList < T > collection, object item )
        {
            context.SerializeGraphChanges ( serializer.CreateWriter ( collection = new List < T > ( ) ), item );
        }

        public static void SerializeGraphChanges < T > ( this DbContext context, IDbContextSerializer < ICollection < T > > serializer, out IList < T > collection, params object [ ] items )
        {
            context.SerializeGraphChanges ( serializer.CreateWriter ( collection = new List < T > ( ) ), items );
        }

        public static int SaveChanges < T > ( this DbContext context, IDbContextSerializer < ICollection < T > > serializer, out IList < T > collection )
        {
            return context.SaveChanges ( serializer.CreateWriter ( collection = new List < T > ( ) ) );
        }
    }
}