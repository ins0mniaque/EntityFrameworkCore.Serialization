using System;

using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace EntityFrameworkCore.Serialization.Tests
{
    public abstract class SqliteDbContextFixture < TDbContext > : IDisposable where TDbContext : DbContext
    {
        private readonly SqliteConnection disconnected;
        private readonly SqliteConnection connection;

        public SqliteDbContextFixture ( )
        {
            disconnected = new SqliteConnection ( );

            connection = new SqliteConnection ( "DataSource=:memory:" );
            connection.Open ( );

            using var dbContext = CreateDbContext ( );

            dbContext.Database.EnsureCreated ( );

            #pragma warning disable CA2214 // Do not call overridable methods in constructors
            Seed ( dbContext );
            #pragma warning restore CA2214 // Do not call overridable methods in constructors
        }

        public TDbContext CreateDbContext             ( ) => Factory ( GetDbContextOptions ( connection   ) );
        public TDbContext CreateDisconnectedDbContext ( ) => Factory ( GetDbContextOptions ( disconnected ) );

        private static DbContextOptions < TDbContext > GetDbContextOptions ( SqliteConnection connection )
        {
            return new DbContextOptionsBuilder < TDbContext > ( ).UseSqlite ( connection )
                                                                 .Options;
        }

        protected abstract TDbContext Factory ( DbContextOptions < TDbContext > options );
        protected abstract void       Seed    ( TDbContext db );

        protected virtual void Dispose ( bool disposing )
        {
            if ( disposing )
            {
                disconnected.Dispose ( );
                connection  .Dispose ( );
            }
        }

        public void Dispose ( )
        {
            Dispose ( true );
            GC.SuppressFinalize ( this );
        }
    }
}