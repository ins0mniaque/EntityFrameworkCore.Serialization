using System.Collections.Generic;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace EntityFrameworkCore.Serialization.Serializable
{
    public class EntityEntryWriter : IEntityEntryWriter
    {
        public EntityEntryWriter ( ICollection < SerializableEntry > entries )
        {
            Entries = entries;
        }

        private ICollection < SerializableEntry > Entries { get; }

        private SerializableEntry Current { get; set; }

        public void WriteStartEntry ( ) => Current = new SerializableEntry ( );

        public void WriteEntityType  ( IEntityType entityType  ) => Current.EntityType  = entityType.ShortName ( );
        public void WriteEntityState ( EntityState entityState ) => Current.EntityState = entityState;

        public void WriteProperty ( IProperty property, object? value )
        {
            if ( Current.Properties == null )
                Current.Properties = new Dictionary < string, object? > ( );

            Current.Properties [ property.Name ] = value;
        }

        public void WriteModifiedProperty ( IProperty property, object? value )
        {
            if ( Current.ModifiedProperties == null )
                Current.ModifiedProperties = new Dictionary < string, object? > ( );

            Current.ModifiedProperties [ property.Name ] = value;
        }

        public void WriteNavigationState ( INavigation navigated )
        {
            if ( Current.NavigationState == null )
                Current.NavigationState = new HashSet < string > ( );

            Current.NavigationState.Add ( navigated.Name );
        }

        public void WriteEndEntry ( )
        {
            Entries.Add ( Current );

            Current = null;
        }
    }
}