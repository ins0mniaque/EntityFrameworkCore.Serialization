using System;
using System.Runtime.CompilerServices;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

using EntityFrameworkCore.Serialization.Binary.Internal;
using EntityFrameworkCore.Serialization.Binary.Format;

namespace EntityFrameworkCore.Serialization.Binary
{
    public class BinaryEntityEntryWriter : IEntityEntryWriter, IDisposable
    {
        public BinaryEntityEntryWriter ( IBinaryWriter writer )
        {
            Writer = writer ?? throw new ArgumentNullException ( nameof ( writer ) );
        }

        private IBinaryWriter Writer { get; }

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
                Writer.Write ( Nullable.GetUnderlyingType ( property.ClrType ) ?? property.ClrType, value );
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
                Writer.Write ( Navigation );

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
                    Writer.Write   ( BinaryEntityEntry.EndOfStreamMarker );
                    Writer.Dispose ( );
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