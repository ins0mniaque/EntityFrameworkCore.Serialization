using System;
using System.IO;
using System.Runtime.Serialization;

namespace EntityFrameworkCore.Serialization.Binary
{
    public static class BinaryObjectReader
    {
        public static object? Read ( this BinaryReader reader, Type type )
        {
            if ( type.IsEnum )
                type = Enum.GetUnderlyingType ( type );

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
            var data     = new object [ members.Length ];

            for ( var index = 0; index < members.Length; index++ )
                data [ index ] = reader.Read ( members [ index ].GetSerializableType ( ) );

            return FormatterServices.PopulateObjectMembers ( instance, members, data );
        }
    }
}