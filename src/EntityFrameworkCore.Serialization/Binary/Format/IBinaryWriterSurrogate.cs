using System;
using System.IO;

namespace EntityFrameworkCore.Serialization.Binary.Format
{
    public interface IBinaryWriterSurrogate
    {
        bool TryWrite ( BinaryWriter writer, Type type, object? value );
    }
}