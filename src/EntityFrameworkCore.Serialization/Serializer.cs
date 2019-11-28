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
    public static class Serializer
    {
        public static void Serialize < T > ( this DbContext context, IDbContextSerializer < T > serializer, T writable )
        {
            context.Serialize ( serializer.CreateWriter ( writable ) );
        }

        public static void SerializeGraph < T > ( this DbContext context, IDbContextSerializer < T > serializer, T writable, object item )
        {
            context.SerializeGraph ( serializer.CreateWriter ( writable ), item );
        }

        public static void SerializeGraph < T > ( this DbContext context, IDbContextSerializer < T > serializer, T writable, params object [ ] items )
        {
            context.SerializeGraph ( serializer.CreateWriter ( writable ), items );
        }

        public static void SerializeChanges < T > ( this DbContext context, IDbContextSerializer < T > serializer, T writable )
        {
            context.SerializeChanges ( serializer.CreateWriter ( writable ) );
        }

        public static void SerializeGraphChanges < T > ( this DbContext context, IDbContextSerializer < T > serializer, T writable, object item )
        {
            context.SerializeGraphChanges ( serializer.CreateWriter ( writable ), item );
        }

        public static void SerializeGraphChanges < T > ( this DbContext context, IDbContextSerializer < T > serializer, T writable, params object [ ] items )
        {
            context.SerializeGraphChanges ( serializer.CreateWriter ( writable ), items );
        }

        public static void Serialize ( this DbContext context, IEntityEntryWriter writer )
        {
            if ( context == null ) throw new ArgumentNullException ( nameof ( context ) );
            if ( writer  == null ) throw new ArgumentNullException ( nameof ( writer  ) );

            foreach ( var entityEntry in context.ChangeTracker.Entries ( ).OrderedByMetadata ( ) )
                writer.Write ( entityEntry, SerializationMode.Full );
        }

        public static void SerializeGraph ( this DbContext context, IEntityEntryWriter writer, object item )
        {
            if ( context == null ) throw new ArgumentNullException ( nameof ( context ) );
            if ( writer  == null ) throw new ArgumentNullException ( nameof ( writer  ) );

            foreach ( var entityEntry in context.Graph ( item ).OrderedByMetadata ( ) )
                writer.Write ( entityEntry, SerializationMode.Full );
        }

        public static void SerializeGraph ( this DbContext context, IEntityEntryWriter writer, params object [ ] items )
        {
            if ( context == null ) throw new ArgumentNullException ( nameof ( context ) );
            if ( writer  == null ) throw new ArgumentNullException ( nameof ( writer  ) );

            foreach ( var entityEntry in context.Graph ( items ).OrderedByMetadata ( ) )
                writer.Write ( entityEntry, SerializationMode.Full );
        }

        public static void SerializeChanges ( this DbContext context, IEntityEntryWriter writer )
        {
            if ( context == null ) throw new ArgumentNullException ( nameof ( context ) );
            if ( writer  == null ) throw new ArgumentNullException ( nameof ( writer  ) );

            foreach ( var entityEntry in context.ChangeTracker.Entries ( ).Where ( IsChanged ).OrderedByMetadata ( ) )
                writer.Write ( entityEntry, SerializationMode.Changes );
        }

        public static void SerializeGraphChanges ( this DbContext context, IEntityEntryWriter writer, object item )
        {
            if ( context == null ) throw new ArgumentNullException ( nameof ( context ) );
            if ( writer  == null ) throw new ArgumentNullException ( nameof ( writer  ) );

            foreach ( var entityEntry in context.Graph ( item ).Where ( IsChanged ).OrderedByMetadata ( ) )
                writer.Write ( entityEntry, SerializationMode.Changes );
        }

        public static void SerializeGraphChanges ( this DbContext context, IEntityEntryWriter writer, params object [ ] items )
        {
            if ( context == null ) throw new ArgumentNullException ( nameof ( context ) );
            if ( writer  == null ) throw new ArgumentNullException ( nameof ( writer  ) );

            foreach ( var entityEntry in context.Graph ( items ).Where ( IsChanged ).OrderedByMetadata ( ) )
                writer.Write ( entityEntry, SerializationMode.Changes );
        }

        internal enum SerializationMode
        {
            Full,
            Changes,
            ValuesGeneratedOnAdd,
            ValuesGeneratedOnUpdate
        }

        internal static void Write ( this IEntityEntryWriter writer, EntityEntry entityEntry, SerializationMode mode, IDictionary < IProperty, object? > originalValues = null )
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

        private static IEnumerable < PropertyEntry > HavingValueGeneratedFlag ( this IEnumerable < PropertyEntry > properties, ValueGenerated valueGenerated )
        {
            return properties.Where ( property => property.Metadata.ValueGenerated.HasFlag ( valueGenerated ) );
        }
    }
}