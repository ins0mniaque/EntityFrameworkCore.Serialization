using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace EntityFrameworkCore.Serialization.Binary
{
    public static class BinaryObjectReader
    {
        public static object? Read ( this BinaryReader reader, Type type )
        {
            if ( type == typeof ( bool    ) ) return reader.ReadBoolean ( );
            if ( type == typeof ( byte    ) ) return reader.ReadByte    ( );
            if ( type == typeof ( char    ) ) return reader.ReadChar    ( );
            if ( type == typeof ( decimal ) ) return reader.ReadDecimal ( );
            if ( type == typeof ( double  ) ) return reader.ReadDouble  ( );
            if ( type == typeof ( short   ) ) return reader.ReadInt16   ( );
            if ( type == typeof ( int     ) ) return reader.ReadInt32   ( );
            if ( type == typeof ( long    ) ) return reader.ReadInt64   ( );
            if ( type == typeof ( sbyte   ) ) return reader.ReadSByte   ( );
            if ( type == typeof ( float   ) ) return reader.ReadSingle  ( );
            if ( type == typeof ( string  ) ) return reader.ReadString  ( );
            if ( type == typeof ( ushort  ) ) return reader.ReadUInt16  ( );
            if ( type == typeof ( uint    ) ) return reader.ReadUInt32  ( );
            if ( type == typeof ( ulong   ) ) return reader.ReadUInt64  ( );

            if ( type.IsArray )
            {
                var length = reader.ReadInt32 ( );
                if ( length == 0 )
                    return null;

                length--;

                var elementType = type.GetElementType ( );
                var array       = Array.CreateInstance ( elementType, length );
                for ( var index = 0; index < array.Length; index++ )
                    array.SetValue ( reader.Read ( elementType ), index );

                return array;
            }

            var nullableOfType = Nullable.GetUnderlyingType ( type );
            if ( nullableOfType != null )
            {
                var isNull = reader.ReadBoolean ( );
                if ( isNull )
                    return null;

                return reader.Read ( nullableOfType );
            }

            var converter = TypeDescriptor.GetConverter ( type );
            if ( converter.CanConvertFrom ( typeof ( byte [ ] ) ) &&
                 converter.CanConvertTo   ( typeof ( byte [ ] ) ) )
            {
                var bytes = (byte [ ]) reader.Read ( typeof ( byte [ ] ) );

                return converter.ConvertFrom ( bytes );
            }

            return new BinaryFormatter ( ).Deserialize ( reader.BaseStream );
        }
    }
}