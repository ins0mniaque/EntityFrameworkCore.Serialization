using System;
using System.Collections.Generic;
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
                default                   : throw new ArgumentOutOfRangeException ( nameof ( originalState ), "Original state must be EntityState.Added or EntityState.Modified" );
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
                                                          properties.Where  ( property => property.Metadata.IsPrimaryKey ( ) ||
                                                                                          property.Metadata.IsConcurrencyToken )
                                                                    .ToList ( );

            serializer.WriteProperties ( entry,
                                         writtenProperties.Select ( property => property.Metadata      ).ToArray ( ),
                                         writtenProperties.Select ( property => property.OriginalValue ).ToArray ( ) );

            var modifiedProperties = mode switch
            {
                SerializationMode.ValuesGeneratedOnAdd    => properties.HavingValueGeneratedFlag ( ValueGenerated.OnAdd    ).ToList ( ),
                SerializationMode.ValuesGeneratedOnUpdate => properties.HavingValueGeneratedFlag ( ValueGenerated.OnUpdate ).ToList ( ),
                _                                         => properties.Where ( property => property.IsModified ).ToList ( )
            };

            if ( modifiedProperties.Count > 0 )
                serializer.WriteModifiedProperties ( entry, modifiedProperties.Select ( property => property.Metadata     ).ToArray ( ),
                                                            modifiedProperties.Select ( property => property.CurrentValue ).ToArray ( ) );

            if ( mode == SerializationMode.Full )
            {
                var navigated = entityEntry.Collections
                                           .Where   ( collection => collection.IsLoaded )
                                           .Select  ( collection => collection.Metadata )
                                           .ToArray ( );

                if ( navigated.Length > 0 )
                    serializer.WriteNavigationState ( entry, navigated );
            }

            return entry;
        }

        private static IEnumerable < PropertyEntry > HavingValueGeneratedFlag ( this IEnumerable < PropertyEntry > properties, ValueGenerated valueGenerated )
        {
            return properties.Where ( property => property.Metadata.ValueGenerated.HasFlag ( valueGenerated ) );
        }
    }
}