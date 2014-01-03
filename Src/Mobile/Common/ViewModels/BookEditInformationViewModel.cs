using System;
using System.Globalization;
using apcurium.MK.Booking.Mobile.Framework.Extensions;
using ServiceStack.Text;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Common.Entity;
using apcurium.MK.Booking.Mobile.Extensions;

namespace apcurium.MK.Booking.Mobile.ViewModels
{
    public class BookEditInformationViewModel : BaseSubViewModel<Order>
    {
        public BookEditInformationViewModel(string messageId, string order)
            : base(messageId)
        {
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
        public ListItem<int>[] Vehicles
        {
            get
            {
                return RideSettings != null ? RideSettings.Vehicles : null;
            }
        }

        public ListItem<int>[] Payments
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

        


        public AsyncCommand SaveCommand
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
            string result = apt.HasValue() ? apt : this.Services().Resources.GetString("ConfirmNoApt");
            result += @" / ";
            result += rCode.HasValue() ? rCode : this.Services().Resources.GetString("ConfirmNoRingCode");
            return result;
        }

        private string FormatBuildingName(string buildingName)
        {
            if (buildingName.HasValue())
            {
                return buildingName;
            }
            return this.Services().Resources.GetString(this.Services().Resources.GetString("HistoryDetailBuildingNameNotSpecified"));
        }

        private string FormatDateTime(DateTime? pickupDate)
        {
            var formatTime = new CultureInfo(CultureProvider.CultureInfoString).DateTimeFormat.ShortTimePattern;
            string format = "{0:ddd, MMM d}, {0:" + formatTime + "}";
            string result = pickupDate.HasValue ? string.Format(format, pickupDate.Value) : this.Services().Resources.GetString("TimeNow");
            return result;
        }
       
        public bool ShowPassengerName
        {
            get
            {
                try
                {
                    return Boolean.Parse(this.Services().Config.GetSetting("Client.ShowPassengerName"));
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
                    return Boolean.Parse(this.Services().Config.GetSetting("Client.ShowPassengerPhone"));
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
                    return Boolean.Parse(this.Services().Config.GetSetting("Client.ShowPassengerNumber"));
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }
    }
}