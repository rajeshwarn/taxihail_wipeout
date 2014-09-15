using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Mail;
using System.Text;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.Database;
using apcurium.MK.Booking.Email;
using apcurium.MK.Booking.Maps;
using apcurium.MK.Booking.Maps.Geo;
using apcurium.MK.Booking.PushNotifications;
using apcurium.MK.Booking.ReadModel;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Booking.SMS;
using apcurium.MK.Common;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Diagnostic;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Enumeration;
using apcurium.MK.Common.Extensions;
using MK.Common.Configuration;

namespace apcurium.MK.Booking.Services.Impl
{
    public class NotificationService : INotificationService
    {
        private const int TaxiDistanceThresholdForPushNotification = 200; // In meters

        private const string ApplicationNameSetting = "TaxiHail.ApplicationName";
        private const string AccentColorSetting = "TaxiHail.AccentColor";
        private const string EmailFontColorSetting = "TaxiHail.EmailFontColor";
        private const string ApplicationKeySetting = "TaxiHail.ApplicationKey";

        private readonly Func<BookingDbContext> _contextFactory;
        private readonly IPushNotificationService _pushNotificationService;
        private readonly ITemplateService _templateService;
        private readonly IEmailSender _emailSender;
        private readonly IConfigurationManager _configurationManager;
        private readonly IAppSettings _appSettings;
        private readonly IConfigurationDao _configurationDao;
        private readonly IOrderDao _orderDao;
        private readonly IStaticMap _staticMap;
        private readonly ISmsService _smsService;
        private readonly ILogger _logger;
        private readonly Resources.Resources _resources;

        private BaseUrls _baseUrls;


        public NotificationService(
            Func<BookingDbContext> contextFactory, 
            IPushNotificationService pushNotificationService,
            ITemplateService templateService,
            IEmailSender emailSender,
            IConfigurationManager configurationManager,
            IAppSettings appSettings,
            IConfigurationDao configurationDao,
            IOrderDao orderDao,
            IStaticMap staticMap,
            ISmsService smsService,
            ILogger logger)
        {
            _contextFactory = contextFactory;
            _pushNotificationService = pushNotificationService;
            _templateService = templateService;
            _emailSender = emailSender;
            _configurationManager = configurationManager;
            _appSettings = appSettings;
            _configurationDao = configurationDao;
            _orderDao = orderDao;
            _staticMap = staticMap;
            _smsService = smsService;
            _logger = logger;

            

            var applicationKey = configurationManager.GetSetting(ApplicationKeySetting);
            _resources = new Resources.Resources(applicationKey);
        }

        public void SetBaseUrl(Uri baseUrl)
        {
            this._baseUrls = new BaseUrls(baseUrl, _configurationManager);
        }


        public void SendAssignedPush(OrderStatusDetail orderStatusDetail)
        {
            var order = _orderDao.FindById(orderStatusDetail.OrderId);
            if (ShouldSendNotification(order.AccountId, x => x.DriverAssignedPush))
            {
                SendPushOrSms(order.AccountId,
                    string.Format(_resources.Get("PushNotification_wosASSIGNED", order.ClientLanguageCode), orderStatusDetail.VehicleNumber),
                    new Dictionary<string, object> { { "orderId", order.Id }, { "isPairingNotification", false } });
            }
        }

        public void SendArrivedPush(OrderStatusDetail orderStatusDetail)
        {
            var order = _orderDao.FindById(orderStatusDetail.OrderId);
            if (ShouldSendNotification(order.AccountId, x => x.VehicleAtPickupPush))
            {
                SendPushOrSms(order.AccountId,
                    string.Format(_resources.Get("PushNotification_wosARRIVED", order.ClientLanguageCode), orderStatusDetail.VehicleNumber),
                    new Dictionary<string, object> { { "orderId", order.Id }, { "isPairingNotification", false } });
            }
        }

        public void SendPairingInquiryPush(OrderStatusDetail orderStatusDetail)
        {
            var order = _orderDao.FindById(orderStatusDetail.OrderId);
            if (_configurationManager.GetPaymentSettings().AutomaticPayment
                    && !_configurationManager.GetPaymentSettings().AutomaticPaymentPairing
                    && order.Settings.ChargeTypeId == ChargeTypes.CardOnFile.Id // Only send notification if using card on file
                    && ShouldSendNotification(order.AccountId, x => x.ConfirmPairingPush))
            {
                SendPushOrSms(order.AccountId,
                    _resources.Get("PushNotification_wosLOADED", order.ClientLanguageCode),
                    new Dictionary<string, object> { { "orderId", order.Id }, { "isPairingNotification", true } });
            }
        }

