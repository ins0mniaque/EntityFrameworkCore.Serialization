﻿using System;
using System.Data;
using System.Linq;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;

namespace EntityFrameworkCore.Serialization.Internal
{
    public class EntityEntryFinder < TEntry >
    {
        public EntityEntryFinder ( DbContext context, IDbContextSerializer < TEntry > serializer )
        {
            Context    = context;
            Entries    = context.ChangeTracker.Entries ( ).ToLookup ( entry => entry.Metadata );
            Serializer = serializer;
        }

        public EntityEntry FindOrCreate ( TEntry entry ) => Find ( entry ) ?? Create ( entry );

        public EntityEntry Find ( TEntry entry )
        {
            var entityType = Serializer.ReadEntityType ( entry, Context.Model );

            var values = Serializer.ReadProperties ( entry, entityType, out var properties );

            // TODO: Something to cache this...
            var primaryKeyIndices = entityType.GetProperties ( )
                                              .Select ( (s, index) => new {s, index} )
                                              .Where ( p => p.s.IsPrimaryKey ( ) )
                                              .Select ( p => p.index )
                                              .ToList ( );

            var primaryKeyProperties = primaryKeyIndices.Select ( i => properties [ i ] ).ToArray ( );
            var primaryKey           = primaryKeyIndices.Select ( i => values     [ i ] ).ToArray ( );

            var entityEntry = Entries [ entityType ].FirstOrDefault ( e =>
            {
                return primaryKey.Select ( (value, index) =>
                {
                    var property = e.Property ( primaryKeyProperties [ index ].Name );

                    return PropertyEquals ( property.Metadata, property.CurrentValue, value );
                } ).All ( isEqual => isEqual );
            } );

            return entityEntry;
        }

        public EntityEntry Create ( TEntry entry )
        {
            var entityType  = Serializer.ReadEntityType ( entry, Context.Model );
            var entityEntry = Context.Entry ( Activator.CreateInstance ( entityType.ClrType ) );

            // TODO: Move setting the primary key and concurrency token here...

            return entityEntry;
        }

        private DbContext                            Context    { get; }
        private ILookup < IEntityType, EntityEntry > Entries    { get; }
        private IDbContextSerializer < TEntry >      Serializer { get; }

        private static bool PropertyEquals ( IProperty property, object left, object right )
        {
            var comparer = property.GetStructuralValueComparer ( );

            return comparer != null ? comparer.Equals ( left, right ) :
                                      object  .Equals ( left, right );
        }
    }
}