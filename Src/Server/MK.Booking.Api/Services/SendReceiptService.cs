using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using apcurium.MK.Booking.Api.Client.Cmt.Payments.Pair;
using apcurium.MK.Booking.Api.Client.Payments.CmtPayments;
using apcurium.MK.Booking.Api.Contract.Resources.Payments.Cmt;
using apcurium.MK.Booking.CommandBuilder;
using apcurium.MK.Booking.ReadModel;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Enumeration;
using apcurium.MK.Common.Extensions;
using Infrastructure.Messaging;
using ServiceStack.Common.Web;
using ServiceStack.ServiceInterface;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.IBS;
using apcurium.MK.Booking.ReadModel.Query;
using apcurium.MK.Common;

namespace apcurium.MK.Booking.Api.Services
{
    public class SendReceiptService : RestServiceBase<SendReceipt>
    {
        private readonly ICommandBus _commandBus;
        private readonly IBookingWebServiceClient _bookingWebServiceClient;
        private readonly IOrderDao _orderDao;
        private readonly IOrderPaymentDao _orderPaymentDao;
        private readonly IAccountDao _accountDao;
        private readonly IConfigurationManager _configurationManager;
        private readonly ICreditCardDao _creditCardDao;

        public SendReceiptService(
            ICommandBus commandBus, 
            IBookingWebServiceClient bookingWebServiceClient, 
            IOrderDao orderDao,
            IOrderPaymentDao orderPaymentDao,            
            ICreditCardDao creditCardDao,
            IAccountDao accountDao, 
            IConfigurationManager configurationManager
            )
        {
            _bookingWebServiceClient = bookingWebServiceClient;
            _orderDao = orderDao;
            _orderPaymentDao = orderPaymentDao;
            _accountDao = accountDao;
            _configurationManager = configurationManager;
            _creditCardDao = creditCardDao;
            _commandBus = commandBus;
        }

        public override object OnPost(SendReceipt request)
        {
            var order = _orderDao.FindById(request.OrderId);
            var account = _accountDao.FindById(new Guid(this.GetSession().UserAuthId));

            if (!order.IBSOrderId.HasValue)
            {
                throw new HttpError(HttpStatusCode.BadRequest, ErrorCode.OrderNotInIbs.ToString());
            }

            if (account.Id != order.AccountId)
            {
                throw new HttpError(HttpStatusCode.Unauthorized, "Not your order");
            }
                        
            var ibsOrder = _bookingWebServiceClient.GetOrderDetails(order.IBSOrderId.Value, account.IBSAccountId, order.Settings.Phone);
            
            var orderPayment = _orderPaymentDao.FindByOrderId(order.Id);
            var pairingInfo = _orderDao.FindOrderPairingById(order.Id);

            if ((orderPayment != null) && (orderPayment.IsCompleted))
            {
                var creditCard = orderPayment.CardToken.HasValue() ? _creditCardDao.FindByToken(orderPayment.CardToken) : null;
                _commandBus.Send(SendReceiptCommandBuilder.GetSendReceiptCommand(order, account, ibsOrder.VehicleNumber,  Convert.ToDouble( orderPayment.Meter), 0, Convert.ToDouble( orderPayment.Tip),0, orderPayment, creditCard ));
            }
            else if ((pairingInfo != null) && (pairingInfo.AutoTipPercentage.HasValue))
            {                
                var creditCard = pairingInfo.TokenOfCardToBeUsedForPayment.HasValue() ? _creditCardDao.FindByToken(pairingInfo.TokenOfCardToBeUsedForPayment) : null;
                var tripData = GetTripData(pairingInfo.PairingToken);
                if ( (tripData != null) && ( tripData.EndTime.HasValue ))
                {
                    _commandBus.Send(SendReceiptCommandBuilder.GetSendReceiptCommand(order, account,
                        ibsOrder.VehicleNumber, tripData.Fare, tripData.Extra, tripData.Tip, tripData.Tax , null, creditCard));
                }
                else
                {
                    _commandBus.Send(SendReceiptCommandBuilder.GetSendReceiptCommand(order, account,
                        ibsOrder.VehicleNumber, ibsOrder.Fare, ibsOrder.Toll, pairingInfo.AutoTipPercentage / 100,ibsOrder.VAT, null, creditCard));
                }
            }
            else
            {
                _commandBus.Send(SendReceiptCommandBuilder.GetSendReceiptCommand(order, account, ibsOrder.VehicleNumber, ibsOrder.Fare, ibsOrder.Toll, ibsOrder.Tip, ibsOrder.VAT, null,null ));
            }
            
            return new HttpResult(HttpStatusCode.OK, "OK");
        }

        private Trip GetTripData(string pairingToken)
        {
            try
            {
                var cmtClient = new CmtMobileServiceClient(_configurationManager.GetPaymentSettings().CmtPaymentSettings, null, "TaxiHail");
                var trip = cmtClient.Get(new TripRequest { Token = pairingToken });
                
                return trip;
            }
            catch (Exception)
            {
                return null;
                
            }
            
        }

        //private Commands.SendReceipt GetSendReceiptCommand(OrderDetail order,  IBSOrderDetails ibsOrder, OrderPaymentDetail orderPayment, AccountDetail account)
        //{

        //    var paidFare = Convert.ToDouble((orderPayment != null) ? orderPayment.Meter : 0);
        //    var paidTip = Convert.ToDouble((orderPayment != null) ? orderPayment.Tip : 0);
                //PickupAddress = order.PickupAddress,
                //DropOffAddress = order.DropOffAddress,

        //    var fare = useIbsValue ? ibsOrder.Fare.GetValueOrDefault() : paidFare;
        //    var tip = useIbsValue ? ibsOrder.Tip.GetValueOrDefault() : paidTip;


        //    var command = new Commands.SendReceipt
        //    {
        //        Id = Guid.NewGuid(),
        //        OrderId =  order.Id ,
        //        EmailAddress = account.Email,
        //        IBSOrderId = order.IBSOrderId.Value,
        //        TransactionDate = order.PickupDate,
        //        VehicleNumber = ibsOrder.VehicleNumber,
        //        Fare = fare,
        //        Toll = ibsOrder.Toll.GetValueOrDefault(),
        //        Tip = tip,
        //        Tax = ibsOrder.VAT.GetValueOrDefault(),
        //    };


        //    if (orderPayment != null)
        //    {
        //        command.CardOnFileInfo = new Commands.SendReceipt.CardOnFile(
        //            orderPayment.Amount,
        //            orderPayment.TransactionId,
        //            orderPayment.AuthorizationCode,
        //            orderPayment.Type == PaymentType.CreditCard ? "Credit Card" : orderPayment.Type.ToString());

        //        if (orderPayment.CardToken.HasValue())
        //        {
        //            var creditCard = _creditCardDao.FindByToken(orderPayment.CardToken);
        //            if (creditCard != null)
        //            {
        //                command.CardOnFileInfo.LastFour = creditCard.Last4Digits;
        //                command.CardOnFileInfo.Company = creditCard.CreditCardCompany;
        //                command.CardOnFileInfo.FriendlyName = creditCard.FriendlyName;
        //            }
        //        }
        //    }
        //    return command;
        //}
    }
}
