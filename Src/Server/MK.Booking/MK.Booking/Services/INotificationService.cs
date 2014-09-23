using System;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Common.Entity;

namespace apcurium.MK.Booking.Services
{
    public interface INotificationService
    {
        void SetBaseUrl(Uri baseUrl);
        void SendAssignedPush(OrderStatusDetail orderStatusDetail);
        void SendArrivedPush(OrderStatusDetail orderStatusDetail);
        void SendPairingInquiryPush(OrderStatusDetail orderStatusDetail);
        void SendTimeoutPush(OrderStatusDetail orderStatusDetail);

        void SendPaymentCapturePush(Guid orderId, decimal amount);
        void SendTaxiNearbyPush(Guid orderId, string ibsStatus, double? newLatitude, double? newLongitude);
        void SendAutomaticPairingPush(Guid orderId, int? autoTipPercentage, string last4Digits, bool success);

        void SendAccountConfirmationSMS(string phoneNumber, string code, string clientLanguageCode);

        void SendAccountConfirmationEmail(Uri confirmationUrl, string clientEmailAddress, string clientLanguageCode);
        void SendBookingConfirmationEmail(int ibsOrderId, string note, Address pickupAddress, Address dropOffAddress,
            DateTime pickupDate, SendBookingConfirmationEmail.BookingSettings settings, string clientEmailAddress, string clientLanguageCode, bool bypassNotificationSetting = false);
        void SendPasswordResetEmail(string password, string clientEmailAddress, string clientLanguageCode);
        void SendReceiptEmail(Guid orderId, int ibsOrderId, string vehicleNumber, string driverName, double fare, double toll, double tip,
            double tax, double totalFare, SendReceipt.CardOnFile cardOnFileInfo, Address pickupAddress, Address dropOffAddress,
            DateTime pickupDate, DateTime? dropOffDate, string clientEmailAddress, string clientLanguageCode, bool bypassNotificationSetting = false);
    }
}