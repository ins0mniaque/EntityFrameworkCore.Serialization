using System;
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
            var context    = (DbContext?) null;
            var serializer = new POCO.DbContextEntrySerializer ( );

            Assert.Throws < ArgumentNullException > ( ( ) => context.Serialize ( serializer ) );
        }

        [ Fact ]
        public void SerializeThrowsArgumentNullExceptionOnNullSerializer ( )
        {
            var context    = fixture.CreateDisconnectedDbContext ( );
            var serializer = (IDbContextSerializer < object >?) null;

            Assert.Throws < ArgumentNullException > ( ( ) => context.Serialize ( serializer ) );
        }

        [ Fact ]
        public void CanSerialize ( )
        {
            var serializer = new POCO.DbContextEntrySerializer ( );

            using var context = fixture.CreateDbContext ( );

            var customers = context.Customers.ToList ( );
            var count     = context.ChangeTracker.Entries ( ).Count ( );
            var data      = context.Serialize ( serializer ).ToList ( );

            using var emptyContext = fixture.CreateDbContext ( );

            emptyContext.Deserialize ( data, serializer );

            Assert.Equal ( count, emptyContext.ChangeTracker.Entries ( ).Count ( ) );

            using var disconnectedContext = fixture.CreateDisconnectedDbContext ( );

            disconnectedContext.Deserialize ( data, serializer );

            Assert.Equal ( count, disconnectedContext.ChangeTracker.Entries ( ).Count ( ) );
        }

        [ Fact ]
        public void CanSerializeChanges ( )
        {
            var serializer = new POCO.DbContextIndexedEntrySerializer ( );

            using var context = fixture.CreateDbContext ( );

            var customers = context.Customers
                                   .Include ( customer => customer.Orders )
                                   .ToList ( );

            customers.First ( ).Orders.RemoveAt ( 0 );

            var serializedContext = context.SerializeChanges ( serializer ).ToList ( );

            context.Deserialize ( serializedContext, serializer );

            var reserializedContext = context.SerializeChanges ( serializer ).ToList ( );

            Assert.Single ( reserializedContext );
            Assert.Equal  ( EntityState.Modified, reserializedContext [ 0 ].EntityState );

            using var emptyContext = fixture.CreateDbContext ( );

            emptyContext.Deserialize ( serializedContext, serializer );

            var serializedEmptyContext = emptyContext.Serialize ( serializer ).ToList ( );

            Assert.Single ( serializedEmptyContext );
            Assert.Equal  ( EntityState.Modified, serializedEmptyContext [ 0 ].EntityState );

            using var nonEmptyContext = fixture.CreateDbContext ( );

            customers = nonEmptyContext.Customers
                                       .Include ( customer => customer.Orders )
                                       .ToList  ( );

            nonEmptyContext.Deserialize ( serializedContext, serializer );

            var serializedNonEmptyContext = nonEmptyContext.SerializeChanges ( serializer ).ToList ( );

            Assert.Single ( serializedNonEmptyContext );
            Assert.Equal  ( EntityState.Modified, serializedNonEmptyContext [ 0 ].EntityState );
        }

        [ Fact ]
        public void DeserializeLoadsNavigationProperties ( )
        {
            var serializer = new POCO.DbContextIndexedEntrySerializer ( );

            using var context = fixture.CreateDbContext ( );

            var customers = context.Customers
                                   .Include ( customer => customer.Orders )
                                   .ToList ( );

            var serializedContext = context.Serialize ( serializer ).ToList ( );

            using var disconnectedContext = fixture.CreateDisconnectedDbContext ( );

            disconnectedContext.Deserialize ( serializedContext, serializer );

            Assert.All ( disconnectedContext.ChangeTracker.Entries < Order > ( ),
                         entityEntry => Assert.True ( entityEntry.Navigation ( nameof ( Order.Customer ) ).IsLoaded ) );

            Assert.All ( disconnectedContext.ChangeTracker.Entries < Customer > ( ),
                         entityEntry => Assert.True ( entityEntry.Navigation ( nameof ( Customer.Orders ) ).IsLoaded ) );
        }

        [ Fact ]
        public void CanCompleteServerClientWorkflow ( )
        {
            var serializer = new POCO.DbContextIndexedEntrySerializer ( );

            using var fixture = new Fixture ( );

            using var serverContext = fixture.CreateDbContext ( );

            var customer = serverContext.Customers.Include ( c => c.Orders ).First ( );

            var graph = serverContext.SerializeGraph ( customer, serializer ).ToList ( );

            using var clientContext = fixture.CreateDisconnectedDbContext ( );

            clientContext.Deserialize ( graph, serializer );

            customer = clientContext.Find < Customer > ( customer.CustomerID );
            customer.ContactName = "Would prefer not to be contacted";

            customer.Orders.RemoveAt ( 3 );

            clientContext.Remove ( customer.Orders [ 3 ] );

            var newCustomer = new Customer ( ) { CustomerID = "NOPE", Country = "Canada" };
            newCustomer.Orders = new System.Collections.Generic.List<Order>( new [ ] { new Order ( ) { OrderDate = DateTime.UtcNow } } );

            clientContext.Customers.Add ( newCustomer );

            var changes = clientContext.SerializeChanges ( serializer ).ToList ( );

            serverContext.Deserialize ( changes, serializer );

            serverContext.SaveChanges ( serializer, out var databaseGeneratedValues );

            clientContext.AcceptChanges ( databaseGeneratedValues, serializer );

            var finalServerState = serverContext.Serialize ( serializer ).ToList ( );
            var finalClientState = clientContext.Serialize ( serializer ).ToList ( );

            Assert.All ( finalServerState,
                         entry => Assert.True ( entry.EntityState == EntityState.Unchanged ) );

            Assert.All ( finalClientState,
                         entry => Assert.True ( entry.EntityState == EntityState.Unchanged ) );

            Assert.Equal ( finalClientState.Count, finalServerState.Count );
        }

        public class Fixture : SqliteDbContextFixture < NorthwindRelationalContext >
        {
            protected override NorthwindRelationalContext Factory ( DbContextOptions < NorthwindRelationalContext > options ) => new NorthwindRelationalContext ( options );
            protected override void                       Seed    ( NorthwindRelationalContext db )                           => NorthwindData.Seed ( db );
        }
    }
}