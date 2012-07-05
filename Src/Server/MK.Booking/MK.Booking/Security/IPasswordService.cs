namespace apcurium.MK.Booking.Security
{
    public interface IPasswordService
    {
        byte[] EncodePassword(string password, string salt);
        string GeneratePassword(string password);
        bool IsValid(string passwordSubmitted, string salt, byte[] password);
    }
}