using System;
using System.IO;

using Xunit;

namespace EntityFrameworkCore.Serialization.Binary.Tests
{
    public class BinaryObjectSerializationTests
    {
        public class ComplexObject
        {
            public string?        Name     { get; set; }
            public Version?       Version  { get; set; }
            public TypeCode       TypeCode { get; set; }
            public ComplexObject? Parent   { get; set; }
        }

        [ Fact ]
        public void CanWriteNullString ( )
        {
            using var stream = new MemoryStream ( );
            using var writer = new BinaryWriterWith7BitEncoding ( stream );

            BinaryObjectWriter.Write ( writer, typeof ( string ), null );

            Assert.Equal ( stream.ToArray ( ), new byte [ ] { 0 } );
        }

        [ Fact ]
        public void CanWriteNullObject ( )
        {
            var complex = (ComplexObject?) null;

            using var stream = new MemoryStream ( );
            using var writer = new BinaryWriterWith7BitEncoding ( stream );

            BinaryObjectWriter.Write ( writer, typeof ( ComplexObject ), complex );

            Assert.Equal ( stream.ToArray ( ), new byte [ ] { 0 } );
        }

        [ Fact ]
        public void CanWriteComplexObject ( )
        {
            var complex = new ComplexObject { Name = "Name", Version = new Version ( "1.2.3.4" ), TypeCode = TypeCode.DBNull };

            using var stream = new MemoryStream ( );
            using var writer = new BinaryWriterWith7BitEncoding ( stream );

            BinaryObjectWriter.Write ( writer, typeof ( ComplexObject ), complex );

            Assert.Equal ( stream.ToArray ( ),
                           new byte [ ] { 1, 1, 4, (byte) 'N', (byte) 'a', (byte) 'm', (byte) 'e',
                                             1, 1, 2, 3, 4,
                                             (byte) TypeCode.DBNull,
                                             0 } );
        }

        [ Fact ]
        public void CanReadComplexObject ( )
        {
            var complex = new ComplexObject { Name = "Name", Version = new Version ( "1.2.3.4" ), TypeCode = TypeCode.DBNull };

            using var output = new MemoryStream ( );
            using var writer = new BinaryWriterWith7BitEncoding ( output );

            BinaryObjectWriter.Write ( writer, typeof ( ComplexObject ), complex );

            using var input  = new MemoryStream ( output.ToArray ( ) );
            using var reader = new BinaryReaderWith7BitEncoding ( input );

            var deserializedComplex = (ComplexObject?) BinaryObjectReader.Read ( reader, typeof ( ComplexObject ) );

            Assert.NotNull ( deserializedComplex );
            Assert.Equal   ( complex.Name,     deserializedComplex?.Name     );
            Assert.Equal   ( complex.Version,  deserializedComplex?.Version  );
            Assert.Equal   ( complex.TypeCode, deserializedComplex?.TypeCode );
            Assert.Null    ( deserializedComplex?.Parent );
        }
    }
}