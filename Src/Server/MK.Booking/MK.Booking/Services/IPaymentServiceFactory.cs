namespace apcurium.MK.Booking.Services
{
    public interface IPaymentServiceFactory
    {
        IPaymentService GetInstance();
    }
}