using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

using EntityFrameworkCore.Serialization.Binary.Internal;

namespace EntityFrameworkCore.Serialization.Binary
{
    public class BinaryEntityEntryWriter : IEntityEntryWriter, IDisposable
    {
        public BinaryEntityEntryWriter ( Stream       stream ) : this ( stream, null ) { }
        public BinaryEntityEntryWriter ( BinaryWriter writer ) : this ( writer, null ) { }
        public BinaryEntityEntryWriter ( Stream       stream, IBinaryObjectWriterSurrogate? surrogate ) : this ( new BinaryWriterWith7BitEncoding ( stream, new UTF8Encoding ( false, true ), true ), surrogate ) { }
        public BinaryEntityEntryWriter ( BinaryWriter writer, IBinaryObjectWriterSurrogate? surrogate )
        {
            Writer    = writer ?? throw new ArgumentNullException ( nameof ( writer ) );
            Surrogate = surrogate;
        }

        private BinaryWriter                  Writer    { get; }
        private IBinaryObjectWriterSurrogate? Surrogate { get; }

        private IEntityType? EntityType { get; set; }
        private bool         EncodeType { get; set; }
        private byte [ ]?    Navigation { get; set; }

        private IEntityType EnsureEntityType ( [CallerMemberName] string? writeMethod = null )
        {
            return EntityType ?? throw new InvalidOperationException ( $"{ nameof ( WriteEntityType ) } was not called prior to { writeMethod }" );
        }

        public void WriteStartEntry ( )
        {
            EncodeType = false;
            Navigation = null;
        }

        public void WriteEntityType ( IEntityType entityType )
        {
            if ( entityType == null )
                throw new ArgumentNullException ( nameof ( entityType ) );

            if ( EntityType != entityType )
            {
                EntityType = entityType;
                EncodeType = true;
            }
        }

        public void WriteEntityState ( EntityState entityState )
        {
            if ( EncodeType )
            {
                Writer.Write ( (byte) ( (byte) entityState | BinaryEntityEntry.EntityTypeFlag ) );
                Writer.Write ( EnsureEntityType ( ).ShortName ( ) );

                EncodeType = false;
            }
            else
                Writer.Write ( (byte) entityState );
        }

        public void WriteProperty ( IProperty property, object? value )
        {
            WriteProperty ( property, value, 0 );
        }

        public void WriteModifiedProperty ( IProperty property, object? value )
        {
            WriteProperty ( property, value, BinaryEntityEntry.ModifiedFlag );
        }

        private void WriteProperty ( IProperty property, object? value, int flag )
        {
            if ( property == null )
                throw new ArgumentNullException ( nameof ( property ) );

            var index = property.EncodeIndex ( ) | flag;

            var isDefaultValue = property.IsDefaultValue ( value );
            if ( isDefaultValue )
                index |= BinaryEntityEntry.DefaultValueFlag;

            Writer.Write ( index );

            if ( ! isDefaultValue )
                Writer.Write ( Nullable.GetUnderlyingType ( property.ClrType ) ?? property.ClrType, value, Surrogate );
        }

        public void WriteNavigationState ( INavigation navigated )
        {
            if ( navigated == null )
                throw new ArgumentNullException ( nameof ( navigated ) );

            if ( Navigation == null )
            {
                Navigation = new byte [ (int) Math.Ceiling ( EnsureEntityType ( ).GetNavigationMaxIndex ( ) + 1 / 8.0 ) ];

                Writer.Write ( BinaryEntityEntry.NavigationMarker );
            }

            var index = navigated.EncodeIndex ( );

            Navigation [ index / 8 ] |= (byte) ( 1 << ( index % 8 ) );
        }

        public void WriteEndEntry ( )
        {
            if ( Navigation != null )
            {
                Writer.Write < byte [ ] > ( Navigation );

                Navigation = null;
            }
            else
                Writer.Write ( BinaryEntityEntry.EndMarker );
        }

        private bool disposed;

        protected virtual void Dispose ( bool disposing )
        {
            if ( ! disposed )
            {
                if ( disposing )
                {
                    Writer.Write ( BinaryEntityEntry.EndOfStreamMarker );
                    Writer.Flush ( );
                }

                disposed = true;
            }
        }

        public void Dispose ( )
        {
            Dispose ( true );
            GC.SuppressFinalize ( this );
        }
    }
}