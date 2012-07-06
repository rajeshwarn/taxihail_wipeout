namespace apcurium.MK.Booking.Security
{
    public interface IPasswordService
    {
        byte[] EncodePassword(string password, string salt);
        string GeneratePassword();
        bool IsValid(string passwordSubmitted, string salt, byte[] password);
    }
}