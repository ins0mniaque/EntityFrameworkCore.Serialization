using System.IO;

using EntityFrameworkCore.Serialization.Binary.Format;

namespace EntityFrameworkCore.Serialization.Binary
{
    public class BinaryDbContextSerializer : IDbContextSerializer < Stream >
    {
        private readonly IBinaryReaderSurrogate?  readSurrogate;
        private readonly IBinaryWriterSurrogate? writeSurrogate;

        public BinaryDbContextSerializer ( ) : this ( null, null ) { }
        public BinaryDbContextSerializer ( IBinaryFormatSurrogate? surrogate ) : this ( surrogate, surrogate ) { }
        public BinaryDbContextSerializer ( IBinaryReaderSurrogate? readSurrogate, IBinaryWriterSurrogate? writeSurrogate )
        {
            this.readSurrogate  = readSurrogate;
            this.writeSurrogate = writeSurrogate;
        }

        public IEntityEntryReader CreateReader ( Stream stream ) => new BinaryEntityEntryReader ( new StreamBinaryReader ( stream, readSurrogate  ) );
        public IEntityEntryWriter CreateWriter ( Stream stream ) => new BinaryEntityEntryWriter ( new StreamBinaryWriter ( stream, writeSurrogate ) );
    }
}