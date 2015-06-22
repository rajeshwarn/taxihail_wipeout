using System;
using System.Threading.Tasks;
using apcurium.MK.Common.Resources;

namespace apcurium.MK.Booking.Api.Client
{
    public interface IPairingServiceClient
    {
        Task<BasePaymentResponse> Unpair(Guid orderId);

        Task<bool> UpdateAutoTip(Guid orderId, int autoTipPercentage);
    }
}