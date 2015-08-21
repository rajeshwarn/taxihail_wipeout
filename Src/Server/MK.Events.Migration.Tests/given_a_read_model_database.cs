using System;
using System.Reflection;
using apcurium.MK.Booking.Database;

namespace MK.Events.Migration.Tests
{
    // ReSharper disable once InconsistentNaming
    public class given_a_read_model_database : IDisposable
    {
        protected string DbName;

        static given_a_read_model_database()
        {
           
        }

        public given_a_read_model_database()
        {
            DbName = GetType().Name + "-" + Guid.NewGuid();
            using (var context = new BookingDbContext(DbName))
            {
                if (context.Database.Exists())
                    context.Database.Delete();

                context.Database.Create();
            }
        }

        public void Dispose()
        {
            using (var context = new BookingDbContext(DbName))
            {
                if (context.Database.Exists())
                    context.Database.Delete();
            }
        }
    }
}