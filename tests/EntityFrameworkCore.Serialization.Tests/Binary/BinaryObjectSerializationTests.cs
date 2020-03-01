using System;
using System.IO;

using Xunit;

using EntityFrameworkCore.Serialization.Binary.Format;

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
            using var writer = new IO.BinaryWriterWith7BitEncoding ( stream );

            BinaryFormatWriter.Write < string > ( writer, null );

            Assert.Equal ( stream.ToArray ( ), new byte [ ] { 0 } );
        }

        [ Fact ]
        public void CanWriteNullObject ( )
        {
            var complex = (ComplexObject?) null;

            using var stream = new MemoryStream ( );
            using var writer = new IO.BinaryWriterWith7BitEncoding ( stream );

            BinaryFormatWriter.Write < ComplexObject > ( writer, complex );

            Assert.Equal ( stream.ToArray ( ), new byte [ ] { 0 } );
        }

        [ Fact ]
        public void CanWriteComplexObject ( )
        {
            var complex = new ComplexObject { Name = "Name", Version = new Version ( "1.2.3.4" ), TypeCode = TypeCode.DBNull };

            using var stream = new MemoryStream ( );
            using var writer = new IO.BinaryWriterWith7BitEncoding ( stream );

            BinaryFormatWriter.Write < ComplexObject > ( writer, complex );

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
            using var writer = new IO.BinaryWriterWith7BitEncoding ( output );

            BinaryFormatWriter.Write ( writer, typeof ( ComplexObject ), complex );

            using var input  = new MemoryStream ( output.ToArray ( ) );
            using var reader = new IO.BinaryReaderWith7BitEncoding ( input );

            var deserializedComplex = BinaryFormatReader.Read < ComplexObject > ( reader );

            Assert.NotNull ( deserializedComplex );
            if ( deserializedComplex == null )
                return;

            Assert.Equal   ( complex.Name,     deserializedComplex.Name     );
            Assert.Equal   ( complex.Version,  deserializedComplex.Version  );
            Assert.Equal   ( complex.TypeCode, deserializedComplex.TypeCode );
            Assert.Null    ( deserializedComplex.Parent );
        }

        [ Fact ]
        public void CanReadObjectUsingConverter ( )
        {
            var surrogate = new BinaryFormatSurrogate ( );

            surrogate.AddConverter ( ReadComplexObjectNameOnly, WriteComplexObjectNameOnly );

            var complex = new ComplexObject { Name = "Name", Version = new Version ( "1.2.3.4" ), TypeCode = TypeCode.DBNull };

            using var output = new MemoryStream ( );
            using var writer = new IO.BinaryWriterWith7BitEncoding ( output );

            BinaryFormatWriter.Write ( writer, typeof ( ComplexObject ), complex, surrogate );

            using var input  = new MemoryStream ( output.ToArray ( ) );
            using var reader = new IO.BinaryReaderWith7BitEncoding ( input );

            var deserializedComplex = BinaryFormatReader.Read < ComplexObject > ( reader, surrogate );

            Assert.NotNull ( deserializedComplex );
            if ( deserializedComplex == null )
                return;

            Assert.Equal ( complex.Name, deserializedComplex.Name );
            Assert.Equal ( default, deserializedComplex.Version   );
            Assert.Equal ( default, deserializedComplex.TypeCode  );
            Assert.Equal ( default, deserializedComplex.Parent    );
        }

        private static ComplexObject ReadComplexObjectNameOnly ( BinaryReader reader )
        {
            return new ComplexObject { Name = reader.Read < string > ( ) };
        }

        private static void WriteComplexObjectNameOnly ( BinaryWriter writer, ComplexObject complexObject )
        {
            writer.Write < string > ( complexObject.Name );
        }
    }
}