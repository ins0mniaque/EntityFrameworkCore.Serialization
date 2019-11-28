﻿using System;
using System.IO;
using System.Text;

namespace EntityFrameworkCore.Serialization.Binary
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
                    throw new FormatException ( "Too many bytes in what should have been a 7 bit encoded Int32." );

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
                    throw new FormatException ( "Too many bytes in what should have been a 7 bit encoded UInt32." );

                b = ReadByte ( );
                count |= (uint) ( b & 0x7F ) << shift;
                shift += 7;
            }
            while ( ( b & 0x80 ) != 0 );

            return count;
        }

        protected long Read7BitEncodedInt64 ( )
        {
            long count = 0;
            int  shift = 0;
            byte b;
            do
            {
                if ( shift == 10 * 7 )
                    throw new FormatException ( "Too many bytes in what should have been a 7 bit encoded Int64." );

                b = ReadByte ( );
                count |= (long) ( b & 0x7F ) << shift;
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
                    throw new FormatException ( "Too many bytes in what should have been a 7 bit encoded UInt64." );

                b = ReadByte ( );
                count |= (ulong) ( b & 0x7F ) << shift;
                shift += 7;
            }
            while ( ( b & 0x80 ) != 0 );

            return count;
        }
    }
}