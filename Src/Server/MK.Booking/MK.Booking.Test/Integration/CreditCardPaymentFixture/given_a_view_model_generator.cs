using apcurium.MK.Booking.Database;
using apcurium.MK.Booking.EventHandlers;

namespace apcurium.MK.Booking.Test.Integration.CreditCardPaymentFixture
{
    public class given_a_view_model_generator: given_a_read_model_database
    {
        protected CreditCardPaymentDetailsGenerator Sut;

        public given_a_view_model_generator()
        {
            Sut = new CreditCardPaymentDetailsGenerator(() => new BookingDbContext(dbName));
        }
    }
}
