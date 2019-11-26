using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

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

            return StructuralComparisons.StructuralComparer.Compare ( GetOriginalValues ( left  ),
                                                                      GetOriginalValues ( right ) );
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
                                                                             GetOriginalValues ( right ) );
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

            return hashCode.ToHashCode ( );
        }

        private static object [ ] GetOriginalValues ( EntityEntry entry ) => entry.Properties.Select ( property => property.OriginalValue ).ToArray ( );
        private static object [ ] GetCurrentValues  ( EntityEntry entry ) => entry.Properties.Select ( property => property.CurrentValue  ).ToArray ( );
    }
}