using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Mail;
using System.Text;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.Database;
using apcurium.MK.Booking.Email;
using apcurium.MK.Booking.Maps.Geo;
using apcurium.MK.Booking.PushNotifications;
using apcurium.MK.Booking.ReadModel;
using apcurium.MK.Common;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Enumeration;

namespace apcurium.MK.Booking.Services.Impl
{
    public class NotificationService : INotificationService
    {
        private const string ApplicationNameSetting = "TaxiHail.ApplicationName";
        private const string AccentColorSetting = "TaxiHail.AccentColor";
        private const string VATEnabledSetting = "VATIsEnabled";
        private const string VATPercentageSetting = "VATPercentage";
        private const string VATRegistrationNumberSetting = "VATRegistrationNumber";

        private const string PasswordResetTemplateName = "PasswordReset";
        private const string PasswordResetEmailSubject = "Email_Subject_PasswordReset";

        private const string AccountConfirmationTemplateName = "AccountConfirmation";
        private const string AccountConfirmationEmailSubject = "Email_Subject_AccountConfirmation";

        private const string ReceiptEmailSubject = "Email_Subject_Receipt";
        private const string ReceiptTemplateName = "Receipt";

        private const string BookingConfirmationTemplateName = "BookingConfirmation";
        private const string BookingConfirmationEmailSubject = "Email_Subject_BookingConfirmation";

        private const string DriverAssignedTemplateName = "DriverAssigned";
        private const string DriverAssignedWithVATTemplateName = "DriverAssignedWithVAT";
        private const string DriverAssignedEmailSubject = "Email_Subject_DriverAssigned";

        private readonly Func<BookingDbContext> _contextFactory;
        private readonly IPushNotificationService _pushNotificationService;
        private readonly IConfigurationManager _configurationManager;
        private readonly IAppSettings _appSettings;
        private readonly ITemplateService _templateService;
        private readonly IEmailSender _emailSender;
        private readonly Resources.Resources _resources;

        public NotificationService(
            Func<BookingDbContext> contextFactory, 
            IPushNotificationService pushNotificationService,
            ITemplateService templateService,
            IEmailSender emailSender,
            IConfigurationManager configurationManager, 
            IAppSettings appSettings)
        {
            _contextFactory = contextFactory;
            _pushNotificationService = pushNotificationService;
            _templateService = templateService;
            _emailSender = emailSender;
            _configurationManager = configurationManager;
            _appSettings = appSettings;

            var applicationKey = configurationManager.GetSetting("TaxiHail.ApplicationKey");
            _resources = new Resources.Resources(applicationKey);
        }

        public void SendStatusChangedNotification(OrderStatusDetail orderStatusDetail)
        {
            var shouldSendPushNotification = orderStatusDetail.IBSStatusId == VehicleStatuses.Common.Assigned ||
                                             orderStatusDetail.IBSStatusId == VehicleStatuses.Common.Arrived ||
                                             (orderStatusDetail.IBSStatusId == VehicleStatuses.Common.Loaded && _appSettings.Data.AutomaticPayment) ||
                                             orderStatusDetail.IBSStatusId == VehicleStatuses.Common.Timeout;

            if (shouldSendPushNotification)
            {
                using (var context = _contextFactory.Invoke())
                {
                    var order = context.Find<OrderDetail>(orderStatusDetail.OrderId);

                    string alert;
                    switch (orderStatusDetail.IBSStatusId)
                    {
                        case VehicleStatuses.Common.Assigned:
                            alert = string.Format(_resources.Get("PushNotification_wosASSIGNED", order.ClientLanguageCode),
                                orderStatusDetail.VehicleNumber);
                            break;
                        case VehicleStatuses.Common.Arrived:
                            alert = string.Format(_resources.Get("PushNotification_wosARRIVED", order.ClientLanguageCode),
                                orderStatusDetail.VehicleNumber);
                            break;
                        case VehicleStatuses.Common.Loaded:
                            if (order.Settings.ChargeTypeId != ChargeTypes.CardOnFile.Id)
                            {
                                // Only send notification if card on file
                                return;
                            }
                            alert = _resources.Get("PushNotification_wosLOADED", order.ClientLanguageCode);
                            break;
                        case VehicleStatuses.Common.Timeout:
                            alert = _resources.Get("PushNotification_wosTIMEOUT", order.ClientLanguageCode);
                            break;
                        default:
                            throw new InvalidOperationException("No push notification for this order status");
                    }

                    var devices =
                        context.Set<DeviceDetail>().Where(x => x.AccountId == order.AccountId);
                    var data = new Dictionary<string, object>();

                    if (orderStatusDetail.IBSStatusId == VehicleStatuses.Common.Assigned ||
                        orderStatusDetail.IBSStatusId == VehicleStatuses.Common.Arrived)
                    {
                        data.Add("orderId", order.Id);
                        data.Add("isPairingNotification", false);
                    }
                    if (orderStatusDetail.IBSStatusId == VehicleStatuses.Common.Loaded)
                    {
                        data.Add("orderId", order.Id);
                        data.Add("isPairingNotification", true);
                    }

                    foreach (var device in devices)
                    {
                        _pushNotificationService.Send(alert, data, device.DeviceToken, device.Platform);
                    }
                }
            }
        }

