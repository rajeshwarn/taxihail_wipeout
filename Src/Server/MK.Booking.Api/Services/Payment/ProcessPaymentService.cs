using System;
using apcurium.MK.Booking.Api.Contract.Requests.Payment;
using apcurium.MK.Booking.Api.Contract.Requests.Payment.PayPal;
using apcurium.MK.Booking.IBS;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Booking.Services;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Configuration.Impl;
using apcurium.MK.Common.Enumeration;
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

        public BasePaymentResponse Post(UnpairingForPaymentRequest request)
        {
            var order = _orderDao.FindById(request.OrderId);
            var ibsAccountId = _accountDao.GetIbsAccountId(order.AccountId, null, order.Settings.ServiceType);

            if (UpdateIBSOrderPaymentType(ibsAccountId.Value, order.IBSOrderId.Value, order.Settings.ServiceType))
            {
                var response = _paymentService.Unpair(order.CompanyKey, request.OrderId);
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

        private bool UpdateIBSOrderPaymentType(int ibsAccountId, int ibsOrderId, ServiceType serviceType, string companyKey = null)
        {
            // Change payment type to Pay in Car            
            return _ibsServiceProvider.Booking(companyKey, serviceType).UpdateOrderPaymentType(ibsAccountId, ibsOrderId, _serverSettings.ServerData.IBS.PaymentTypePaymentInCarId);
        }
    }
}