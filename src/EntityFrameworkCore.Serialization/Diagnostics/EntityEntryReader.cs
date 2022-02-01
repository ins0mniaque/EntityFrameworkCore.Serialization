using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace EntityFrameworkCore.Serialization.Diagnostics
{
    public class EntityEntryReader : IEntityEntryReader
    {
        public EntityEntryReader ( IEnumerable < EntityEntryData > entries )
        {
            if ( entries == null )
                throw new ArgumentNullException ( nameof ( entries ) );

            Entries = entries.GetEnumerator ( );
        }

        private IEnumerator < EntityEntryData > Entries { get; }

        private IEntityType?                                      EntityType         { get; set; }
        private IEnumerator < KeyValuePair < string, object? > >? Property           { get; set; }
        private IEnumerator < KeyValuePair < string, object? > >? ModifiedProperties { get; set; }
        private IEnumerator < string >?                           NavigationState    { get; set; }

        private IEntityType EnsureEntityType ( [CallerMemberName] string? readMethod = null )
        {
            return EntityType ?? throw new InvalidOperationException ( $"{ nameof ( ReadEntityType ) } was not called prior to { readMethod }" );
        }

        public bool ReadEntry ( )
        {
            EntityType         = null;
            Property           = null;
            ModifiedProperties = null;
            NavigationState    = null;

            return Entries.MoveNext ( );
        }

        public IEntityType ReadEntityType ( IModel model )
        {
            if ( model == null )
                throw new ArgumentNullException ( nameof ( model ) );

            var shortName = Entries.Current.EntityType;

            return EntityType = model.GetEntityTypes ( ).FirstOrDefault ( type => type.ShortName ( ) == shortName ) ??
                                throw new KeyNotFoundException ( $"Entity type { shortName } was not found in model" );
        }

        public EntityState ReadEntityState ( ) => Entries.Current.EntityState;

        public bool ReadProperty ( [ NotNullWhen ( true ) ] out IProperty? property, out object? value )
        {
            if ( Property == null )
                Property = Entries.Current.Properties?.GetEnumerator ( );

            if ( Property == null || ! Property.MoveNext ( ) )
            {
                property = null;
                value    = null;
                return false;
            }

            property = EnsureEntityType ( ).FindProperty ( Property.Current.Key ) ??
                       throw new KeyNotFoundException ( $"Property { Property.Current.Key } was not found in model" );
            value    = Property.Current.Value;
            return true;
        }

        public bool ReadModifiedProperty ( [ NotNullWhen ( true ) ] out IProperty? property, out object? value )
        {
            if ( ModifiedProperties == null )
                ModifiedProperties = Entries.Current.ModifiedProperties?.GetEnumerator ( );

            if ( ModifiedProperties == null || ! ModifiedProperties.MoveNext ( ) )
            {
                property = null;
                value    = null;
                return false;
            }

            property = EnsureEntityType ( ).FindProperty ( ModifiedProperties.Current.Key ) ??
                       throw new KeyNotFoundException ( $"Property { ModifiedProperties.Current.Key } was not found in model" );
            value    = ModifiedProperties.Current.Value;
            return true;
        }

        public bool ReadNavigationState ( [ NotNullWhen ( true ) ] out INavigation? navigated )
        {
            if ( NavigationState == null )
                NavigationState = Entries.Current.NavigationState?.GetEnumerator ( );

            if ( NavigationState == null || ! NavigationState.MoveNext ( ) )
            {
                navigated = null;
                return false;
            }

            navigated = EnsureEntityType ( ).FindNavigation ( NavigationState.Current ) ??
                        throw new KeyNotFoundException ( $"Navigation { NavigationState.Current } was not found in model" );
            return true;
        }
    }
}