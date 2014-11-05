using apcurium.MK.Booking.Api.Contract.Requests.Payment;
using apcurium.MK.Booking.Services;
using apcurium.MK.Common.Resources;
using ServiceStack.ServiceInterface;

namespace apcurium.MK.Booking.Api.Services.Payment
{
    public class ProcessPaymentService : Service
    {
        private readonly IPaymentService _paymentService;

        public ProcessPaymentService(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        public CommitPreauthorizedPaymentResponse Post(CommitPaymentRequest request)
        {
            return _paymentService.CommitPayment(request.Amount, request.MeterAmount, request.TipAmount, request.CardToken, request.OrderId, request.IsNoShowFee);
        }

        public DeleteTokenizedCreditcardResponse Delete(DeleteTokenizedCreditcardRequest request)
        {
            return _paymentService.DeleteTokenizedCreditcard(request.CardToken);
        }

        public PairingResponse Post(PairingForPaymentRequest request)
        {
            return _paymentService.Pair(request.OrderId, request.CardToken, request.AutoTipPercentage, request.AutoTipAmount);
        }

        public BasePaymentResponse Post(UnpairingForPaymentRequest request)
        {
            return _paymentService.Unpair(request.OrderId);
        }
    }
}