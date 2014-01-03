﻿#region

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
        private readonly IConfigurationManager _configurationManager;
        private readonly IIbsOrderService _ibs;
        private readonly IOrderDao _orderDao;
        private readonly IOrderPaymentDao _orderPaymentDao;

        public ResendPaymentConfirmationSevice(IOrderDao orderDao, IOrderPaymentDao orderPaymentDao,
            IConfigurationManager configurationManager, IIbsOrderService ibs)
        {
            _orderDao = orderDao;
            _orderPaymentDao = orderPaymentDao;
            _configurationManager = configurationManager;
            _ibs = ibs;
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

            SendPaymentConfirmationToDriver(orderStatus.VehicleNumber, paymentInfo.Amount, paymentInfo.TransactionId,
                paymentInfo.AuthorizationCode);

            return new HttpResult(HttpStatusCode.OK);
        }


        private void SendPaymentConfirmationToDriver(string vehicleNumber, decimal amount, string transactionId,
            string authorizationCode)
        {
            var applicationKey = _configurationManager.GetSetting("TaxiHail.ApplicationKey");
            var resources = new DynamicResources(applicationKey);

            var line1 = string.Format(resources.GetString("PaymentConfirmationToDriver1"), amount);
            line1 = line1.PadRight(32, ' ');
                //Padded with 32 char because the MDT displays line of 32 char.  This will cause to write the auth code on the second line
            var line2 = string.Format(resources.GetString("PaymentConfirmationToDriver2"), authorizationCode);
            _ibs.SendMessageToDriver(line1 + line2, vehicleNumber);
        }
    }
}