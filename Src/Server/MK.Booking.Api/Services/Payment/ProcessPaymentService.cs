using System;
using System.Net;
using System.Threading;
using System.Web;
using apcurium.MK.Booking.Api.Contract.Requests.Payment;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Booking.Services;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Resources;
using ServiceStack.Common.Web;
using ServiceStack.ServiceInterface;

namespace apcurium.MK.Booking.Api.Services.Payment
{
    public class ProcessPaymentService : Service
    {
        private readonly IPaymentService _paymentService;
        private readonly IAccountDao _accountDao;
        private readonly IOrderDao _orderDao;
        private readonly IServerSettings _serverSettings;

        public ProcessPaymentService(IPaymentService paymentService, IAccountDao accountDao, IOrderDao orderDao, IServerSettings serverSettings)
        {
            _paymentService = paymentService;
            _accountDao = accountDao;
            _orderDao = orderDao;
            _serverSettings = serverSettings;
        }

        public CommitPreauthorizedPaymentResponse Post(CommitPaymentRequest request)
        {
        	// TODO RedeemPromotion
        
            if (!_serverSettings.GetPaymentSettings().IsPreAuthEnabled)
            {
                // PreAutorization was not done on create order, so we do it here before processing the payment

                var orderDetail = _orderDao.FindById(request.OrderId);
                if (orderDetail == null)
                {
                    throw new HttpError(HttpStatusCode.NotFound, "Order not found");
                }

                var account = _accountDao.FindById(orderDetail.AccountId);

                var preAuthResponse = _paymentService.PreAuthorize(request.OrderId, account.Email, request.CardToken, request.Amount);
                if (!preAuthResponse.IsSuccessful)
                {
                    return new CommitPreauthorizedPaymentResponse
                    {
                        IsSuccessful = false,
                        Message = string.Format("PreAuthorization Failed: {0}", preAuthResponse.Message)
                    };
                }

                // Wait for OrderPaymentDetail to be created
                Thread.Sleep(500);
            }

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