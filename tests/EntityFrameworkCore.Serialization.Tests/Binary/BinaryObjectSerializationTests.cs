using System;
using System.IO;

using Xunit;

namespace EntityFrameworkCore.Serialization.Binary.Tests
{
    public class BinaryObjectSerializationTests
    {
        public class ComplexObject
        {
            public string        Text     { get; set; }
            public Version       Version  { get; set; }
            public TypeCode      TypeCode { get; set; }
            public ComplexObject Parent   { get; set; }
        }

        [ Fact ]
        public void CanWriteNullObject ( )
        {
            var complex = (ComplexObject) null;

            using var stream = new MemoryStream ( );
            var writer = new BinaryWriterWith7BitEncoding ( stream );

            BinaryObjectWriter.Write ( writer, typeof ( ComplexObject ), complex );

            Assert.Equal ( stream.ToArray ( ), new byte [ ] { 0 } );
        }

        [ Fact ]
        public void CanWriteComplexObject ( )
        {
            var complex = new ComplexObject { Text = "Text", Version = new Version ( "1.2.3.4" ), TypeCode = TypeCode.DBNull };

            using var stream = new MemoryStream ( );
            var writer = new BinaryWriterWith7BitEncoding ( stream );

            BinaryObjectWriter.Write ( writer, typeof ( ComplexObject ), complex );

            Assert.Equal ( stream.ToArray ( ),
                           new byte [ ] { 1, 4, (byte) 'T', (byte) 'e', (byte) 'x', (byte) 't',
                                             1, 1, 2, 3, 4,
                                             (byte) TypeCode.DBNull,
                                             0 } );
        }

        [ Fact ]
        public void CanReadComplexObject ( )
        {
            var complex = new ComplexObject { Text = "Text", Version = new Version ( "1.2.3.4" ), TypeCode = TypeCode.DBNull };

            using var output = new MemoryStream ( );
            var writer = new BinaryWriterWith7BitEncoding ( output );

            BinaryObjectWriter.Write ( writer, typeof ( ComplexObject ), complex );

            using var input = new MemoryStream ( output.ToArray ( ) );
            var reader = new BinaryReaderWith7BitEncoding ( input );

            var deserializedComplex = (ComplexObject) BinaryObjectReader.Read ( reader, typeof ( ComplexObject ) );

            Assert.Equal ( complex.Text,     deserializedComplex.Text     );
            Assert.Equal ( complex.Version,  deserializedComplex.Version  );
            Assert.Equal ( complex.TypeCode, deserializedComplex.TypeCode );
            Assert.Null  ( deserializedComplex.Parent );
        }
    }
}