#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.PushNotifications;
using apcurium.MK.Booking.PushNotifications.Impl;
using apcurium.MK.Booking.ReadModel;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Booking.Services;
using apcurium.MK.Booking.Services.Impl;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Diagnostic;
using apcurium.MK.Common.Entity;
using ServiceStack.Common.Web;
using ServiceStack.ServiceInterface;
using SendReceipt = apcurium.MK.Booking.Commands.SendReceipt;

#endregion

namespace apcurium.MK.Booking.Api.Services
{
    public class NotificationAdministrationService : Service
    {
        private readonly IAccountDao _dao;
        private readonly IDeviceDao _daoDevice;
        private readonly ILogger _logger;
        private readonly INotificationService _notificationService;
        private readonly IConfigurationManager _configurationManager;

        public NotificationAdministrationService(
            IAccountDao dao, 
            IDeviceDao device,
            INotificationService notificationService,
            IConfigurationManager configurationManager,
            ILogger logger)
        {
            _dao = dao;
            _daoDevice = device;
            _logger = logger;
            _notificationService = notificationService;
            _configurationManager = configurationManager;
        }

        public object Post(PushNotificationAdministrationRequest request)
        {
            var account = _dao.FindByEmail(request.EmailAddress);

            if (account == null)
            {
                throw new HttpError(HttpStatusCode.InternalServerError, "sendPushNotificationErrorNoAccount");
            }

            var devices = _daoDevice.FindByAccountId(account.Id);

            var deviceDetails = devices as DeviceDetail[] ?? devices.ToArray();
            if (devices == null || !deviceDetails.Any())
            {
                throw new HttpError(HttpStatusCode.InternalServerError, "sendPushNotificationErrorNoDevice");
            }

            // We create a new instance each time as we need to start from a clean state to get meaningful error messages
            var pushNotificationService = new PushNotificationService(_configurationManager, _logger);

            foreach (var device in deviceDetails)
            {
                try
                {
                    pushNotificationService.Send(request.Message, new Dictionary<string, object>(), device.DeviceToken,
                        device.Platform);
                }
                catch (Exception e)
                {
                    _logger.LogError(e);
                    throw new HttpError(HttpStatusCode.InternalServerError, device.Platform + "-" + e.Message);
                }
            }

            return new HttpResult(HttpStatusCode.OK);
        }

        public object Post(TestEmailAdministrationRequest request)
        {
            try
            {
                switch (request.TemplateName)
                {
                    case NotificationService.EmailConstant.Template.AccountConfirmation:
                        _notificationService.SendAccountConfirmationEmail(new Uri("http://www.google.com"),
                            request.EmailAddress, "fr");
                        break;
                    case NotificationService.EmailConstant.Template.BookingConfirmation:
                        _notificationService.SendBookingConfirmationEmail(12345, "This is a standard note",
                            _pickupAddress, _dropOffAddress,
                            DateTime.Now, _bookingSettings, request.EmailAddress, "fr-FR", true);
                        break;
                    case NotificationService.EmailConstant.Template.PasswordReset:
                        _notificationService.SendPasswordResetEmail("N3wp@s5w0rd", request.EmailAddress, "fr-FR");
                        break;
                    case NotificationService.EmailConstant.Template.Receipt:
                        _notificationService.SendReceiptEmail(12345, "9007", "Alex Proteau", 45, 2, 6.75, 4.5, 58.25,
                            _cardOnFile, _pickupAddress, _dropOffAddress, DateTime.Now.AddMinutes(-15), DateTime.Now, request.EmailAddress, "fr-FR", true);
                        break;
                    default:
                        throw new Exception("sendTestEmailErrorNoMatchingTemplate");
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e);
                throw new HttpError(HttpStatusCode.InternalServerError, e.Message);
            }

            return new HttpResult(HttpStatusCode.OK);
        }

        public object Get(EmailTemplateNamesRequest request)
        {
            var fields = typeof(NotificationService.EmailConstant.Template).GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
            return fields.Where(fieldInfo => fieldInfo.IsLiteral && !fieldInfo.IsInitOnly && fieldInfo.FieldType == typeof (string))
                         .Select(fi => fi.GetRawConstantValue());
        }

        private readonly Address _pickupAddress = new Address
        {
            Apartment = "709",
            BuildingName = "Place Décarie",
            City = "Montreal",
            FriendlyName = "apcurium",
            Latitude = 45.498116,
            Longitude = -73.656629,
            RingCode = "709",
            State = "QC",
            Street = "Ferrier Street",
            StreetNumber = "5250",
            ZipCode = "H4P 1L3"
        };

        private readonly Address _dropOffAddress = new Address
        {
            City = "Montreal",
            FriendlyName = "Vices & Versa",
            Latitude = 45.531442,
            Longitude = -73.610760,
            State = "QC",
            Street = "Boulevard Saint-Laurent",
            StreetNumber = "6631",
            ZipCode = "H2S 3C5"
        };

        private readonly SendBookingConfirmationEmail.BookingSettings _bookingSettings = new SendBookingConfirmationEmail.BookingSettings
        {
            ChargeType = "Card on File",
            LargeBags = 0,
            Name = "Tony Apcurium",
            Passengers = 1,
            Phone = "5551234567",
            VehicleType = "Taxi"
        };

        private readonly SendReceipt.CardOnFile _cardOnFile = new SendReceipt.CardOnFile((decimal) 51.75, "ad51d", "1155", "Visa")
        {
            ExpirationMonth = "2",
            ExpirationYear = "14",
            LastFour = "4111",
            NameOnCard = "Tony Apcurium"
        };
    }
}