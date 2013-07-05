using System;
using System.Data.Entity;
using System.Linq;
using apcurium.MK.Common.Extensions;

namespace apcurium.MK.Common.Configuration.Impl
{
    public class ConfigurationDbContext : DbContext
    {
        public const string SchemaName = "Config";

        public ConfigurationDbContext(string nameOrConnectionString)
            : base(nameOrConnectionString)
        {
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Make the name of the views match exactly the name of the corresponding property.
            modelBuilder.Entity<AppSetting>().ToTable("AppSettings", SchemaName);

            modelBuilder.CreateTable<ServerPaymentSettings>(SchemaName);
            modelBuilder.CreateTable<PayPalSettings>(SchemaName);
        }

        public T Find<T>(Guid id) where T : class
        {
            return this.Set<T>().Find(id);
        }

        public IQueryable<T> Query<T>() where T : class
        {
            return Set<T>();
        }

        public void Save<T>(T entity) where T : class
        {
            var entry = this.Entry(entity);

            if (entry.State == System.Data.Entity.EntityState.Detached)
                this.Set<T>().Add(entity);

            this.SaveChanges();
        }
    }
}