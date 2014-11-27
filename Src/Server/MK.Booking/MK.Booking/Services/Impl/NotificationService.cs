using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.Database;
using apcurium.MK.Booking.Email;
using apcurium.MK.Booking.Maps;
using apcurium.MK.Booking.Maps.Geo;
using apcurium.MK.Booking.Maps.Impl;
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

        private readonly Func<BookingDbContext> _contextFactory;
        private readonly IPushNotificationService _pushNotificationService;
        private readonly ITemplateService _templateService;
        private readonly IEmailSender _emailSender;
        private readonly IServerSettings _serverSettings;
        private readonly IConfigurationDao _configurationDao;
        private readonly IOrderDao _orderDao;
        private readonly IStaticMap _staticMap;
        private readonly ISmsService _smsService;
        private readonly IGeocoding _geocoding;
        private readonly ILogger _logger;
        private readonly Booking.Resources.Resources _resources;

        private BaseUrls _baseUrls;

        public NotificationService(
            Func<BookingDbContext> contextFactory, 
            IPushNotificationService pushNotificationService,
            ITemplateService templateService,
            IEmailSender emailSender,
            IServerSettings serverSettings,
            IConfigurationDao configurationDao,
            IOrderDao orderDao,
            IStaticMap staticMap,
            ISmsService smsService,
            IGeocoding geocoding,
            ILogger logger)
        {
            _contextFactory = contextFactory;
            _pushNotificationService = pushNotificationService;
            _templateService = templateService;
            _emailSender = emailSender;
            _serverSettings = serverSettings;
            _configurationDao = configurationDao;
            _orderDao = orderDao;
            _staticMap = staticMap;
            _smsService = smsService;
            _geocoding = geocoding;
            _logger = logger;

            _resources = new Booking.Resources.Resources(serverSettings);
        }

        public void SetBaseUrl(Uri baseUrl)
        {
            this._baseUrls = new BaseUrls(baseUrl, _serverSettings);
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
            if (_serverSettings.GetPaymentSettings().AutomaticPayment
                    && !_serverSettings.GetPaymentSettings().AutomaticPaymentPairing
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

        public void SendChangeDispatchCompanyPush(Guid orderId)
        {
            var order = _orderDao.FindById(orderId);
            SendPushOrSms(order.AccountId,
                        string.Format(_resources.Get("PushNotification_ChangeNetworkCompany", order.ClientLanguageCode), order.CompanyName),
                        new Dictionary<string, object> { { "orderId", orderId }, { "isPairingNotification", false } });
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

                var formattedAmount = _resources.FormatPrice(Convert.ToDouble(amount));
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
            string imageLogoUrl = GetRefreshableImageUrl(GetBaseUrls().LogoImg);

            var templateData = new
            {
                confirmationUrl,
                ApplicationName = _serverSettings.ServerData.TaxiHail.ApplicationName,
                EmailFontColor = _serverSettings.ServerData.TaxiHail.EmailFontColor,
                AccentColor = _serverSettings.ServerData.TaxiHail.AccentColor,
                LogoImg = imageLogoUrl
            };

            SendEmail(clientEmailAddress, EmailConstant.Template.AccountConfirmation, EmailConstant.Subject.AccountConfirmation, templateData, clientLanguageCode);
        }

        public void SendAccountConfirmationSMS(string phoneNumber, string code, string clientLanguageCode)
        {
            var template = _resources.Get(SMSConstant.Template.AccountConfirmation, clientLanguageCode);
            var message = string.Format(template, _serverSettings.ServerData.TaxiHail.ApplicationName, code);

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

            var dateFormat = CultureInfo.GetCultureInfo(clientLanguageCode.IsNullOrEmpty()
                    ? SupportedLanguages.en.ToString()
                    : clientLanguageCode);

            string imageLogoUrl = GetRefreshableImageUrl(GetBaseUrls().LogoImg);

            var templateData = new
            {
                ApplicationName = _serverSettings.ServerData.TaxiHail.ApplicationName,
                AccentColor = _serverSettings.ServerData.TaxiHail.AccentColor,
                EmailFontColor = _serverSettings.ServerData.TaxiHail.EmailFontColor,
                ibsOrderId,
                PickupDate = pickupDate.ToString("D", dateFormat),
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
                LogoImg = imageLogoUrl
            };

            SendEmail(clientEmailAddress, EmailConstant.Template.BookingConfirmation, EmailConstant.Subject.BookingConfirmation, templateData, clientLanguageCode);
        }

        public void SendPasswordResetEmail(string password, string clientEmailAddress, string clientLanguageCode)
        {
            string imageLogoUrl = GetRefreshableImageUrl(GetBaseUrls().LogoImg);

            var templateData = new
            {
                password,
                ApplicationName = _serverSettings.ServerData.TaxiHail.ApplicationName,
                AccentColor = _serverSettings.ServerData.TaxiHail.AccentColor,
                EmailFontColor = _serverSettings.ServerData.TaxiHail.EmailFontColor,
                LogoImg = imageLogoUrl
            };

            SendEmail(clientEmailAddress, EmailConstant.Template.PasswordReset, EmailConstant.Subject.PasswordReset, templateData, clientLanguageCode);
        }

        public void SendReceiptEmail(Guid orderId, int ibsOrderId, string vehicleNumber, DriverInfos driverInfos, double fare, double toll, double tip,
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

            var vatIsEnabled = _serverSettings.ServerData.VATIsEnabled;

            var dateFormat = CultureInfo.GetCultureInfo(clientLanguageCode);

            var isCardOnFile = cardOnFileInfo != null;
            var cardOnFileAmount = string.Empty;
            var cardNumber = string.Empty;
            var cardOnFileTransactionId = string.Empty;
            var cardOnFileAuthorizationCode = string.Empty;

            if (isCardOnFile)
            {
                cardOnFileAmount = _resources.FormatPrice(Convert.ToDouble(cardOnFileInfo.Amount));
                cardNumber = cardOnFileInfo.Company;
                cardOnFileAuthorizationCode = cardOnFileInfo.AuthorizationCode;

                if (!string.IsNullOrWhiteSpace(cardOnFileInfo.LastFour))
                {
                    cardNumber += " XXXX " + cardOnFileInfo.LastFour;
                }

                cardOnFileTransactionId = cardOnFileInfo.TransactionId;
            }

            var addressToUseForDropOff = TryToGetExactDropOffAddress(orderId, dropOffAddress, clientLanguageCode);
            var positionForStaticMap = TryToGetPositionOfDropOffAddress(orderId, dropOffAddress);
            
            var hasDropOffAddress = addressToUseForDropOff != null
                && (!string.IsNullOrWhiteSpace(addressToUseForDropOff.FullAddress)
                    || !string.IsNullOrWhiteSpace(addressToUseForDropOff.DisplayAddress));

            var points = _orderDao.GetVehiclePositions(orderId);
            var encodedPath = PathUtility.GetEncodedPolylines(points);
            var staticMapUri = positionForStaticMap.HasValue
                ? _staticMap.GetStaticMapUri(
                    new Position(pickupAddress.Latitude, pickupAddress.Longitude),
                    positionForStaticMap.Value, encodedPath,
                    300, 300, 1)
                : string.Empty;

            var dropOffTime = dropOffDate.HasValue
                ? dropOffDate.Value.ToString("t" /* Short time pattern */)
                : string.Empty;
            var baseUrls = GetBaseUrls();

            string imageLogoUrl = GetRefreshableImageUrl(baseUrls.LogoImg);

            var templateData = new
            {
                // template is missing the toll, if we decide to add it, we need to make sure we hide it if it's empty
                ApplicationName = _serverSettings.ServerData.TaxiHail.ApplicationName,
                AccentColor = _serverSettings.ServerData.TaxiHail.AccentColor,
                EmailFontColor = _serverSettings.ServerData.TaxiHail.EmailFontColor,
                ibsOrderId,
                VehicleNumber = vehicleNumber,
                DriverInfos = driverInfos,
                PickupDate = pickupDate.ToString("D", dateFormat),
                PickupTime = pickupDate.ToString("t", dateFormat /* Short time pattern */),
                DropOffDate = dropOffDate.HasValue
                    ? dropOffDate.Value.ToString("D", dateFormat)
                    : pickupDate.ToString("D", dateFormat), // assume it ends on the same day...
                DropOffTime = dropOffTime,
                ShowDropOffTime = !string.IsNullOrEmpty(dropOffTime),
                Fare = _resources.FormatPrice(fare),
                Toll = _resources.FormatPrice(toll),
                Tip = _resources.FormatPrice(tip),
                TotalFare = _resources.FormatPrice(totalFare),
                Note = _serverSettings.ServerData.Receipt.Note,
                Tax = _resources.FormatPrice(tax),
                vatIsEnabled,
                IsCardOnFile = isCardOnFile,
                CardOnFileAmount = cardOnFileAmount,
                CardNumber = cardNumber,
                CardOnFileTransactionId = cardOnFileTransactionId,
                CardOnFileAuthorizationCode = cardOnFileAuthorizationCode,
                PickupAddress = pickupAddress.DisplayAddress,
                DropOffAddress = hasDropOffAddress ? addressToUseForDropOff.DisplayAddress : "-",
                SubTotal = _resources.FormatPrice(totalFare - tip), // represents everything except tip
                StaticMapUri = staticMapUri,
                ShowStaticMap = !string.IsNullOrEmpty(staticMapUri),
                BaseUrlImg = baseUrls.BaseUrlAssetsImg,
                RedDotImg = String.Concat(baseUrls.BaseUrlAssetsImg, "email_red_dot.png"),
                GreenDotImg = String.Concat(baseUrls.BaseUrlAssetsImg, "email_green_dot.png"),
                LogoImg = imageLogoUrl
            };

            SendEmail(clientEmailAddress, EmailConstant.Template.Receipt, EmailConstant.Subject.Receipt, templateData, clientLanguageCode);
        }

        private Address TryToGetExactDropOffAddress(Guid orderId, Address dropOffAddress, string clientLanguageCode)
        {
            var orderStatus = _orderDao.FindOrderStatusById(orderId);
            if (orderStatus == null
                || !orderStatus.VehicleLatitude.HasValue
                || !orderStatus.VehicleLongitude.HasValue)
            {
                return dropOffAddress;
            }

            // Find the exact dropoff address using the last vehicle position
            var exactDropOffAddress = _geocoding.Search(
                orderStatus.VehicleLatitude.Value,
                orderStatus.VehicleLongitude.Value,
                clientLanguageCode).FirstOrDefault();

            return exactDropOffAddress ?? dropOffAddress;
        }

        private Position? TryToGetPositionOfDropOffAddress(Guid orderId, Address dropOffAddress)
        {
            var orderStatus = _orderDao.FindOrderStatusById(orderId);
            if (orderStatus != null 
                && orderStatus.VehicleLatitude.HasValue 
                && orderStatus.VehicleLongitude.HasValue)
            {
                return new Position(orderStatus.VehicleLatitude.Value, orderStatus.VehicleLongitude.Value);
            }
                
            if (dropOffAddress != null)
            {
                return new Position(dropOffAddress.Latitude, dropOffAddress.Longitude);
            }

            return null;
        }

        private void SendEmail(string to, string bodyTemplate, string subjectTemplate, object templateData, string languageCode, params KeyValuePair<string, string>[] embeddedIMages)
        {
            var messageSubject = _templateService.Render(_resources.Get(subjectTemplate, languageCode), templateData);

            var template = _templateService.Find(bodyTemplate, languageCode);
            if (template == null)
            {
                throw new InvalidOperationException("Template not found: " + bodyTemplate);
            }
                
            var mailMessage = new MailMessage(_serverSettings.ServerData.Email.NoReply, to, messageSubject, null)
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
            try
            {
                if (_serverSettings.ServerData.SendPushAsSMS)
                {
                    SendSms(accountId, alert);
                }
                else
                {
                    SendPush(accountId, alert, data);
                }
            }
            catch (Exception ex)
            {
                _logger.Maybe(() => _logger.LogError(ex));
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
            public BaseUrls(Uri baseUrl, IServerSettings serverSettings)
            {
                LogoImg = String.Concat(baseUrl, "/themes/" + serverSettings.ServerData.TaxiHail.ApplicationKey + "/img/email_logo.png");
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

        /// <summary>
        /// This method generates a unique URL that will ensure that the image
        /// will not be cached be the browser or email clients if it has changed.
        /// </summary>
        /// <param name="imageUrl">The URL of the image.</param>
        /// <returns>An unique image URL based on the image content.</returns>
        private string GetRefreshableImageUrl(string imageUrl)
        {
            using (var webClient = new WebClient())
            {
                try
                {
                    // Get the image
                    var imageData = webClient.DownloadData(imageUrl);
                    if (imageData != null)
                    {
                        // Hash it
                        var md5Hasher = MD5.Create();
                        var hashedImagedata = md5Hasher.ComputeHash(imageData);

                        var sBuilder = new StringBuilder();

                        foreach (byte b in hashedImagedata)
                        {
                            sBuilder.Append(b.ToString("x2"));
                        }

                        // Append its hash to its URL
                        return string.Format("{0}?refresh={1}", imageUrl, sBuilder);
                    }

                    return imageUrl;
                }
                catch (Exception ex)
                {
                    if (_logger != null)
                    {
                        _logger.LogError(ex);
                    }
                    
                    return imageUrl;
                }
            }
        }
    }
}