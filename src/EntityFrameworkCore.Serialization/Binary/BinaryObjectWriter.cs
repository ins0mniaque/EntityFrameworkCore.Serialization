﻿using System;
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

            switch ( Type.GetTypeCode ( type ) )
            {
                case TypeCode.Boolean : writer.Write ( (bool)    value ); return;
                case TypeCode.SByte   : writer.Write ( (sbyte)   value ); return;
                case TypeCode.Byte    : writer.Write ( (byte)    value ); return;
                case TypeCode.Int16   : writer.Write ( (short)   value ); return;
                case TypeCode.UInt16  : writer.Write ( (ushort)  value ); return;
                case TypeCode.Int32   : writer.Write ( (int)     value ); return;
                case TypeCode.UInt32  : writer.Write ( (uint)    value ); return;
                case TypeCode.Int64   : writer.Write ( (long)    value ); return;
                case TypeCode.UInt64  : writer.Write ( (ulong)   value ); return;
                case TypeCode.Single  : writer.Write ( (float)   value ); return;
                case TypeCode.Double  : writer.Write ( (double)  value ); return;
                case TypeCode.Decimal : writer.Write ( (decimal) value ); return;
                case TypeCode.Char    : writer.Write ( (char)    value ); return;
                case TypeCode.String  : writer.Write ( (string)  value ); return;
            }

            if ( type.IsArray )
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