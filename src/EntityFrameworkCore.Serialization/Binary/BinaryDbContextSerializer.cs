using System.IO;

namespace EntityFrameworkCore.Serialization.Binary
{
    public class BinaryDbContextSerializer : IDbContextSerializer   < Stream >,
                                             IDbContextDeserializer < Stream >
    {
        public IEntityEntryReader CreateReader ( Stream stream ) => new BinaryEntityEntryReader ( stream );
        public IEntityEntryWriter CreateWriter ( Stream stream ) => new BinaryEntityEntryWriter ( stream );
    }
}