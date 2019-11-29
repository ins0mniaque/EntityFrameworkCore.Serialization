using System;
using System.Collections.Generic;

using Microsoft.EntityFrameworkCore;

namespace EntityFrameworkCore.Serialization
{
    public static partial class Serializer
    {
        public static void Serialize < T > ( this DbContext context, IDbContextSerializer < ICollection < T > > serializer, out IList < T > collection )
        {
            if ( serializer == null )
                throw new ArgumentNullException ( nameof ( serializer ) );

            context.Serialize ( serializer.CreateWriter ( collection = new List < T > ( ) ) );
        }

        public static void SerializeGraph < T > ( this DbContext context, IDbContextSerializer < ICollection < T > > serializer, out IList < T > collection, object item )
        {
            if ( serializer == null )
                throw new ArgumentNullException ( nameof ( serializer ) );

            context.SerializeGraph ( serializer.CreateWriter ( collection = new List < T > ( ) ), item );
        }

        public static void SerializeGraph < T > ( this DbContext context, IDbContextSerializer < ICollection < T > > serializer, out IList < T > collection, params object [ ] items )
        {
            if ( serializer == null )
                throw new ArgumentNullException ( nameof ( serializer ) );

            context.SerializeGraph ( serializer.CreateWriter ( collection = new List < T > ( ) ), items );
        }

        public static void SerializeChanges < T > ( this DbContext context, IDbContextSerializer < ICollection < T > > serializer, out IList < T > collection )
        {
            if ( serializer == null )
                throw new ArgumentNullException ( nameof ( serializer ) );

            context.SerializeChanges ( serializer.CreateWriter ( collection = new List < T > ( ) ) );
        }

        public static void SerializeGraphChanges < T > ( this DbContext context, IDbContextSerializer < ICollection < T > > serializer, out IList < T > collection, object item )
        {
            if ( serializer == null )
                throw new ArgumentNullException ( nameof ( serializer ) );

            context.SerializeGraphChanges ( serializer.CreateWriter ( collection = new List < T > ( ) ), item );
        }

        public static void SerializeGraphChanges < T > ( this DbContext context, IDbContextSerializer < ICollection < T > > serializer, out IList < T > collection, params object [ ] items )
        {
            if ( serializer == null )
                throw new ArgumentNullException ( nameof ( serializer ) );

            context.SerializeGraphChanges ( serializer.CreateWriter ( collection = new List < T > ( ) ), items );
        }

        public static int SaveChanges < T > ( this DbContext context, IDbContextSerializer < ICollection < T > > serializer, out IList < T > collection )
        {
            if ( serializer == null )
                throw new ArgumentNullException ( nameof ( serializer ) );

            return context.SaveChanges ( serializer.CreateWriter ( collection = new List < T > ( ) ) );
        }
    }
}