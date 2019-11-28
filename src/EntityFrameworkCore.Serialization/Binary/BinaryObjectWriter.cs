using System;
using System.IO;
using System.Runtime.Serialization;

namespace EntityFrameworkCore.Serialization.Binary
{
    public static class BinaryObjectWriter
    {
        public static void Write ( this BinaryWriter writer, Type type, object? value )
        {
            if ( type.IsEnum )
                type = Enum.GetUnderlyingType ( type );

            if      ( type == typeof ( bool    ) ) writer.Write ( (bool   ) value );
            else if ( type == typeof ( byte    ) ) writer.Write ( (byte   ) value );
            else if ( type == typeof ( char    ) ) writer.Write ( (char   ) value );
            else if ( type == typeof ( decimal ) ) writer.Write ( (decimal) value );
            else if ( type == typeof ( double  ) ) writer.Write ( (double ) value );
            else if ( type == typeof ( short   ) ) writer.Write ( (short  ) value );
            else if ( type == typeof ( int     ) ) writer.Write ( (int    ) value );
            else if ( type == typeof ( long    ) ) writer.Write ( (long   ) value );
            else if ( type == typeof ( sbyte   ) ) writer.Write ( (sbyte  ) value );
            else if ( type == typeof ( float   ) ) writer.Write ( (float  ) value );
            else if ( type == typeof ( string  ) ) writer.Write ( (string ) value );
            else if ( type == typeof ( ushort  ) ) writer.Write ( (ushort ) value );
            else if ( type == typeof ( uint    ) ) writer.Write ( (uint   ) value );
            else if ( type == typeof ( ulong   ) ) writer.Write ( (ulong  ) value );
            else if ( type.IsArray )
            {
                if ( value != null )
                {
                    var array = (Array) value;

                    writer.Write ( array.Length + 1 );

                    var elementType = type.GetElementType ( );
                    foreach ( var element in array )
                        writer.Write ( elementType, element );
                }
                else
                    writer.Write ( 0 );
            }
            else if ( value != null )
            {
                var nullableOfType = Nullable.GetUnderlyingType ( type );
                if ( nullableOfType != null )
                {
                    writer.Write ( true );
                    writer.Write ( nullableOfType, value );
                }

                if ( ! type.IsValueType )
                    writer.Write ( true );

                var members = type.GetSerializableMembers ( );
                var data    = FormatterServices.GetObjectData ( value, members );

                for ( var index = 0; index < members.Length; index++ )
                    writer.Write ( members [ index ].GetSerializableType ( ), data [ index ] );
            }
            else
                writer.Write ( false );
        }
    }
}