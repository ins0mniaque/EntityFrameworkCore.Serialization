using System.Collections.Generic;

namespace EntityFrameworkCore.Serialization.Serializable
{
    public class DbContextSerializer : IDbContextSerializer   < ICollection < SerializableEntry > >,
                                       IDbContextDeserializer < IEnumerable < SerializableEntry > >
    {
        public IEntityEntryReader CreateReader ( IEnumerable < SerializableEntry > readable ) => new EntityEntryReader ( readable );
        public IEntityEntryWriter CreateWriter ( ICollection < SerializableEntry > writable ) => new EntityEntryWriter ( writable );
    }
}