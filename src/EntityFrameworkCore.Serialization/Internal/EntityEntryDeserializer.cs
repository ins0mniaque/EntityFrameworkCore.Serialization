using System.Collections;
using System.Data;
using System.Linq;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;

namespace EntityFrameworkCore.Serialization.Internal
{
    public static class EntityEntryDeserializer
    {
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

        public static void DeserializeProperties < TEntry > ( this IDbContextSerializer < TEntry > serializer, TEntry entry, EntityEntry entityEntry )
        {
            var values = serializer.ReadProperties ( entry, entityEntry.Metadata, out var properties );
            if ( values == null )
                return;

            var state = serializer.ReadEntityState ( entry );
            var index = 0;

            foreach ( var value in values )
            {
                var property = entityEntry.Property ( properties [ index++ ].Name );

                if ( property.Metadata.IsConcurrencyToken )
                {
                    // TODO: Check original version...
                }

                property.OriginalValue = value;
                property.CurrentValue  = value;
                property.IsModified    = false;

                if ( state == EntityState.Added && property.Metadata.IsPrimaryKey ( ) && property.Metadata.ValueGenerated.HasFlag ( ValueGenerated.OnAdd ) )
                    property.IsTemporary = true;
            }
        }

        public static void DeserializeModifiedProperties < TEntry > ( this IDbContextSerializer < TEntry > serializer, TEntry entry, EntityEntry entityEntry )
        {
            var values = serializer.ReadModifiedProperties ( entry, entityEntry.Metadata, out var properties );
            if ( values == null )
                return;

            var index = 0;

            foreach ( var value in values )
            {
                var property = entityEntry.Property ( properties [ index++ ].Name );

                property.CurrentValue = value;
                property.IsModified   = true;
            }
        }

        public static void DeserializeDatabaseGeneratedValues < TEntry > ( this IDbContextSerializer < TEntry > serializer, TEntry entry, EntityEntry entityEntry )
        {
            var values = serializer.ReadModifiedProperties ( entry, entityEntry.Metadata, out var properties );
            if ( values == null )
                return;

            var index = 0;

            foreach ( var value in values )
            {
                var property = entityEntry.Property ( properties [ index++ ].Name );

                property.OriginalValue = value;
                property.CurrentValue  = value;
                property.IsModified    = false;
            }
        }

        public static void DeserializeNavigationState < TEntry > ( this IDbContextSerializer < TEntry > serializer, TEntry entry, EntityEntry entityEntry )
        {
            serializer.ReadNavigationState ( entry, entityEntry.Metadata, out var navigated );
            if ( navigated == null )
                return;

            foreach ( var navigation in navigated )
            {
                var collection = entityEntry.Collection ( navigation.Name );
                if ( collection.IsLoaded )
                    continue;

                if ( collection.CurrentValue == null )
                    collection.CurrentValue = (IEnumerable) collection.Metadata.GetCollectionAccessor ( ).Create ( );

                collection.IsLoaded = true;
            }
        }
    }
}