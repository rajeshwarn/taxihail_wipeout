using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Entity;
using apcurium.MK.Booking.ReadModel;
using apcurium.MK.Common.Entity;

namespace apcurium.MK.Booking.Database
{
    
    public class BookingDbContext : DbContext
    {
        public const string SchemaName = "Booking";

        public BookingDbContext(string nameOrConnectionString)
            : base(nameOrConnectionString)
        {
        }

        
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Make the name of the views match exactly the name of the corresponding property.
            modelBuilder.ComplexType<Address>(); //doing here because address is shared among several projects, layers
            modelBuilder.Entity<AccountDetail>().ToTable("AccountDetail", SchemaName);
            modelBuilder.Entity<AddressDetails>().ToTable("AddressDetails", SchemaName);
            modelBuilder.Entity<OrderDetail>().ToTable("OrderDetail", SchemaName);
            modelBuilder.Entity<RateDetail>().ToTable("RateDetail", SchemaName);
            modelBuilder.Entity<DefaultAddressDetails>().ToTable("DefaultAddressDetails", SchemaName);
        }

        public T Find<T>(Guid id) where T : class
        {
            return this.Set<T>().Find(id);
        }

        public IQueryable<T> Query<T>() where T : class
        {
            return this.Set<T>();
        }

        public void Save<T>(T entity) where T : class
        {
            var entry = this.Entry(entity);

            if (entry.State == System.Data.EntityState.Detached)
                this.Set<T>().Add(entity);

            this.SaveChanges();
        }
    }
}