        public void SendPaymentCaptureNotification(Guid orderId, decimal amount)
        {
            using (var context = _contextFactory.Invoke())
            {
                var order = context.Find<OrderDetail>(orderId);

                var alert = string.Format(string.Format(_resources.Get("PushNotification_PaymentReceived"), amount), order.ClientLanguageCode);
                var data = new Dictionary<string, object> { { "orderId", order.Id } };
                var devices = context.Set<DeviceDetail>().Where(x => x.AccountId == order.AccountId);

                // Send push notifications
                foreach (var device in devices)
                {
                    _pushNotificationService.Send(alert, data, device.DeviceToken, device.Platform);
                }
            }
        }

        private const int TaxiDistanceThresholdForPushNotification = 200; // In meters
        public void SendTaxiNearbyNotification(Guid orderId, string ibsStatus, double? newLatitude, double? newLongitude)
        {
            using (var context = _contextFactory.Invoke())
            {
                var order = context.Find<OrderDetail>(orderId);
                var orderStatus = context.Query<OrderStatusDetail>().Single(x => x.OrderId == orderId);

                var shouldSendPushNotification = newLatitude.HasValue &&
                                                 newLongitude.HasValue &&
                                                 ibsStatus == VehicleStatuses.Common.Assigned &&
                                                 !orderStatus.IsTaxiNearbyNotificationSent;

                if (shouldSendPushNotification)
                {
                    var taxiPosition = new Position(newLatitude.Value, newLongitude.Value);
                    var pickupPosition = new Position(order.PickupAddress.Latitude, order.PickupAddress.Longitude);

                    if (taxiPosition.DistanceTo(pickupPosition) <= TaxiDistanceThresholdForPushNotification)
                    {
                        orderStatus.IsTaxiNearbyNotificationSent = true;
                        context.Save(orderStatus);

                        var alert = string.Format(_resources.Get("PushNotification_NearbyTaxi", order.ClientLanguageCode));
                        var data = new Dictionary<string, object> { { "orderId", order.Id } };
                        var devices = context.Set<DeviceDetail>().Where(x => x.AccountId == order.AccountId);

                        // Send push notifications
                        foreach (var device in devices)
                        {
                            _pushNotificationService.Send(alert, data, device.DeviceToken, device.Platform);
                        }
                    }
                }
            }
        }

        public void SendAccountConfirmationEmail(Uri confirmationUrl, string clientEmailAddress, string clientLanguageCode)
        {
            var templateData = new
            {
                confirmationUrl,
                ApplicationName = _configurationManager.GetSetting(ApplicationNameSetting),
                AccentColor = _configurationManager.GetSetting(AccentColorSetting)
            };

            SendEmail(clientEmailAddress, AccountConfirmationTemplateName, AccountConfirmationEmailSubject, templateData, clientLanguageCode);
        }

