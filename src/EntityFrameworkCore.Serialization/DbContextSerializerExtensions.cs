using System.Data;
using System.Linq;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace EntityFrameworkCore.Serialization
{
    public static class DbContextSerializerExtensions
    {
        public static TEntry Serialize < TEntry > ( this IDbContextSerializer < TEntry > serializer, EntityEntry entityEntry )
        {
            return serializer.Serialize ( entityEntry, false );
        }

        public static TEntry SerializeChanges < TEntry > ( this IDbContextSerializer < TEntry > serializer, EntityEntry entityEntry )
        {
            return serializer.Serialize ( entityEntry, true );
        }

        private static TEntry Serialize < TEntry > ( this IDbContextSerializer < TEntry > serializer, EntityEntry entityEntry, bool changesOnly )
        {
            var entry = serializer.CreateEntry ( );

            serializer.WriteEntityType ( entry, entityEntry.Metadata );
            serializer.WriteEntityState ( entry, entityEntry.State );

            // TODO: Something to cache this...
            var props = entityEntry.Properties.ToList ( );

            var primaryKeyProperties       = props.Where ( p => p.Metadata.IsPrimaryKey ( )   ).ToArray ( );
            var concurrencyTokenProperties = props.Where ( p => p.Metadata.IsConcurrencyToken ).ToArray ( );
            var otherProperties            = props.Where ( p => ! p.Metadata.IsConcurrencyToken &&
                                                                ! p.Metadata.IsPrimaryKey ( ) ).ToArray ( );

            serializer.WritePrimaryKey ( entry, primaryKeyProperties.Select ( p => p.Metadata     ).ToArray ( ),
                                                primaryKeyProperties.Select ( p => p.CurrentValue ).ToArray ( ) );

            serializer.WriteConcurrencyToken ( entry, concurrencyTokenProperties.Select ( p => p.Metadata      ).ToArray ( ),
                                                      concurrencyTokenProperties.Select ( p => p.OriginalValue ).ToArray ( ) );

            if ( ! changesOnly ) // TODO: || entityEntry.State == EntityState.Deleted ?
                serializer.WriteProperties ( entry, otherProperties.Select ( p => p.Metadata      ).ToArray ( ),
                                                    otherProperties.Select ( p => p.OriginalValue ).ToArray ( ) );

            var modifiedProperties = props.Where ( p => p.IsModified ).ToArray ( );

            serializer.WriteModifiedProperties ( entry, modifiedProperties.Select ( p => p.Metadata     ).ToArray ( ),
                                                        modifiedProperties.Select ( p => p.CurrentValue ).ToArray ( ) );

            return entry;
        }

        public static void Deserialize < TEntry > ( this IDbContextSerializer < TEntry > serializer, TEntry entry, EntityEntry entityEntry )
        {
            // TODO: Something to cache this...
            var props = entityEntry.Properties.ToList ( );

            var primaryKeyProperties       = props.Where ( p => p.Metadata.IsPrimaryKey ( )   ).ToArray ( );
            var concurrencyTokenProperties = props.Where ( p => p.Metadata.IsConcurrencyToken ).ToArray ( );
            var otherProperties            = props.Where ( p => ! p.Metadata.IsConcurrencyToken &&
                                                                ! p.Metadata.IsPrimaryKey ( ) ).ToArray ( );

            var state = serializer.ReadEntityState ( entry );

            var primaryKey = serializer.ReadPrimaryKey ( entry, primaryKeyProperties.Select ( p => p.Metadata ).ToArray ( ) );

            if ( primaryKey != null )
            {
                var index = 0;
                foreach ( var value in primaryKey )
                {
                    var property = primaryKeyProperties [ index++ ];

                    property.OriginalValue = value;
                    property.CurrentValue  = value;

                    if ( state == EntityState.Added )
                        property.IsTemporary = true;
                }
            }

            var concurrencyToken = serializer.ReadConcurrencyToken ( entry, concurrencyTokenProperties.Select ( p => p.Metadata ).ToArray ( ) );

            if ( concurrencyToken != null )
            {
                var index = 0;
                foreach ( var value in concurrencyToken )
                {
                    var property = concurrencyTokenProperties [ index++ ];

                    property.OriginalValue = value;
                    property.CurrentValue  = value;

                    // TODO: Check original version...
                }
            }

            var properties = serializer.ReadProperties ( entry, otherProperties.Select ( p => p.Metadata ).ToArray ( ) );

            if ( properties != null )
            {
                var index = 0;
                foreach ( var value in properties )
                {
                    var property = otherProperties [ index++ ];

                    property.OriginalValue = value;
                    property.CurrentValue  = value;
                    property.IsModified    = false;
                }
            }
        }

        public static void DeserializeModifiedProperties < TEntry > ( this IDbContextSerializer < TEntry > serializer, TEntry entry, EntityEntry entityEntry )
        {
            var modifiedProperties = serializer.ReadModifiedProperties ( entry, entityEntry.Metadata, out var modifiedProps );

            if ( modifiedProperties != null )
            {
                var index = 0;
                foreach ( var value in modifiedProperties )
                {
                    // TODO: Something to cache this...
                    var property = entityEntry.Property ( modifiedProps [ index++ ].Name );

                    property.CurrentValue = value;
                    property.IsModified   = true;
                }
            }
        }
    }
}