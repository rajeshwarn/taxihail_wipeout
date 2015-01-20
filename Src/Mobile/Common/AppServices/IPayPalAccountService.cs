using System.Threading.Tasks;

namespace apcurium.MK.Booking.Mobile.AppServices
{
    public interface IPayPalAccountService
    {
        Task LinkAccount(string authCode);

        Task UnLinkAccount();
    }
}