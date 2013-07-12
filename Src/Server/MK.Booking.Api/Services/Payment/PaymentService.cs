using System.Globalization;
using System.Net;
using Infrastructure.Messaging;
using ServiceStack.ServiceInterface;
using apcurium.MK.Booking.Api.Client;
using apcurium.MK.Booking.Api.Contract.Requests.Orders;
using apcurium.MK.Booking.Api.Contract.Requests.Payment;
using apcurium.MK.Booking.Api.Contract.Resources.Payments;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.Commands.Orders;
using apcurium.MK.Booking.IBS;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Common.Extensions;

namespace apcurium.MK.Booking.Api.Services.Payment
{
    public class PaymentService : Service
    {
        private readonly ICommandBus _commandBus;
        private readonly IConfigurationDao _configurationDao;
        private readonly IBookingWebServiceClient _bookingWebServiceClient;
        private readonly IPaymentServiceClient _paymentClient;

        public PaymentService(ICommandBus commandBus, IConfigurationDao configurationDao)
        {
            _commandBus = commandBus;
            _configurationDao = configurationDao;
            _bookingWebServiceClient = bookingWebServiceClient;
            _paymentClient = paymentClient;
        }

        public PaymentSettingsResponse Get(PaymentSettingsRequest request)
        {
            return new PaymentSettingsResponse()
                {
                    ClientPaymentSettings = _configurationDao.GetPaymentSettings()
                };
        }

        public ServerPaymentSettingsResponse Get(ServerPaymentSettingsRequest request)
        {
            var settings = _configurationDao.GetPaymentSettings();
            return new ServerPaymentSettingsResponse()
            {
                ServerPaymentSettings = settings
            };
        }

        public void Post(UpdateServerPaymentSettingsRequest request)
        {
            _commandBus.Send(new UpdatePaymentSettings()
                {
                    ServerPaymentSettings = request.ServerPaymentSettings,
                });
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

            bool isIbsInFailure;
            try
            {
                isIbsInFailure = !_bookingWebServiceClient.SendAuthCode(request.IbsOrderNumber, request.Amount,
                                                        request.TransactionId.ToString(CultureInfo.InvariantCulture));
            }
            catch (Exception)
            {
                isIbsInFailure = true;
            }
            if (isIbsInFailure)
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
