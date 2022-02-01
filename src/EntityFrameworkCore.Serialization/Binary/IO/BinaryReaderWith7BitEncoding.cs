using System;
using System.IO;
using System.Text;

namespace EntityFrameworkCore.Serialization.Binary.IO
{
    public class BinaryReaderWith7BitEncoding : BinaryReader
    {
        public BinaryReaderWith7BitEncoding ( Stream input )                                    : base ( input ) { }
        public BinaryReaderWith7BitEncoding ( Stream input, Encoding encoding )                 : base ( input, encoding ) { }
        public BinaryReaderWith7BitEncoding ( Stream input, Encoding encoding, bool leaveOpen ) : base ( input, encoding, leaveOpen ) { }

        public override int   ReadInt32  ( ) => Read7BitEncodedInt32  ( );
        public override uint  ReadUInt32 ( ) => Read7BitEncodedUInt32 ( );
        public override long  ReadInt64  ( ) => Read7BitEncodedInt64  ( );
        public override ulong ReadUInt64 ( ) => Read7BitEncodedUInt64 ( );

        protected int Read7BitEncodedInt32 ( )
        {
            int  count = 0;
            int  shift = 0;
            byte b;
            do
            {
                if ( shift == 5 * 7 )
                    throw TooManyBytesError ( typeof ( int ) );

                b = ReadByte ( );
                count |= ( b & 0x7F ) << shift;
                shift += 7;
            }
            while ( ( b & 0x80 ) != 0 );

            return count;
        }

        protected uint Read7BitEncodedUInt32 ( )
        {
            uint count = 0;
            int  shift = 0;
            byte b;
            do
            {
                if ( shift == 5 * 7 )
                    throw TooManyBytesError ( typeof ( uint ) );

                b = ReadByte ( );
                count |= (uint) ( b & 0x7F ) << shift;
                shift += 7;
            }
            while ( ( b & 0x80 ) != 0 );

            return count;
        }

        protected ulong Read7BitEncodedUInt64 ( )
        {
            ulong count = 0;
            int   shift = 0;
            byte  b;
            do
            {
                if ( shift == 10 * 7 )
                    throw TooManyBytesError ( typeof ( ulong ) );

                b = ReadByte ( );
                count |= (ulong) ( b & 0x7F ) << shift;
                shift += 7;
            }
            while ( ( b & 0x80 ) != 0 );

            return count;
        }

        private static FormatException TooManyBytesError ( Type type ) => new FormatException ( $"Too many bytes in what should have been a 7 bit encoded { type.Name }." );
    }
}