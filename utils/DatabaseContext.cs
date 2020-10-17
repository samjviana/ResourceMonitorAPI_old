using System;
using System.Collections;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using Npgsql;
using ResourceMonitorAPI.models;

namespace ResourceMonitorAPI.utils {
    class DatabaseContext : DbContext {
        public DbSet<Computador> computadores { get; set; }
        public DbSet<CPU> cpus { get; set; }
        public DbSet<GPU> gpus { get; set; }
        public DbSet<RAM> rams { get; set; }
        public DbSet<Armazenamento> armazenamentos { get; set; }

        public DatabaseContext() : base("ResourceMonitorDB") {
            Database.SetInitializer<DatabaseContext>(new DropCreateDatabaseIfModelChanges<DatabaseContext>());
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder) {
            modelBuilder.HasDefaultSchema("public");

            base.OnModelCreating(modelBuilder);
        }

        public override int SaveChanges() {
            var entries = ChangeTracker.Entries().Where(
                (entry) => {
                    if (entry.Entity is Model && (entry.State == EntityState.Added || entry.State == EntityState.Modified)) {
                        return true;
                    }
                    return false;
                }
            );

            foreach(var entry in entries) {
                if (entry.State == EntityState.Added) {
                    ((Model)entry.Entity).dataCriacao = DateTime.Now;
                }

                ((Model)entry.Entity).dataUpdate = DateTime.Now;
            }

            return base.SaveChanges();
        }
    }

    class NpgSqlConfiguration : DbConfiguration {
        public NpgSqlConfiguration() {
            var name = "Npgsql";

            SetProviderFactory(
                providerInvariantName: name,
                providerFactory: NpgsqlFactory.Instance
            );

            SetProviderServices(
                providerInvariantName: name,
                provider: NpgsqlServices.Instance
            );

            SetDefaultConnectionFactory(
                connectionFactory: new NpgsqlConnectionFactory()
            );
        }
    }
}