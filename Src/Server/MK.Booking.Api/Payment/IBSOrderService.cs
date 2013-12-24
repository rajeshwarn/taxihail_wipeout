using apcurium.MK.Booking.EventHandlers.Integration;
using apcurium.MK.Booking.IBS;

namespace apcurium.MK.Booking.Api.Payment
{
    public class IBSOrderService: IIbsOrderService
    {
        readonly IBookingWebServiceClient _client;

        public IBSOrderService(IBookingWebServiceClient client)
        {
            _client = client;
        }

        public void ConfirmExternalPayment(int orderId, decimal amount, string type, string provider, string transactionId, string authorizationCode)
        {
            _client.ConfirmExternalPayment(orderId, amount,type, provider , transactionId, authorizationCode);
        }

        public void SendMessageToDriver(string message, string vehicleNumber)
        {
            _client.SendMessageToDriver(message, vehicleNumber);
        }
    }
}
