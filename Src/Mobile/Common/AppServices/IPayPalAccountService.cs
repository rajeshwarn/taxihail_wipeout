using System;
using System.Threading.Tasks;

namespace apcurium.MK.Booking.Mobile.AppServices
{
    public interface IPayPalAccountService
    {
        Task LinkAccount(Guid accoundId, string authCode);

        Task UnLinkAccount(Guid accoundId);
    }
}