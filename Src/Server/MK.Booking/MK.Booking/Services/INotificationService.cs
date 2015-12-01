using System;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.ReadModel;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common;

namespace apcurium.MK.Booking.Services
{
    public interface INotificationService
    {
        void SetBaseUrl(Uri baseUrl);

        void SendPromotionUnlockedPush(Guid orderId, PromotionDetail promotionDetail);
        void SendAssignedPush(OrderStatusDetail orderStatusDetail);
        void SendArrivedPush(OrderStatusDetail orderStatusDetail);
        void SendTimeoutPush(OrderStatusDetail orderStatusDetail);
        void SendBailedPush(OrderStatusDetail orderStatusDetail);
        void SendChangeDispatchCompanyPush(Guid orderId);
        void SendPaymentCapturePush(Guid orderId, decimal amount);
        void SendTaxiNearbyPush(Guid orderId, string ibsStatus, double? newLatitude, double? newLongitude);
        void SendUnpairingReminderPush(Guid orderId);
        void SendAutomaticPairingPush(Guid orderId, CreditCardDetails creditCard, int autoTipPercentage, bool success);
        void SendOrderCreationErrorPush(Guid orderId, string errorDescription);

        void SendAccountConfirmationSMS(CountryISOCode countryCode, string phoneNumber, string code, string clientLanguageCode);

        void SendAccountConfirmationEmail(Uri confirmationUrl, string clientEmailAddress, string clientLanguageCode);
        void SendBookingConfirmationEmail(int ibsOrderId, string note, Address pickupAddress, Address dropOffAddress,
            DateTime pickupDate, SendBookingConfirmationEmail.BookingSettings settings, string clientEmailAddress, string clientLanguageCode, bool bypassNotificationSetting = false);
        void SendPasswordResetEmail(string password, string clientEmailAddress, string clientLanguageCode);

        void SendCancellationFeesReceiptEmail(int ibsOrderId, double feeAmount, string last4Digits, string clientEmailAddress, string clientLanguageCode, bool bypassNotificationSetting = false);

        void SendNoShowFeesReceiptEmail(int ibsOrderId, double feeAmount, Address pickUpAddress, string last4Digits, string clientEmailAddress, string clientLanguageCode, bool bypassNotificationSetting = false);

        void SendTripReceiptEmail(Guid orderId, int ibsOrderId, string vehicleNumber, DriverInfos driverInfos, double fare, double toll, double tip, double tax,
            double extra, double surcharge, double bookingFees, double totalFare, SendReceipt.Payment paymentInfo, Address pickupAddress, Address dropOffAddress,
            DateTime pickupDate, DateTime? dropOffDateInUtc, string clientEmailAddress, string clientLanguageCode, double amountSavedByPromotion,
            string promoCode, SendReceipt.CmtRideLinqReceiptFields cmtRideLinqFields, bool bypassNotificationSetting = false);
        void SendPromotionUnlockedEmail(string name, string code, DateTime? expirationDate, string clientEmailAddress, string clientLanguageCode, bool bypassNotificationSettings = false);

        void SendCreditCardDeactivatedEmail(string creditCardCompany, string last4Digits, string clientEmailAddress, string clientLanguageCode, bool bypassNotificationSetting = false);

        void SendCreditCardDeactivatedPush(AccountDetail account);

        void SendNoShowWarning(Guid orderId);

        void SendInfoAboutGratuity(Guid orderId);
    }
}