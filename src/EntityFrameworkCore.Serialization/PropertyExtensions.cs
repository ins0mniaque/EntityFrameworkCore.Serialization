using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace EntityFrameworkCore.Serialization
{
    public static class PropertyExtensions
    {
        public static bool AreEquals ( this IProperty property, object left, object right )
        {
            var comparer = property.GetStructuralValueComparer ( );

            return comparer != null ? comparer.Equals ( left, right ) :
                                      object  .Equals ( left, right );
        }
    }
}