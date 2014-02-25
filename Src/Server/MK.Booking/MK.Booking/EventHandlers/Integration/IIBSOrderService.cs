namespace apcurium.MK.Booking.EventHandlers.Integration
{
    public interface IIbsOrderService
    {
        void ConfirmExternalPayment(int orderId, string vehicleId, string text, double amount, double fareAmount, string cardType, string cardNumber, string cardExpiry, string transactionId, string authorizationCode);

        void SendPaymentNotification(string message, string vehicleNumber, int ibsOrderId);
    }
}