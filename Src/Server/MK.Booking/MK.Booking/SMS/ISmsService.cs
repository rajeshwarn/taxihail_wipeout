namespace apcurium.MK.Booking.SMS
{
    public interface ISmsService
    {
        void Send(string toNumber, string message);
    }
}