        public void SendAssignedConfirmationEmail(int ibsOrderId, double fare, string vehicleNumber, 
            Address pickupAddress, Address dropOffAddress, DateTime pickupDate, DateTime transactionDate, 
            SendBookingConfirmationEmail.BookingSettings settings, string clientEmailAddress, string clientLanguageCode)
        {
            var vatEnabled = _configurationManager.GetSetting(VATEnabledSetting, false);
            var templateName = vatEnabled
                ? DriverAssignedWithVATTemplateName
                : DriverAssignedTemplateName;

            var priceFormat = CultureInfo.GetCultureInfo(_configurationManager.GetSetting("PriceFormat"));

            var vatAmount = 0d;
            var fareAmountWithoutVAT = fare;
            if (vatEnabled)
            {
                fareAmountWithoutVAT = fare / (1 + _configurationManager.GetSetting<double>(VATPercentageSetting, 0) / 100);
                vatAmount = fare - fareAmountWithoutVAT;
            }

            var hasDropOffAddress = dropOffAddress != null && !string.IsNullOrWhiteSpace(dropOffAddress.FullAddress);

            var templateData = new
            {
                ApplicationName = _configurationManager.GetSetting(ApplicationNameSetting),
                AccentColor = _configurationManager.GetSetting(AccentColorSetting),
                ibsOrderId,
                PickupDate = pickupDate.ToString("dddd, MMMM d"),
                PickupTime = pickupDate.ToString("t" /* Short time pattern */),
                PickupAddress = pickupAddress.DisplayAddress,
                DropOffAddress = hasDropOffAddress ? dropOffAddress.DisplayAddress : "-",
                settings.Name,
                settings.Phone,
                settings.Passengers,
                settings.VehicleType,
                settings.ChargeType,
                Apartment = string.IsNullOrWhiteSpace(pickupAddress.Apartment) ? "-" : pickupAddress.Apartment,
                RingCode = string.IsNullOrWhiteSpace(pickupAddress.RingCode) ? "-" : pickupAddress.RingCode,
                vehicleNumber,
                TransactionDate = transactionDate.ToString("dddd, MMMM d, yyyy"),
                TransactionTime = transactionDate.ToString("t" /* Short time pattern */),
                Fare = fareAmountWithoutVAT.ToString("C", priceFormat),
                VATAmount = vatAmount.ToString("C", priceFormat),
                TotalFare = fare.ToString("C", priceFormat)
            };

            SendEmail(clientEmailAddress, templateName, DriverAssignedEmailSubject, templateData, clientLanguageCode);
        }

        public void SendBookingConfirmationEmail(int ibsOrderId, string note, Address pickupAddress, Address dropOffAddress, DateTime pickupDate,
            SendBookingConfirmationEmail.BookingSettings settings, string clientEmailAddress, string clientLanguageCode)
        {
            var hasDropOffAddress = dropOffAddress != null && !string.IsNullOrWhiteSpace(dropOffAddress.FullAddress);

            var templateData = new
            {
                ApplicationName = _configurationManager.GetSetting(ApplicationNameSetting),
                AccentColor = _configurationManager.GetSetting(AccentColorSetting),
                ibsOrderId,
                PickupDate = pickupDate.ToString("dddd, MMMM d"),
                PickupTime = pickupDate.ToString("t" /* Short time pattern */),
                PickupAddress = pickupAddress.DisplayAddress,
                DropOffAddress = hasDropOffAddress ? dropOffAddress.DisplayAddress : "-",
                /* Mandatory settings */
                settings.Name,
                settings.Phone,
                settings.Passengers,
                settings.VehicleType,
                settings.ChargeType,
                /* Optional settings */
                settings.LargeBags,
                Note = string.IsNullOrWhiteSpace(note) ? "-" : note,
                Apartment = string.IsNullOrWhiteSpace(pickupAddress.Apartment) ? "-" : pickupAddress.Apartment,
                RingCode = string.IsNullOrWhiteSpace(pickupAddress.RingCode) ? "-" : pickupAddress.RingCode,
                /* Mandatory visibility settings */
                VisibilityLargeBags = _configurationManager.GetSetting("Client.ShowLargeBagsIndicator", false) || settings.LargeBags > 0
            };

            SendEmail(clientEmailAddress, BookingConfirmationTemplateName, BookingConfirmationEmailSubject, templateData, clientLanguageCode);
        }

        public void SendPasswordResetEmail(string password, string clientEmailAddress, string clientLanguageCode)
        {
            var templateData = new
            {
                password,
                ApplicationName = _configurationManager.GetSetting(ApplicationNameSetting),
            };

            SendEmail(clientEmailAddress, PasswordResetTemplateName, PasswordResetEmailSubject, templateData, clientLanguageCode);
        }

