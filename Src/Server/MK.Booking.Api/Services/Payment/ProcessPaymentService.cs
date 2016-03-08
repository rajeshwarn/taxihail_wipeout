using System.Threading.Tasks;
using apcurium.MK.Booking.Api.Contract.Requests.Payment;
using apcurium.MK.Booking.Api.Contract.Requests.Payment.PayPal;
using apcurium.MK.Booking.IBS;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Booking.Services;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Resources;

namespace apcurium.MK.Booking.Api.Services.Payment
{
    public class ProcessPaymentService : BaseApiService
    {
        private readonly IPayPalServiceFactory _payPalServiceFactory;
        private readonly IPaymentService _paymentService;
        private readonly IAccountDao _accountDao;
        private readonly IIBSServiceProvider _ibsServiceProvider;
        private readonly IOrderDao _orderDao;
        private readonly IServerSettings _serverSettings;

        public ProcessPaymentService(
            IPayPalServiceFactory payPalServiceFactory,
            IPaymentService paymentService,
            IAccountDao accountDao, 
            IOrderDao orderDao,
            IIBSServiceProvider ibsServiceProvider,
            IServerSettings serverSettings)
        {
            _payPalServiceFactory = payPalServiceFactory;
            _paymentService = paymentService;
            _accountDao = accountDao;
            _orderDao = orderDao;
            _ibsServiceProvider = ibsServiceProvider;
            _serverSettings = serverSettings;
        }

        public BasePaymentResponse Post(LinkPayPalAccountRequest request)
        {
            return _payPalServiceFactory.GetInstance().LinkAccount(Session.UserId, request.AuthCode);
        }

        public BasePaymentResponse Post(UnlinkPayPalAccountRequest request)
        {
            return _payPalServiceFactory.GetInstance().UnlinkAccount(Session.UserId);
        }

        public DeleteTokenizedCreditcardResponse Delete(string cardToken)
        {
            return _paymentService.DeleteTokenizedCreditcard(cardToken);
        }

        public async Task<BasePaymentResponse> Post(UnpairingForPaymentRequest request)
        {
            var order = _orderDao.FindById(request.OrderId);
            var ibsAccountId = _accountDao.GetIbsAccountId(order.AccountId, null);

            if (ibsAccountId.HasValue && order.IBSOrderId.HasValue &&  UpdateIBSOrderPaymentType(ibsAccountId.Value, order.IBSOrderId.Value))
            {
                var response = await _paymentService.Unpair(order.CompanyKey, request.OrderId);
                if (response.IsSuccessful)
                {
                    _paymentService.VoidPreAuthorization(order.CompanyKey, request.OrderId);
                }
                else
                {
                    response.IsSuccessful = false;
                }
                return response;
            }
            return new BasePaymentResponse { IsSuccessful = false };
        }

        private bool UpdateIBSOrderPaymentType(int ibsAccountId, int ibsOrderId, string companyKey = null)
        {
            // Change payment type to Pay in Car            
            return _ibsServiceProvider.Booking(companyKey).UpdateOrderPaymentType(ibsAccountId, ibsOrderId, _serverSettings.ServerData.IBS.PaymentTypePaymentInCarId);
        }
    }
}