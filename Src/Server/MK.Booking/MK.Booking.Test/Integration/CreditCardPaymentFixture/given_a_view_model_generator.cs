#region

using apcurium.MK.Booking.Database;
using apcurium.MK.Booking.EventHandlers;

#endregion

namespace apcurium.MK.Booking.Test.Integration.CreditCardPaymentFixture
{
// ReSharper disable once InconsistentNaming
    public class given_a_view_model_generator : given_a_read_model_database
    {
        protected CreditCardPaymentDetailsGenerator Sut;

        public given_a_view_model_generator()
        {
            Sut = new CreditCardPaymentDetailsGenerator(() => new BookingDbContext(DbName));
        }
    }
}