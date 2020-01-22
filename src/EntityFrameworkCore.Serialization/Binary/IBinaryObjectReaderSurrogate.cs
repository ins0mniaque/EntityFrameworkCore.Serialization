using System;
using System.IO;

namespace EntityFrameworkCore.Serialization.Binary
{
    public interface IBinaryObjectReaderSurrogate
    {
         bool TryRead ( BinaryReader reader, Type type, out object? value );
    }
}