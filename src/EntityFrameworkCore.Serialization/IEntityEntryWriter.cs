using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace EntityFrameworkCore.Serialization
{
    public interface IEntityEntryWriter
    {
        void WriteStartEntry ( );

        void WriteEntityType  ( IEntityType entityType  );
        void WriteEntityState ( EntityState entityState );

        void WriteProperty         ( IProperty   property, object? value );
        void WriteModifiedProperty ( IProperty   property, object? value );
        void WriteNavigationState  ( INavigation navigated );

        void WriteEndEntry ( );
    }
}