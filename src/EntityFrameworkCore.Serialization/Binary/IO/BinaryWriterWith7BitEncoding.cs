using System.IO;
using System.Text;

namespace EntityFrameworkCore.Serialization.Binary.IO
{
    public class BinaryWriterWith7BitEncoding : BinaryWriter
    {
        public BinaryWriterWith7BitEncoding ( Stream input )                                    : base ( input ) { }
        public BinaryWriterWith7BitEncoding ( Stream input, Encoding encoding )                 : base ( input, encoding ) { }
        public BinaryWriterWith7BitEncoding ( Stream input, Encoding encoding, bool leaveOpen ) : base ( input, encoding, leaveOpen ) { }

        public override void Write ( int   value ) => Write7BitEncoded ( value );
        public override void Write ( uint  value ) => Write7BitEncoded ( value );
        public override void Write ( long  value ) => Write7BitEncoded ( value );
        public override void Write ( ulong value ) => Write7BitEncoded ( value );

        protected void Write7BitEncoded ( int value )
        {
            Write7BitEncoded ( (uint) value );
        }

        protected void Write7BitEncoded ( uint value )
        {
            while ( value >= 0x80 )
            {
                Write ( (byte) ( value | 0x80 ) );
                value >>= 7;
            }
            Write ( (byte) value );
        }

        protected void Write7BitEncoded ( long value )
        {
            Write7BitEncoded ( (ulong) value );
        }

        protected void Write7BitEncoded ( ulong value )
        {
            while ( value >= 0x80 )
            {
                Write ( (byte) ( value | 0x80 ) );
                value >>= 7;
            }
            Write ( (byte) value );
        }
    }
}