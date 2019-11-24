using System;

using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace EntityFrameworkCore.Serialization.Tests
{
    public abstract class SqliteDbContextFixture < TDbContext > : IDisposable where TDbContext : DbContext
    {
        private readonly SqliteConnection connection;

        public SqliteDbContextFixture ( )
        {
            connection = new SqliteConnection ( "DataSource=:memory:" );
            connection.Open ( );

            using var dbContext = CreateDbContext ( );

            dbContext.Database.EnsureCreated ( );

            Seed ( dbContext );
        }

        public TDbContext CreateDbContext             ( ) => Factory ( GetDbContextOptions ( connection ) );
        public TDbContext CreateDisconnectedDbContext ( ) => Factory ( GetDbContextOptions ( new SqliteConnection ( ) ) );

        private DbContextOptions < TDbContext > GetDbContextOptions ( SqliteConnection connection )
        {
            return new DbContextOptionsBuilder < TDbContext > ( ).UseSqlite ( connection )
                                                                 .Options;
        }

        protected abstract TDbContext Factory ( DbContextOptions < TDbContext > options );
        protected abstract void       Seed    ( TDbContext db );

        public void Dispose ( ) => connection.Dispose ( );
    }
}