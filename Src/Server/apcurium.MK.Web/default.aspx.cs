﻿#region

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
        protected string FacebookEnabled { get; private set; }
        protected string HideDispatchButton { get; private set; }
        protected string GeolocSearchFilter { get; private set; }
        protected string GeolocSearchRegion { get; private set; }
        protected string GeolocSearchBounds { get; private set; }
        protected string AccountActivationDisabled { get; private set; }
        protected string EstimateEnabled { get; private set; }
        protected string EstimateWarningEnabled { get; private set; }
        protected string EtaEnabled { get; private set; }
        protected string DestinationIsRequired { get; private set; }
        protected string DirectionTarifMode { get; private set; }
        protected bool DirectionNeedAValidTarif { get; private set; }
        protected bool ShowPassengerNumber { get; private set; }
        protected string ReferenceData { get; private set; }
        protected string VehicleTypes { get; private set; }
        protected string DisableFutureBooking { get; private set; }
        protected bool IsWebSignupVisible { get; private set; }
        protected double MaxFareEstimate { get; private set; }
        protected bool IsChargeAccountPaymentEnabled { get; private set; }
        
        protected void Page_Load(object sender, EventArgs e)
        {
            var config = ServiceLocator.Current.GetInstance<IConfigurationManager>();

            ApplicationKey = config.GetSetting("TaxiHail.ApplicationKey");
            ApplicationName = config.GetSetting("TaxiHail.ApplicationName");
            DefaultLatitude = config.GetSetting("GeoLoc.DefaultLatitude");
            DefaultLongitude = config.GetSetting("GeoLoc.DefaultLongitude");
            DefaultPhoneNumber = config.GetSetting("DefaultPhoneNumberDisplay");
            IsAuthenticated = base.UserSession.IsAuthenticated;
            FacebookAppId = config.GetSetting("FacebookAppId");
            FacebookEnabled = config.GetSetting("FacebookEnabled");
            HideDispatchButton = config.GetSetting("Client.HideCallDispatchButton");
            DisableFutureBooking = config.GetSetting("Client.DisableFutureBooking");
            IsWebSignupVisible = !config.GetSetting<bool>("IsWebSignupHidden", false);

            DirectionTarifMode = config.GetSetting("Direction.TarifMode");
            DirectionNeedAValidTarif = config.GetSetting("Direction.NeedAValidTarif", false);

            ApplicationVersion = Assembly.GetAssembly(typeof (_default)).GetName().Version.ToString();

            EstimateEnabled = config.GetSetting("Client.ShowEstimate");
            EstimateWarningEnabled = config.GetSetting("Client.ShowEstimateWarning");
            EtaEnabled = config.GetSetting("Client.ShowEta");
            DestinationIsRequired = config.GetSetting("Client.DestinationIsRequired");
            MaxFareEstimate = config.GetSetting<double>("Client.MaxFareEstimate", 100);
            IsChargeAccountPaymentEnabled = config.GetPaymentSettings().IsChargeAccountPaymentEnabled;
            
            var accountActivationDisabled = config.GetSetting("AccountActivationDisabled");
            AccountActivationDisabled = string.IsNullOrWhiteSpace(accountActivationDisabled)
                ? bool.FalseString.ToLower()
                : accountActivationDisabled;

            ShowPassengerNumber = config.GetSetting("Client.ShowPassengerNumber", true);

            var filters = config.GetSetting("GeoLoc.SearchFilter").Split('&');
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