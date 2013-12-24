#region

using System.Data.Entity;
using apcurium.MK.Common.Database;

#endregion

namespace apcurium.MK.Common.Caching
{
    [DbConfigurationType(typeof(CustomDbConfiguration))]
    public class CachingDbContext : DbContext
    {
        public const string SchemaName = "Cache";

        public CachingDbContext(string nameOrConnectionString)
            : base(nameOrConnectionString)
        {
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Make the name of the views match exactly the name of the corresponding property.
            modelBuilder.Entity<CacheItem>().ToTable("Items", SchemaName);
        }

        public CacheItem Find(string key)
        {
            return Set<CacheItem>().Find(key);
        }

        public void Save(CacheItem entity)
        {
            var entry = Entry(entity);

            if (entry.State == EntityState.Detached)
                Set<CacheItem>().Add(entity);

            SaveChanges();
        }
    }
}