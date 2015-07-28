namespace apcurium.MK.Booking.SMS
{
    public interface ISmsService
    {
        void Send(libphonenumber.PhoneNumber toNumber, string message);
    }
}
