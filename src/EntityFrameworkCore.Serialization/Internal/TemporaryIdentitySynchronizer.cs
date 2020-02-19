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
        private static IComparer comparer = Comparer.DefaultInvariant;

        private Dictionary < IEntityType, Generator > cache = new Dictionary < IEntityType, Generator > ( );

        public static bool IsPartOfTemporaryIdentity ( IProperty property )
        {
            if ( property == null )
                throw new ArgumentNullException ( nameof ( property ) );

            return property.ValueGenerated.HasFlag ( ValueGenerated.OnAdd ) && property.IsPrimaryKey ( );
        }

        public void SynchronizeTemporaryIdentity ( PropertyEntry propertyEntry )
        {
            if ( propertyEntry == null )
                throw new ArgumentNullException ( nameof ( propertyEntry ) );

            propertyEntry.IsTemporary = true;

            var entityType = propertyEntry.EntityEntry.Metadata;
            if ( ! cache.TryGetValue ( entityType, out var generator ) )
                cache.Add ( entityType, generator = new Generator ( propertyEntry.EntityEntry.Context, entityType ) );

            var currentValue       = propertyEntry.CurrentValue;
            var lastGeneratedValue = generator.PrimaryKey [ propertyEntry.Metadata ];

            while ( comparer.Compare ( lastGeneratedValue, currentValue ) < 0 )
            {
                generator.Next ( );

                lastGeneratedValue = generator.PrimaryKey [ propertyEntry.Metadata ];
            }
        }

        #pragma warning disable EF1001 // Internal EF Core API usage.
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

                StateManager.ValueGenerationManager.Generate ( entry );

                return Key.Properties.ToDictionary ( property => property,
                                                     property => entry.GetCurrentValue ( property ) );
            }
        }
        #pragma warning restore EF1001 // Internal EF Core API usage.
    }
}