using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.TestModels.Northwind;

using Xunit;

namespace EntityFrameworkCore.Serialization.Tests
{
    public class SerializerTests : IClassFixture < SerializerTests.Fixture >
    {
        private readonly Fixture fixture;

        public SerializerTests ( Fixture fixture )
        {
            this.fixture = fixture;
        }

        [ Fact ]
        public void SerializeThrowsArgumentNullExceptionOnNullContext ( )
        {
            var context = (DbContext?) null;
            var writer  = new Dictionary.EntityEntryWriter ( new List < Dictionary.Entry > ( ) );

            Assert.Throws < ArgumentNullException > ( ( ) => context.Serialize ( writer ) );
        }

        [ Fact ]
        public void SerializeThrowsArgumentNullExceptionOnNullReader ( )
        {
            var context = fixture.CreateDisconnectedDbContext ( );
            var writer  = (IEntityEntryWriter) null;

            Assert.Throws < ArgumentNullException > ( ( ) => context.Serialize ( writer ) );
        }

        [ Fact ]
        public void CanSerialize ( )
        {
            var serializer = new Dictionary.DbContextSerializer ( );

            using var context = fixture.CreateDbContext ( );

            var customers = context.Customers.ToList ( );
            var count     = context.ChangeTracker.Entries ( ).Count ( );
            var data      = new List < Dictionary.Entry > ( );

            context.Serialize ( serializer, data );

            using var emptyContext = fixture.CreateDbContext ( );

            emptyContext.Deserialize ( serializer, data );

            Assert.Equal ( count, emptyContext.ChangeTracker.Entries ( ).Count ( ) );

            using var disconnectedContext = fixture.CreateDisconnectedDbContext ( );

            disconnectedContext.Deserialize ( serializer, data );

            Assert.Equal ( count, disconnectedContext.ChangeTracker.Entries ( ).Count ( ) );
        }

        [ Fact ]
        public void CanSerializeChanges ( )
        {
            var serializer = new Dictionary.DbContextSerializer ( );

            using var context = fixture.CreateDbContext ( );

            var customers = context.Customers
                                   .Include ( customer => customer.Orders )
                                   .ToList ( );

            customers.First ( ).Orders.RemoveAt ( 0 );

            var serializedContext = new List < Dictionary.Entry > ( );

            context.SerializeChanges ( serializer, serializedContext );

            context.Deserialize ( serializer, serializedContext );

            var reserializedContext = new List < Dictionary.Entry > ( );

            context.SerializeChanges ( serializer, reserializedContext );

            Assert.Single ( reserializedContext );
            Assert.Equal  ( EntityState.Modified, reserializedContext [ 0 ].EntityState );

            using var emptyContext = fixture.CreateDbContext ( );

            emptyContext.Deserialize ( serializer, serializedContext );

            var serializedEmptyContext = new List < Dictionary.Entry > ( );

            emptyContext.Serialize ( serializer, serializedEmptyContext );

            Assert.Single ( serializedEmptyContext );
            Assert.Equal  ( EntityState.Modified, serializedEmptyContext [ 0 ].EntityState );

            using var nonEmptyContext = fixture.CreateDbContext ( );

            customers = nonEmptyContext.Customers
                                       .Include ( customer => customer.Orders )
                                       .ToList  ( );

            nonEmptyContext.Deserialize ( serializer, serializedContext );

            var serializedNonEmptyContext = new List < Dictionary.Entry > ( );

            nonEmptyContext.SerializeChanges ( serializer, serializedNonEmptyContext );

            Assert.Single ( serializedNonEmptyContext );
            Assert.Equal  ( EntityState.Modified, serializedNonEmptyContext [ 0 ].EntityState );
        }

