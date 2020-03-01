using System;
using System.IO;
using System.Text;

namespace EntityFrameworkCore.Serialization.Binary.Format
{
    public sealed class StreamBinaryWriter : IBinaryWriter
    {
        public StreamBinaryWriter ( Stream stream, IBinaryWriterSurrogate? surrogate )
        {
            BinaryWriter = new IO.BinaryWriterWith7BitEncoding ( stream, new UTF8Encoding ( false, true ), true );
            Surrogate    = surrogate;
        }

        public Stream                  BaseStream => BinaryWriter.BaseStream;
        public BinaryWriter            BinaryWriter { get; }
        public IBinaryWriterSurrogate? Surrogate    { get; }

        public void Write ( Type type, object? value ) => BinaryWriter.Write ( type, value, Surrogate );

        public void Dispose ( ) => BinaryWriter.Dispose ( );
    }
}