using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace EntityFrameworkCore.Serialization
{
    public interface IEntityEntryReader
    {
        bool ReadEntry ( );

        IEntityType ReadEntityType  ( IModel model );
        EntityState ReadEntityState ( );

        bool ReadProperty         ( out IProperty   property, out object? value );
        bool ReadModifiedProperty ( out IProperty   property, out object? value );
        bool ReadNavigationState  ( out INavigation navigated );
    }
}