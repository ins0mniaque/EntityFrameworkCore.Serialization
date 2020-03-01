using System;
using System.IO;

namespace EntityFrameworkCore.Serialization.Binary.Format
{
    public static class BinaryFormat
    {
        public static void Serialize ( Stream stream, Type type, object? value, IBinaryWriterSurrogate? surrogate = default )
        {
            using var writer = new StreamBinaryWriter ( stream, surrogate );

            writer.Write ( type, value );
        }

        public static void Serialize < T > ( Stream stream, T value, IBinaryWriterSurrogate? surrogate = default )
        {
            using var writer = new StreamBinaryWriter ( stream, surrogate );

            writer.Write ( typeof ( T ), value );
        }

        public static byte [ ] Serialize ( Type type, object? value, IBinaryWriterSurrogate? surrogate = default )
        {
            using var stream = new MemoryStream ( );

            Serialize ( stream, type, value, surrogate );

            return stream.ToArray ( );
        }

        public static byte [ ] Serialize < T > ( T value, IBinaryWriterSurrogate? surrogate = default )
        {
            using var stream = new MemoryStream ( );

            Serialize ( stream, value, surrogate );

            return stream.ToArray ( );
        }

        public static object? Deserialize ( Stream stream, Type type, IBinaryReaderSurrogate? surrogate = default )
        {
            using var reader = new StreamBinaryReader ( stream, surrogate );

            return reader.Read ( type );
        }

        public static T Deserialize < T > ( Stream stream, IBinaryReaderSurrogate? surrogate = default )
        {
            using var reader = new StreamBinaryReader ( stream, surrogate );

            return (T) reader.Read ( typeof ( T ) )!;
        }

        public static object? Deserialize ( Type type, byte [ ] data, IBinaryReaderSurrogate? surrogate = default )
        {
            using var stream = new MemoryStream ( data );

            return Deserialize ( stream, type, surrogate );
        }

        public static T Deserialize < T > ( byte [ ] data, IBinaryReaderSurrogate? surrogate = default )
        {
            using var stream = new MemoryStream ( data );

            return Deserialize < T > ( stream, surrogate );
        }
    }
}