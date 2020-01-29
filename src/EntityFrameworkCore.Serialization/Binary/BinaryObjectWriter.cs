using System;
using System.IO;
using System.Runtime.Serialization;

namespace EntityFrameworkCore.Serialization.Binary
{
    public static class BinaryObjectWriter
    {
        public static void Write < T > ( this BinaryWriter writer, object? value, IBinaryObjectWriterSurrogate? surrogate = default )
        {
            writer.Write ( typeof ( T ), value, surrogate );
        }

        public static void Write ( this BinaryWriter writer, Type type, object? value, IBinaryObjectWriterSurrogate? surrogate = default )
        {
            if ( writer == null ) throw new ArgumentNullException ( nameof ( writer ) );
            if ( type   == null ) throw new ArgumentNullException ( nameof ( type   ) );

            if ( type.IsEnum )
                type = Enum.GetUnderlyingType ( type );

            switch ( Type.GetTypeCode ( type ) )
            {
                case TypeCode.Boolean : writer.Write ( (bool)    ( value ?? throw new InvalidCastException ( ) ) ); return;
                case TypeCode.SByte   : writer.Write ( (sbyte)   ( value ?? throw new InvalidCastException ( ) ) ); return;
                case TypeCode.Byte    : writer.Write ( (byte)    ( value ?? throw new InvalidCastException ( ) ) ); return;
                case TypeCode.Int16   : writer.Write ( (short)   ( value ?? throw new InvalidCastException ( ) ) ); return;
                case TypeCode.UInt16  : writer.Write ( (ushort)  ( value ?? throw new InvalidCastException ( ) ) ); return;
                case TypeCode.Int32   : writer.Write ( (int)     ( value ?? throw new InvalidCastException ( ) ) ); return;
                case TypeCode.UInt32  : writer.Write ( (uint)    ( value ?? throw new InvalidCastException ( ) ) ); return;
                case TypeCode.Int64   : writer.Write ( (long)    ( value ?? throw new InvalidCastException ( ) ) ); return;
                case TypeCode.UInt64  : writer.Write ( (ulong)   ( value ?? throw new InvalidCastException ( ) ) ); return;
                case TypeCode.Single  : writer.Write ( (float)   ( value ?? throw new InvalidCastException ( ) ) ); return;
                case TypeCode.Double  : writer.Write ( (double)  ( value ?? throw new InvalidCastException ( ) ) ); return;
                case TypeCode.Decimal : writer.Write ( (decimal) ( value ?? throw new InvalidCastException ( ) ) ); return;
                case TypeCode.Char    : writer.Write ( (char)    ( value ?? throw new InvalidCastException ( ) ) ); return;
                case TypeCode.String  :
                {
                    if ( value is string text )
                    {
                        writer.Write ( true );
                        writer.Write ( text );
                    }
                    else
                        writer.Write ( false );

                    return;
                }
            }

            if ( type.IsArray )
            {
                if ( value != null )
                {
                    var array = (Array) value;

                    writer.Write ( array.Length + 1 );

                    var elementType = type.GetElementType ( );
                    foreach ( var element in array )
                        writer.Write ( elementType, element, surrogate );
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
                    writer.Write ( nullableOfType, value, surrogate );
                    return;
                }

                if ( ! type.IsValueType )
                    writer.Write ( true );

                if ( surrogate != null && surrogate.TryWrite ( writer, type, value ) )
                    return;

                var members = type.GetSerializableMembers ( );
                var data    = FormatterServices.GetObjectData ( value, members );

                for ( var index = 0; index < members.Length; index++ )
                    writer.Write ( members [ index ].GetSerializableType ( ), data [ index ], surrogate );
            }
            else
                writer.Write ( false );
        }
    }
}