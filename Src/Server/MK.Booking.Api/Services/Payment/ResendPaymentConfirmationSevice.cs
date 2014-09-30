#region

using System;
using System.Net;
using apcurium.MK.Booking.Api.Contract.Requests.Payment;
using apcurium.MK.Booking.EventHandlers.Integration;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Booking.Resources;
using apcurium.MK.Common.Configuration;
using ServiceStack.Common.Web;
using ServiceStack.ServiceInterface;

#endregion

namespace apcurium.MK.Booking.Api.Services.Payment
{
    public class ResendPaymentConfirmationSevice : Service
    {
        private readonly IIbsOrderService _ibs;
        private readonly IOrderDao _orderDao;
        private readonly IOrderPaymentDao _orderPaymentDao;
        private readonly Resources.Resources _resources;

        public ResendPaymentConfirmationSevice(IOrderDao orderDao, IOrderPaymentDao orderPaymentDao,
            IConfigurationManager configurationManager, IIbsOrderService ibs, IServerSettings appSettings)
        {
            _orderDao = orderDao;
            _orderPaymentDao = orderPaymentDao;
            _ibs = ibs;

            _resources = new Resources.Resources(configurationManager.GetSetting("TaxiHail.ApplicationKey"), appSettings);
        }

        public object Post(ResendPaymentConfirmationRequest request)
        {
            var session = this.GetSession();

            var order = _orderDao.FindById(request.OrderId);

            if (order == null) return new HttpResult(HttpStatusCode.NotFound);

            if (order.AccountId != new Guid(session.UserAuthId)) return new HttpResult(HttpStatusCode.Unauthorized);

            var orderStatus = _orderDao.FindOrderStatusById(request.OrderId);
            if (orderStatus == null) return new HttpResult(HttpStatusCode.NotFound);


            var paymentInfo = _orderPaymentDao.FindByOrderId(order.Id);
            if (paymentInfo == null) return new HttpResult(HttpStatusCode.NotFound);

            SendPaymentConfirmationToDriver( order.IBSOrderId.Value, orderStatus.VehicleNumber, paymentInfo.Meter, paymentInfo.Tip, paymentInfo.Amount, paymentInfo.TransactionId,
                paymentInfo.AuthorizationCode);

            return new HttpResult(HttpStatusCode.OK);
        }

        private void SendPaymentConfirmationToDriver(int ibsOrderId, string vehicleNumber, decimal meter, decimal tip, decimal amount, string transactionId, 
            string authorizationCode)
        {

           
            var amountString = _resources.FormatPrice((double)amount);
            var meterString = _resources.FormatPrice((double?)meter);
            var tipString = _resources.FormatPrice((double?)tip);

            // Padded with 32 char because the MDT displays line of 32 char.  This will cause to write each string on a new line
            var line1 = string.Format(_resources.Get("PaymentConfirmationToDriver1"));
            line1 = line1.PadRight(32, ' ');
            var line2 = string.Format(_resources.Get("PaymentConfirmationToDriver2"), meterString, tipString);
            line2 = line2.PadRight(32, ' ');
            var line3 = string.Format(_resources.Get("PaymentConfirmationToDriver3"), amountString);
            line3 = line3.PadRight(32, ' ');
            var line4 = string.Format(_resources.Get("PaymentConfirmationToDriver4"), authorizationCode);

            _ibs.SendPaymentNotification(line1 + line2 + line3 + line4, vehicleNumber, ibsOrderId);
        }
    }
}