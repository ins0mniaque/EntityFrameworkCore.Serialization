using System;
using System.IO;

namespace EntityFrameworkCore.Serialization.Binary
{
    public interface IBinaryObjectWriterSurrogate
    {
         bool TryWrite ( BinaryWriter writer, Type type, object? value );
    }
}