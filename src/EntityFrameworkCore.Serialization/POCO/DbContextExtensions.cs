using System.Collections.Generic;

using Microsoft.EntityFrameworkCore;

namespace EntityFrameworkCore.Serialization.POCO
{
    public static class DbContextExtensions
    {
        public static IEnumerable < DbContextEntry > Serialize ( this DbContext dbContext )
        {
            return dbContext.Serialize ( new DbContextEntrySerializer ( ) );
        }

        public static IEnumerable < DbContextEntry > SerializeChanges ( this DbContext dbContext )
        {
            return dbContext.SerializeChanges ( new DbContextEntrySerializer ( ) );
        }

        public static IEnumerable < DbContextIndexedEntry > SerializeIndexed ( this DbContext dbContext )
        {
            return dbContext.Serialize ( new DbContextIndexedEntrySerializer ( ) );
        }

        public static IEnumerable < DbContextIndexedEntry > SerializeChangesIndexed ( this DbContext dbContext )
        {
            return dbContext.SerializeChanges ( new DbContextIndexedEntrySerializer ( ) );
        }

        public static void Deserialize ( this DbContext dbContext, IEnumerable < DbContextEntry > entries )
        {
            dbContext.Deserialize ( entries, new DbContextEntrySerializer ( ) );
        }

        public static void Deserialize ( this DbContext dbContext, IEnumerable < DbContextIndexedEntry > entries )
        {
            dbContext.Deserialize ( entries, new DbContextIndexedEntrySerializer ( ) );
        }
    }
}