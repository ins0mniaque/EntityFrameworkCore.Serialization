using System;
using System.IO;
using System.Runtime.Serialization;

namespace EntityFrameworkCore.Serialization.Binary
{
    public static class BinaryObjectReader
    {
        public static object? Read ( this BinaryReader reader, Type type )
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
                case TypeCode.String  : return reader.ReadString  ( );
            }

            if ( type.IsArray )
            {
                var length = reader.ReadInt32 ( ) - 1;
                if ( length == -1 )
                    return null;

                var elementType = type.GetElementType ( );
                var array       = Array.CreateInstance ( elementType, length );
                for ( var index = 0; index < array.Length; index++ )
                    array.SetValue ( reader.Read ( elementType ), index );

                return array;
            }

            var nullableOfType = Nullable.GetUnderlyingType ( type );
            if ( nullableOfType != null )
            {
                if ( ! reader.ReadBoolean ( ) )
                    return null;

                return reader.Read ( nullableOfType );
            }

            if ( ! type.IsValueType && ! reader.ReadBoolean ( ) )
                return null;

            var instance = FormatterServices.GetUninitializedObject ( type );
            var members  = type.GetSerializableMembers ( );
            var data     = new object? [ members.Length ];

            for ( var index = 0; index < members.Length; index++ )
                data [ index ] = reader.Read ( members [ index ].GetSerializableType ( ) );

            return FormatterServices.PopulateObjectMembers ( instance, members, data );
        }

        public static bool TryReadByte ( this BinaryReader reader, out byte value )
        {
            if ( reader == null )
                throw new ArgumentNullException ( nameof ( reader ) );

            if ( ! reader.BaseStream.CanSeek )
                return reader.TryReadByteWithoutSeeking ( out value );

            if ( reader.BaseStream.Position == reader.BaseStream.Length )
            {
                value = default;
                return false;
            }

            value = reader.ReadByte ( );
            return true;
        }

        private static bool TryReadByteWithoutSeeking ( this BinaryReader reader, out byte value )
        {
            try
            {
                value = reader.ReadByte ( );
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