        public void SendReceiptEmail(int ibsOrderId, string vehicleNumber, string driverName, double fare, double toll, double tip,
            double tax, double totalFare, SendReceipt.CardOnFile cardOnFileInfo, Address pickupAddress, Address dropOffAddress, 
            DateTime transactionDate, string clientEmailAddress, string clientLanguageCode)
        {
            var vatEnabled = _configurationManager.GetSetting(VATEnabledSetting, false);
            var priceFormat = CultureInfo.GetCultureInfo(_configurationManager.GetSetting("PriceFormat"));

            var isCardOnFile = cardOnFileInfo != null;
            var cardOnFileAmount = string.Empty;
            var cardNumber = string.Empty;
            var cardOnFileTransactionId = string.Empty;
            var cardOnFileAuthorizationCode = string.Empty;
            if (isCardOnFile)
            {
                cardOnFileAmount = cardOnFileInfo.Amount.ToString("C", priceFormat);
                cardNumber = cardOnFileInfo.Company;
                cardOnFileAuthorizationCode = cardOnFileInfo.AuthorizationCode;

                if (!string.IsNullOrWhiteSpace(cardOnFileInfo.LastFour))
                {
                    cardNumber += " XXXX " + cardOnFileInfo.LastFour;
                }

                cardOnFileTransactionId = cardOnFileInfo.TransactionId;
            }

            var hasDropOffAddress = dropOffAddress != null && !string.IsNullOrWhiteSpace(dropOffAddress.FullAddress);

            var templateData = new
            {
                ApplicationName = _configurationManager.GetSetting(ApplicationNameSetting),
                AccentColor = _configurationManager.GetSetting(AccentColorSetting),
                ibsOrderId,
                vehicleNumber,
                driverName,
                Date = transactionDate.ToString("dddd, MMMM d, yyyy"),
                Fare = fare.ToString("C", priceFormat),
                Toll = toll.ToString("C", priceFormat),
                Tip = tip.ToString("C", priceFormat),
                TotalFare = totalFare.ToString("C", priceFormat),
                Note = _configurationManager.GetSetting("Receipt.Note"),
                VATAmount = tax.ToString("C", priceFormat),
                VatEnabled = vatEnabled,
                VATRegistrationNumber = _configurationManager.GetSetting(VATRegistrationNumberSetting),
                IsCardOnFile = isCardOnFile,
                CardOnFileAmount = cardOnFileAmount,
                CardNumber = cardNumber,
                CardOnFileTransactionId = cardOnFileTransactionId,
                CardOnFileAuthorizationCode = cardOnFileAuthorizationCode,
                PickupAddress = pickupAddress.DisplayAddress,
                DropOffAddress = hasDropOffAddress ? dropOffAddress.DisplayAddress : "-",
            };


            SendEmail(clientEmailAddress, ReceiptTemplateName, ReceiptEmailSubject, templateData, clientLanguageCode);
        }

        private void SendEmail(string to, string bodyTemplate, string subjectTemplate, object templateData, string languageCode, params KeyValuePair<string, string>[] embeddedIMages)
        {
            var messageSubject = _templateService.Render(_resources.Get(subjectTemplate, languageCode), templateData);

            var template = _templateService.Find(bodyTemplate, languageCode);
            if (template == null)
            {
                throw new InvalidOperationException("Template not found: " + bodyTemplate);
            }
                
            var mailMessage = new MailMessage(_configurationManager.GetSetting("Email.NoReply"), to, messageSubject, null)
            {
                IsBodyHtml = true, 
                BodyEncoding = Encoding.UTF8, 
                SubjectEncoding = Encoding.UTF8
            };

            var view = AlternateView.CreateAlternateViewFromString(_templateService.Render(template, templateData), Encoding.UTF8, "text/html");
            mailMessage.AlternateViews.Add(view);

            if (embeddedIMages != null)
            {
                foreach (var image in embeddedIMages)
                {
                    var linkedImage = new LinkedResource(image.Value) { ContentId = image.Key };
                    view.LinkedResources.Add(linkedImage);
                }
            }

            _emailSender.Send(mailMessage);
        }
    }
}