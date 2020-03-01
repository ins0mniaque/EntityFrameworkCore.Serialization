namespace EntityFrameworkCore.Serialization
{
    public interface IDbContextSerializer < in T >
    {
        IEntityEntryReader CreateReader ( T readable );
        IEntityEntryWriter CreateWriter ( T writable );
    }
}