namespace EntityFrameworkCore.Serialization
{
    public interface IDbContextSerializer < in T >
    {
        IEntityEntryWriter CreateWriter ( T writable );
    }
}