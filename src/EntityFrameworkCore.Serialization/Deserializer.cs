using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;

using EntityFrameworkCore.Serialization.Internal;

namespace EntityFrameworkCore.Serialization
{
    public static partial class Deserializer
    {
        public static IReadOnlyList < object > Deserialize ( this DbContext context, IEntityEntryReader reader )
        {
            if ( context == null ) throw new ArgumentNullException ( nameof ( context ) );
            if ( reader  == null ) throw new ArgumentNullException ( nameof ( reader  ) );

            var finder      = new EntityEntryFinder ( context );
            var entities    = new List < object > ( );
            var properties  = new Dictionary < IProperty, object? > ( );
            var collections = new List < CollectionEntry > ( );

            while ( reader.ReadEntry ( ) )
            {
                var entityType  = reader.ReadEntityType  ( context.Model );
                var entityState = reader.ReadEntityState ( );

                properties.Clear ( );
                while ( reader.ReadProperty ( out var property, out var value ) )
                    properties [ property ] = value;

                var entityEntry = finder.FindOrCreate ( entityType, properties );
                foreach ( var entry in properties )
                    entityEntry.SetProperty ( entry.Key, entry.Value, entityState );

                entityEntry.SetState ( entityState );

                while ( reader.ReadModifiedProperty ( out var property, out var value ) )
                    entityEntry.SetModifiedProperty ( property, value );

                while ( reader.ReadNavigationState ( out var navigation ) )
                {
                    var collection = entityEntry.Collection ( navigation );
                    if ( ! collection.IsLoaded )
                        collections.Add ( collection );
                }

                entities.Add ( entityEntry.Entity );
            }

            foreach ( var collection in collections )
            {
                if ( collection.CurrentValue == null )
                    collection.CurrentValue = (IEnumerable) collection.Metadata.GetCollectionAccessor ( ).Create ( );

                collection.IsLoaded = true;
            }

            return entities.AsReadOnly ( );
        }

        public static void AcceptChanges ( this DbContext context, IEntityEntryReader reader )
        {
            if ( context == null ) throw new ArgumentNullException ( nameof ( context ) );
            if ( reader  == null ) throw new ArgumentNullException ( nameof ( reader  ) );

            var finder     = new EntityEntryFinder ( context );
            var properties = new Dictionary < IProperty, object? > ( );

            while ( reader.ReadEntry ( ) )
            {
                var entityType  = reader.ReadEntityType  ( context.Model );
                var entityState = reader.ReadEntityState ( );

                properties.Clear ( );
                while ( reader.ReadProperty ( out var property, out var value ) )
                    properties [ property ] = value;

                var entityEntry = finder.Find ( entityType, properties );
                if ( entityEntry == null )
                {
                    // TODO: Log entries not found
                    continue;
                }

                while ( reader.ReadModifiedProperty ( out var property, out var value ) )
                    entityEntry.SetDatabaseGeneratedProperty ( property, value );

                while ( reader.ReadNavigationState ( out var navigation ) );
            }

            context.ChangeTracker.AcceptAllChanges ( );
        }

        private static void SetState ( this EntityEntry entityEntry, EntityState entityState )
        {
            if ( entityState == EntityState.Modified )
            {
                var modified = entityEntry.Properties.Where ( property => property.IsModified ).ToHashSet ( );
                entityEntry.State = entityState;
                foreach ( var property in entityEntry.Properties )
                    property.IsModified = modified.Contains ( property );
            }
            else
                entityEntry.State = entityState;
        }

        private static void SetProperty ( this EntityEntry entityEntry, IProperty property, object? value, EntityState entityState )
        {
            var propertyEntry = entityEntry.Property ( property );

            if ( propertyEntry.Metadata.IsConcurrencyToken )
            {
                // TODO: Check original version...
            }

            propertyEntry.OriginalValue = value;
            propertyEntry.CurrentValue  = value;
            propertyEntry.IsModified    = false;

            if ( entityState == EntityState.Added && property.IsPrimaryKey ( ) && property.ValueGenerated.HasFlag ( ValueGenerated.OnAdd ) )
                propertyEntry.IsTemporary = true;
        }

        private static void SetModifiedProperty ( this EntityEntry entityEntry, IProperty property, object? value )
        {
            var propertyEntry = entityEntry.Property ( property );

            propertyEntry.CurrentValue = value;
            propertyEntry.IsModified   = true;
        }

        private static void SetDatabaseGeneratedProperty ( this EntityEntry entityEntry, IProperty property, object? value )
        {
            var propertyEntry = entityEntry.Property ( property );

            propertyEntry.OriginalValue = value;
            propertyEntry.CurrentValue  = value;
            propertyEntry.IsModified    = false;
        }
    }
}