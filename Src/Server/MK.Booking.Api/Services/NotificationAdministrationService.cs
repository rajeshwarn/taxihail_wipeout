#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.PushNotifications.Impl;
using apcurium.MK.Booking.ReadModel;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Booking.Services;
using apcurium.MK.Booking.Services.Impl;
using apcurium.MK.Common;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Diagnostic;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Enumeration;
using apcurium.MK.Common.Extensions;
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
        private readonly IServerSettings _serverSettings;

        public NotificationAdministrationService(
            IAccountDao dao, 
            IDeviceDao device,
            INotificationService notificationService,
            IServerSettings serverSettings,
            ILogger logger)
        {
            _dao = dao;
            _daoDevice = device;
            _logger = logger;
            _notificationService = notificationService;
            _serverSettings = serverSettings;
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
            var pushNotificationService = new PushNotificationService(_serverSettings, _logger);

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
                            request.EmailAddress, request.Language);
                        break;
                    case NotificationService.EmailConstant.Template.BookingConfirmation:
                        _notificationService.SendBookingConfirmationEmail(12345, "This is a standard note",
                            _pickupAddress, _dropOffAddress,
                            DateTime.Now, _bookingSettings, request.EmailAddress, request.Language, true);
                        break;
                    case NotificationService.EmailConstant.Template.PasswordReset:
                        _notificationService.SendPasswordResetEmail("N3wp@s5w0rd", request.EmailAddress, request.Language);
                        break;
                    case NotificationService.EmailConstant.Template.CancellationFeesReceipt:
                        _notificationService.SendCancellationFeesReceiptEmail(1234, 24.42, "1111", request.EmailAddress, request.Language);
                        break;
                    case NotificationService.EmailConstant.Template.NoShowFeesReceipt:
                        _notificationService.SendNoShowFeesReceiptEmail(1234, 10.00, _pickupAddress, "1111", request.EmailAddress, request.Language);
                        break;
                    case NotificationService.EmailConstant.Template.Receipt:
                        var fareObject = _serverSettings.ServerData.VATIsEnabled
                            ? FareHelper.GetFareFromAmountInclTax(45m, _serverSettings.ServerData.VATPercentage)
                            : FareHelper.GetFareFromAmountInclTax(45m, 0);
                        var toll = 3;
                        var tip = (double)45*((double)15/(double)100);
                        var amountSavedByPromo = 10;
                        var extra = 2;
                        var surcharge = 5;
                        var bookingFees = 7;
                        var tipIncentive = 10;
                        
                        var driverInfos = new DriverInfos
                        {
                            DriverId = "7009",
                            FirstName = "Alex",
                            LastName = "Proteau",
                            MobilePhone = "5551234567",
                            VehicleColor = "Silver",
                            VehicleMake = "DMC",
                            VehicleModel = "Delorean",
                            VehicleRegistration = "OUTATIME",
                            VehicleType = "Time Machine"
                        };

                        var fare = Convert.ToDouble(fareObject.AmountExclTax);
                        var tax = Convert.ToDouble(fareObject.TaxAmount);

                        _notificationService.SendTripReceiptEmail(Guid.NewGuid(), 12345, "9007", driverInfos, fare, toll, tip, tax, extra,
                            surcharge, bookingFees, fare + toll + tip + tax + bookingFees + extra + tipIncentive - amountSavedByPromo,
                            _payment, _pickupAddress, _dropOffAddress, DateTime.Now.AddMinutes(-15), DateTime.UtcNow,
                            request.EmailAddress, request.Language, amountSavedByPromo, "PROMO10", new SendReceipt.CmtRideLinqReceiptFields
                            {
                                Distance = 13,
                                DriverId = "D1337",
                                DropOffDateTime = DateTime.Now,
                                AccessFee = 0.3,
                                LastFour = "1114",
                                TripId = 9874,
                                RateAtTripStart = 1,
                                RateAtTripEnd = 4,
                                FareAtAlternateRate = 23.45,
                                LastLatitudeOfVehicle = 45.546571, 
                                LastLongitudeOfVehicle = -73.586309,
                                Tolls = new[]
                                {
                                    new TollDetail
                                    {
                                        TollName = "Toll 1",
                                        TollAmount = 95
                                    },
                                    new TollDetail
                                    {
                                        TollName = "Toll 2",
                                        TollAmount = 5
                                    },
                                    new TollDetail
                                    {
                                        TollName = "Toll 3",
                                        TollAmount = 30
                                    },
                                    new TollDetail
                                    {
                                        TollName = "Toll 4",
                                        TollAmount = 35
                                    }
                                },
                                TipIncentive = tipIncentive
                            }, true);
                        break;
                    case NotificationService.EmailConstant.Template.PromotionUnlocked:
                        _notificationService.SendPromotionUnlockedEmail("10% Off your next ride", "PROMO123", DateTime.Now.AddMonths(1), request.EmailAddress, request.Language, true);
                        break;
                    case NotificationService.EmailConstant.Template.CreditCardDeactivated:
                        _notificationService.SendCreditCardDeactivatedEmail("Visa", "1234", request.EmailAddress, request.Language, true);
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

        private readonly SendReceipt.Payment _payment = new SendReceipt.Payment((decimal) 41.75, "ad51d", "1155", "Visa")
        {
            ExpirationMonth = "2",
            ExpirationYear = "14",
            Last4Digits = "4111",
            NameOnCard = "Tony Apcurium"
        };
    }
}