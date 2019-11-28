using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;

using EntityFrameworkCore.Serialization.Graph;

namespace EntityFrameworkCore.Serialization
{
    public static partial class Serializer
    {
        public static void Serialize ( this DbContext context, IEntityEntryWriter writer )
        {
            if ( context == null ) throw new ArgumentNullException ( nameof ( context ) );
            if ( writer  == null ) throw new ArgumentNullException ( nameof ( writer  ) );

            writer.Write ( context.ChangeTracker.Entries ( ), SerializationMode.Full );
        }

        public static void SerializeGraph ( this DbContext context, IEntityEntryWriter writer, object item )
        {
            if ( context == null ) throw new ArgumentNullException ( nameof ( context ) );
            if ( writer  == null ) throw new ArgumentNullException ( nameof ( writer  ) );

            writer.Write ( context.Graph ( item ), SerializationMode.Full );
        }

        public static void SerializeGraph ( this DbContext context, IEntityEntryWriter writer, params object [ ] items )
        {
            if ( context == null ) throw new ArgumentNullException ( nameof ( context ) );
            if ( writer  == null ) throw new ArgumentNullException ( nameof ( writer  ) );

            writer.Write ( context.Graph ( items ), SerializationMode.Full );
        }

        public static void SerializeChanges ( this DbContext context, IEntityEntryWriter writer )
        {
            if ( context == null ) throw new ArgumentNullException ( nameof ( context ) );
            if ( writer  == null ) throw new ArgumentNullException ( nameof ( writer  ) );

            writer.Write ( context.ChangeTracker.Entries ( ).Where ( IsChanged ), SerializationMode.Changes );
        }

        public static void SerializeGraphChanges ( this DbContext context, IEntityEntryWriter writer, object item )
        {
            if ( context == null ) throw new ArgumentNullException ( nameof ( context ) );
            if ( writer  == null ) throw new ArgumentNullException ( nameof ( writer  ) );

            writer.Write ( context.Graph ( item ).Where ( IsChanged ), SerializationMode.Changes );
        }

        public static void SerializeGraphChanges ( this DbContext context, IEntityEntryWriter writer, params object [ ] items )
        {
            if ( context == null ) throw new ArgumentNullException ( nameof ( context ) );
            if ( writer  == null ) throw new ArgumentNullException ( nameof ( writer  ) );

            writer.Write ( context.Graph ( items ).Where ( IsChanged ), SerializationMode.Changes );
        }

        public static int SaveChanges ( this DbContext context, IEntityEntryWriter writer )
        {
            var snapshots = context.ChangeTracker
                                   .Entries ( )
                                   .Where  ( HasDatabaseGeneratedValues )
                                   .OrderedByMetadata ( )
                                   .Select ( entityEntry =>
                                             {
                                                 var properties = entityEntry.Properties
                                                                             .Where  ( property => property.Metadata.IsPrimaryKey ( ) ||
                                                                                                   property.Metadata.IsConcurrencyToken )
                                                                             .ToList ( );

                                                 return new { EntityEntry = entityEntry,
                                                              State       = entityEntry.State,
                                                              Properties  = properties.ToDictionary ( property => property.Metadata,
                                                                                                      property => property.CurrentValue ) };
                                             } )
                                   .ToList ( );

            var rowCount = context.SaveChanges ( );

            foreach ( var snapshot in snapshots )
            {
                var mode = snapshot.State == EntityState.Added ? SerializationMode.ValuesGeneratedOnAdd:
                                                                 SerializationMode.ValuesGeneratedOnUpdate;

                writer.Write ( snapshot.EntityEntry, mode, snapshot.Properties );
            }

            return rowCount;
        }

        private enum SerializationMode
        {
            Full,
            Changes,
            ValuesGeneratedOnAdd,
            ValuesGeneratedOnUpdate
        }

        private static void Write ( this IEntityEntryWriter writer, IEnumerable < EntityEntry > entries, SerializationMode mode )
        {
            foreach ( var entityEntry in entries.OrderedByMetadata ( ) )
                writer.Write ( entityEntry, mode );
        }

