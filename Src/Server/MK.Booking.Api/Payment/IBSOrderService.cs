using apcurium.MK.Booking.EventHandlers.Integration;
using apcurium.MK.Booking.IBS;

namespace apcurium.MK.Booking.Api.Payment
{
    public class IBSOrderService: IIBSOrderService
    {
        readonly IBookingWebServiceClient _client;

        public IBSOrderService(IBookingWebServiceClient client)
        {
            _client = client;
        }

        public void ConfirmExternalPayment(int orderId, decimal amount, string transactionId)
        {
            _client.ConfirmExternalPayment(orderId, amount, transactionId);
        }

        public void SendMessageToDriver(string message, string vehicleNumber)
        {
            _client.SendMessageToDriver(message, vehicleNumber);
        }
    }
}
