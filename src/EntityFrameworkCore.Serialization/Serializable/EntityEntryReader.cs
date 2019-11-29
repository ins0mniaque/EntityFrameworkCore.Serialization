using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace EntityFrameworkCore.Serialization.Serializable
{
    public class EntityEntryReader : IEntityEntryReader
    {
        public EntityEntryReader ( IEnumerable < SerializableEntry > entries )
        {
            Entries = entries.GetEnumerator ( );
        }

        private IEnumerator < SerializableEntry > Entries { get; }

        private IEntityType?                                      EntityType         { get; set; }
        private IEnumerator < KeyValuePair < string, object? > >? Property           { get; set; }
        private IEnumerator < KeyValuePair < string, object? > >? ModifiedProperties { get; set; }
        private IEnumerator < string >?                           NavigationState    { get; set; }

        public bool ReadEntry ( )
        {
            EntityType         = null;
            Property           = null;
            ModifiedProperties = null;
            NavigationState    = null;

            return Entries.MoveNext ( );
        }

        public IEntityType ReadEntityType  ( IModel model ) => EntityType = model.GetEntityTypes ( ).First ( type => type.ShortName ( ) == Entries.Current.EntityType );
        public EntityState ReadEntityState ( )              => Entries.Current.EntityState;

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

            property = EntityType.FindProperty ( Property.Current.Key );
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

            property = EntityType.FindProperty ( ModifiedProperties.Current.Key );
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

            navigated = EntityType.FindNavigation ( NavigationState.Current );
            return true;
        }
    }
}