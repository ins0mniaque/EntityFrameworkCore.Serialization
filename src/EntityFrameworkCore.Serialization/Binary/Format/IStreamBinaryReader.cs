using System.IO;

namespace EntityFrameworkCore.Serialization.Binary.Format
{
    public interface IStreamBinaryReader : IBinaryReader
    {
        Stream BaseStream { get; }
    }
}