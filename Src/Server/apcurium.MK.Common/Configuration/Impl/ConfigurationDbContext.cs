using System;
using System.Data.Entity;
using System.Linq;
using apcurium.MK.Common.Database;

namespace apcurium.MK.Common.Configuration.Impl
{
    [DbConfigurationType(typeof(CustomDbConfiguration))]
    public class ConfigurationDbContext : DbContext
    {
        public const string SchemaName = "Config";

        public ConfigurationDbContext()
        {
            
        }

        public ConfigurationDbContext(string nameOrConnectionString)
            : base(nameOrConnectionString)
        {
        }

        public DbSet<ServerPaymentSettings> ServerPaymentSettings { get; set; }
        public DbSet<NotificationSettings> NotificationSettings { get; set; }
        public DbSet<UserTaxiHailNetworkSettings> UserTaxiHailNetworkSettings { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            // Make the name of the views match exactly the name of the corresponding property.
            modelBuilder.Entity<AppSetting>().ToTable("AppSettings", SchemaName);
            modelBuilder.ComplexType<PayPalServerSettings>();
            modelBuilder.ComplexType<PayPalClientSettings>();
            modelBuilder.Entity<ServerPaymentSettings>().ToTable("PaymentSettings", SchemaName);
            modelBuilder.Entity<NotificationSettings>().ToTable("NotificationSettings", SchemaName);
            modelBuilder.Entity<UserTaxiHailNetworkSettings>().ToTable("UserTaxiHailNetworkSettings", SchemaName);

            base.OnModelCreating(modelBuilder);
        }

        public T Find<T>(Guid id) where T : class
        {
            return Set<T>().Find(id);
        }

        public virtual IQueryable<T> Query<T>() where T : class
        {
            return Set<T>();
        }

        public void Save<T>(T entity) where T : class
        {
            var entry = Entry(entity);

            if (entry.State == EntityState.Detached)
                Set<T>().Add(entity);

            SaveChanges();
        }
    }
}