using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace EntityFrameworkCore.Serialization
{
    // TODO: Switch to read/write OriginalValues/CurrentValues ?
    public interface IDbContextSerializer < TEntry >
    {
        TEntry CreateEntry ( );

        IEntityType ReadEntityType  ( TEntry entry, IModel model );
        EntityState ReadEntityState ( TEntry entry );

        object [ ]? ReadPrimaryKey         ( TEntry entry, IProperty [ ] properties );
        object [ ]? ReadConcurrencyToken   ( TEntry entry, IProperty [ ] properties );
        object [ ]? ReadProperties         ( TEntry entry, IProperty [ ] properties );
        object [ ]? ReadModifiedProperties ( TEntry entry, IEntityType entityType, out IProperty [ ] properties );

        void WriteEntityType  ( TEntry entry, IEntityType entityType  );
        void WriteEntityState ( TEntry entry, EntityState entityState );

        void WritePrimaryKey         ( TEntry entry, IProperty [ ] properties, object [ ] values );
        void WriteConcurrencyToken   ( TEntry entry, IProperty [ ] properties, object [ ] values );
        void WriteProperties         ( TEntry entry, IProperty [ ] properties, object [ ] values );
        void WriteModifiedProperties ( TEntry entry, IProperty [ ] properties, object [ ] values );
    }
}