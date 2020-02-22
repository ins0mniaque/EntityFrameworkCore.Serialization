using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Metadata;

namespace EntityFrameworkCore.Serialization.Internal
{
    public class TemporaryIdentitySynchronizer
    {
        private static readonly IComparer                   comparer          = Comparer.DefaultInvariant;
        private static readonly Dictionary < Type, object > defaultValueCache = new Dictionary < Type, object > ( );

        private Dictionary < IEntityType, Generator > generators = new Dictionary < IEntityType, Generator > ( );

        public void SynchronizeTemporaryIdentity ( PropertyEntry propertyEntry )
        {
            if ( propertyEntry == null )
                throw new ArgumentNullException ( nameof ( propertyEntry ) );

            if ( ! propertyEntry.Metadata.IsPrimaryKey ( ) || ! HasTemporaryValue ( propertyEntry ) )
                return;

            propertyEntry.IsTemporary = true;

            var entityType = propertyEntry.EntityEntry.Metadata;
            if ( ! generators.TryGetValue ( entityType, out var generator ) )
                generators.Add ( entityType, generator = new Generator ( propertyEntry.EntityEntry.Context, entityType ) );

            var currentValue       = propertyEntry.CurrentValue;
            var lastGeneratedValue = generator.PrimaryKey [ propertyEntry.Metadata ];

            while ( comparer.Compare ( lastGeneratedValue, currentValue ) < 0 )
            {
                generator.Next ( );

                lastGeneratedValue = generator.PrimaryKey [ propertyEntry.Metadata ];
            }
        }

        private static bool HasTemporaryValue ( PropertyEntry propertyEntry )
        {
            if ( MayGetTemporaryValue ( propertyEntry ) )
            {
                var propertyType = propertyEntry.Metadata.ClrType;
                if ( ! defaultValueCache.TryGetValue ( propertyType, out var defaultValue ) )
                    defaultValueCache [ propertyType ] = defaultValue = Activator.CreateInstance ( propertyType );

                return comparer.Compare ( propertyEntry.CurrentValue, defaultValue ) < 0;
            }

            return false;
        }

        #pragma warning disable EF1001 // Internal EF Core API usage.
        private static bool MayGetTemporaryValue ( PropertyEntry propertyEntry )
        {
            var valueGeneration = propertyEntry.EntityEntry.Context.GetDependencies ( ).StateManager.ValueGenerationManager;
            if ( valueGeneration.MayGetTemporaryValue ( propertyEntry.Metadata, propertyEntry.Metadata.DeclaringEntityType ) )
                return true;

            var principal = propertyEntry.Metadata.FindFirstPrincipal ( );
            if ( principal == null )
                return false;

            return valueGeneration.MayGetTemporaryValue ( principal, principal.DeclaringEntityType );
        }

        private class Generator
        {
            private static InternalEntityEntryFactory factory = new InternalEntityEntryFactory ( );

            public Generator ( DbContext dbContext, IEntityType entityType )
            {
                StateManager = dbContext.GetDependencies ( ).StateManager;
                EntityType   = entityType;
                Key          = entityType.FindPrimaryKey ( );
                Entity       = Activator.CreateInstance ( entityType.ClrType );
                PrimaryKey   = GenerateNextPrimaryKey ( );
            }

            private IStateManager StateManager { get; }
            private IEntityType   EntityType   { get; }
            private IKey          Key          { get; }
            private object        Entity       { get; }

            public Dictionary < IProperty, object > PrimaryKey { get; private set; }

            public void Next ( ) => PrimaryKey = GenerateNextPrimaryKey ( );

            private Dictionary < IProperty, object > GenerateNextPrimaryKey ( )
            {
                var entry = factory.Create ( StateManager, EntityType, Entity );

                StateManager.ValueGenerationManager.Generate  ( entry );
                StateManager.ValueGenerationManager.Propagate ( entry );

                return Key.Properties.ToDictionary ( property => property,
                                                     property => entry.GetCurrentValue ( property ) );
            }
        }
        #pragma warning restore EF1001 // Internal EF Core API usage.
    }
}