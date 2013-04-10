using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;

namespace apcurium.MK.Common.Caching
{
    public class CachingDbContext: DbContext
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
            return this.Set<CacheItem>().Find(key);
        }

        public void Save(CacheItem entity)
        {
            var entry = this.Entry(entity);

            if (entry.State == System.Data.Entity.EntityState.Detached)
                this.Set<CacheItem>().Add(entity);

            this.SaveChanges();
        }
    }
}
