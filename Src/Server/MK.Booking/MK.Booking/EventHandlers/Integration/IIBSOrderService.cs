namespace apcurium.MK.Booking.EventHandlers.Integration
{
    public interface IIBSOrderService
    {
        void ConfirmExternalPayment(int orderId, decimal amount, string transactionId);
        void SendMessageToDriver(string message, string vehicleNumber);
    }
}