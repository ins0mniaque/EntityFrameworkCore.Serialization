using System.IO;

namespace EntityFrameworkCore.Serialization.Binary
{
    public class BinaryDbContextSerializer : IDbContextSerializer   < Stream >,
                                             IDbContextDeserializer < Stream >
    {
        private readonly IBinaryObjectReaderSurrogate? readerSurrogate;
        private readonly IBinaryObjectWriterSurrogate? writerSurrogate;

        public BinaryDbContextSerializer ( ) : this ( null, null ) { }
        public BinaryDbContextSerializer ( IBinaryObjectReaderSurrogate? readerSurrogate, IBinaryObjectWriterSurrogate? writerSurrogate )
        {
            this.readerSurrogate = readerSurrogate;
            this.writerSurrogate = writerSurrogate;
        }

        public IEntityEntryReader CreateReader ( Stream stream ) => new BinaryEntityEntryReader ( stream, readerSurrogate );
        public IEntityEntryWriter CreateWriter ( Stream stream ) => new BinaryEntityEntryWriter ( stream, writerSurrogate );
    }
}