using System.Net;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Common;
using apcurium.MK.Common.Provider;
using Infrastructure.Messaging;
using MK.Common.Configuration;
using ServiceStack.Common.Web;
using ServiceStack.ServiceInterface;

namespace apcurium.MK.Booking.Api.Services
{
    public class ServiceTypeService : Service
    {
        private readonly IServiceTypeSettingsProvider _serviceTypeSettingsProvider;
        private readonly ICommandBus _commandBus;

        public ServiceTypeService(IServiceTypeSettingsProvider serviceTypeSettingsProvider, ICommandBus commandBus)
        {
            _serviceTypeSettingsProvider = serviceTypeSettingsProvider;
            _commandBus = commandBus;
        }

        public object Get(ServiceTypeRequest request)
        {
            if (!request.ServiceType.HasValue)
            {
                return _serviceTypeSettingsProvider.GetAll();
            }

            var serviceTypeSettings = _serviceTypeSettingsProvider.GetSettings(request.ServiceType.Value);
            if (serviceTypeSettings == null)
            {
                throw new HttpError(HttpStatusCode.NotFound, "Service Type Not Found");
            }

            return serviceTypeSettings;
        }

        public object Put(ServiceTypeRequest request)
        {
            if (!request.ServiceType.HasValue)
            {
                throw new HttpError(HttpStatusCode.NotFound, "Service Type must have a value");
            }

            var command = new UpdateServiceTypeSettings
            {
                CompanyId = AppConstants.CompanyId,
                ServiceTypeSettings = new ServiceTypeSettings
                {
                    ServiceType = request.ServiceType.Value,
                    IBSWebServicesUrl = request.IBSWebServicesUrl,
                    FutureBookingThresholdInMinutes = request.FutureBookingThresholdInMinutes,
                    WaitTimeRatePerMinute = request.WaitTimeRatePerMinute,
                    AirportMeetAndGreetRate = request.AirportMeetAndGreetRate
                }
            };

            _commandBus.Send(command);

            return new
            {
                ServiceType = command.ServiceTypeSettings.ServiceType
            };
        }
    }
}