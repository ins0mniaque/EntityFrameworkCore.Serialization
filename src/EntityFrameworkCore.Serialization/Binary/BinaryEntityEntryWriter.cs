using System;
using System.IO;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

using EntityFrameworkCore.Serialization.Binary.Internal;

namespace EntityFrameworkCore.Serialization.Binary
{
    public class BinaryEntityEntryWriter : IEntityEntryWriter
    {
        public BinaryEntityEntryWriter ( Stream       stream ) : this ( new BinaryWriterWith7BitEncoding ( stream ) ) { }
        public BinaryEntityEntryWriter ( BinaryWriter writer )
        {
            Writer = writer;
        }

        private BinaryWriter Writer { get; }

        private IEntityType? EntityType { get; set; }
        private bool         EncodeType { get; set; }
        private byte [ ]?    Navigation { get; set; }

        public void WriteStartEntry ( )
        {
            EncodeType = false;
            Navigation = null;
        }

        public void WriteEntityType ( IEntityType entityType )
        {
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
                Writer.Write ( EntityType.ShortName ( ) );

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
            if ( Navigation == null )
            {
                Navigation = new byte [ (int) Math.Ceiling ( EntityType.GetNavigationMaxIndex ( ) + 1 / 8.0 ) ];

                Writer.Write ( BinaryEntityEntry.NavigationMarker );
            }

            var index = navigated.EncodeIndex ( );

            Navigation [ index / 8 ] |= (byte) ( 1 << ( index % 8 ) );
        }

        public void WriteEndEntry ( )
        {
            if ( Navigation != null )
            {
                Writer.Write ( typeof ( byte [ ] ), Navigation );

                Navigation = null;
            }
            else
                Writer.Write ( BinaryEntityEntry.EndMarker );
        }
    }
}