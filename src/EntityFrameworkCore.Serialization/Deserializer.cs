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

            var finder       = new EntityEntryFinder ( context );
            var synchronizer = new TemporaryIdentitySynchronizer ( );
            var entities     = new List < object > ( );
            var properties   = new Dictionary < IProperty, object? > ( );
            var collections  = new List < CollectionEntry > ( );

            while ( reader.ReadEntry ( ) )
            {
                var entityType  = reader.ReadEntityType  ( context.Model );
                var entityState = reader.ReadEntityState ( );

                properties.Clear ( );
                while ( reader.ReadProperty ( out var property, out var value ) )
                    properties [ property ] = value;

                var entityEntry = finder.FindOrCreate ( entityType, properties );

                foreach ( var entry in properties )
                {
                    var propertyEntry = entityEntry.SetProperty ( entry.Key, entry.Value );

                    if ( propertyEntry.Metadata.IsConcurrencyToken )
                    {
                        // TODO: Check original version...
                    }

                    if ( entityState != EntityState.Unchanged )
                        synchronizer.SynchronizeTemporaryIdentity ( propertyEntry );
                }

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
                if ( entityEntry != null )
                {
                    while ( reader.ReadModifiedProperty ( out var property, out var value ) )
                        entityEntry.SetProperty ( property, value );
                }
                else
                {
                    var modifiedProperties = new List < (IProperty Property, object? Value) > ( );
                    while ( reader.ReadModifiedProperty ( out var property, out var value ) )
                    {
                        modifiedProperties.Add ( (property, value) );
                        properties [ property ] = value;
                    }

                    entityEntry = finder.Find ( entityType, properties );
                    if ( entityEntry != null )
                    {
                        foreach ( var modified in modifiedProperties )
                            entityEntry.SetProperty ( modified.Property, modified.Value );
                    }
                    else
                    {
                        // TODO: Log entries not found
                    }
                }

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

        private static PropertyEntry SetProperty ( this EntityEntry entityEntry, IProperty property, object? value )
        {
            var propertyEntry = entityEntry.Property ( property );

            propertyEntry.OriginalValue = value;
            propertyEntry.CurrentValue  = value;
            propertyEntry.IsModified    = false;

            return propertyEntry;
        }

        private static void SetModifiedProperty ( this EntityEntry entityEntry, IProperty property, object? value )
        {
            var propertyEntry = entityEntry.Property ( property );

            propertyEntry.CurrentValue = value;
            propertyEntry.IsModified   = true;
        }
    }
}