        public void SendTimeoutPush(OrderStatusDetail orderStatusDetail)
        {
            var order = _orderDao.FindById(orderStatusDetail.OrderId);
            SendPushOrSms(order.AccountId,
                        _resources.Get("PushNotification_wosTIMEOUT", order.ClientLanguageCode),
                        new Dictionary<string, object>());
        }

        public void SendPaymentCapturePush(Guid orderId, decimal amount)
        {
            using (var context = _contextFactory.Invoke())
            {
                var order = context.Find<OrderDetail>(orderId);

                if (!ShouldSendNotification(order.AccountId, x => x.PaymentConfirmationPush))
                {
                    return;
                }

                var formattedAmount = string.Format(new CultureInfo(_appSettings.Data.PriceFormat), "{0:C}", amount);
                var message = _resources.Get("PushNotification_PaymentReceived", order.ClientLanguageCode);
                var alert = string.Format(message, formattedAmount);
                var data = new Dictionary<string, object> { { "orderId", orderId } };

                SendPushOrSms(order.AccountId, alert, data);
            }
        }

        public void SendTaxiNearbyPush(Guid orderId, string ibsStatus, double? newLatitude, double? newLongitude)
        {
            using (var context = _contextFactory.Invoke())
            {
                var orderStatus = context.Query<OrderStatusDetail>().Single(x => x.OrderId == orderId);

                if (!ShouldSendNotification(orderStatus.AccountId, x => x.NearbyTaxiPush))
                {
                    return;
                }

                var shouldSendPushNotification = newLatitude.HasValue &&
                                                 newLongitude.HasValue &&
                                                 ibsStatus == VehicleStatuses.Common.Assigned &&
                                                 !orderStatus.IsTaxiNearbyNotificationSent;

                if (shouldSendPushNotification)
                {
                    var order = context.Find<OrderDetail>(orderId);

                    var taxiPosition = new Position(newLatitude.Value, newLongitude.Value);
                    var pickupPosition = new Position(order.PickupAddress.Latitude, order.PickupAddress.Longitude);

                    if (taxiPosition.DistanceTo(pickupPosition) <= TaxiDistanceThresholdForPushNotification)
                    {
                        orderStatus.IsTaxiNearbyNotificationSent = true;
                        context.Save(orderStatus);

                        var alert = string.Format(_resources.Get("PushNotification_NearbyTaxi", order.ClientLanguageCode));
                        var data = new Dictionary<string, object> { { "orderId", order.Id } };

                        SendPushOrSms(order.AccountId, alert, data);
                    }
                }
            }
        }

        public void SendAutomaticPairingPush(Guid orderId, int? autoTipPercentage, string last4Digits, bool success)
        {
            using (var context = _contextFactory.Invoke())
            {
                var order = context.Find<OrderDetail>(orderId);

                var alert = success
                    ? string.Format(_resources.Get("PushNotification_OrderPairingSuccessful", order.ClientLanguageCode), order.IBSOrderId, last4Digits, autoTipPercentage)
                    : string.Format(_resources.Get("PushNotification_OrderPairingFailed", order.ClientLanguageCode), order.IBSOrderId);

                var data = new Dictionary<string, object> { { "orderId", orderId } };

                SendPushOrSms(order.AccountId, alert, data);
            }
        }

        public void SendAccountConfirmationEmail(Uri confirmationUrl, string clientEmailAddress, string clientLanguageCode)
        {
            var templateData = new
            {
                confirmationUrl,
                ApplicationName = _configurationManager.GetSetting(ApplicationNameSetting),
                EmailFontColor = _configurationManager.GetSetting(EmailFontColorSetting),
                AccentColor = _configurationManager.GetSetting(AccentColorSetting),
                GetBaseUrls().LogoImg
            };

            SendEmail(clientEmailAddress, EmailConstant.Template.AccountConfirmation, EmailConstant.Subject.AccountConfirmation, templateData, clientLanguageCode);
        }

        public void SendAccountConfirmationSMS(string phoneNumber, string code, string clientLanguageCode)
        {
            var template = _resources.Get(SMSConstant.Template.AccountConfirmation, clientLanguageCode);
            var message = string.Format(template, _configurationManager.GetSetting(ApplicationNameSetting), code);

            SendSms(phoneNumber, message);
        }

