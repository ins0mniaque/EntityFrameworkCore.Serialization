using System;
using System.IO;
using System.Text;

namespace EntityFrameworkCore.Serialization.Binary.Format
{
    public sealed class StreamBinaryReader : IStreamBinaryReader
    {
        public StreamBinaryReader ( Stream stream, IBinaryReaderSurrogate? surrogate = default )
        {
            BinaryReader = new IO.BinaryReaderWith7BitEncoding ( stream, new UTF8Encoding ( false, true ), true );
            Surrogate    = surrogate;
        }

        public Stream                  BaseStream => BinaryReader.BaseStream;
        public BinaryReader            BinaryReader { get; }
        public IBinaryReaderSurrogate? Surrogate    { get; }

        public object? Read ( Type type ) => BinaryReader.Read ( type, Surrogate );

        public void Dispose ( ) => BinaryReader.Dispose ( );
    }
}