using System.Diagnostics.CodeAnalysis;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace EntityFrameworkCore.Serialization
{
    public interface IEntityEntryWriter
    {
        void WriteStartEntry ( );

        void WriteEntityType  ( IEntityType entityType  );
        void WriteEntityState ( EntityState entityState );

        [ SuppressMessage ( "Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", MessageId="Property" ) ]
        void WriteProperty ( IProperty property, object? value );

        [ SuppressMessage ( "Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", MessageId="Property" ) ]
        void WriteModifiedProperty ( IProperty property, object? value );

        void WriteNavigationState ( INavigationBase navigated );

        void WriteEndEntry ( );
    }
}