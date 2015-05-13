#region

using System;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.ReadModel;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Enumeration;
using apcurium.MK.Common.Extensions;

#endregion

namespace apcurium.MK.Booking.CommandBuilder
{
    public static class SendReceiptCommandBuilder
    {
        public static SendReceipt GetSendReceiptCommand(OrderDetail order, AccountDetail account, int? orderId, string vehicleNumber, DriverInfos driverInfos,
            double? fare, double? toll, double? tip, double? tax, double? surcharge, OrderPaymentDetail orderPayment = null, double? amountSavedByPromotion = null,
            PromotionUsageDetail promotionUsed = null, CreditCardDetails creditCard = null)
        {
            var command = new SendReceipt
            {
                Id = Guid.NewGuid(),
                OrderId = order.Id,
                EmailAddress = account.Email,
                IBSOrderId = orderId ?? 0,
                PickupDate = order.PickupDate,
                DropOffDate = order.DropOffDate,
                VehicleNumber = vehicleNumber,
                DriverInfos = driverInfos,
                Fare = fare.GetValueOrDefault(),
                Toll = toll.GetValueOrDefault(),
                Tip = tip.GetValueOrDefault(),
                Tax = tax.GetValueOrDefault(),
                Surcharge = surcharge.GetValueOrDefault(),
                PickupAddress = order.PickupAddress,
                DropOffAddress = order.DropOffAddress,
                ClientLanguageCode = order.ClientLanguageCode,
            };

            if (promotionUsed != null)
            {
                command.AmountSavedByPromotion = amountSavedByPromotion.GetValueOrDefault();
                command.PromoCode = promotionUsed.Code;
                command.PromoDiscountType = promotionUsed.DiscountType;
                command.PromoDiscountValue = promotionUsed.DiscountValue;
            }

            if (orderPayment != null)
            {
                command.PaymentInfo = new SendReceipt.Payment(
                    orderPayment.Amount,
                    orderPayment.TransactionId,
                    orderPayment.AuthorizationCode,
                    orderPayment.Type == PaymentType.CreditCard ? "Credit Card" : orderPayment.Type.ToString());

                if ((orderPayment.CardToken.HasValue()) && (creditCard != null))
                {
                    command.PaymentInfo.Last4Digits = creditCard.Last4Digits;
                    command.PaymentInfo.Company = creditCard.CreditCardCompany;
                    command.PaymentInfo.NameOnCard = creditCard.NameOnCard;
                    command.PaymentInfo.ExpirationMonth = creditCard.ExpirationMonth;
                    command.PaymentInfo.ExpirationYear = creditCard.ExpirationYear;
                }
            }
            return command;
        }
    }
}