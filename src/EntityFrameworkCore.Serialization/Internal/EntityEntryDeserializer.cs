using System.Data;
using System.Linq;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;

namespace EntityFrameworkCore.Serialization.Internal
{
    public static class EntityEntryDeserializer
    {
        public static void DeserializeProperties < TEntry > ( this IDbContextSerializer < TEntry > serializer, TEntry entry, EntityEntry entityEntry )
        {
            // TODO: Something to cache this...
            var props = entityEntry.Properties.ToList ( );

            var state = serializer.ReadEntityState ( entry );

            var properties = serializer.ReadProperties ( entry, props.Select ( p => p.Metadata ).ToArray ( ) );

            if ( properties != null )
            {
                var index = 0;
                foreach ( var value in properties )
                {
                    var property = props [ index++ ];

                    if ( property.Metadata.IsConcurrencyToken )
                    {
                        // TODO: Check original version...
                    }

                    property.OriginalValue = value;
                    property.CurrentValue  = value;
                    property.IsModified    = false;

                    if ( property.Metadata.IsPrimaryKey ( ) && state == EntityState.Added && property.Metadata.ValueGenerated.HasFlag ( ValueGenerated.OnAdd ) )
                        property.IsTemporary = true;
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

        public static void DeserializeGeneratedValues < TEntry > ( this IDbContextSerializer < TEntry > serializer, TEntry entry, EntityEntry entityEntry )
        {
            var modifiedProperties = serializer.ReadModifiedProperties ( entry, entityEntry.Metadata, out var modifiedProps );

            if ( modifiedProperties != null )
            {
                var index = 0;
                foreach ( var value in modifiedProperties )
                {
                    // TODO: Something to cache this...
                    var property = entityEntry.Property ( modifiedProps [ index++ ].Name );

                    property.OriginalValue = value;
                    property.CurrentValue  = value;
                    property.IsModified    = false;
                }
            }
        }

        public static void DeserializeLoadedCollections < TEntry > ( this IDbContextSerializer < TEntry > serializer, TEntry entry, EntityEntry entityEntry )
        {
            serializer.ReadLoadedCollections ( entry, entityEntry.Metadata, out var loadedCollections );

            if ( loadedCollections != null )
            {
                foreach ( var collection in loadedCollections )
                {
                    var nav = entityEntry.Navigation ( collection.Name );
                    if ( ! nav.IsLoaded )
                    {
                        if ( nav.CurrentValue == null )
                            nav.CurrentValue = nav.Metadata.GetCollectionAccessor ( ).Create ( );

                        nav.IsLoaded = true;
                    }
                }
            }
        }

        public static void DeserializeEntityState < TEntry > ( this IDbContextSerializer < TEntry > serializer, TEntry entry, EntityEntry entityEntry )
        {
            var state = serializer.ReadEntityState ( entry );

            if ( state == EntityState.Modified )
            {
                var modified = entityEntry.Properties.Where ( property => property.IsModified ).ToHashSet ( );
                entityEntry.State = state;
                foreach ( var property in entityEntry.Properties )
                    property.IsModified = modified.Contains ( property );
            }
            else
                entityEntry.State = state;
        }
    }
}