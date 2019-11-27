using System.Collections.Generic;

namespace EntityFrameworkCore.Serialization.Dictionary
{
    public class DbContextSerializer : IDbContextSerializer   < ICollection < Entry > >,
                                       IDbContextDeserializer < IEnumerable < Entry > >
    {
        public IEntityEntryReader CreateReader ( IEnumerable < Entry > readable ) => new EntityEntryReader ( readable );
        public IEntityEntryWriter CreateWriter ( ICollection < Entry > writable ) => new EntityEntryWriter ( writable );
    }
}