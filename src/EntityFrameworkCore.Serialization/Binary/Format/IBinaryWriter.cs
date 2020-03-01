using System;

namespace EntityFrameworkCore.Serialization.Binary.Format
{
    public interface IBinaryWriter : IDisposable
    {
        void Write ( Type type, object? value );
    }
}