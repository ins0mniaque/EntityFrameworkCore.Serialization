using System;
using System.Collections.Generic;
using System.IO;

namespace EntityFrameworkCore.Serialization.Binary
{
    public class BinarySerializerSurrogate : IBinaryObjectReaderSurrogate, IBinaryObjectWriterSurrogate
    {
        private readonly Dictionary < Type, Converter > converters = new Dictionary < Type, Converter > ( );

        public void AddConverter ( Type type, Func < BinaryReader, object? > read, Action < BinaryWriter, object? > write )
        {
            converters.Add ( type, new Converter ( read, write ) );
        }

        public void RemoveConverter ( Type type )
        {
            converters.Remove ( type );
        }

        public void AddConverter < T > ( Func < BinaryReader, T > read, Action < BinaryWriter, T > write )
        {
            AddConverter ( typeof ( T ),
                           reader => read ( reader ),
                           (writer, value) =>
                           {
                               if ( value is T t )
                                   write ( writer, t );
                               else
                                   throw new InvalidCastException ( );
                           } );
        }

        public void RemoveConverter < T > ( ) => RemoveConverter ( typeof ( T ) );

        public bool TryRead ( BinaryReader reader, Type type, out object? value )
        {
            if ( converters.TryGetValue ( type, out var converter ) )
            {
                value = converter.Read ( reader );
                return true;
            }

            value = null;
            return false;
        }

        public bool TryWrite ( BinaryWriter writer, Type type, object? value )
        {
            if ( converters.TryGetValue ( type, out var converter ) )
            {
                converter.Write ( writer, value );
                return true;
            }

            return false;
        }

        private class Converter
        {
            public Converter ( Func < BinaryReader, object? > read, Action < BinaryWriter, object? > write )
            {
                Read  = read;
                Write = write;
            }

            public Func   < BinaryReader, object? > Read  { get; }
            public Action < BinaryWriter, object? > Write { get; }
        }
    }
}