        private static void Write ( this IEntityEntryWriter writer, EntityEntry entityEntry, SerializationMode mode, IDictionary < IProperty, object? > originalValues = null )
        {
            var state = entityEntry.State;

            writer.WriteStartEntry  ( );
            writer.WriteEntityType  ( entityEntry.Metadata );
            writer.WriteEntityState ( state );

            var properties         = entityEntry.Properties.ToList ( );
            var writeAllProperties = mode == SerializationMode.Full && state != EntityState.Deleted || state == EntityState.Added;

            if ( originalValues != null )
            {
                foreach ( var property in properties )
                {
                    if ( writeAllProperties || property.Metadata.IsPrimaryKey ( ) || property.Metadata.IsConcurrencyToken )
                    {
                        if ( originalValues.TryGetValue ( property.Metadata, out var originalValue ) )
                            writer.WriteProperty ( property.Metadata, originalValue );
                        else
                            writer.WriteProperty ( property.Metadata, property.OriginalValue );
                    }
                }
            }
            else
                foreach ( var property in properties )
                    if ( writeAllProperties || property.Metadata.IsPrimaryKey ( ) || property.Metadata.IsConcurrencyToken )
                        writer.WriteProperty ( property.Metadata, property.OriginalValue );

            var modifiedProperties = mode switch
            {
                SerializationMode.ValuesGeneratedOnAdd    => properties.HavingValueGeneratedFlag ( ValueGenerated.OnAdd    ),
                SerializationMode.ValuesGeneratedOnUpdate => properties.HavingValueGeneratedFlag ( ValueGenerated.OnUpdate ),
                _                                         => properties.Where ( property => property.IsModified )
            };

            foreach ( var modifiedProperty in modifiedProperties )
                writer.WriteModifiedProperty ( modifiedProperty.Metadata, modifiedProperty.CurrentValue );

            if ( mode == SerializationMode.Full )
                foreach ( var collection in entityEntry.Collections )
                    if ( collection.IsLoaded )
                        writer.WriteNavigationState ( collection.Metadata );

            writer.WriteEndEntry ( );
        }

        private static List < EntityEntry > Graph ( this DbContext dbContext, object item )
        {
            var entries = new List < EntityEntry > ( );
            dbContext.TraverseGraph ( item, node => entries.Add ( node.Entry ) );
            return entries;
        }

        private static List < EntityEntry > Graph ( this DbContext dbContext, IEnumerable < object > items )
        {
            var entries = new List < EntityEntry > ( );
            dbContext.TraverseGraph ( items, node => entries.Add ( node.Entry ) );
            return entries;
        }

        private static IEnumerable < EntityEntry > OrderedByMetadata ( this IEnumerable < EntityEntry > entries )
        {
            return entries.OrderBy ( entry => entry.Metadata.Name )
                          .ThenBy  ( entry => entry.State );
        }

        private static bool IsChanged ( this EntityEntry entityEntry )
        {
            return entityEntry.State == EntityState.Added    ||
                   entityEntry.State == EntityState.Modified ||
                   entityEntry.State == EntityState.Deleted;
        }

        private static bool HasDatabaseGeneratedValues ( this EntityEntry entityEntry )
        {
            return entityEntry.State == EntityState.Added    && entityEntry.HasValueGeneratedFlag ( ValueGenerated.OnAdd    ) ||
                   entityEntry.State == EntityState.Modified && entityEntry.HasValueGeneratedFlag ( ValueGenerated.OnUpdate );
        }

        private static bool HasValueGeneratedFlag ( this EntityEntry entityEntry, ValueGenerated valueGenerated )
        {
            return entityEntry.Metadata.GetProperties ( ).Any ( property => property.ValueGenerated.HasFlag ( valueGenerated ) );
        }

        private static IEnumerable < PropertyEntry > HavingValueGeneratedFlag ( this IEnumerable < PropertyEntry > properties, ValueGenerated valueGenerated )
        {
            return properties.Where ( property => property.Metadata.ValueGenerated.HasFlag ( valueGenerated ) );
        }
    }
}