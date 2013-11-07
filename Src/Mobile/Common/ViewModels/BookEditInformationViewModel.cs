using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

using Cirrious.MvvmCross.ExtensionMethods;
using Cirrious.MvvmCross.Interfaces.Commands;
using Cirrious.MvvmCross.Interfaces.ServiceProvider;
using ServiceStack.Text;
using TinyIoC;
using apcurium.Framework.Extensions;
using apcurium.MK.Booking.Api.Contract.Resources;
using ServiceStack.DesignPatterns.Serialization;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Booking.Mobile.Client;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Entity;
using apcurium.MK.Booking.Mobile.Extensions;

namespace apcurium.MK.Booking.Mobile.ViewModels
{
    public class BookEditInformationViewModel : BaseSubViewModel<Order>, IMvxServiceConsumer<IAccountService>
    {
        private IAccountService _accountService;
        public BookEditInformationViewModel(string messageId, string order)
            : base(messageId)
        {
            _accountService = this.GetService<IAccountService>();

            Order = JsonSerializer.DeserializeFromString<Order>(order);
            RideSettings = new RideSettingsViewModel( Order.Settings);
            RideSettings.OnPropertyChanged().Subscribe(p => 
                                                       {
                FirePropertyChanged(()=> RideSettings);
                FirePropertyChanged(() => Payments);
                FirePropertyChanged(() => Vehicles);
                FirePropertyChanged(() => VehicleName);
                FirePropertyChanged(() => ChargeType);
            });


        }

        public RideSettingsViewModel RideSettings { get; set; }

        public void SetVehicleTypeId(int? id)
        {
            Order.Settings.VehicleTypeId = id;
            FirePropertyChanged(() => VehicleName);
        }

        public void SetChargeTypeId(int? id)
        {
            Order.Settings.ChargeTypeId = id;
            FirePropertyChanged(() => ChargeType);
        }



        public int? VehicleTypeId
        {
            get { return Order.Settings.VehicleTypeId; }
            set { SetVehicleTypeId(value); }
        }
        public int? ChargeTypeId
        {
            get { return Order.Settings.ChargeTypeId; }
            set { SetChargeTypeId(value); }
        }




        public ListItem[] Vehicles
        {
            get
            {
                return RideSettings != null ? RideSettings.Vehicles : null;
            }
        }

        public ListItem[] Payments
        {
            get
            {
                return RideSettings != null ? RideSettings.Payments : null;
            }
        }

        public string VehicleName
        {
            get
            {
                return RideSettings != null ? RideSettings.VehicleTypeName : null;
            }
        }

        public string ChargeType
        {
            get
            {
                return RideSettings != null ? RideSettings.ChargeTypeName : null;
            }
        }

        public string AptRingCode
        {
            get
            {
                return FormatAptRingCode(Order.PickupAddress.Apartment, Order.PickupAddress.RingCode);
            }
        }
        public string BuildingName
        {
            get
            {
                return FormatBuildingName(Order.PickupAddress.BuildingName);
            }
        }
        public string FormattedPickupDate
        {
            get
            {
                return FormatDateTime(Order.PickupDate);
            }
        }
        private string _fareEstimate;
        public string FareEstimate
        {
            get
            {
                return _fareEstimate;
            }
            set
            {
                if (value != _fareEstimate)
                {
                    _fareEstimate = value;
                    FirePropertyChanged("FareEstimate");
                }
            }
        }

        private Order _order;
        public Order Order
        {
            get { return _order; }
            set
            {
                _order = value;
                FirePropertyChanged(() => Order);
            }
        }

        


        public IMvxCommand SaveCommand
        {
            get
            {

                return GetCommand(() => 
                                  { 
                                    if(RideSettings.ValidateRideSettings()) {
                                        ReturnResult(Order);
                                    }
                                  });
            }
        }

        private string FormatAptRingCode(string apt, string rCode)
        {
            string result = apt.HasValue() ? apt : Resources.GetString("ConfirmNoApt");
            result += @" / ";
            result += rCode.HasValue() ? rCode : Resources.GetString("ConfirmNoRingCode");
            return result;
        }

        private string FormatBuildingName(string buildingName)
        {
            if (buildingName.HasValue())
            {
                return buildingName;
            }
            else
            {
                return Resources.GetString(Resources.GetString("HistoryDetailBuildingNameNotSpecified"));
            }
        }

        private string FormatDateTime(DateTime? pickupDate)
        {
            var formatTime = new CultureInfo(CultureInfoString).DateTimeFormat.ShortTimePattern;
            string format = "{0:ddd, MMM d}, {0:" + formatTime + "}";
            string result = pickupDate.HasValue ? string.Format(format, pickupDate.Value) : Resources.GetString("TimeNow");
            return result;
        }
        public string CultureInfoString
        {
            get
            {
                var culture = TinyIoCContainer.Current.Resolve<IConfigurationManager>().GetSetting("PriceFormat");
                if (culture.IsNullOrEmpty())
                {
                    return "en-US";
                }
                else
                {
                    return culture;
                }
            }
        }
        public bool ShowPassengerName
        {
            get
            {
                try
                {
                    return Boolean.Parse(TinyIoCContainer.Current.Resolve<IConfigurationManager>().GetSetting("Client.ShowPassengerName"));
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }

        public bool ShowPassengerPhone
        {
            get
            {
                try
                {
                    return Boolean.Parse(TinyIoCContainer.Current.Resolve<IConfigurationManager>().GetSetting("Client.ShowPassengerPhone"));
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }

        public bool ShowPassengerNumber
        {
            get
            {
                try
                {
                    return Boolean.Parse(TinyIoCContainer.Current.Resolve<IConfigurationManager>().GetSetting("Client.ShowPassengerNumber"));
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }
    }
}