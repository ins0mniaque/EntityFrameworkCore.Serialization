namespace EntityFrameworkCore.Serialization
{
    public interface IDbContextDeserializer < in T >
    {
        IEntityEntryReader CreateReader ( T readable );
    }
}