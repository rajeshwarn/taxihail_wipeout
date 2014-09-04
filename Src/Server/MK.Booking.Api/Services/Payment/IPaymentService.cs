using apcurium.MK.Booking.Api.Contract.Requests.Payment;
using apcurium.MK.Booking.Api.Contract.Resources.Payments;

namespace apcurium.MK.Booking.Api.Services.Payment
{
    public interface IPaymentService
    {
        CommitPreauthorizedPaymentResponse PreAuthorizeAndCommitPayment(PreAuthorizeAndCommitPaymentRequest request);
        DeleteTokenizedCreditcardResponse DeleteTokenizedCreditcard(DeleteTokenizedCreditcardRequest request);
        PairingResponse Pair(PairingForPaymentRequest request);
        BasePaymentResponse Unpair(UnpairingForPaymentRequest request);
    }
}