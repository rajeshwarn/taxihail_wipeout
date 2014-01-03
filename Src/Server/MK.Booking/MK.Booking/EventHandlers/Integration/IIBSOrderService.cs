namespace apcurium.MK.Booking.EventHandlers.Integration
{
    public interface IIbsOrderService
    {
        void ConfirmExternalPayment(int orderId, decimal amount, string type, string provider, string transactionId,
            string authorizationCode);

        void SendMessageToDriver(string message, string vehicleNumber);
    }
}