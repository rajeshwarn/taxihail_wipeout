using apcurium.MK.Booking.Api.Contract.Requests.Payment;
using apcurium.MK.Booking.IBS;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Booking.Services;
using apcurium.MK.Common.Resources;
using ServiceStack.ServiceInterface;

namespace apcurium.MK.Booking.Api.Services.Payment
{
    public class ProcessPaymentService : Service
    {
        private readonly IPaymentService _paymentService;
        private readonly IIBSServiceProvider _ibsServiceProvider;
        private readonly IOrderDao _orderDao;
        private readonly IAccountDao _accountDao;

        public ProcessPaymentService(IPaymentService paymentService,
            IIBSServiceProvider ibsServiceProvider,
            IOrderDao orderDao, IAccountDao accountDao)
        {
            _accountDao = accountDao;
            _orderDao = orderDao;
            _paymentService = paymentService;
            _ibsServiceProvider = ibsServiceProvider;
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
            var response =  _paymentService.Pair(request.OrderId, request.CardToken, request.AutoTipPercentage, request.AutoTipAmount);
            if ( response.IsSuccessful )
            {
                var o = _orderDao.FindById( request.OrderId );
                var a = _accountDao.FindById( o.AccountId );
                if (!UpdateOrderPaymentType(a.IBSAccountId.Value, o.IBSOrderId.Value, 7))
                {
                    response.IsSuccessful = false;
                    _paymentService.VoidPreAuthorization(request.OrderId);
                }
            }
            return response;

        }

        private bool UpdateOrderPaymentType(int ibsAccountId, int ibsOrderId, int chargeTypeId, string companyKey = null)
        {
            var result = _ibsServiceProvider.Booking(companyKey).UpdateOrderPaymentType( ibsAccountId, ibsOrderId, chargeTypeId );
            return result;
        }

        public BasePaymentResponse Post(UnpairingForPaymentRequest request)
        {
            return _paymentService.Unpair(request.OrderId);
        }
    }
}