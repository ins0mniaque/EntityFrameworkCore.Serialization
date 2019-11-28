using System.Collections.Generic;

using Microsoft.EntityFrameworkCore;

namespace EntityFrameworkCore.Serialization
{
    public static class CollectionDbContextExtensions
    {
        public static int SaveChanges < T > ( this DbContext context, IDbContextSerializer < ICollection < T > > serializer, out IList < T > collection )
        {
            return context.SaveChanges ( serializer.CreateWriter ( collection = new List < T > ( ) ) );
        }
    }
}