        [ Fact ]
        public void DeserializeLoadsNavigationProperties ( )
        {
            var serializer = new Dictionary.DbContextSerializer ( );

            using var context = fixture.CreateDbContext ( );

            var customers = context.Customers
                                   .Include ( customer => customer.Orders )
                                   .ToList ( );

            var serializedContext = new List < Dictionary.Entry > ( );

            context.Serialize ( serializer, serializedContext );

            using var disconnectedContext = fixture.CreateDisconnectedDbContext ( );

            disconnectedContext.Deserialize ( serializer, serializedContext );

            Assert.All ( disconnectedContext.ChangeTracker.Entries < Order > ( ),
                         entityEntry => Assert.True ( entityEntry.Navigation ( nameof ( Order.Customer ) ).IsLoaded ) );

            Assert.All ( disconnectedContext.ChangeTracker.Entries < Customer > ( ),
                         entityEntry => Assert.True ( entityEntry.Navigation ( nameof ( Customer.Orders ) ).IsLoaded ) );
        }

        [ Fact ]
        public void CanCompleteServerClientWorkflow ( )
        {
            var serializer = new Dictionary.DbContextSerializer ( );

            using var fixture = new Fixture ( );

            using var serverContext = fixture.CreateDbContext ( );

            var customer = serverContext.Customers.Include ( c => c.Orders ).First ( );

            var graph = new List < Dictionary.Entry > ( );

            serverContext.SerializeGraph ( serializer, graph, customer );

            using var clientContext = fixture.CreateDisconnectedDbContext ( );

            clientContext.Deserialize ( serializer, graph );

            Assert.Equal ( serverContext.ChangeTracker.Entries ( ).OrderBy ( entry => entry, EntityEntryComparer.Instance ),
                           clientContext.ChangeTracker.Entries ( ).OrderBy ( entry => entry, EntityEntryComparer.Instance ),
                           EntityEntryComparer.Instance );

            customer = clientContext.Find < Customer > ( customer.CustomerID );
            customer.ContactName = "Would prefer not to be contacted";

            customer.Orders.RemoveAt ( 3 );

            clientContext.Remove ( customer.Orders [ 3 ] );

            var newCustomer = new Customer { CustomerID = "NOPE",
                                             Country    = "Canada",
                                             Orders     = new List < Order > { new Order { OrderDate = DateTime.UtcNow } } };

            clientContext.Customers.Add ( newCustomer );

            var changes = new List < Dictionary.Entry > ( );

            clientContext.SerializeChanges ( serializer, changes );

            serverContext.Deserialize ( serializer, changes );

            Assert.Equal ( serverContext.ChangeTracker.Entries ( ).OrderBy ( entry => entry, EntityEntryComparer.Instance ),
                           clientContext.ChangeTracker.Entries ( ).OrderBy ( entry => entry, EntityEntryComparer.Instance ),
                           EntityEntryComparer.Instance );

            var databaseGeneratedValues = new List < Dictionary.Entry > ( );

            serverContext.SaveChanges ( serializer, databaseGeneratedValues );

            clientContext.AcceptChanges ( serializer, databaseGeneratedValues );

            Assert.All ( serverContext.ChangeTracker.Entries ( ),
                         entry => Assert.True ( entry.State == EntityState.Unchanged ) );

            Assert.All ( clientContext.ChangeTracker.Entries ( ),
                         entry => Assert.True ( entry.State == EntityState.Unchanged ) );

            Assert.Equal ( serverContext.ChangeTracker.Entries ( ).OrderBy ( entry => entry, EntityEntryComparer.Instance ),
                           clientContext.ChangeTracker.Entries ( ).OrderBy ( entry => entry, EntityEntryComparer.Instance ),
                           EntityEntryComparer.Instance );
        }

        public class Fixture : SqliteDbContextFixture < NorthwindRelationalContext >
        {
            protected override NorthwindRelationalContext Factory ( DbContextOptions < NorthwindRelationalContext > options ) => new NorthwindRelationalContext ( options );
            protected override void                       Seed    ( NorthwindRelationalContext db )                           => NorthwindData.Seed ( db );
        }
    }
}