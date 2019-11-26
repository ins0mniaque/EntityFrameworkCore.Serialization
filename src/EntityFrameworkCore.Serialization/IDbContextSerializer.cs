using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace EntityFrameworkCore.Serialization
{
    public interface IDbContextSerializer < TEntry >
    {
        TEntry CreateEntry ( );

        IEntityType ReadEntityType  ( TEntry entry, IModel model );
        EntityState ReadEntityState ( TEntry entry );

        object [ ]? ReadProperties         ( TEntry entry, IEntityType entityType, out IProperty   [ ] properties );
        object [ ]? ReadModifiedProperties ( TEntry entry, IEntityType entityType, out IProperty   [ ] properties );
        void        ReadNavigationState    ( TEntry entry, IEntityType entityType, out INavigation [ ] navigated  );

        void WriteEntityType  ( TEntry entry, IEntityType entityType  );
        void WriteEntityState ( TEntry entry, EntityState entityState );

        void WriteProperties         ( TEntry entry, IProperty   [ ] properties, object [ ] values );
        void WriteModifiedProperties ( TEntry entry, IProperty   [ ] properties, object [ ] values );
        void WriteNavigationState    ( TEntry entry, INavigation [ ] navigated );
    }
}