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
using apcurium.MK.Common.Enumeration;
using Microsoft.Practices.ServiceLocation;
using ServiceStack.Text;

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
        protected bool DisableFutureBooking { get; private set; }
        protected bool IsWebSignupVisible { get; private set; }
        protected double MaxFareEstimate { get; private set; }
        
        protected void Page_Load(object sender, EventArgs e)
        {
            var config = ServiceLocator.Current.GetInstance<IConfigurationManager>();

            ApplicationKey = config.ServerData.TaxiHail.ApplicationKey;
            ApplicationName = config.ServerData.TaxiHail.ApplicationName;
            DefaultLatitude = config.ServerData.GeoLoc.DefaultLatitude.ToString();
            DefaultLongitude = config.ServerData.GeoLoc.DefaultLongitude.ToString();
            DefaultPhoneNumber = config.ServerData.DefaultPhoneNumberDisplay;
            IsAuthenticated = base.UserSession.IsAuthenticated;
            FacebookAppId = config.ServerData.FacebookAppId;
            FacebookEnabled = config.ServerData.FacebookEnabled;
            HideDispatchButton = config.ServerData.HideCallDispatchButton;
            DisableFutureBooking = config.ServerData.DisableFutureBooking;
            IsWebSignupVisible = !config.ServerData.IsWebSignupHidden;

            DirectionTarifMode = config.ServerData.Direction.TarifMode.ToString("G");
            DirectionNeedAValidTarif = config.ServerData.Direction.NeedAValidTarif;

            ApplicationVersion = Assembly.GetAssembly(typeof (_default)).GetName().Version.ToString();

            EstimateEnabled = config.ServerData.ShowEstimate;
            EstimateWarningEnabled = config.ServerData.ShowEstimateWarning;
            EtaEnabled = config.ServerData.ShowEta;
            DestinationIsRequired = config.ServerData.DestinationIsRequired;
            MaxFareEstimate = config.ServerData.MaxFareEstimate;
            AccountActivationDisabled = config.ServerData.AccountActivationDisabled;

            ShowPassengerNumber = config.ServerData.ShowPassengerNumber;

            var filters = config.ServerData.GeoLoc.SearchFilter.Split('&');
            GeolocSearchFilter = filters.Length > 0
                ? Uri.UnescapeDataString(filters[0]).Replace('+', ' ')
                : "{0}";
            GeolocSearchRegion = FindParam(filters, "region");
            GeolocSearchBounds = FindParam(filters, "bounds");

            var referenceDataService = ServiceLocator.Current.GetInstance<ReferenceDataService>();
            var referenceData = (ReferenceData) referenceDataService.Get(new ReferenceDataRequest());

            // remove the card on file charge type since it's not possible to use card on file with the web app
            referenceData.PaymentsList = HidePaymentType(referenceData.PaymentsList, ChargeTypes.CardOnFile.Id);

            ReferenceData = referenceData.ToString();

            var vehicleService = ServiceLocator.Current.GetInstance<VehicleService>();
            var vehicleTypes = (IList<VehicleTypeDetail>)vehicleService.Get(new VehicleTypeRequest());
            VehicleTypes = JsonSerializer.SerializeToString(vehicleTypes, vehicleTypes.GetType());
        }

        protected string FindParam(string[] filters, string param)
        {
            var pair = filters.FirstOrDefault(x => x.StartsWith(param + "="));
            return pair == null
                ? string.Empty
                : Uri.UnescapeDataString(pair.Split('=')[1]);
        }

        private List<Common.Entity.ListItem> HidePaymentType(IEnumerable<Common.Entity.ListItem> paymentList, int? paymentTypeToHide)
        {
            return paymentList.Where(i => i.Id != paymentTypeToHide).ToList();
        }
    }
}