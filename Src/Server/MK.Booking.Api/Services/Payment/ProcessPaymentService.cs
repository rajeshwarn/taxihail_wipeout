using System;
using apcurium.MK.Booking.Api.Contract.Requests.Payment;
using apcurium.MK.Booking.Api.Contract.Requests.Payment.PayPal;
using apcurium.MK.Booking.IBS;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Booking.Services;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Resources;
using ServiceStack.ServiceInterface;

namespace apcurium.MK.Booking.Api.Services.Payment
{
    public class ProcessPaymentService : Service
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
            var session = this.GetSession();

            return _payPalServiceFactory.GetInstance().LinkAccount(new Guid(session.UserAuthId), request.AuthCode);
        }

        public BasePaymentResponse Post(UnlinkPayPalAccountRequest request)
        {
            var session = this.GetSession();

            return _payPalServiceFactory.GetInstance().UnlinkAccount(new Guid(session.UserAuthId));
        }

        public DeleteTokenizedCreditcardResponse Delete(DeleteTokenizedCreditcardRequest request)
        {
            return _paymentService.DeleteTokenizedCreditcard(request.CardToken);
        }

        public PairingResponse Post(PairingForPaymentRequest request)
        {
            var order = _orderDao.FindById(request.OrderId);

            var response = _paymentService.Pair(request.OrderId, request.CardToken, request.AutoTipPercentage);

            if (response.IsSuccessful)
            {
                var ibsAccountId = _accountDao.GetIbsAccountId(order.AccountId, null);
                if (!UpdateOrderPaymentType(ibsAccountId.Value, order.IBSOrderId.Value))
                {
                    response.IsSuccessful = false;
                    _paymentService.VoidPreAuthorization(request.OrderId);
                }
            }
            return response;
        }

        private bool UpdateOrderPaymentType(int ibsAccountId, int ibsOrderId, string companyKey = null)
        {
            return _ibsServiceProvider.Booking(companyKey).UpdateOrderPaymentType(ibsAccountId, ibsOrderId, _serverSettings.ServerData.IBS.PaymentTypeCardOnFileId);
        }

        public BasePaymentResponse Post(UnpairingForPaymentRequest request)
        {
            return _paymentService.Unpair(request.OrderId);
        }
    }
}