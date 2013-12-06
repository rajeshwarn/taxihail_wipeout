using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using apcurium.MK.Booking.ReadModel;
using apcurium.MK.Common.Enumeration;
using apcurium.MK.Common.Extensions;
using apcurium.MK.Booking.Commands;

namespace apcurium.MK.Booking.CommandBuilder
{
    public static class SendReceiptCommandBuilder
    {
        public static SendReceipt GetSendReceiptCommand(OrderDetail order, AccountDetail account, string vehicleNumber, double? fare, double? toll, double? tip, double? tax,  OrderPaymentDetail orderPayment = null, CreditCardDetails creditCard = null)
        {

       
            var command = new Commands.SendReceipt
            {
                Id = Guid.NewGuid(),
                OrderId = order.Id,
                EmailAddress = account.Email,
                IBSOrderId = order.IBSOrderId.Value,
                TransactionDate = order.PickupDate,
                VehicleNumber = vehicleNumber,
                Fare = fare.GetValueOrDefault(),
                Toll = toll.GetValueOrDefault(),
                Tip = tip.GetValueOrDefault(),
                Tax = tax.GetValueOrDefault(),
                PickupAddress = order.PickupAddress,
                DropOffAddress = order.PickupAddress
            };


            if (orderPayment != null)
            {
                command.CardOnFileInfo = new Commands.SendReceipt.CardOnFile(
                    orderPayment.Amount,
                    orderPayment.TransactionId,
                    orderPayment.AuthorizationCode,
                    orderPayment.Type == PaymentType.CreditCard ? "Credit Card" : orderPayment.Type.ToString());

                if ((orderPayment.CardToken.HasValue()) && (creditCard != null))
                {
                    command.CardOnFileInfo.LastFour = creditCard.Last4Digits;
                    command.CardOnFileInfo.Company = creditCard.CreditCardCompany;
                    command.CardOnFileInfo.FriendlyName = creditCard.FriendlyName;
                }
            }
            return command;
        }

    }
}
