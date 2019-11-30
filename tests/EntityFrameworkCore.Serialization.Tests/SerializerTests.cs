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
            var writer  = new Serializable.EntityEntryWriter ( new List < Serializable.SerializableEntry > ( ) );

            Assert.Throws < ArgumentNullException > ( ( ) => context!.Serialize ( writer ) );
        }

        [ Fact ]
        public void SerializeThrowsArgumentNullExceptionOnNullReader ( )
        {
            var context = fixture.CreateDisconnectedDbContext ( );
            var writer  = (IEntityEntryWriter?) null;

            Assert.Throws < ArgumentNullException > ( ( ) => context.Serialize ( writer! ) );
        }

        [ Fact ]
        public void CanSerialize ( )
        {
            var serializer = new Serializable.DbContextSerializer ( );

            using var context = fixture.CreateDbContext ( );

            var customers = context.Customers.ToList ( );
            var count     = context.ChangeTracker.Entries ( ).Count ( );

            context.Serialize ( serializer, out var data );

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
            var serializer = new Serializable.DbContextSerializer ( );

            using var context = fixture.CreateDbContext ( );

            var customers = context.Customers
                                   .Include ( customer => customer.Orders )
                                   .ToList ( );

            customers.First ( ).Orders.RemoveAt ( 0 );

            context.SerializeChanges ( serializer, out var serializedContext );

            context.Deserialize ( serializer, serializedContext );

            context.SerializeChanges ( serializer, out var reserializedContext );

            Assert.Single ( reserializedContext );
            Assert.Equal  ( EntityState.Modified, reserializedContext [ 0 ].EntityState );

            using var emptyContext = fixture.CreateDbContext ( );

            emptyContext.Deserialize ( serializer, serializedContext );

            emptyContext.Serialize ( serializer, out var serializedEmptyContext );

            Assert.Single ( serializedEmptyContext );
            Assert.Equal  ( EntityState.Modified, serializedEmptyContext [ 0 ].EntityState );

            using var nonEmptyContext = fixture.CreateDbContext ( );

            customers = nonEmptyContext.Customers
                                       .Include ( customer => customer.Orders )
                                       .ToList  ( );

            nonEmptyContext.Deserialize ( serializer, serializedContext );

            nonEmptyContext.SerializeChanges ( serializer, out var serializedNonEmptyContext );

            Assert.Single ( serializedNonEmptyContext );
            Assert.Equal  ( EntityState.Modified, serializedNonEmptyContext [ 0 ].EntityState );
        }

        [ Fact ]
        public void DeserializeLoadsNavigationProperties ( )
        {
            var serializer = new Serializable.DbContextSerializer ( );

            using var context = fixture.CreateDbContext ( );

            var customers = context.Customers
                                   .Include ( customer => customer.Orders )
                                   .ToList ( );

            context.Serialize ( serializer, out var serializedContext );

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
            var serializer = new Serializable.DbContextSerializer ( );

            using var fixture = new Fixture ( );

            using var serverContext = fixture.CreateDbContext ( );

            var customer = serverContext.Customers.Include ( c => c.Orders ).First ( );

            serverContext.SerializeGraph ( serializer, out var graph, customer );

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

            clientContext.SerializeChanges ( serializer, out var changes );

            serverContext.Deserialize ( serializer, changes );

            Assert.Equal ( serverContext.ChangeTracker.Entries ( ).OrderBy ( entry => entry, EntityEntryComparer.Instance ),
                           clientContext.ChangeTracker.Entries ( ).OrderBy ( entry => entry, EntityEntryComparer.Instance ),
                           EntityEntryComparer.Instance );

            serverContext.SaveChanges ( serializer, out var databaseGeneratedValues );

            clientContext.AcceptChanges ( serializer, databaseGeneratedValues );

            Assert.All ( serverContext.ChangeTracker.Entries ( ),
                         entry => Assert.True ( entry.State == EntityState.Unchanged ) );

            Assert.All ( clientContext.ChangeTracker.Entries ( ),
                         entry => Assert.True ( entry.State == EntityState.Unchanged ) );

            Assert.Equal ( serverContext.ChangeTracker.Entries ( ).OrderBy ( entry => entry, EntityEntryComparer.Instance ),
                           clientContext.ChangeTracker.Entries ( ).OrderBy ( entry => entry, EntityEntryComparer.Instance ),
                           EntityEntryComparer.Instance );
        }

        [ Fact ]
        public void CanCompleteBinaryServerClientWorkflow ( )
        {
            var serializer = new Binary.BinaryDbContextSerializer ( );

            using var fixture = new Fixture ( );

            using var serverContext = fixture.CreateDbContext ( );

            var customer = serverContext.Customers.Include ( c => c.Orders ).First ( );

            serverContext.SerializeGraph ( serializer, out var graph, customer );

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

            clientContext.SerializeChanges ( serializer, out var changes );

            serverContext.Deserialize ( serializer, changes );

            Assert.Equal ( serverContext.ChangeTracker.Entries ( ).OrderBy ( entry => entry, EntityEntryComparer.Instance ),
                           clientContext.ChangeTracker.Entries ( ).OrderBy ( entry => entry, EntityEntryComparer.Instance ),
                           EntityEntryComparer.Instance );

            serverContext.SaveChanges ( serializer, out var databaseGeneratedValues );

            clientContext.AcceptChanges ( serializer, databaseGeneratedValues );

            Assert.All ( serverContext.ChangeTracker.Entries ( ),
                         entry => Assert.True ( entry.State == EntityState.Unchanged ) );

            Assert.All ( clientContext.ChangeTracker.Entries ( ),
                         entry => Assert.True ( entry.State == EntityState.Unchanged ) );

            Assert.Equal ( serverContext.ChangeTracker.Entries ( ).OrderBy ( entry => entry, EntityEntryComparer.Instance ),
                           clientContext.ChangeTracker.Entries ( ).OrderBy ( entry => entry, EntityEntryComparer.Instance ),
                           EntityEntryComparer.Instance );
        }

        [ Fact ]
        public void CanSerializeNorthwindDatabase ( )
        {
            var serializer = new Serializable.DbContextSerializer ( );

            using var serverContext = fixture.CreateDbContext ( );

            var orders = serverContext.Customers
                                      .Include     ( customer => customer.Orders )
                                      .ThenInclude ( order => order.OrderDetails )
                                      .ThenInclude ( orderDetail => orderDetail.Product )
                                      .ToList ( );

            var employees = serverContext.Employees.ToList ( );

            serverContext.Serialize ( serializer, out var serializedContext );

            using var clientContext = fixture.CreateDisconnectedDbContext ( );

            clientContext.Deserialize ( serializer, serializedContext );

            Assert.Equal ( serverContext.ChangeTracker.Entries ( ).OrderBy ( entry => entry, EntityEntryComparer.Instance ),
                           clientContext.ChangeTracker.Entries ( ).OrderBy ( entry => entry, EntityEntryComparer.Instance ),
                           EntityEntryComparer.Instance );
        }

        [ Fact ]
        public void CanBinarySerializeNorthwindDatabase ( )
        {
            var serializer = new Binary.BinaryDbContextSerializer ( );

            using var serverContext = fixture.CreateDbContext ( );

            var orders = serverContext.Customers
                                      .Include     ( customer => customer.Orders )
                                      .ThenInclude ( order => order.OrderDetails )
                                      .ThenInclude ( orderDetail => orderDetail.Product )
                                      .ToList ( );

            var employees = serverContext.Employees.ToList ( );

            serverContext.Serialize ( serializer, out var serializedContext );

            Assert.Equal ( 102793, serializedContext.Length );

            using var clientContext = fixture.CreateDisconnectedDbContext ( );

            clientContext.Deserialize ( serializer, serializedContext );

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