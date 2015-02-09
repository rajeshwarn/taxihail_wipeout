using System;
using System.Threading.Tasks;
using apcurium.MK.Common.Resources;

namespace apcurium.MK.Booking.Api.Client
{
    public interface IPairingServiceClient
    {
        Task<PairingResponse> Pair(Guid orderId, string cardToken, int? autoTipPercentage);
        Task<BasePaymentResponse> Unpair(Guid orderId);
    }
}