        public void SendBookingConfirmationEmail(int ibsOrderId, string note, Address pickupAddress, Address dropOffAddress, DateTime pickupDate,
            SendBookingConfirmationEmail.BookingSettings settings, string clientEmailAddress, string clientLanguageCode, bool bypassNotificationSetting = false)
        {
            if (!bypassNotificationSetting)
            {
                using (var context = _contextFactory.Invoke())
                {
                    var account = context.Query<AccountDetail>().SingleOrDefault(c => c.Email.ToLower() == clientEmailAddress.ToLower());
                    if (account == null || !ShouldSendNotification(account.Id, x => x.BookingConfirmationEmail))
                    {
                        return;
                    }
                }
            }

            var hasDropOffAddress = dropOffAddress != null
                && (!string.IsNullOrWhiteSpace(dropOffAddress.FullAddress)
                    || !string.IsNullOrWhiteSpace(dropOffAddress.DisplayAddress));

            var templateData = new
            {
                ApplicationName = _configurationManager.GetSetting(ApplicationNameSetting),
                AccentColor = _configurationManager.GetSetting(AccentColorSetting),
                EmailFontColor = _configurationManager.GetSetting(EmailFontColorSetting),
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
                VisibilityLargeBags = _configurationManager.GetSetting("Client.ShowLargeBagsIndicator", false) || settings.LargeBags > 0,
                GetBaseUrls().LogoImg
            };

            SendEmail(clientEmailAddress, EmailConstant.Template.BookingConfirmation, EmailConstant.Subject.BookingConfirmation, templateData, clientLanguageCode);
        }

        public void SendPasswordResetEmail(string password, string clientEmailAddress, string clientLanguageCode)
        {
            var templateData = new
            {
                password,
                ApplicationName = _configurationManager.GetSetting(ApplicationNameSetting),
                AccentColor = _configurationManager.GetSetting(AccentColorSetting),
                EmailFontColor = _configurationManager.GetSetting(EmailFontColorSetting),
                GetBaseUrls().LogoImg
            };

            SendEmail(clientEmailAddress, EmailConstant.Template.PasswordReset, EmailConstant.Subject.PasswordReset, templateData, clientLanguageCode);
        }

