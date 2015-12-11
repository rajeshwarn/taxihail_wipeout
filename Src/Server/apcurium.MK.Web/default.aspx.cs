#region

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web.UI.WebControls;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Api.Services;
using apcurium.MK.Booking.ReadModel;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Configuration.Impl;
using apcurium.MK.Common.Enumeration;
using apcurium.MK.Common.Extensions;
using Microsoft.Practices.ServiceLocation;
using ServiceStack.Text;
using System.Web;
using apcurium.MK.Common;
using System.Globalization;

#endregion

namespace apcurium.MK.Web
{
    public partial class _default : PageBase
    {
        protected string ApplicationKey { get; private set; }
        protected string ApplicationName { get; private set; }
        protected string ApplicationVersion { get; private set; }
        protected string DefaultLatitude { get; private set; }
        protected string DefaultLongitude { get; private set; }
        protected string DefaultPhoneNumber { get; private set; }
        protected bool IsAuthenticated { get; private set; }
        protected string FacebookAppId { get; private set; }
        protected bool FacebookEnabled { get; private set; }
        protected bool HideDispatchButton { get; private set; }
        protected bool ShowCallDriver { get; private set; }
        protected bool ShowMessageDriver { get; private set; }
        protected string GeolocSearchFilter { get; private set; }
        protected string GeolocSearchRegion { get; private set; }
        protected string GeolocSearchBounds { get; private set; }
        protected bool AccountActivationDisabled { get; private set; }
        protected bool EstimateEnabled { get; private set; }
        protected bool EstimateWarningEnabled { get; private set; }
        protected bool EtaEnabled { get; private set; }
        protected bool DestinationIsRequired { get; private set; }
        protected string DirectionTarifMode { get; private set; }
        protected bool DirectionNeedAValidTarif { get; private set; }
        protected bool ShowPassengerNumber { get; private set; }
        protected string ReferenceData { get; private set; }
        protected string VehicleTypes { get; private set; }

        protected bool DisableImmediateBooking { get; private set; }

        protected bool DisableFutureBooking { get; private set; }
       
        protected bool IsWebSignupVisible { get; private set; }
        protected double MaxFareEstimate { get; private set; }
        protected bool IsChargeAccountPaymentEnabled { get; private set; }
        protected bool IsBraintreePrepaidEnabled { get; private set; }
        protected int MaxNumberOfCreditCards { get; private set; }
        protected bool IsPayPalEnabled { get; private set; }
        protected string PayPalMerchantId { get; private set; }
        protected bool IsCreditCardMandatory { get; private set; }
        protected bool? IsPayBackRegistrationFieldRequired { get; private set; }
        protected int DefaultTipPercentage { get; private set; }
        protected bool WarnForFeesOnCancel { get; private set; }
        protected bool IsWebSocialMediaVisible { get; private set; }
        protected string SocialMediaFacebookURL { get; private set; }
        protected string SocialMediaTwitterURL { get; private set; }
        protected string SocialMediaGoogleURL { get; private set; }
        protected string SocialMediaPinterestURL { get; private set; }
        protected bool HideMarketChangeWarning { get; private set; }
        protected bool AutoConfirmFleetChange { get; private set; }
        protected bool AlwaysDisplayCoFOption { get; private set; }
        protected bool AskForCVVAtBooking { get; private set; }
        protected int AvailableVehicleRefreshRate { get; private set; }
        protected bool IsCraftyClicksEnabled { get; private set; }
        protected string WebSiteRootPath { get; private set; }
        protected string CountryCodes { get; private set; }
        protected string DefaultCountryCode { get; private set; }
        protected bool ShowOrderNumber { get; private set; }
        protected string IsPaymentOutOfAppDisabled { get; private set; }

