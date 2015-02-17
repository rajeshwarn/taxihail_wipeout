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

        // DELETE?
        public PairingResponse Post(PairingForPaymentRequest request)
        {
            var order = _orderDao.FindById(request.OrderId);

            var response = _paymentService.Pair(request.OrderId, request.CardToken, request.AutoTipPercentage);

            if (response.IsSuccessful)
            {
                var ibsAccountId = _accountDao.GetIbsAccountId(order.AccountId, null);
                if (!UpdateOrderPaymentType(ibsAccountId.Value, order.IBSOrderId.Value, ChargeTypes.CardOnFile.Id))
                {
                    response.IsSuccessful = false;
                    _paymentService.VoidPreAuthorization(request.OrderId);
                }
            }
            return response;
        }

        public BasePaymentResponse Post(UnpairingForPaymentRequest request)
        {
            var order = _orderDao.FindById(request.OrderId);
            var response = _paymentService.Unpair(request.OrderId);

            if (response.IsSuccessful)
            {
                var ibsAccountId = _accountDao.GetIbsAccountId(order.AccountId, null);
                if (UpdateOrderPaymentType(ibsAccountId.Value, order.IBSOrderId.Value, ChargeTypes.PaymentInCar.Id))
                {
                    _paymentService.VoidPreAuthorization(request.OrderId);
                }
                else
                {
                    response.IsSuccessful = false;
                }
            }
            return response;
        }

        // TODO: refactor if PairingForPaymentRequest is removed
        private bool UpdateOrderPaymentType(int ibsAccountId, int ibsOrderId, int? chargeTypeId, string companyKey = null)
        {
            int? ibsChargeType = null;
            if (chargeTypeId == ChargeTypes.CardOnFile.Id)
            {
                ibsChargeType = _serverSettings.ServerData.IBS.PaymentTypeCardOnFileId;
            }
            else if (chargeTypeId == ChargeTypes.PaymentInCar.Id)
            {
                ibsChargeType = _serverSettings.ServerData.IBS.PaymentTypePaymentInCarId;
            }

            return _ibsServiceProvider.Booking(companyKey).UpdateOrderPaymentType(ibsAccountId, ibsOrderId, ibsChargeType);
        }
    }
}