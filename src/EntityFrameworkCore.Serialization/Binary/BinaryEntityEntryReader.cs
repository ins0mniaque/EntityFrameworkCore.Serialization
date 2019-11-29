using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

using EntityFrameworkCore.Serialization.Binary.Internal;

namespace EntityFrameworkCore.Serialization.Binary
{
    public class BinaryEntityEntryReader : IEntityEntryReader
    {
        public BinaryEntityEntryReader ( Stream       stream ) : this ( new BinaryReaderWith7BitEncoding ( stream ) ) { }
        public BinaryEntityEntryReader ( BinaryReader reader )
        {
            Reader = reader ?? throw new ArgumentNullException ( nameof ( reader ) );
        }

        private BinaryReader Reader { get; }

        private IEntityType? EntityType  { get; set; }
        private byte         EntityState { get; set; }

        private int? ReadIndex { get; set; }

        private IEnumerator < INavigation >? Navigation { get; set; }

        private IEntityType EnsureEntityType ( [CallerMemberName] string? readMethod = null )
        {
            return EntityType ?? throw new InvalidOperationException ( $"{ nameof ( ReadEntityType ) } found no entity type or was not called prior to { readMethod }" );
        }

        public bool ReadEntry ( )
        {
            ReadIndex  = null;
            Navigation = null;

            var entityState = Reader.BaseStream.ReadByte ( );
            if ( entityState == -1 )
                return false;

            EntityState = (byte) entityState;
            return true;
        }

        public IEntityType ReadEntityType ( IModel model )
        {
            if ( model == null )
                throw new ArgumentNullException ( nameof ( model ) );

            if ( ( EntityState & BinaryEntityEntry.EntityTypeFlag ) == BinaryEntityEntry.EntityTypeFlag )
            {
                var shortName = Reader.ReadString ( );
                EntityType  = model.GetEntityTypes ( ).First ( type => type.ShortName ( ) == shortName );
                EntityState = (byte) ( EntityState & ~BinaryEntityEntry.EntityTypeFlag );
            }

            return EnsureEntityType ( );
        }

        public EntityState ReadEntityState ( ) => (EntityState) EntityState;

        public bool ReadProperty ( [ NotNullWhen ( true ) ] out IProperty? property, out object? value )
        {
            if ( ReadIndex == null )
                ReadIndex = Reader.ReadInt32 ( );

            var index = ReadIndex.Value;

            property = null;
            value    = null;

            if ( index == BinaryEntityEntry.EndMarker || index == BinaryEntityEntry.NavigationMarker )
                return false;
            if ( ( index & BinaryEntityEntry.ModifiedFlag ) == BinaryEntityEntry.ModifiedFlag )
                return false;

            ReadProperty ( index, out property, out value );

            ReadIndex = null;
            return true;
        }

        public bool ReadModifiedProperty ( [ NotNullWhen ( true ) ] out IProperty? property, out object? value )
        {
            if ( ReadIndex == null )
                ReadIndex = Reader.ReadInt32 ( );

            var index = ReadIndex.Value;

            property = null;
            value    = null;

            if ( index == BinaryEntityEntry.EndMarker || index == BinaryEntityEntry.NavigationMarker )
                return false;
            if ( ( index & BinaryEntityEntry.ModifiedFlag ) == 0 )
                throw new InvalidOperationException ( );

            index &= ~BinaryEntityEntry.ModifiedFlag;

            ReadProperty ( index, out property, out value );

            ReadIndex = null;
            return true;
        }

        private void ReadProperty ( int index, out IProperty property, out object? value )
        {
            var isDefaultValue = ( index & BinaryEntityEntry.DefaultValueFlag ) == BinaryEntityEntry.DefaultValueFlag;
            if ( isDefaultValue )
                index &= ~BinaryEntityEntry.DefaultValueFlag;

            index    = BinaryEntityEntry.DecodePropertyIndex ( index );
            property = EnsureEntityType ( ).FindProperty ( index );

            if ( ! isDefaultValue )
                value = Reader.Read ( Nullable.GetUnderlyingType ( property.ClrType ) ?? property.ClrType );
            else
                value = property.GetDefaultValue ( );
        }

        public bool ReadNavigationState ( [ NotNullWhen ( true ) ] out INavigation? navigated )
        {
            navigated = null;

            if ( Navigation != null )
            {
                if ( Navigation.MoveNext ( ) )
                {
                    navigated = Navigation.Current;
                    return true;
                }
                else
                    return false;
            }

            if ( ReadIndex == null )
                ReadIndex = Reader.ReadInt32 ( );

            var index = ReadIndex.Value;

            if ( index == BinaryEntityEntry.EndMarker )
                return false;
            if ( index != BinaryEntityEntry.NavigationMarker )
                throw new InvalidOperationException ( );

            var navigation = (byte [ ]?) Reader.Read ( typeof ( byte [ ] ) );
            if ( navigation == null )
                throw new InvalidOperationException ( );

            var navigations = new List < INavigation > ( );

            for ( var block = 0; block < navigation.Length; block++ )
            {
                var bits = navigation [ block ];
                for ( var bit = 0; bit < 8; bit++ )
                {
                    var shift = 1 << bit;
                    if ( ( bits & shift ) == shift )
                    {
                        var navigationIndex = BinaryEntityEntry.DecodeNavigationIndex ( block * 8 + bit );

                        navigations.Add ( EnsureEntityType ( ).FindNavigation ( navigationIndex ) );
                    }
                }
            }

            Navigation = navigations.GetEnumerator ( );

            return ReadNavigationState ( out navigated );
        }
    }
}