using System;
using System.IO;

namespace EntityFrameworkCore.Serialization.Binary.Format
{
    public static class BinaryFormatReader
    {
        public static T Read < T > ( this BinaryReader reader, IBinaryReaderSurrogate? surrogate = default )
        {
            #pragma warning disable CS8601 // Possible null reference assignment; T can be null
            return (T) reader.Read ( typeof ( T ), surrogate );
            #pragma warning restore CS8601 // Possible null reference assignment; T can be null
        }

        public static object? Read ( this BinaryReader reader, Type type, IBinaryReaderSurrogate? surrogate = default )
        {
            if ( reader == null ) throw new ArgumentNullException ( nameof ( reader ) );
            if ( type   == null ) throw new ArgumentNullException ( nameof ( type   ) );

            if ( type.IsEnum )
                type = Enum.GetUnderlyingType ( type );

            switch ( Type.GetTypeCode ( type ) )
            {
                case TypeCode.Boolean : return reader.ReadBoolean ( );
                case TypeCode.SByte   : return reader.ReadSByte   ( );
                case TypeCode.Byte    : return reader.ReadByte    ( );
                case TypeCode.Int16   : return reader.ReadInt16   ( );
                case TypeCode.UInt16  : return reader.ReadUInt16  ( );
                case TypeCode.Int32   : return reader.ReadInt32   ( );
                case TypeCode.UInt32  : return reader.ReadUInt32  ( );
                case TypeCode.Int64   : return reader.ReadInt64   ( );
                case TypeCode.UInt64  : return reader.ReadUInt64  ( );
                case TypeCode.Single  : return reader.ReadSingle  ( );
                case TypeCode.Double  : return reader.ReadDouble  ( );
                case TypeCode.Decimal : return reader.ReadDecimal ( );
                case TypeCode.Char    : return reader.ReadChar    ( );
                case TypeCode.String  :
                {
                    if ( ! reader.ReadBoolean ( ) )
                        return null;

                    return reader.ReadString ( );
                }
            }

            if ( type.IsArray )
            {
                var length = reader.ReadInt32 ( ) - 1;
                if ( length == -1 )
                    return null;

                var elementType = type.GetElementType ( );
                var array       = Array.CreateInstance ( elementType, length );
                for ( var index = 0; index < array.Length; index++ )
                    array.SetValue ( reader.Read ( elementType, surrogate ), index );

                return array;
            }

            var nullableOfType = Nullable.GetUnderlyingType ( type );
            if ( nullableOfType != null )
            {
                if ( ! reader.ReadBoolean ( ) )
                    return null;

                return reader.Read ( nullableOfType, surrogate );
            }

            if ( ! type.IsValueType && ! reader.ReadBoolean ( ) )
                return null;

            if ( surrogate != null && surrogate.TryRead ( reader, type, out var value ) )
                return value;

            var instance = type.GetUninitializedObject ( );
            var members  = type.GetSerializableMembers ( );
            var data     = new object? [ members.Length ];

            for ( var index = 0; index < members.Length; index++ )
                data [ index ] = reader.Read ( members [ index ].GetSerializableType ( ), surrogate );

            return members.SetObjectData ( instance, data );
        }

        public static T Read < T > ( this IBinaryReader reader )
        {
            if ( reader == null )
                throw new ArgumentNullException ( nameof ( reader ) );

            return (T) reader.Read ( typeof ( T ) )!;
        }

        public static bool TryReadByte ( this IBinaryReader reader, out byte value )
        {
            if ( reader == null )
                throw new ArgumentNullException ( nameof ( reader ) );

            if ( reader is IStreamBinaryReader streamReader && streamReader.BaseStream.CanSeek )
            {
                if ( streamReader.BaseStream.Position == streamReader.BaseStream.Length )
                {
                    value = default;
                    return false;
                }

                value = reader.Read < byte > ( );
                return true;
            }

            return reader.TryReadByteWithoutSeeking ( out value );
        }

        private static bool TryReadByteWithoutSeeking ( this IBinaryReader reader, out byte value )
        {
            try
            {
                value = reader.Read < byte > ( );
                return true;
            }
            catch ( EndOfStreamException )
            {
                value = default;
                return false;
            }
        }
    }
}