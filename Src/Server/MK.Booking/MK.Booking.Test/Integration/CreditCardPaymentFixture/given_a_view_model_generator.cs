#region

using apcurium.MK.Booking.Database;
using apcurium.MK.Booking.EventHandlers;
using apcurium.MK.Booking.Projections;
using apcurium.MK.Booking.ReadModel;
using apcurium.MK.Common.Entity;

#endregion

namespace apcurium.MK.Booking.Test.Integration.CreditCardPaymentFixture
{
// ReSharper disable once InconsistentNaming
    public class given_a_view_model_generator : given_a_read_model_database
    {
        protected CreditCardPaymentDetailsGenerator Sut;

        public given_a_view_model_generator()
        {
            Sut = new CreditCardPaymentDetailsGenerator(() => new BookingDbContext(DbName),
                new EntityProjectionSet<OrderDetail>(() => new BookingDbContext(DbName)), 
                new EntityProjectionSet<OrderStatusDetail>(() => new BookingDbContext(DbName)), 
                new TestServerSettings());
        }
    }
}