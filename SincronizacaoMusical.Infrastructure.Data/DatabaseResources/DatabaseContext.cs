using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SincronizacaoMusical.Domain.Entities;

namespace SincronizacaoMusical.Infrastructure.Data.DatabaseResources
{
    internal class DatabaseContext : DbContext
    {
        // Entities to map
        //public DbSet<User> Users { get; set; }
        public DbSet<Log> Logs { get; set; }

        public DatabaseContext()
        {
            Database.SetInitializer(new DropCreateDatabaseIfModelChanges<DatabaseContext>());
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            // Table names
            //modelBuilder.Entity<User>().ToTable("Users");
            modelBuilder.Entity<Log>().ToTable("EventLog");

            // Complex types
            //modelBuilder.ComplexType<Address>();

            base.OnModelCreating(modelBuilder);
        }
    }
}
