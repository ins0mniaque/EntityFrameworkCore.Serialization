using System;

namespace EntityFrameworkCore.Serialization.Binary.Format
{
    public interface IBinaryReader : IDisposable
    {
        object? Read ( Type type );
    }
}