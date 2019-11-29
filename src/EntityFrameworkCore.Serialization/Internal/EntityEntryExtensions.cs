using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;

namespace EntityFrameworkCore.Serialization.Internal
{
    public static class EntityEntryExtensions
    {
        public static PropertyEntry Property ( this EntityEntry entityEntry, IProperty property )
        {
            #pragma warning disable EF1001 // Internal EF Core API usage.
            return new PropertyEntry ( entityEntry.GetInfrastructure ( ), property );
            #pragma warning restore EF1001 // Internal EF Core API usage.
        }

        public static CollectionEntry Collection ( this EntityEntry entityEntry, INavigation navigation )
        {
            #pragma warning disable EF1001 // Internal EF Core API usage.
            return new CollectionEntry ( entityEntry.GetInfrastructure ( ), navigation );
            #pragma warning restore EF1001 // Internal EF Core API usage.
        }
    }
}