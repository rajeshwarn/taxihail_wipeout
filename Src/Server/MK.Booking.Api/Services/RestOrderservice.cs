using System.Globalization;
using System.Net;
using Infrastructure.Messaging;
using MK.Booking.Api.Client;
using ServiceStack.ServiceInterface;
using apcurium.MK.Booking.Api.Contract.Requests.Orders;
using apcurium.MK.Booking.Commands.Orders;
using apcurium.MK.Booking.IBS;
using apcurium.MK.Booking.ReadModel.Query;
using apcurium.MK.Common.Extensions;

namespace apcurium.MK.Booking.Api.Services
{
    public class RestOrderService : Service
    {
        private readonly ICommandBus _commandBus;
        private readonly IPaymentServiceClient _paymentClient;
        private readonly IBookingWebServiceClient _bookingWebServiceClient;
        protected IOrderDao Dao { get; set; }

        public RestOrderService(IOrderDao dao, ICommandBus commandBus, IPaymentServiceClient paymentClient, IBookingWebServiceClient bookingWebServiceClient)
        {
            _commandBus = commandBus;
            _paymentClient = paymentClient;
            _bookingWebServiceClient = bookingWebServiceClient;
            Dao = dao;
        }

        public void Post(CapturePaymentRequest request)
        {
            var commitResponse = _paymentClient.CommitPreAuthorized(request.TransactionId + "",
                                                                    request.IbsOrderNumber.ToString(
                                                                        CultureInfo.InvariantCulture));
            if (!commitResponse.IsSuccessfull)
            {
                throw new WebException("Payment Error: Cannot complete transaction\n" + commitResponse.Message);
            }

            _bookingWebServiceClient.SendMessageToDriver("The passenger has payed " + request.Amount.ToString("C"), request.CarNumber);


            if (!_bookingWebServiceClient.SendAuthCode(request.IbsOrderNumber, request.Amount,
                                                       request.TransactionId.ToString(CultureInfo.InvariantCulture)))
            {
                //TODO not sure what to do here
            }

            _commandBus.Send(new CommitPaymentCommand()
            {
                TransactionId = request.TransactionId.ToLong(),
                OrderId = request.OrderId
            });


        }

    }
}
