using System;
using System.IO;

namespace EntityFrameworkCore.Serialization.Binary.Format
{
    public interface IBinaryReaderSurrogate
    {
         bool TryRead ( BinaryReader reader, Type type, out object? value );
    }

    public interface IBinaryFormatSurrogate : IBinaryReaderSurrogate, IBinaryWriterSurrogate { }
}