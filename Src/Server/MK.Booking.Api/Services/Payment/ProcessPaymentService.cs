using System;
using System.Net;
using System.Threading;
using System.Web;
using apcurium.MK.Booking.Api.Contract.Requests.Payment;
using apcurium.MK.Booking.Domain;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Booking.Services;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Resources;
using Infrastructure.EventSourcing;
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
        private readonly IPromotionDao _promotionDao;
        private readonly IEventSourcedRepository<Promotion> _promoRepository;

        public ProcessPaymentService(
            IPaymentService paymentService, 
            IAccountDao accountDao, 
            IOrderDao orderDao, 
            IServerSettings serverSettings, 
            IPromotionDao promotionDao,
            IEventSourcedRepository<Promotion> promoRepository)
        {
            _paymentService = paymentService;
            _accountDao = accountDao;
            _orderDao = orderDao;
            _serverSettings = serverSettings;
            _promotionDao = promotionDao;
            _promoRepository = promoRepository;
        }

        public CommitPreauthorizedPaymentResponse Post(CommitPaymentRequest request)
        {
            var totalAmount = request.Amount;

//            // TODO RedeemPromotion
//            var promotionUsed = _promotionDao.FindByOrderId(request.OrderId);
//            if (promotionUsed != null)
//            {
//                var promoDomainObject = _promoRepository.Get(promotionUsed.PromoId);
//                var amountSaved = promoDomainObject.GetAmountSaved(request.OrderId, totalAmount);
//                totalAmount = totalAmount - amountSaved;
//            }

            var preAuthResponse = PreauthorizePaymentIfNecessary(request.OrderId, request.CardToken, totalAmount);
            if (preAuthResponse.IsSuccessful)
            {
                return _paymentService.CommitPayment(totalAmount, request.MeterAmount, request.TipAmount, request.CardToken, request.OrderId, request.IsNoShowFee);
            }

            return new CommitPreauthorizedPaymentResponse
            {
                IsSuccessful = false,
                Message = string.Format("PreAuthorization Failed: {0}", preAuthResponse.Message)
            };
        }

        private PreAuthorizePaymentResponse PreauthorizePaymentIfNecessary(Guid orderId, string cardToken, decimal amount)
        {
            if (_serverSettings.GetPaymentSettings().IsPreAuthEnabled)
            {
                // Already preautorized on create order, do nothing
                return new PreAuthorizePaymentResponse { IsSuccessful = true };
            }

            var orderDetail = _orderDao.FindById(orderId);
            if (orderDetail == null)
            {
                throw new HttpError(HttpStatusCode.NotFound, "Order not found");
            }

            var account = _accountDao.FindById(orderDetail.AccountId);

            var result = _paymentService.PreAuthorize(orderId, account.Email, cardToken, amount);

            if (result.IsSuccessful)
            {
                // Wait for OrderPaymentDetail to be created
                Thread.Sleep(500);
            }

            return result;
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