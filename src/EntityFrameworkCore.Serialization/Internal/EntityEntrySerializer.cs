using System;
using System.Data;
using System.Linq;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;

namespace EntityFrameworkCore.Serialization.Internal
{
    public static class EntityEntrySerializer
    {
        public static TEntry Serialize < TEntry > ( this IDbContextSerializer < TEntry > serializer, EntityEntry entityEntry )
        {
            return serializer.Serialize ( entityEntry, SerializationMode.Full );
        }

        public static TEntry SerializeChanges < TEntry > ( this IDbContextSerializer < TEntry > serializer, EntityEntry entityEntry )
        {
            return serializer.Serialize ( entityEntry, SerializationMode.Changes );
        }

        public static TEntry SerializeDatabaseGeneratedValues < TEntry > ( this IDbContextSerializer < TEntry > serializer, EntityEntry entityEntry, EntityState originalState )
        {
            switch ( originalState )
            {
                case EntityState.Added    : return serializer.Serialize ( entityEntry, SerializationMode.ValuesGeneratedOnAdd    );
                case EntityState.Modified : return serializer.Serialize ( entityEntry, SerializationMode.ValuesGeneratedOnUpdate );
                default                   : throw new ArgumentOutOfRangeException ( );
            }
        }

        private enum SerializationMode
        {
            Full,
            Changes,
            ValuesGeneratedOnAdd,
            ValuesGeneratedOnUpdate
        }

        private static TEntry Serialize < TEntry > ( this IDbContextSerializer < TEntry > serializer, EntityEntry entityEntry, SerializationMode mode )
        {
            var entry = serializer.CreateEntry ( );
            var state = entityEntry.State;

            serializer.WriteEntityType  ( entry, entityEntry.Metadata );
            serializer.WriteEntityState ( entry, state );

            var properties         = entityEntry.Properties.ToList ( );
            var writeAllProperties = mode == SerializationMode.Full && state != EntityState.Deleted || state == EntityState.Added;
            var writtenProperties  = writeAllProperties ? properties :
                                                          properties.Where ( property => property.Metadata.IsPrimaryKey ( ) ||
                                                                                         property.Metadata.IsConcurrencyToken )
                                                                    .ToList ( );

            serializer.WriteProperties ( entry,
                                         writtenProperties.Select ( property => property.Metadata      ).ToArray ( ),
                                         writtenProperties.Select ( property => property.OriginalValue ).ToArray ( ) );

            var modifiedProperties = mode == SerializationMode.ValuesGeneratedOnAdd    ? properties.Where ( p => p.Metadata.ValueGenerated.HasFlag ( ValueGenerated.OnAdd ) ).ToList ( ) :
                                     mode == SerializationMode.ValuesGeneratedOnUpdate ? properties.Where ( p => p.Metadata.ValueGenerated.HasFlag ( ValueGenerated.OnUpdate ) ).ToList ( ) :
                                     properties.Where ( p => p.IsModified ).ToList ( );

            if ( modifiedProperties.Count > 0 )
                serializer.WriteModifiedProperties ( entry, modifiedProperties.Select ( property => property.Metadata     ).ToArray ( ),
                                                            modifiedProperties.Select ( property => property.CurrentValue ).ToArray ( ) );

            if ( mode == SerializationMode.Full )
            {
                var loadedCollections = entityEntry.Collections.Where   ( collection => collection.IsLoaded )
                                                               .Select  ( collection => collection.Metadata )
                                                               .ToArray ( );
                if ( loadedCollections.Length > 0 )
                    serializer.WriteLoadedCollections ( entry, loadedCollections );
            }

            return entry;
        }

        private static bool HasValueGeneratedFlag ( PropertyEntry property, ValueGenerated valueGenerated )
        {
            return property.Metadata.ValueGenerated.HasFlag ( valueGenerated );
        }
    }
}