        protected void Page_Load(object sender, EventArgs e)
        {
            var config = ServiceLocator.Current.GetInstance<IServerSettings>();

            ApplicationKey = config.ServerData.TaxiHail.ApplicationKey;
            ApplicationName = config.ServerData.TaxiHail.ApplicationName;
            DefaultLatitude = config.ServerData.GeoLoc.DefaultLatitude.ToString();
            DefaultLongitude = config.ServerData.GeoLoc.DefaultLongitude.ToString();
            DefaultPhoneNumber = config.ServerData.DefaultPhoneNumberDisplay;
            IsAuthenticated = base.UserSession.IsAuthenticated;
            FacebookAppId = config.ServerData.FacebookAppId;
            FacebookEnabled = config.ServerData.FacebookEnabled;
            HideDispatchButton = config.ServerData.HideCallDispatchButton;
            ShowCallDriver = config.ServerData.ShowCallDriver;
            ShowMessageDriver = config.ServerData.ShowMessageDriver;
            DisableImmediateBooking = config.ServerData.DisableImmediateBooking;
            DisableFutureBooking = config.ServerData.DisableFutureBooking;
            IsWebSignupVisible = !config.ServerData.IsWebSignupHidden;
            ShowOrderNumber = config.ServerData.ShowOrderNumber;

            IsWebSocialMediaVisible = config.ServerData.IsWebSocialMediaVisible;
            SocialMediaFacebookURL = config.ServerData.SocialMediaFacebookURL;
            SocialMediaTwitterURL = config.ServerData.SocialMediaTwitterURL;
            SocialMediaGoogleURL = config.ServerData.SocialMediaGoogleURL;
            SocialMediaPinterestURL = config.ServerData.SocialMediaPinterestURL;

            AvailableVehicleRefreshRate = config.ServerData.AvailableVehicleRefreshRate;

            DirectionTarifMode = config.ServerData.Direction.TarifMode.ToString("G");
            DirectionNeedAValidTarif = config.ServerData.Direction.NeedAValidTarif;

            DefaultTipPercentage = config.ServerData.DefaultTipPercentage;

            ApplicationVersion = Assembly.GetAssembly(typeof (_default)).GetName().Version.ToString();

            EstimateEnabled = config.ServerData.ShowEstimate;
            EstimateWarningEnabled = config.ServerData.ShowEstimateWarning;
            EtaEnabled = config.ServerData.ShowEta;
            DestinationIsRequired = config.ServerData.DestinationIsRequired;
            MaxFareEstimate = config.ServerData.MaxFareEstimate;
            AccountActivationDisabled = config.ServerData.AccountActivationDisabled;
            IsPayBackRegistrationFieldRequired = config.ServerData.IsPayBackRegistrationFieldRequired;
            WarnForFeesOnCancel = config.ServerData.WarnForFeesOnCancel;
            HideMarketChangeWarning = config.ServerData.Network.HideMarketChangeWarning;
            AutoConfirmFleetChange = config.ServerData.Network.AutoConfirmFleetChange;
            IsCraftyClicksEnabled = config.ServerData.CraftyClicksApiKey.HasValue();
            WebSiteRootPath = HttpContext.Current.Request.ApplicationPath;

            var paymentSettings = config.GetPaymentSettings();

            IsCreditCardMandatory = paymentSettings.CreditCardIsMandatory;

            AlwaysDisplayCoFOption = paymentSettings.AlwaysDisplayCoFOption;
            AskForCVVAtBooking = paymentSettings.AskForCVVAtBooking;

            MaxNumberOfCreditCards = config.ServerData.MaxNumberOfCardsOnFile;

            IsBraintreePrepaidEnabled = paymentSettings.PaymentMode == PaymentMethod.Braintree 
                && paymentSettings.IsPayInTaxiEnabled
                && paymentSettings.IsPrepaidEnabled;
            IsPayPalEnabled = paymentSettings.PayPalClientSettings.IsEnabled
                && paymentSettings.IsPrepaidEnabled;
            IsChargeAccountPaymentEnabled = paymentSettings.IsChargeAccountPaymentEnabled;

            PayPalMerchantId = paymentSettings.PayPalClientSettings.IsSandbox
                ? paymentSettings.PayPalServerSettings.SandboxCredentials.MerchantId
                : paymentSettings.PayPalServerSettings.Credentials.MerchantId;

            ShowPassengerNumber = config.ServerData.ShowPassengerNumber;

            var filters = config.ServerData.GeoLoc.SearchFilter
                .SelectOrDefault(filterString => filterString.Split('&'), new string[0]);

            GeolocSearchFilter = filters.Length > 0
                ? Uri.UnescapeDataString(filters[0]).Replace('+', ' ')
                : "{0}";
            GeolocSearchRegion = FindParam(filters, "region");
            GeolocSearchBounds = FindParam(filters, "bounds");

            var referenceDataService = ServiceLocator.Current.GetInstance<ReferenceDataService>();
            var referenceData = (ReferenceData) referenceDataService.Get(new ReferenceDataRequest());

            if (paymentSettings.IsPaymentOutOfAppDisabled == OutOfAppPaymentDisabled.AppOnly &&
                !referenceData.PaymentsList.Any(p => p.Id == ChargeTypes.PaymentInCar.Id))
            {
                referenceData.PaymentsList.Add(ChargeTypes.PaymentInCar);
            }

            referenceData.PaymentsList = HidePaymentTypes(referenceData.PaymentsList, IsBraintreePrepaidEnabled, IsPayPalEnabled);

            ReferenceData = referenceData.ToString();

            var vehicleService = ServiceLocator.Current.GetInstance<VehicleService>();
            var vehicleTypes = (IList<VehicleTypeDetail>)vehicleService.Get(new VehicleTypeRequest());
            VehicleTypes = JsonSerializer.SerializeToString(vehicleTypes, vehicleTypes.GetType());
            CountryCodes = Newtonsoft.Json.JsonConvert.SerializeObject(CountryCode.CountryCodes);

            var defaultCultureInfo = CultureInfo.GetCultureInfo(config.ServerData.PriceFormat);

            DefaultCountryCode = defaultCultureInfo != null 
                ? (new RegionInfo(defaultCultureInfo.LCID)).TwoLetterISORegionName 
                : "CA";

            IsPaymentOutOfAppDisabled = paymentSettings.IsPaymentOutOfAppDisabled.ToString();
        }

        protected string FindParam(string[] filters, string param)
        {
            var pair = filters.FirstOrDefault(x => x.StartsWith(param + "="));
            return pair == null
                ? string.Empty
                : Uri.UnescapeDataString(pair.Split('=')[1]);
        }

        private List<Common.Entity.ListItem> HidePaymentTypes(IEnumerable<Common.Entity.ListItem> paymentList, bool creditCardPrepaidEnabled, bool payPalPrepaidEnabled)
        {
            var paymentTypesToHide = new List<int?>();

            if (!creditCardPrepaidEnabled)
            {
                paymentTypesToHide.Add(ChargeTypes.CardOnFile.Id);
            }

            if (!payPalPrepaidEnabled)
            {
                paymentTypesToHide.Add(ChargeTypes.PayPal.Id);
            }

            return paymentList.Where(paymentType => !paymentTypesToHide.Contains(paymentType.Id)).ToList();
        }
    }
}