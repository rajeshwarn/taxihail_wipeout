using apcurium.MK.Booking.Api.Contract.Requests.Payment;
using apcurium.MK.Booking.Api.Contract.Resources.Payments;
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

        public CommitPreauthorizedPaymentResponse Post(PreAuthorizeAndCommitPaymentRequest request)
        {
            return _paymentService.PreAuthorizeAndCommitPayment(request);
        }
    }
}