using System;
using System.Linq;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace EntityFrameworkCore.Serialization.Binary.Internal
{
    public static class BinaryEntityEntry
    {
        public const int EntityTypeFlag   = 0b10000000;
        public const int DefaultValueFlag = 0b00000001;
        public const int ModifiedFlag     = 0b00000010;
        public const int NavigationMarker = 1;
        public const int EndMarker        = 0;

        public static int EncodeIndex ( this IProperty property )
        {
            return ( property.GetIndex ( ) + 1 ) << 2;
        }

        public static int DecodePropertyIndex ( int index )
        {
            return ( index >> 2 ) - 1;
        }

        public static int EncodeIndex ( this INavigation navigation )
        {
            return navigation.GetIndex ( );
        }

        public static int DecodeNavigationIndex ( int index )
        {
            return index;
        }

        public static IProperty FindProperty ( this IEntityType entityType, int index )
        {
            return entityType.GetProperties ( ).First ( property => property.GetIndex ( ) == index );
        }

        public static INavigation FindNavigation ( this IEntityType entityType, int index )
        {
            return entityType.GetNavigations ( ).First ( navigation => navigation.GetIndex ( ) == index );
        }

        public static int GetNavigationMaxIndex ( this IEntityType entityType )
        {
            return entityType.GetNavigations ( ).Max ( navigation => navigation.GetIndex ( ) );
        }

        public static bool IsDefaultValue ( this IProperty property, object? value )
        {
            return object.Equals ( value, property.GetDefaultValue ( ) );
        }

        public static object? GetDefaultValue ( this IProperty property )
        {
            return property.ClrType.IsValueType ? Activator.CreateInstance ( property.ClrType ) : null;
        }
    }
}