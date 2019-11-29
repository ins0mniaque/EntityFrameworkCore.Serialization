using System.Diagnostics.CodeAnalysis;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace EntityFrameworkCore.Serialization
{
    public interface IEntityEntryReader
    {
        bool ReadEntry ( );

        IEntityType ReadEntityType  ( IModel model );
        EntityState ReadEntityState ( );

        [ SuppressMessage ( "Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", MessageId="Property" ) ]
        bool ReadProperty ( [ NotNullWhen ( true ) ] out IProperty? property, out object? value );

        [ SuppressMessage ( "Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", MessageId="Property" ) ]
        bool ReadModifiedProperty ( [ NotNullWhen ( true ) ] out IProperty? property, out object? value );

        bool ReadNavigationState ( [ NotNullWhen ( true ) ] out INavigation? navigated );
    }
}