using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace EntityFrameworkCore.Serialization.Diagnostics
{
    public class EntityEntryWriter : IEntityEntryWriter
    {
        public EntityEntryWriter ( ICollection < EntityEntryData > entries )
        {
            Entries = entries ?? throw new ArgumentNullException ( nameof ( entries ) );
        }

        private ICollection < EntityEntryData > Entries { get; }

        private EntityEntryData? CurrentEntry { get; set; }

        private EntityEntryData EnsureCurrentEntry ( [CallerMemberName] string? writeMethod = null )
        {
            return CurrentEntry ?? throw new InvalidOperationException ( $"{ nameof ( WriteStartEntry ) } was not called prior to { writeMethod }" );
        }

        public void WriteStartEntry ( ) => CurrentEntry = new EntityEntryData ( );

        public void WriteEntityType  ( IEntityType entityType  ) => EnsureCurrentEntry ( ).EntityType  = entityType.ShortName ( );
        public void WriteEntityState ( EntityState entityState ) => EnsureCurrentEntry ( ).EntityState = entityState;

        public void WriteProperty ( IProperty property, object? value )
        {
            if ( property == null )
                throw new ArgumentNullException ( nameof ( property ) );

            var entry = EnsureCurrentEntry ( );
            if ( entry.Properties == null )
                entry.Properties = new Dictionary < string, object? > ( );

            entry.Properties [ property.Name ] = value;
        }

        public void WriteModifiedProperty ( IProperty property, object? value )
        {
            if ( property == null )
                throw new ArgumentNullException ( nameof ( property ) );

            var entry = EnsureCurrentEntry ( );
            if ( entry.ModifiedProperties == null )
                entry.ModifiedProperties = new Dictionary < string, object? > ( );

            entry.ModifiedProperties [ property.Name ] = value;
        }

        public void WriteNavigationState ( INavigation navigated )
        {
            if ( navigated == null )
                throw new ArgumentNullException ( nameof ( navigated ) );

            var entry = EnsureCurrentEntry ( );
            if ( entry.NavigationState == null )
                entry.NavigationState = new HashSet < string > ( );

            entry.NavigationState.Add ( navigated.Name );
        }

        public void WriteEndEntry ( )
        {
            Entries.Add ( EnsureCurrentEntry ( ) );

            CurrentEntry = null;
        }
    }
}