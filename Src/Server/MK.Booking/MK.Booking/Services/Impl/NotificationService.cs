using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
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
using apcurium.MK.Common.Cryptography;
using apcurium.MK.Common.Diagnostic;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Enumeration;
using apcurium.MK.Common.Enumeration.TimeZone;
using apcurium.MK.Common.Extensions;
using CustomerPortal.Client;
using MK.Common.Configuration;
using Infrastructure.Messaging;

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
        private readonly IOrderNotificationsDetailDao _orderNotificationDetailDao;
        private readonly IAccountDao _accountDao;
        private readonly IStaticMap _staticMap;
        private readonly ISmsService _smsService;
        private readonly IGeocoding _geocoding;
        private readonly ICommandBus _commandBus;
        private readonly ITaxiHailNetworkServiceClient _taxiHailNetworkServiceClient;
        private readonly ILogger _logger;
        private readonly Resources.Resources _resources;

        private BaseUrls _baseUrls;

        public NotificationService(
            Func<BookingDbContext> contextFactory, 
            IPushNotificationService pushNotificationService,
            ITemplateService templateService,
            IEmailSender emailSender,
            IServerSettings serverSettings,
            IConfigurationDao configurationDao,
            IOrderDao orderDao,
            IOrderNotificationsDetailDao orderNotificationDetailDao,
            IAccountDao accountDao,
            IStaticMap staticMap,
            ISmsService smsService,
            IGeocoding geocoding,
            ITaxiHailNetworkServiceClient taxiHailNetworkServiceClient,
            ILogger logger,
            ICommandBus commandBus)
        {
            _contextFactory = contextFactory;
            _pushNotificationService = pushNotificationService;
            _templateService = templateService;
            _emailSender = emailSender;
            _serverSettings = serverSettings;
            _configurationDao = configurationDao;
            _orderDao = orderDao;
            _orderNotificationDetailDao = orderNotificationDetailDao;
            _accountDao = accountDao;
            _staticMap = staticMap;
            _smsService = smsService;
            _geocoding = geocoding;
            _taxiHailNetworkServiceClient = taxiHailNetworkServiceClient;
            _logger = logger;
            _commandBus = commandBus;
            _resources = new Resources.Resources(serverSettings);
        }

        public void SetBaseUrl(Uri baseUrl)
        {
            this._baseUrls = new BaseUrls(baseUrl, _serverSettings);
        }
        
        public void SendPromotionUnlockedPush(Guid accountId, PromotionDetail promotionDetail)
        {
            var account = _accountDao.FindById(accountId);
            if (ShouldSendNotification(accountId, x => x.PromotionUnlockedPush))
            {
                SendPushOrSms(accountId,
                    string.Format(_resources.Get("PushNotification_PromotionUnlocked", account.Language), promotionDetail.Name, promotionDetail.Code),
                    new Dictionary<string, object>());
            }
        }

        public void SendAssignedPush(OrderStatusDetail orderStatusDetail)
        {
            var order = _orderDao.FindById(orderStatusDetail.OrderId);
            if (ShouldSendNotification(order.AccountId, x => x.DriverAssignedPush))
            {
                SendPushOrSms(order.AccountId,
                    string.Format(_resources.Get("PushNotification_wosASSIGNED", order.ClientLanguageCode, order.Settings.ServiceType == ServiceType.Luxury ? "luxury" : null), 
                    orderStatusDetail.VehicleNumber),
                    new Dictionary<string, object> { { "orderId", order.Id }, { "isPairingNotification", false } });
            }
        }

        public void SendArrivedPush(OrderStatusDetail orderStatusDetail)
        {
            var order = _orderDao.FindById(orderStatusDetail.OrderId);
            var orderNotificationDetails = _orderNotificationDetailDao.FindByOrderId(orderStatusDetail.OrderId);

            if (orderNotificationDetails != null && orderNotificationDetails.NoShowWarningSent)
                return; // leave the previous warning message up so the user can see it


            if (ShouldSendNotification(order.AccountId, x => x.VehicleAtPickupPush))
            {
                SendPushOrSms(order.AccountId,
                    string.Format(_resources.Get("PushNotification_wosARRIVED", order.ClientLanguageCode, order.Settings.ServiceType == ServiceType.Luxury ? "luxury" : null), 
                    orderStatusDetail.VehicleNumber),
                    new Dictionary<string, object> { { "orderId", order.Id }, { "isPairingNotification", false } });
            }
        }

        public void SendTimeoutPush(OrderStatusDetail orderStatusDetail)
        {
            var order = _orderDao.FindById(orderStatusDetail.OrderId); 
            SendPushOrSms(order.AccountId,
                        _resources.Get("PushNotification_wosTIMEOUT", order.ClientLanguageCode),
                        new Dictionary<string, object>());
        }

        public void SendBailedPush(OrderStatusDetail orderStatusDetail)
        {
            var order = _orderDao.FindById(orderStatusDetail.OrderId);
            if (ShouldSendNotification(order.AccountId, x => x.DriverBailedPush))
            {
                SendPushOrSms(order.AccountId,
                        _resources.Get("PushNotification_BAILED", order.ClientLanguageCode, order.Settings.ServiceType == ServiceType.Luxury ? "luxury" : null),
                    new Dictionary<string, object>());
            }
        }

        public void SendNoShowPush(OrderStatusDetail orderStatusDetail)
        {
            var order = _orderDao.FindById(orderStatusDetail.OrderId);
            if (ShouldSendNotification(order.AccountId, x => x.NoShowPush))
            {
                SendPushOrSms(order.AccountId,
                    _resources.Get("PushNotification_wosNOSHOW", order.ClientLanguageCode),
                    new Dictionary<string, object>());
            }
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
            var order = _orderDao.FindById(orderId);

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

        public void SendTaxiNearbyPush(Guid orderId, string ibsStatus, double? newLatitude, double? newLongitude)
        {
            var order = _orderDao.FindById(orderId);

            if (!ShouldSendNotification(order.AccountId, x => x.NearbyTaxiPush))
            {
                return;
            }

            using (var context = _contextFactory.Invoke())
            {
                var orderNotifications = context.Query<OrderNotificationDetail>().SingleOrDefault(x => x.Id == orderId);

                var shouldSendPushNotification =
                    newLatitude.HasValue
                    && newLongitude.HasValue
                    && ibsStatus == VehicleStatuses.Common.Assigned
                    && (orderNotifications == null || !orderNotifications.IsTaxiNearbyNotificationSent);

                if (shouldSendPushNotification)
                {
                    var taxiPosition = new Position(newLatitude.Value, newLongitude.Value);
                    var pickupPosition = new Position(order.PickupAddress.Latitude, order.PickupAddress.Longitude);

                    if (taxiPosition.DistanceTo(pickupPosition) <= TaxiDistanceThresholdForPushNotification)
                    {
                        _commandBus.Send(new UpdateOrderNotificationDetail
                        {
                            IsTaxiNearbyNotificationSent = true,
                            OrderId = orderId
                        });

                        var alert = string.Format(_resources.Get("PushNotification_NearbyTaxi", order.ClientLanguageCode));
                        var data = new Dictionary<string, object> { { "orderId", order.Id } };

                        SendPushOrSms(order.AccountId, alert, data);
                    }
                }
            }            
        }

        public void SendUnpairingReminderPush(Guid orderId)
        {
            var order = _orderDao.FindById(orderId);

            using (var context = _contextFactory.Invoke())
            {
                var orderNotifications = context.Query<OrderNotificationDetail>().SingleOrDefault(x => x.Id == orderId);

                if (!ShouldSendNotification(order.AccountId, x => x.UnpairingReminderPush)
                    || (orderNotifications != null && orderNotifications.IsUnpairingReminderNotificationSent))
                {
                    return;
                }

                _commandBus.Send(new UpdateOrderNotificationDetail
                {
                    IsUnpairingReminderNotificationSent = true,
                    OrderId = orderId
                });
            }

            var alert = string.Format(_resources.Get("PushNotification_OrderUnpairingTimeOutWarning", order.ClientLanguageCode));
            var data = new Dictionary<string, object> { { "orderId", orderId } };

            SendPushOrSms(order.AccountId, alert, data);
        }

        public void SendAutomaticPairingPush(Guid orderId, CreditCardDetails creditCard, int autoTipPercentage, bool success, string errorMessageKey = "")
        {
            var order = _orderDao.FindById(orderId);
            var isPayPal = order.Settings.ChargeTypeId == ChargeTypes.PayPal.Id;
            var isAutomaticPairingEnabled = !_serverSettings.GetPaymentSettings(order.CompanyKey).IsUnpairingDisabled;

            string successMessage;
            if (isPayPal)
            {
                successMessage = string.Format(
                    isAutomaticPairingEnabled
                        ? _resources.Get("PushNotification_OrderPairingSuccessfulPayPalUnpair", order.ClientLanguageCode)
                        : _resources.Get("PushNotification_OrderPairingSuccessfulPayPal", order.ClientLanguageCode),
                    order.IBSOrderId,
                    autoTipPercentage);
            }
            else
            {
                successMessage = string.Format(
                    isAutomaticPairingEnabled
                        ? _resources.Get("PushNotification_OrderPairingSuccessfulUnpair", order.ClientLanguageCode)
                        : _resources.Get("PushNotification_OrderPairingSuccessful", order.ClientLanguageCode),
                    order.IBSOrderId,
                    creditCard != null ? creditCard.Last4Digits : "",
                    autoTipPercentage);
            }

                errorMessageKey = errorMessageKey.IsNullOrEmpty()
                    ? "PushNotification_OrderPairingFailed"
                    : errorMessageKey;

            var alert = success
                ? successMessage
                    : string.Format(_resources.Get(errorMessageKey, order.ClientLanguageCode), order.IBSOrderId);

            var data = new Dictionary<string, object> { { "orderId", orderId } };

            SendPushOrSms(order.AccountId, alert, data);
        }

        public void SendOrderCreationErrorPush(Guid orderId, string errorDescription)
        {
            var order = _orderDao.FindById(orderId);

            var data = new Dictionary<string, object> { { "orderId", orderId } };

            SendPushOrSms(order.AccountId, errorDescription, data);
        }

        public void SendAccountConfirmationEmail(Uri confirmationUrl, string clientEmailAddress, string clientLanguageCode)
        {
            string imageLogoUrl = GetRefreshableImageUrl(GetBaseUrls().LogoImg);
            string imageAppleLogoUrl = GetRefreshableImageUrl(GetBaseUrls().AppleLogoImg);
            string imagePlayLogoUrl = GetRefreshableImageUrl(GetBaseUrls().PlayLogoImg);

            var templateData = new
            {
                confirmationUrl = new Uri(UrlCombine(_baseUrls.Uri.ToString(), confirmationUrl.ToString())),
                ApplicationName = _serverSettings.ServerData.TaxiHail.ApplicationName,
                EmailFontColor = _serverSettings.ServerData.TaxiHail.EmailFontColor,
                AccentColor = _serverSettings.ServerData.TaxiHail.AccentColor,
                LogoImg = imageLogoUrl,
                PlayLogoImg = imagePlayLogoUrl,
                AppleLogoImg = imageAppleLogoUrl,
                PlayLink = _serverSettings.ServerData.Store.PlayLink,
                AppleLink = _serverSettings.ServerData.Store.AppleLink
            };

            SendEmail(clientEmailAddress, EmailConstant.Template.AccountConfirmation, EmailConstant.Subject.AccountConfirmation, templateData, clientLanguageCode);
        }

        private string UrlCombine(string url1, string url2)
        {
            if (url1.Length == 0)
            {
                return url2;
            }

            if (url2.Length == 0)
            {
                return url1;
            }

            url1 = url1.TrimEnd('/', '\\');
            url2 = url2.TrimStart('/', '\\');

            return string.Format("{0}/{1}", url1, url2);
        }

        public void SendAccountConfirmationSMS(CountryISOCode countryCode, string phoneNumber, string code, string clientLanguageCode)
        {
            var template = _resources.Get(SMSConstant.Template.AccountConfirmation, clientLanguageCode);
            var message = string.Format(template, _serverSettings.ServerData.TaxiHail.ApplicationName, code);

            libphonenumber.PhoneNumber toPhoneNumber = new libphonenumber.PhoneNumber();
            toPhoneNumber.CountryCode = CountryCode.GetCountryCodeByIndex(CountryCode.GetCountryCodeIndexByCountryISOCode(countryCode)).CountryDialCode;
            toPhoneNumber.NationalNumber = long.Parse(phoneNumber);
            toPhoneNumber.ItalianLeadingZero = (phoneNumber[0] == '0');

            SendSms(toPhoneNumber, message);
        }

        public void SendBookingConfirmationEmail(int ibsOrderId, string note, Address pickupAddress, Address dropOffAddress, DateTime pickupDate,
            SendBookingConfirmationEmail.BookingSettings settings, string clientEmailAddress, string clientLanguageCode, bool bypassNotificationSetting = false)
        {
            if (!bypassNotificationSetting)
            {
                var account = _accountDao.FindByEmail(clientEmailAddress.ToLower());
                if (account == null || !ShouldSendNotification(account.Id, x => x.BookingConfirmationEmail))
                {
                    return;
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
                LogoImg = imageLogoUrl,
                ShowOrderNumber = _serverSettings.ServerData.ShowOrderNumber
            };

            SendEmail(clientEmailAddress, EmailConstant.Template.BookingConfirmation, EmailConstant.Subject.BookingConfirmation, templateData, clientLanguageCode, _serverSettings.ServerData.Email.CC);
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

        public void SendCancellationFeesReceiptEmail(int ibsOrderId, double feeAmount, string last4Digits,
            string clientEmailAddress, string clientLanguageCode, bool bypassNotificationSetting = false)
        {
            if (!bypassNotificationSetting)
            {
                var account = _accountDao.FindByEmail(clientEmailAddress.ToLower());
                if (account == null || !ShouldSendNotification(account.Id, x => x.ReceiptEmail))
                {
                    return;
                }
            }
            
            var imageLogoUrl = GetRefreshableImageUrl(GetBaseUrls().LogoImg);

            var templateData = new
            {
                ApplicationName = _serverSettings.ServerData.TaxiHail.ApplicationName,
                AccentColor = _serverSettings.ServerData.TaxiHail.AccentColor,
                EmailFontColor = _serverSettings.ServerData.TaxiHail.EmailFontColor,
                LogoImg = imageLogoUrl,
                IbsOrderId = ibsOrderId,
                FeeAmount = _resources.FormatPrice(feeAmount),
                Last4Digits = last4Digits
            };

            SendEmail(clientEmailAddress, EmailConstant.Template.CancellationFeesReceipt, EmailConstant.Subject.CancellationFeesReceipt, templateData, clientLanguageCode);
        }

        public void SendNoShowFeesReceiptEmail(int ibsOrderId, double feeAmount, Address pickUpAddress, string last4Digits,
            string clientEmailAddress, string clientLanguageCode, bool bypassNotificationSetting = false)
        {
            if (!bypassNotificationSetting)
            {
                var account = _accountDao.FindByEmail(clientEmailAddress.ToLower());
                if (account == null || !ShouldSendNotification(account.Id, x => x.ReceiptEmail))
                {
                    return;
                }
            }

            var imageLogoUrl = GetRefreshableImageUrl(GetBaseUrls().LogoImg);

            var templateData = new
            {
                ApplicationName = _serverSettings.ServerData.TaxiHail.ApplicationName,
                AccentColor = _serverSettings.ServerData.TaxiHail.AccentColor,
                EmailFontColor = _serverSettings.ServerData.TaxiHail.EmailFontColor,
                LogoImg = imageLogoUrl,
                IbsOrderId = ibsOrderId,
                FeeAmount = _resources.FormatPrice(feeAmount),
                Last4Digits = last4Digits,
                PickUpAddress = pickUpAddress.DisplayAddress
            };

            SendEmail(clientEmailAddress, EmailConstant.Template.NoShowFeesReceipt, EmailConstant.Subject.NoShowFeesReceipt, templateData, clientLanguageCode);
        }


        public void SendTripReceiptEmail(Guid orderId, int ibsOrderId, string vehicleNumber, DriverInfos driverInfos, double fare, double toll, double tip,
            double tax, double extra, double surcharge, double bookingFees, double totalFare, SendReceipt.Payment paymentInfo, Address pickupAddress, Address dropOffAddress,
            DateTime pickupDate, DateTime? dropOffDateInUtc, string clientEmailAddress, string clientLanguageCode, double amountSavedByPromotion, string promoCode,
            SendReceipt.CmtRideLinqReceiptFields cmtRideLinqFields, bool bypassNotificationSetting = false, bool isUpdatedReceipt = false, double additionalGratuity = 0)
        {
            if (!bypassNotificationSetting)
            {
                var account = _accountDao.FindByEmail(clientEmailAddress.ToLower());
                if (account == null || !ShouldSendNotification(account.Id, x => x.ReceiptEmail))
                {
                    return;
                }
            }

            try
            {
                // Email formatting is different for CMTRideLinQ
                var isCmtRideLinqReceipt = cmtRideLinqFields != null;

                var vatIsEnabled = _serverSettings.ServerData.VATIsEnabled;
                var dateFormat = CultureInfo.GetCultureInfo(clientLanguageCode);

                if (vatIsEnabled && tax == 0)
                {
                    //aexid hotfix compute tax amount from fare
                    var newFare = FareHelper.GetFareFromAmountInclTax(fare, _serverSettings.ServerData.VATPercentage);
                    tax = Convert.ToDouble(newFare.TaxAmount);
                    fare = Convert.ToDouble(newFare.AmountExclTax);
                }

                var hasPaymentInfo = paymentInfo != null;

                var hasDriverInfo = driverInfos != null && (driverInfos.FullName.HasValue() || driverInfos.VehicleMake != null || driverInfos.VehicleModel != null);

                _logger.LogMessage(String.Format("showTripSection = hasDriverInfo = {0}", hasDriverInfo.ToString()));

                var showMinimalDriverInfo = !hasDriverInfo && cmtRideLinqFields != null;
                var showTripSection = hasDriverInfo || cmtRideLinqFields != null;
                var paymentAmount = string.Empty;
                var paymentMethod = string.Empty;
                var paymentTransactionId = string.Empty;
                var paymentAuthorizationCode = string.Empty;

                var hasFare = Math.Abs(fare) > double.Epsilon;
                var hasCmtTollDetails = cmtRideLinqFields != null
                    && cmtRideLinqFields.Tolls != null
                    && cmtRideLinqFields.Tolls.Length > 0;
                var showFareAndPaymentDetails = hasPaymentInfo || (!_serverSettings.ServerData.HideFareInfoInReceipt && hasFare);

                int? rateClassStart = null;
                int? rateClassEnd = null;
                double? fareAtAlternateRate = null;
                double tipIncentive = 0;

                // RideLinQ Rate class & fare
                if (cmtRideLinqFields != null)
                {
                    rateClassStart = cmtRideLinqFields.RateAtTripStart;
                    tipIncentive = cmtRideLinqFields.TipIncentive;

                    if (cmtRideLinqFields.RateAtTripStart != cmtRideLinqFields.RateAtTripEnd)
                    {
                        rateClassEnd = cmtRideLinqFields.RateAtTripEnd;
                        fareAtAlternateRate = cmtRideLinqFields.FareAtAlternateRate ?? fare;
                    }
                }

                if (hasPaymentInfo)
                {
                    paymentAmount = _resources.FormatPrice(Convert.ToDouble(paymentInfo.Amount));
                    paymentMethod = paymentInfo.Company;
                    paymentAuthorizationCode = paymentInfo.AuthorizationCode;

                    if (!string.IsNullOrWhiteSpace(paymentInfo.Last4Digits))
                    {
                        paymentMethod += " XXXX " + paymentInfo.Last4Digits;
                    }

                    paymentTransactionId = paymentInfo.TransactionId;
                }

                var orderStatusDetail = _orderDao.FindOrderStatusById(orderId);

                var latitude = cmtRideLinqFields == null
                    ? null
                    : cmtRideLinqFields.LastLatitudeOfVehicle;

                var longitude = cmtRideLinqFields == null
                    ? null
                    : cmtRideLinqFields.LastLongitudeOfVehicle;

                var addressToUseForDropOff = _geocoding.TryToGetExactDropOffAddress(orderStatusDetail, latitude, longitude, dropOffAddress, clientLanguageCode);
                var positionForStaticMap = TryToGetPositionOfDropOffAddress(orderId, dropOffAddress, cmtRideLinqFields);

                var serviceType = orderStatusDetail.ServiceType.ToString();

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

                var timeZoneOfTheOrder = TryToGetOrderTimeZone(orderId);
                var localDropOffDate = cmtRideLinqFields.SelectOrDefault(x => x.DropOffDateTime);
                var nullSafeDropOffDate = localDropOffDate ?? GetNullSafeDropOffDate(timeZoneOfTheOrder, dropOffDateInUtc, pickupDate);
                var dropOffTime = dropOffDateInUtc.HasValue || localDropOffDate.HasValue
                    ? nullSafeDropOffDate.ToString("t", dateFormat /* Short time pattern */)
                    : string.Empty;

                var baseUrls = GetBaseUrls();
                var imageLogoUrl = GetRefreshableImageUrl(baseUrls.LogoImg);

                if (additionalGratuity > 0)
                    tip = 0;
                    
                var totalAmount = fare + toll + tax + tip + bookingFees + surcharge + tipIncentive + extra + additionalGratuity - amountSavedByPromotion
                        + (cmtRideLinqFields.SelectOrDefault(x => x.FareAtAlternateRate) ?? 0.0)
                        + (cmtRideLinqFields.SelectOrDefault(x => x.AccessFee) ?? 0.0);

               

                
                var showOrderNumber = _serverSettings.ServerData.ShowOrderNumber;

                var marketSpecificNote = string.Empty;
                var receiptLabels = new Dictionary<string, string>();
                try
                {
                    var marketSettings = _taxiHailNetworkServiceClient.GetCompanyMarketSettings(pickupAddress.Latitude, pickupAddress.Longitude);

                    if (marketSettings != null)
                    {
                        if (marketSettings.ReceiptFooter.HasValueTrimmed())
                        {
                            marketSpecificNote = string.Format("<br>{0}", marketSettings.ReceiptFooter);
                        }
                    }

                    //Retrieve labels for the receipt
                    if (marketSettings.ReceiptLines != null)
                    {
                        //Create a dictionary for the current language
                        foreach (var receiptLine in marketSettings.ReceiptLines)
                        {
                            string label;
                            receiptLine.Value.TryGetValue(clientLanguageCode, out label);
                            receiptLabels.Add(receiptLine.Key, label);
                        }
                    }
                }
                catch (Exception)
                {
                    _logger.LogMessage("Could not get market settings [Called GetCompanyMarketSettings with for lat:{0} lng:{1}]", latitude, longitude);
                }

                var emailBodyFare = GetReceiptLabelOrDefault(receiptLabels, "Email_Body_Fare", clientLanguageCode);
                var emailBodyExtra = GetReceiptLabelOrDefault(receiptLabels, "Email_Body_Extra", clientLanguageCode);
                var emailBodySurcharge = GetReceiptLabelOrDefault(receiptLabels, "Email_Body_Surcharge", clientLanguageCode);
                var emailBodyToll = GetReceiptLabelOrDefault(receiptLabels, "Email_Body_Toll", clientLanguageCode);
                var emailBodyImprovementSurcharge = GetReceiptLabelOrDefault(receiptLabels, "Email_Body_ImprovementSurcharge", clientLanguageCode);
                var emailBodyTip = GetReceiptLabelOrDefault(receiptLabels, "Email_Body_Tip", clientLanguageCode);
                var emailBodyTotalFare = GetReceiptLabelOrDefault(receiptLabels, "Email_Body_TotalFare", clientLanguageCode);
                var emailBodyRideLinqLastFour = GetReceiptLabelOrDefault(receiptLabels, "Email_Body_RideLinqLastFour", clientLanguageCode); 
                var emailBodyTax = GetReceiptLabelOrDefault(receiptLabels, "Email_Body_Tax", clientLanguageCode);
                var emailBodyAdditionalGratuity = GetReceiptLabelOrDefault(receiptLabels, "Email_Body_AdditionalGratuity", clientLanguageCode);

                var emailSubject = isUpdatedReceipt
                    ? EmailConstant.Subject.UpdatedReceipt
                    : EmailConstant.Subject.Receipt;

                var templateData = new
                {
                    ApplicationName = _serverSettings.ServerData.TaxiHail.ApplicationName,
                    EmailTitle = emailSubject,
                    AccentColor = _serverSettings.ServerData.TaxiHail.AccentColor,
                    EmailFontColor = _serverSettings.ServerData.TaxiHail.EmailFontColor,
                    ibsOrderId,
                    ShowTripSection = showTripSection,
                    ShowMinimalDriverInfo = showMinimalDriverInfo,
                    HasDriverInfo = hasDriverInfo,
                    HasDriverId = hasDriverInfo && driverInfos.DriverId.HasValue(),
                    HasDriverPhoto = hasDriverInfo && driverInfos.DriverPhotoUrl.HasValue(),
                    DriverPhotoURL = hasDriverInfo ? driverInfos.DriverPhotoUrl : null,
                    HasVehicleRegistration = hasDriverInfo && driverInfos.VehicleRegistration.HasValue(),
                    VehicleNumber = vehicleNumber,
                    ShowExtraInfoInReceipt = _serverSettings.ServerData.ShowExtraInfoInReceipt,
                    DriverName = hasDriverInfo ? driverInfos.FullName : string.Empty,
                    VehicleRegistration = hasDriverInfo ? driverInfos.VehicleRegistration : null,
                    VehicleMake = hasDriverInfo ? driverInfos.VehicleMake : string.Empty,
                    VehicleModel = hasDriverInfo ? driverInfos.VehicleModel : string.Empty,
                    VehicleColor = hasDriverInfo ? driverInfos.VehicleColor : null,
                    VehicleType = hasDriverInfo ? driverInfos.VehicleType : string.Empty,
                    FullVehicleInfo = hasDriverInfo ? driverInfos.FullVehicleInfo : string.Empty,
                    ServiceType = serviceType,
                    DriverInfos = driverInfos ?? new DriverInfos(),
                    DriverId = hasDriverInfo || cmtRideLinqFields != null ? driverInfos.DriverId : string.Empty,
                    PickupDate = cmtRideLinqFields.SelectOrDefault(x => x.PickUpDateTime) != null
                        ? cmtRideLinqFields.PickUpDateTime.Value.ToString("D", dateFormat)
                        : pickupDate.ToString("D", dateFormat),
                    PickupTime = cmtRideLinqFields.SelectOrDefault(x => x.PickUpDateTime) != null
                        ? cmtRideLinqFields.PickUpDateTime.Value.ToString("t", dateFormat /* Short time pattern */)
                        : pickupDate.ToString("t", dateFormat /* Short time pattern */),
                    DropOffDate = nullSafeDropOffDate.ToString("D", dateFormat),
                    DropOffTime = dropOffTime,
                    ShowDropOffTime = dropOffTime.HasValue(),
                    ShowUTCWarning = timeZoneOfTheOrder == TimeZones.NotSet && !localDropOffDate.HasValue,
                    Fare = _resources.FormatPrice(fare),
                    Toll = _resources.FormatPrice(toll),
                    Surcharge = _resources.FormatPrice(surcharge),
                    BookingFees = _resources.FormatPrice(bookingFees),
                    Extra = _resources.FormatPrice(extra),
                    Tip = _resources.FormatPrice(tip),
                    TipIncentive = _resources.FormatPrice(tipIncentive),
                    TotalFare = _resources.FormatPrice(totalAmount),
                    AdditionalGratuity = _resources.FormatPrice(additionalGratuity),
                    Note = _serverSettings.ServerData.Receipt.Note + marketSpecificNote,
                    Tax = _resources.FormatPrice(tax),
                    ImprovementSurcharge = _resources.FormatPrice(cmtRideLinqFields.SelectOrDefault(x => x.AccessFee)),
                    RideLinqLastFour = cmtRideLinqFields.SelectOrDefault(x => x.LastFour),

                    Distance = _resources.FormatDistance(cmtRideLinqFields.SelectOrDefault(x => x.Distance)),
                    TripId = cmtRideLinqFields.SelectOrDefault(x => x.TripId),
                    RateClassStart = rateClassStart,
                    RateClassEnd = rateClassEnd,
                    FareAtAlternateRate = _resources.FormatPrice(fareAtAlternateRate),

                    TollName1 = hasCmtTollDetails && cmtRideLinqFields.Tolls.Length >= 1 ? cmtRideLinqFields.Tolls[0].TollName : string.Empty,
                    TollName2 = hasCmtTollDetails && cmtRideLinqFields.Tolls.Length >= 2 ? cmtRideLinqFields.Tolls[1].TollName : string.Empty,
                    TollName3 = hasCmtTollDetails && cmtRideLinqFields.Tolls.Length >= 3 ? cmtRideLinqFields.Tolls[2].TollName : string.Empty,
                    TollName4 = hasCmtTollDetails && cmtRideLinqFields.Tolls.Length == 4 ? cmtRideLinqFields.Tolls[3].TollName : string.Empty,

                    TollAmount1 = hasCmtTollDetails && cmtRideLinqFields.Tolls.Length >= 1
                        ? _resources.FormatPrice(Math.Round(((double)cmtRideLinqFields.Tolls[0].TollAmount / 100), 2))
                        : _resources.FormatPrice(0),
                    TollAmount2 = hasCmtTollDetails && cmtRideLinqFields.Tolls.Length >= 2
                        ? _resources.FormatPrice(Math.Round(((double)cmtRideLinqFields.Tolls[1].TollAmount / 100), 2))
                        : _resources.FormatPrice(0),
                    TollAmount3 = hasCmtTollDetails && cmtRideLinqFields.Tolls.Length >= 3
                        ? _resources.FormatPrice(Math.Round(((double)cmtRideLinqFields.Tolls[2].TollAmount / 100), 2))
                        : _resources.FormatPrice(0),
                    TollAmount4 = hasCmtTollDetails && cmtRideLinqFields.Tolls.Length == 4
                        ? _resources.FormatPrice(Math.Round(((double)cmtRideLinqFields.Tolls[3].TollAmount / 100), 2))
                        : _resources.FormatPrice(0),

                    ShowToll1 = hasCmtTollDetails && cmtRideLinqFields.Tolls.Length >= 1 && cmtRideLinqFields.Tolls.Length <= 4,
                    ShowToll2 = hasCmtTollDetails && cmtRideLinqFields.Tolls.Length >= 2 && cmtRideLinqFields.Tolls.Length <= 4,
                    ShowToll3 = hasCmtTollDetails && cmtRideLinqFields.Tolls.Length >= 3 && cmtRideLinqFields.Tolls.Length <= 4,
                    ShowToll4 = hasCmtTollDetails && cmtRideLinqFields.Tolls.Length == 4,
                    ShowTollTotal = (hasCmtTollDetails && cmtRideLinqFields.Tolls.Length > 4) || (Math.Abs(toll) >= 0.01 && !hasCmtTollDetails),
                    ShowRideLinqLastFour = isCmtRideLinqReceipt,
                    ShowTripId = isCmtRideLinqReceipt,
                    ShowTax = Math.Abs(tax) >= 0.01 || isCmtRideLinqReceipt,
                    ShowImprovementSurcharge = isCmtRideLinqReceipt,
                    ShowSurcharge = Math.Abs(surcharge) >= 0.01 || isCmtRideLinqReceipt,
                    ShowBookingFees = Math.Abs(bookingFees) >= 0.01,
                    ShowExtra = Math.Abs(extra) >= 0.01 || isCmtRideLinqReceipt,
                    ShowRateClassStart = rateClassStart.HasValue || isCmtRideLinqReceipt,
                    ShowRateClassEnd = rateClassEnd.HasValue,
                    ShowDistance = isCmtRideLinqReceipt,
                    ShowTipIncentive = tipIncentive > 0,
                    ShowAdditionalGratuity = additionalGratuity > 0,
                    vatIsEnabled,
                    HasPaymentInfo = hasPaymentInfo,
                    PaymentAmount = paymentAmount,
                    PaymentMethod = paymentMethod,
                    ShowFareAndPaymentDetails = showFareAndPaymentDetails,
                    PaymentTransactionId = paymentTransactionId,
                    PaymentAuthorizationCode = paymentAuthorizationCode,
                    ShowPaymentAuthorizationCode = paymentAuthorizationCode.HasValue(),
                    PickupAddress = pickupAddress.DisplayAddress,
                    DropOffAddress = hasDropOffAddress ? addressToUseForDropOff.DisplayAddress : "-",
                    StaticMapUri = staticMapUri,
                    ShowStaticMap = !string.IsNullOrEmpty(staticMapUri),
                    BaseUrlImg = baseUrls.BaseUrlAssetsImg,
                    RedDotImg = String.Concat(baseUrls.BaseUrlAssetsImg, "email_red_dot.png"),
                    GreenDotImg = String.Concat(baseUrls.BaseUrlAssetsImg, "email_green_dot.png"),
                    LogoImg = imageLogoUrl,

                    PromotionWasUsed = Math.Abs(amountSavedByPromotion) >= 0.01,
                    promoCode,
                    AmountSavedByPromotion = _resources.FormatPrice(Convert.ToDouble(amountSavedByPromotion)),
                    ShowOrderNumber = showOrderNumber,

                    EmailBodyFare = emailBodyFare,
                    EmailBodyExtra = emailBodyExtra,
                    EmailBodySurcharge = emailBodySurcharge,
                    EmailBodyToll = emailBodyToll,
                    EmailBodyImprovementSurcharge = emailBodyImprovementSurcharge,
                    EmailBodyTip = emailBodyTip,
                    EmailBodyTotalFare = emailBodyTotalFare,
                    EmailBodyRideLinqLastFour = emailBodyRideLinqLastFour,
                    EmailBodyTax = emailBodyTax,
                    EmailBodyAdditionalGratuity = emailBodyAdditionalGratuity,

                    HasSocialMediaGoogleURL = !_serverSettings.ServerData.SocialMediaGoogleURL.IsNullOrEmpty(),
                    SocialMediaGoogleImg = String.Concat(baseUrls.BaseUrlAssetsImg, "google.png"),
                    SocialMediaGoogleURL = _serverSettings.ServerData.SocialMediaGoogleURL,

                    HasSocialMediaFacebookURL = !_serverSettings.ServerData.SocialMediaFacebookURL.IsNullOrEmpty(),
                    SocialMediaFacebookImg = String.Concat(baseUrls.BaseUrlAssetsImg, "facebook.png"),
                    SocialMediaFacebookURL = _serverSettings.ServerData.SocialMediaFacebookURL,

                    HasSocialMediaTwitterURL = !_serverSettings.ServerData.SocialMediaTwitterURL.IsNullOrEmpty(),
                    SocialMediaTwitterImg = String.Concat(baseUrls.BaseUrlAssetsImg, "twitter.png"),
                    SocialMediaTwitterURL = _serverSettings.ServerData.SocialMediaTwitterURL,

                    HasSocialMediaPinterestURL = !_serverSettings.ServerData.SocialMediaPinterestURL.IsNullOrEmpty(),
                    SocialMediaPinterestImg = String.Concat(baseUrls.BaseUrlAssetsImg, "pinterest.png"),
                    SocialMediaPinterestURL = _serverSettings.ServerData.SocialMediaPinterestURL,

                    HasSocialMediaYoutubeURL = !_serverSettings.ServerData.SocialMediaYoutubeURL.IsNullOrEmpty(),
                    SocialMediaYoutubeImg = String.Concat(baseUrls.BaseUrlAssetsImg, "youtube.png"),
                    SocialMediaYoutubeURL = _serverSettings.ServerData.SocialMediaYoutubeURL,

                    HasSocialMediaInstagramURL = !_serverSettings.ServerData.SocialMediaInstagramURL.IsNullOrEmpty(),
                    SocialMediaInstagramImg = String.Concat(baseUrls.BaseUrlAssetsImg, "instagram.png"),
                    SocialMediaInstagramURL = _serverSettings.ServerData.SocialMediaInstagramURL,
                    ShowTip = (orderStatusDetail.ServiceType != ServiceType.Luxury),
                };


                SendEmail(clientEmailAddress, EmailConstant.Template.Receipt, emailSubject, templateData, clientLanguageCode);
            }
            catch (Exception e)
            {
                _logger.LogMessage(string.Format("SendTripReceiptEmail method : OrderId {0} ERROR {1}", ibsOrderId, e.Message));
                _logger.LogError(e);
            }
        }

        public void SendPromotionUnlockedEmail(string name, string code, DateTime? expirationDate, string clientEmailAddress,
            string clientLanguageCode, bool bypassNotificationSetting = false)
        {
            if (!bypassNotificationSetting)
            {
                var account = _accountDao.FindByEmail(clientEmailAddress.ToLower());
                if (account == null || !ShouldSendNotification(account.Id, x => x.PromotionUnlockedEmail))
                {
                    return;
                }
            }

            string imageLogoUrl = GetRefreshableImageUrl(GetBaseUrls().LogoImg);
            
            var dateFormat = CultureInfo.GetCultureInfo(clientLanguageCode);

            var templateData = new
            {
                ApplicationName = _serverSettings.ServerData.TaxiHail.ApplicationName,
                AccentColor = _serverSettings.ServerData.TaxiHail.AccentColor,
                EmailFontColor = _serverSettings.ServerData.TaxiHail.EmailFontColor,
                PromotionName = name,
                PromotionCode = code,
                ExpirationDate = expirationDate.HasValue ? expirationDate.Value.ToString("D", dateFormat) : null,
                ExpirationTime = expirationDate.HasValue ? expirationDate.Value.ToString("t", dateFormat /* Short time pattern */) : null,
                HasExpirationDate = expirationDate.HasValue,
                LogoImg = imageLogoUrl
            };

            SendEmail(clientEmailAddress, EmailConstant.Template.PromotionUnlocked, EmailConstant.Subject.PromotionUnlocked, templateData, clientLanguageCode);
        }

        public void SendCreditCardDeactivatedEmail(string creditCardCompany, string last4Digits, string clientEmailAddress, string clientLanguageCode, bool bypassNotificationSetting = false)
        {
            if (!bypassNotificationSetting)
            {
                var account = _accountDao.FindByEmail(clientEmailAddress.ToLower());
                if (account == null)
                {
                    return;
                }
            }

            string imageLogoUrl = GetRefreshableImageUrl(GetBaseUrls().LogoImg);

            var templateData = new
            {
                ApplicationName = _serverSettings.ServerData.TaxiHail.ApplicationName,
                AccentColor = _serverSettings.ServerData.TaxiHail.AccentColor,
                EmailFontColor = _serverSettings.ServerData.TaxiHail.EmailFontColor,
                CreditCardCompany = creditCardCompany,
                Last4Digits = last4Digits,
                LogoImg = imageLogoUrl
            };

            SendEmail(clientEmailAddress, EmailConstant.Template.CreditCardDeactivated, EmailConstant.Subject.CreditCardDeactivated, templateData, clientLanguageCode);
        }

        public void SendOrderRefundEmail(DateTime refundDate, string last4Digits, double? totalAmount, string clientEmailAddress, string ccEmailAddress, string clientLanguageCode, bool bypassNotificationSetting = false)
        {
            if (!bypassNotificationSetting)
            {
                using (var context = _contextFactory.Invoke())
                {
                    var account = context.Query<AccountDetail>().SingleOrDefault(c => c.Email.ToLower() == clientEmailAddress.ToLower());
                    if (account == null)
                    {
                        return;
                    }
                }
            }

            string imageLogoUrl = GetRefreshableImageUrl(GetBaseUrls().LogoImg);

            var dateFormat = CultureInfo.GetCultureInfo(clientLanguageCode.IsNullOrEmpty()
                    ? SupportedLanguages.en.ToString()
                    : clientLanguageCode);

            var templateData = new
            {
                ApplicationName = _serverSettings.ServerData.TaxiHail.ApplicationName,
                AccentColor = _serverSettings.ServerData.TaxiHail.AccentColor,
                EmailFontColor = _serverSettings.ServerData.TaxiHail.EmailFontColor,
                RefundDate = refundDate.ToString("D", dateFormat),
                RefundTime = refundDate.ToString("t" /* Short time pattern */),
                Last4Digits = last4Digits,
                TotalAmount = _resources.FormatPrice(totalAmount),
                LogoImg = imageLogoUrl
            };

            SendEmail(clientEmailAddress, EmailConstant.Template.OrderRefund, EmailConstant.Subject.OrderRefund, templateData, clientLanguageCode, ccEmailAddress);
        }

        public void SendCreditCardDeactivatedPush(AccountDetail account)
        {
            var alert = _resources.Get("PushNotification_CreditCardDeclined", account.Language);
            var data = new Dictionary<string, object>();
            SendPushOrSms(account.Id, alert, data);
        }

        public void SendNoShowWarning(Guid orderId)
        {
            var order = _orderDao.FindById(orderId);

            using (var context = _contextFactory.Invoke())
            {
                var orderNotifications = context.Query<OrderNotificationDetail>().SingleOrDefault(x => x.Id == orderId);

                if (orderNotifications != null && orderNotifications.NoShowWarningSent)
                {
                    return;
                }

                _commandBus.Send(new UpdateOrderNotificationDetail
                {
                    NoShowWarningSent = true,
                    OrderId = orderId
                });
            }

            var alert = string.Format(_resources.Get("PushNotification_NoShowWarning", order.ClientLanguageCode));
            var data = new Dictionary<string, object> { { "orderId", orderId } };

            SendPushOrSms(order.AccountId, alert, data, true);
        }

        public void SendInfoAboutGratuity(Guid orderId)
        {
            var order = _orderDao.FindById(orderId);

            using (var context = _contextFactory.Invoke())
            {
                var orderNotifications = context.Query<OrderNotificationDetail>().SingleOrDefault(x => x.Id == orderId);

                if (orderNotifications != null && orderNotifications.InfoAboutGratuitySent)
                {
                    return;
                }

                _commandBus.Send(new UpdateOrderNotificationDetail
                {
                    InfoAboutGratuitySent = true,
                    OrderId = orderId
                });
            }

            var alert = string.Format(_resources.Get("PushNotification_InfoAboutGratuity", order.ClientLanguageCode));
            var data = new Dictionary<string, object> { { "orderId", orderId } };

            SendPushOrSms(order.AccountId, alert, data);
        }


        private TimeZones TryToGetOrderTimeZone(Guid orderId)
        {
            var order = _orderDao.FindById(orderId);
            if (order != null
                && order.Market.HasValue())
            {
                // order is in another market, show the date as UTC
                return TimeZones.NotSet;
            }

            return _serverSettings.ServerData.CompanyTimeZone;
        }

        private DateTime GetNullSafeDropOffDate(TimeZones timeZoneOfTheOrder, DateTime? dropOffDateInUtc, DateTime pickupDate)
        {
            if (!dropOffDateInUtc.HasValue)
            {
                // assume it ends on the same day...
                return pickupDate;
            }

            return TimeZoneHelper.TransformToLocalTime(timeZoneOfTheOrder, dropOffDateInUtc.Value);
        }

        private Position? TryToGetPositionOfDropOffAddress(Guid orderId, Address dropOffAddress, SendReceipt.CmtRideLinqReceiptFields cmtRideLinqFields)
        {
            if (cmtRideLinqFields != null
                && cmtRideLinqFields.LastLatitudeOfVehicle.HasValue
                && cmtRideLinqFields.LastLongitudeOfVehicle.HasValue)
            {
                return new Position(cmtRideLinqFields.LastLatitudeOfVehicle.Value, cmtRideLinqFields.LastLongitudeOfVehicle.Value);
            }

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

        private void SendEmail(string to, string bodyTemplate, string subjectTemplate, object templateData, string languageCode, string ccEmailAddress = null, params KeyValuePair<string, string>[] embeddedIMages)
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

            if (ccEmailAddress.HasValue())
            {
                mailMessage.CC.Add(ccEmailAddress);
            }

            var renderedBody = _templateService.Render(template, templateData);
            var inlinedRenderedBody = _templateService.InlineCss(renderedBody);
            var view = AlternateView.CreateAlternateViewFromString(inlinedRenderedBody, Encoding.UTF8, "text/html");
            mailMessage.AlternateViews.Add(view);

            if (embeddedIMages != null)
            {
                foreach (var image in embeddedIMages)
                {
                    var linkedImage = new LinkedResource(image.Value) { ContentId = image.Key };
                    view.LinkedResources.Add(linkedImage);
                }
            }

            var conf = new apcurium.MK.Booking.Email.EmailSender.SmtpConfiguration
            {
                Host = _serverSettings.ServerData.Smtp.Host,
                Port = _serverSettings.ServerData.Smtp.Port,
                EnableSsl = _serverSettings.ServerData.Smtp.EnableSsl,
                DeliveryMethod = _serverSettings.ServerData.Smtp.DeliveryMethod,
                UseDefaultCredentials = _serverSettings.ServerData.Smtp.UseDefaultCredentials,
                Username = _serverSettings.ServerData.Smtp.Credentials.Username,
                Password = _serverSettings.ServerData.Smtp.Credentials.Password,
            };

            var client = new SmtpClient();
            AutoMapper.Mapper.Map(conf, client);
            using (client)
            {
                client.Send(mailMessage);
            }

            _logger.LogMessage(string.Format("SendEmail method : To {0} Content {1} ", to, templateData));
        }

        private void SendPushOrSms(Guid accountId, string alert, Dictionary<string, object> data, bool forceBoth = false)
        {
            try
            {
                if (_serverSettings.ServerData.SendPushAsSMS 
                    || forceBoth)
                {
                    SendSms(accountId, alert);
                }

                if (!_serverSettings.ServerData.SendPushAsSMS
                    || forceBoth)
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
            var account = _accountDao.FindById(accountId);

            libphonenumber.PhoneNumber toPhoneNumber = new libphonenumber.PhoneNumber();
            toPhoneNumber.CountryCode = CountryCode.GetCountryCodeByIndex(CountryCode.GetCountryCodeIndexByCountryISOCode(account.Settings.Country)).CountryDialCode;
            toPhoneNumber.NationalNumber = long.Parse(account.Settings.Phone);
            toPhoneNumber.ItalianLeadingZero = (account.Settings.Phone[0] == '0');

            if (!toPhoneNumber.IsValidNumber)
            {
                _logger.Maybe(() => _logger.LogMessage("Cannot send SMS for account {0}, phone number is {1})", accountId, toPhoneNumber.IsPossibleNumberWithReason.ToString()));
                return;
            }

            SendSms(toPhoneNumber, alert);
        }

        private void SendSms(libphonenumber.PhoneNumber phoneNumber, string alert)
        {
            _smsService.Send(phoneNumber, alert);
        }

		public void SendCmtPaymentFailedPush(Guid accountId, string alertText)
		{
			SendPushOrSms(accountId, alertText, new Dictionary<string, object>());
		}

		private bool ShouldSendNotification(Guid accountId, Expression<Func<NotificationSettings, bool?>> propertySelector)
        {
            var companySettings = _configurationDao.GetNotificationSettings();
            var accountSettings = _configurationDao.GetNotificationSettings(accountId);
            var companyNotificationSettingValue = GetValue(companySettings, propertySelector);

            if (accountSettings == null)
            {
                // take company settings
                return companySettings.Enabled && companyNotificationSettingValue == true;
            }

            // if the account or the company disabled all notifications, then everything will be false
            var enabled = companySettings.Enabled && accountSettings.Enabled;

            // we have to check if the company setting has a value
            // if it doesn't, then the company has disabled the setting and must be false for everyone
            var accountNotificationSettingValue = GetValue(accountSettings, propertySelector);

            return enabled
                   && companyNotificationSettingValue == true
                   && accountNotificationSettingValue == true;
        }

        private bool? GetValue(NotificationSettings settings, Expression<Func<NotificationSettings, bool?>> propertySelector)
        {
            var mexp = propertySelector.Body as MemberExpression;
            var propertyName = mexp.Member.Name;

            var property = settings.GetType().GetProperty(propertyName);
            if (property == null)
            {
                return false;
            }
            return (bool?)property.GetValue(settings, null);
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
                AppleLogoImg = String.Concat(baseUrl, "/themes/" + serverSettings.ServerData.TaxiHail.ApplicationKey + "/img/app-stores-itunes.png");
                PlayLogoImg = String.Concat(baseUrl, "/themes/" + serverSettings.ServerData.TaxiHail.ApplicationKey + "/img/appstores-play.png");
                BaseUrlAssetsImg = String.Concat(baseUrl, "/assets/img/");
                Uri = baseUrl;
            }

            public string LogoImg { get; private set; }

            public string PlayLogoImg { get; private set; }

            public string AppleLogoImg { get; private set; }

            public string BaseUrlAssetsImg { get; private set; }

            public Uri Uri { get; private set; }
        }

        public static class EmailConstant
        {
            public static class Subject
            {
                public const string PasswordReset = "Email_Subject_PasswordReset";
                public const string Receipt = "Email_Subject_Receipt";
                public const string UpdatedReceipt = "Email_Subject_UpdatedReceipt";
                public const string AccountConfirmation = "Email_Subject_AccountConfirmation";
                public const string BookingConfirmation = "Email_Subject_BookingConfirmation";
                public const string PromotionUnlocked = "Email_Subject_PromotionUnlocked";
                public const string CreditCardDeactivated = "Email_Subject_CreditCardDeactivated";
                public const string CancellationFeesReceipt = "Email_Subject_CancellationFeesReceipt";
                public const string NoShowFeesReceipt = "Email_Subject_NoShowFeesReceipt";
                public const string OrderRefund = "Email_Subject_OrderRefund";
            }

            public static class Template
            {
                public const string PasswordReset = "PasswordReset";
                public const string Receipt = "Receipt";
                public const string AccountConfirmation = "AccountConfirmation";
                public const string BookingConfirmation = "BookingConfirmation";
                public const string PromotionUnlocked = "PromotionUnlocked";
                public const string CreditCardDeactivated = "CreditCardDeactivated";
                public const string CancellationFeesReceipt = "CancellationFeesReceipt";
                public const string NoShowFeesReceipt = "NoShowFeesReceipt";
                public const string OrderRefund = "OrderRefund";
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
                        var hashedImagedata = CryptographyHelper.GetHashString(imageData);

                        // Append its hash to its URL
                        return string.Format("{0}?refresh={1}", imageUrl, hashedImagedata);
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

        private string GetMarketReceiptFooter(double latitude, double longitude)
        {
            try
            {
                var marketSettings = _taxiHailNetworkServiceClient.GetCompanyMarketSettings(latitude, longitude);

                if (marketSettings == null || !marketSettings.ReceiptFooter.HasValueTrimmed())
                {
                    return string.Empty;
                }

                return string.Format("<br>{0}", marketSettings.ReceiptFooter);
            }
            catch (Exception)
            {
                _logger.LogMessage("Could not get market receipt footer [Called GetCompanyMarketSettings with for lat:{0} lng:{1}]", latitude, longitude);
                return string.Empty;
            }
        }

        /// <summary>
        /// Returns label from dictionary or from Resx by default
        /// </summary>
        /// <param name="receiptLabels"></param>
        /// <param name="key"></param>
        /// <param name="language"></param>
        /// <returns></returns>
        private string GetReceiptLabelOrDefault(IDictionary<string, string> receiptLabels, string key, string language)
        {
            string receiptLabel;
            var valueFound = receiptLabels.TryGetValue(key, out receiptLabel);
            if (!valueFound || string.IsNullOrEmpty(receiptLabel))
            {
                receiptLabel = _resources.Get(key, language);
            }
            return receiptLabel;
        }
    }
}