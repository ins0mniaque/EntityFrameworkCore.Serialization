using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace EntityFrameworkCore.Serialization.Tests
{
    public class EntityEntryComparer : IComparer < EntityEntry >, IEqualityComparer < EntityEntry >
    {
        public static EntityEntryComparer Instance { get; } = new EntityEntryComparer ( );

        public int Compare ( [AllowNull] EntityEntry left, [AllowNull] EntityEntry right )
        {
            if ( left  == null && right == null ) return 0;
            if ( left  == null ) return -1;
            if ( right == null ) return  1;

            var compare = left.State.CompareTo ( right.State );
            if ( compare != 0 ) return compare;

            compare = string.Compare ( left.Metadata.Name, right.Metadata.Name, StringComparison.Ordinal );
            if ( compare != 0 ) return compare;

            compare = StructuralComparisons.StructuralComparer.Compare ( GetCurrentValues  ( left  ),
                                                                         GetCurrentValues  ( right ) );
            if ( compare != 0 ) return compare;

            compare = StructuralComparisons.StructuralComparer.Compare ( GetOriginalValues ( left  ),
                                                                         GetOriginalValues ( right ) );
            if ( compare != 0 ) return compare;

            return CompareNavigations ( left, right );
        }

        public bool Equals ( [AllowNull] EntityEntry left, [AllowNull] EntityEntry right )
        {
            if ( left == null && right == null ) return true;
            if ( left == null || right == null ) return false;

            return left.State    == right.State    &&
                   left.Metadata == right.Metadata &&
                   StructuralComparisons.StructuralEqualityComparer.Equals ( GetCurrentValues  ( left  ),
                                                                             GetCurrentValues  ( right ) ) &&
                   StructuralComparisons.StructuralEqualityComparer.Equals ( GetOriginalValues ( left  ),
                                                                             GetOriginalValues ( right ) ) &&
                   CompareNavigations ( left, right ) == 0;
        }

        public int GetHashCode ( [DisallowNull] EntityEntry entry )
        {
            if ( entry == null )
                return 0;

            var hashCode = new HashCode ( );

            hashCode.Add ( entry.State );
            hashCode.Add ( entry.Metadata.Name );
            hashCode.Add ( StructuralComparisons.StructuralEqualityComparer.GetHashCode ( GetOriginalValues ( entry ) ) );
            hashCode.Add ( StructuralComparisons.StructuralEqualityComparer.GetHashCode ( GetCurrentValues  ( entry ) ) );

            foreach ( var navigation in entry.Navigations )
            {
                hashCode.Add ( navigation.IsLoaded   );
                hashCode.Add ( navigation.IsModified );
                hashCode.Add ( GetReferenceCount ( navigation ) );
            }

            return hashCode.ToHashCode ( );
        }

        private static int CompareNavigations ( EntityEntry left, EntityEntry right )
        {
            foreach ( var leftNavigation in left.Navigations )
            {
                var rightNavigation = right.Navigation ( leftNavigation.Metadata.Name );

                var compare = leftNavigation.IsLoaded.CompareTo ( rightNavigation.IsLoaded );
                if ( compare != 0 ) return compare;

                compare = leftNavigation.IsModified.CompareTo ( rightNavigation.IsModified );
                if ( compare != 0 ) return compare;

                compare = GetReferenceCount ( leftNavigation ).CompareTo ( GetReferenceCount ( rightNavigation ) );
                if ( compare != 0 ) return compare;
            }

            return 0;
        }

        private static object [ ] GetOriginalValues ( EntityEntry entry ) => entry.Properties.Select ( property => property.OriginalValue ).ToArray ( );
        private static object [ ] GetCurrentValues  ( EntityEntry entry ) => entry.Properties.Select ( property => property.CurrentValue  ).ToArray ( );

        private static int GetReferenceCount ( NavigationEntry navigation )
        {
            if ( ! navigation.IsLoaded )
                return 0;

            var value = navigation.CurrentValue;
            if ( value == null )
                return 0;

            if ( ! navigation.Metadata.IsCollection ( ) )
                return 1;

            var count = 0;
            foreach ( var _ in (IEnumerable) value )
                count++;

            return count;
        }
    }
}