        public void SendReceiptEmail(int ibsOrderId, string vehicleNumber, string driverName, double fare, double toll, double tip,
            double tax, double totalFare, SendReceipt.CardOnFile cardOnFileInfo, Address pickupAddress, Address dropOffAddress,
            DateTime pickupDate, DateTime? dropOffDate, string clientEmailAddress, string clientLanguageCode, bool bypassNotificationSetting = false)
        {
            if (!bypassNotificationSetting)
            {
                using (var context = _contextFactory.Invoke())
                {
                    var account = context.Query<AccountDetail>().SingleOrDefault(c => c.Email.ToLower() == clientEmailAddress.ToLower());
                    if (account == null || !ShouldSendNotification(account.Id, x => x.ReceiptEmail))
                    {
                        return;
                    }
                }
            }

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

            var hasDropOffAddress = dropOffAddress != null 
                && (!string.IsNullOrWhiteSpace(dropOffAddress.FullAddress) 
                    || !string.IsNullOrWhiteSpace(dropOffAddress.DisplayAddress));

            var staticMapUri = dropOffAddress != null
                ? _staticMap.GetStaticMapUri(
                    new Position(pickupAddress.Latitude, pickupAddress.Longitude),
                    new Position(dropOffAddress.Latitude, dropOffAddress.Longitude),
                    300, 300, 1)
                : "";

            var dropOffTime = dropOffDate.HasValue
                ? dropOffDate.Value.ToString("t" /* Short time pattern */)
                : "";
            var baseUrls = GetBaseUrls();
            var templateData = new
            {
                ApplicationName = _configurationManager.GetSetting(ApplicationNameSetting),
                AccentColor = _configurationManager.GetSetting(AccentColorSetting),
                EmailFontColor = _configurationManager.GetSetting(EmailFontColorSetting),
                ibsOrderId,
                vehicleNumber,
                driverName,
                PickupDate = pickupDate.ToString("dddd, MMMM d, yyyy"),
                PickupTime = pickupDate.ToString("t" /* Short time pattern */),
                DropOffDate = dropOffDate.HasValue 
                    ? dropOffDate.Value.ToString("dddd, MMMM d, yyyy")
                    : pickupDate.ToString("dddd, MMMM d, yyyy"), // assume it ends on the same day...
                DropOffTime = dropOffTime,
                ShowDropOffTime = !string.IsNullOrEmpty(dropOffTime),
                Fare = fare.ToString("C", priceFormat),
                Toll = toll.ToString("C", priceFormat),
                Tip = tip.ToString("C", priceFormat),
                TotalFare = totalFare.ToString("C", priceFormat),
                Note = _configurationManager.GetSetting("Receipt.Note"),
                Tax = tax.ToString("C", priceFormat),
                IsCardOnFile = isCardOnFile,
                CardOnFileAmount = cardOnFileAmount,
                CardNumber = cardNumber,
                CardOnFileTransactionId = cardOnFileTransactionId,
                CardOnFileAuthorizationCode = cardOnFileAuthorizationCode,
                PickupAddress = pickupAddress.DisplayAddress,
                DropOffAddress = hasDropOffAddress ? dropOffAddress.DisplayAddress : "-",
                SubTotal=(fare+toll+tip).ToString("C", priceFormat),
                StaticMapUri = staticMapUri,
                ShowStaticMap = !string.IsNullOrEmpty(staticMapUri),
                BaseUrlImg = baseUrls.BaseUrlAssetsImg,
                RedDotImg = String.Concat(baseUrls.BaseUrlAssetsImg, "email_red_dot.png"),
                GreenDotImg = String.Concat(baseUrls.BaseUrlAssetsImg, "email_green_dot.png"),
                GetBaseUrls().LogoImg,
                VehicleType = "taxi"

            };

            SendEmail(clientEmailAddress, EmailConstant.Template.Receipt, EmailConstant.Subject.Receipt, templateData, clientLanguageCode);
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

        private void SendPushOrSms(Guid accountId, string alert, Dictionary<string, object> data)
        {
            if (_appSettings.Data.SendPushAsSMS)
            {
                SendSms(accountId, alert);
            }
            else
            {
                SendPush(accountId, alert, data);
            }
        }

        private void SendPush(Guid accountId, string alert, Dictionary<string, object> data)
        {
            using (var context = _contextFactory.Invoke())
            {
                var devices = context.Set<DeviceDetail>().Where(x => x.AccountId == accountId);
                foreach (var device in devices)
                {
                    _pushNotificationService.Send(alert, data, device.DeviceToken, device.Platform);
                }
            }
        }

        private void SendSms(Guid accountId, string alert)
        {
            using (var context = _contextFactory.Invoke())
            {
                var account = context.Set<AccountDetail>().Find(accountId);
                var phoneNumber = account.Settings.Phone;

                if (string.IsNullOrWhiteSpace(phoneNumber))
                {
                    _logger.Maybe(() => _logger.LogMessage("Cannot send SMS, phone number in account is empty (account: {0})", accountId));
                    return;
                }

                SendSms(phoneNumber, alert);
            }
        }

        private void SendSms(string phoneNumber, string alert)
        {
            // TODO MKTAXI-1836 Support International number
            phoneNumber = phoneNumber.Length == 11
                              ? phoneNumber
                              : string.Concat("1", phoneNumber);

            _smsService.Send(phoneNumber, alert);
        }

        private bool ShouldSendNotification(Guid accountId, Expression<Func<NotificationSettings, bool?>> propertySelector)
        {
            var companySettings = _configurationDao.GetNotificationSettings();
            var accountSettings = _configurationDao.GetNotificationSettings(accountId);
            if (accountSettings == null)
            {
                // take company settings
                return companySettings.Enabled && GetValue(companySettings, propertySelector);
            }

            // if the account or the company disabled all notifications, then everything will be false
            var enabled = companySettings.Enabled && accountSettings.Enabled;

            // we have to check if the company setting has a value
            // if it doesn't, then the company has disabled the setting and must be false for everyone
            return enabled && GetValue(companySettings, propertySelector) && GetValue(accountSettings, propertySelector);
        }

        private bool GetValue(NotificationSettings settings, Expression<Func<NotificationSettings, bool?>> propertySelector)
        {
            var mexp = propertySelector.Body as MemberExpression;
            var propertyName = mexp.Member.Name;

            return (bool)settings.GetType().GetProperty(propertyName).GetValue(settings, null);
        }

        private BaseUrls GetBaseUrls()
        {
            if (_baseUrls == null)
            {
                throw new InvalidOperationException("BaseUrl not yet set");
            }
            return _baseUrls;
        }

        private class BaseUrls
        {
            public BaseUrls(Uri baseUrl, IConfigurationManager configurationManager)
            {
                LogoImg = String.Concat(baseUrl, "/themes/" + configurationManager.GetSetting(ApplicationKeySetting) + "/img/email_logo.png");
                BaseUrlAssetsImg = String.Concat(baseUrl, "/assets/img/");
            }

            public string LogoImg { get; private set; }

            public string BaseUrlAssetsImg { get; private set; }
        }

        public static class EmailConstant
        {
            public static class Subject
            {
                public const string PasswordReset = "Email_Subject_PasswordReset";
                public const string Receipt = "Email_Subject_Receipt";
                public const string AccountConfirmation = "Email_Subject_AccountConfirmation";
                public const string BookingConfirmation = "Email_Subject_BookingConfirmation";
            }

            public static class Template
            {
                public const string PasswordReset = "PasswordReset";
                public const string Receipt = "Receipt";
                public const string AccountConfirmation = "AccountConfirmation";
                public const string BookingConfirmation = "BookingConfirmation";
            }
        }

        private static class SMSConstant
        {
            public static class Template
            {
                public const string AccountConfirmation = "AccountConfirmationSmsBody";
            }
        }
    }
}