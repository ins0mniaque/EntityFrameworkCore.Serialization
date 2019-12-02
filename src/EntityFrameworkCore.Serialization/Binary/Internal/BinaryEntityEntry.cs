using System;
using System.Linq;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace EntityFrameworkCore.Serialization.Binary.Internal
{
    public static class BinaryEntityEntry
    {
        public const int  EntityTypeFlag    = 0b10000000;
        public const int  DefaultValueFlag  = 0b00000001;
        public const int  ModifiedFlag      = 0b00000010;
        public const int  NavigationMarker  = 1;
        public const int  EndMarker         = 0;
        public const byte EndOfStreamMarker = 0b11111111;

        public static int EncodeIndex ( this IProperty property )
        {
            #pragma warning disable EF1001 // Internal EF Core API usage.
            return ( property.GetIndex ( ) + 1 ) << 2;
            #pragma warning restore EF1001 // Internal EF Core API usage.
        }

        public static int DecodePropertyIndex ( int index )
        {
            return ( index >> 2 ) - 1;
        }

        public static int EncodeIndex ( this INavigation navigation )
        {
            #pragma warning disable EF1001 // Internal EF Core API usage.
            return navigation.GetIndex ( );
            #pragma warning restore EF1001 // Internal EF Core API usage.
        }

        public static int DecodeNavigationIndex ( int index )
        {
            return index;
        }

        public static IProperty FindProperty ( this IEntityType entityType, int index )
        {
            if ( entityType == null )
                throw new ArgumentNullException ( nameof ( entityType ) );

            return entityType.GetProperties ( ).ElementAt ( index );
        }

        public static INavigation FindNavigation ( this IEntityType entityType, int index )
        {
            return entityType.GetNavigations ( ).ElementAt ( index );
        }

        public static int GetNavigationMaxIndex ( this IEntityType entityType )
        {
            #pragma warning disable EF1001 // Internal EF Core API usage.
            return entityType.NavigationCount ( ) - 1;
            #pragma warning restore EF1001 // Internal EF Core API usage.
        }

        public static bool IsDefaultValue ( this IProperty property, object? value )
        {
            return object.Equals ( value, property.GetDefaultValue ( ) );
        }

        public static object? GetDefaultValue ( this IProperty property )
        {
            if ( property == null )
                throw new ArgumentNullException ( nameof ( property ) );

            return property.ClrType.IsValueType ? Activator.CreateInstance ( property.ClrType ) : null;
        }
    }
}