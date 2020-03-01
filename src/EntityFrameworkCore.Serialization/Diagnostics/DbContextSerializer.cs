using System.Collections.Generic;

namespace EntityFrameworkCore.Serialization.Diagnostics
{
    public class DbContextSerializer : IDbContextSerializer < ICollection < EntityEntryData > >
    {
        public IEntityEntryReader CreateReader ( ICollection < EntityEntryData > readable ) => new EntityEntryReader ( readable );
        public IEntityEntryWriter CreateWriter ( ICollection < EntityEntryData > writable ) => new EntityEntryWriter ( writable );
    }
}