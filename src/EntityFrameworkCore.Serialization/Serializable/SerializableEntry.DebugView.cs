using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace EntityFrameworkCore.Serialization.Serializable
{
    [ DebuggerDisplay   ( "{DebuggerDisplay(),nq}" ) ]
    [ DebuggerTypeProxy ( typeof ( DebugView )     ) ]
    public partial class SerializableEntry
    {
        private string DebuggerDisplay ( ) => $"{ EntityType } ({ EntityState }): { GuessKey ( EntityType ?? string.Empty, Properties ) }";

        private static string GuessKey ( string entityType, Dictionary < string, object? >? properties )
        {
            if ( properties == null || properties.Count == 0 )
                return "(Keyless)";

            return string.Join ( ", ",
                                 properties.Where   ( property => property.Key.EndsWith   ( "id",       StringComparison.InvariantCultureIgnoreCase ) )
                                           .OrderBy ( property => property.Key.StartsWith ( entityType, StringComparison.InvariantCultureIgnoreCase ) ?
                                                                      property.Key.Length - entityType.Length :
                                                                      property.Key.Length )
                                           .DefaultIfEmpty ( properties.First ( ) )
                                           .Select  ( property => $"{ property.Key } = { property.Value }" ) );
        }

        [ SuppressMessage ( "Microsoft.Naming", "CA1812:AvoidUninstantiatedInternalClasses", Justification = "DebuggerTypeProxy" ) ]
        internal class DebugView
        {
            [ DebuggerBrowsable ( DebuggerBrowsableState.Never ) ]
            private readonly SerializableEntry @this;

            public DebugView ( SerializableEntry entry ) => @this = entry;

            [ DebuggerBrowsable ( DebuggerBrowsableState.RootHidden ) ]
            public Property [ ] Properties => GetProperties ( @this ).ToArray ( );

            private static IEnumerable < Property > GetProperties ( SerializableEntry @this )
            {
                if ( @this.Properties != null )
                    foreach ( var entry in @this.Properties )
                        yield return new Property { Key      = entry.Key,
                                                    Value    = entry.Value,
                                                    Modified = @this.ModifiedProperties != null &&
                                                               @this.ModifiedProperties.TryGetValue ( entry.Key, out var value ) ?
                                                                   new Modified { Value = value } : Missing };

                if ( @this.ModifiedProperties != null )
                    foreach ( var entry in @this.ModifiedProperties )
                        if ( @this.Properties?.ContainsKey ( entry.Key ) != true )
                            yield return new Property { Key      = entry.Key,
                                                        Modified = new Modified { Value = entry.Value } };

                if ( @this.NavigationState != null )
                    foreach ( var entry in @this.NavigationState )
                        yield return new Property { Key   = entry,
                                                    Value = IsLoaded };
            }

            [ DebuggerDisplay ( "{Value}{Modified}", Name = "{Key,nq}" ) ]
            internal class Property
            {
                public string  Key      { get; set; } = string.Empty;
                public object? Value    { get; set; } = Missing;
                public object? Modified { get; set; } = Missing;
            }

            private static object Missing  { get; } = new Empty  ( );
            private static object IsLoaded { get; } = new Loaded ( );

            [ DebuggerDisplay ( "" )            ] private class Empty    { }
            [ DebuggerDisplay ( "(Loaded)" )    ] private class Loaded   { }
            [ DebuggerDisplay ( " => {Value}" ) ] private class Modified { public object? Value { get; set; } }
        }
    }
}