using System;
using apcurium.MK.Booking.Mobile.ViewModels;
using ServiceStack.Text;
using apcurium.MK.Booking.Api.Contract.Requests;
using Cirrious.MvvmCross.Interfaces.Commands;
using Cirrious.MvvmCross.Commands;
using TinyIoC;
using TinyMessenger;
using apcurium.MK.Booking.Mobile.Messages;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.Framework.Extensions;
using apcurium.MK.Booking.Mobile.Client;
using Cirrious.MvvmCross.Interfaces.ServiceProvider;
using Cirrious.MvvmCross.ExtensionMethods;

namespace apcurium.MK.Booking.Mobile.ViewModels
{
    public class BookConfirmationViewModel : BaseViewModel,
		IMvxServiceConsumer<IAccountService>,
		IMvxServiceConsumer<IBookingService>
    {
        public BookConfirmationViewModel (string order)
        {
			var accountService = this.GetService<IAccountService>();
			var bookingService = this.GetService<IBookingService>();
            Order = JsonSerializer.DeserializeFromString<CreateOrder>( order );
			Order.Settings = accountService.CurrentAccount.Settings;

			RideSettings = new RideSettingsModel(Order.Settings, accountService.GetCompaniesList(), accountService.GetVehiclesList(), accountService.GetPaymentsList());
			FareEstimate = bookingService.GetFareEstimateDisplay(Order, null, "NotAvailable");
        }

        public CreateOrder Order { get; private set; }
		public string AptRingCode {
			get {
				return FormatAptRingCode(Order.PickupAddress.Apartment, Order.PickupAddress.RingCode);
			}
		}
		public string BuildingName {
			get {
				return FormatBuildingName(Order.PickupAddress.BuildingName);
			}
		}
		public string FormattedPickupDate {
			get {
				return FormatDateTime(Order.PickupDate);
			}
		}
		private string _fareEstimate;
		public string FareEstimate {
			get {
				return _fareEstimate;
			}
			set {
				if(value != _fareEstimate) {
					_fareEstimate = value;
					FirePropertyChanged("FareEstimate");
				}
			}
		}
		private RideSettingsModel _rideSettings;
		public RideSettingsModel RideSettings {
			get {
				return _rideSettings;
			}
			private set {
				if(value != _rideSettings)
				{
					_rideSettings = value;
					FirePropertyChanged("RideSettings");
				}
			}
		}

        public IMvxCommand ConfirmOrderCommand
        {
            get
            {

                return new MvxRelayCommand(() => 
                    {
                        Close();
                        MessengerHub.Publish(new OrderConfirmed(this, Order ));
                    }); 
            }
        }

        public IMvxCommand CancelOrderCommand
        {
            get
            {

                return new MvxRelayCommand(Close);               
            }
        }

		private string FormatAptRingCode(string apt, string rCode)
		{
			
			string result = apt.HasValue() ? apt : Resources.GetString(Resources.GetString("ConfirmNoApt"));
			result += @" / ";
			result += rCode.HasValue() ? rCode : Resources.GetString(Resources.GetString("ConfirmNoRingCode"));
			return result;
		}

		private string FormatBuildingName(string buildingName)
		{
			if ( buildingName.HasValue() )
			{
				return buildingName;
			}
			else
			{
				return Resources.GetString(Resources.GetString("HistoryDetailBuildingNameNotSpecified"));
			}
			
		}

		private string FormatDateTime(DateTime? pickupDate )
		{
			string format = "{0:ddd, MMM d}, {0:h:mm tt}";
			string result = pickupDate.HasValue ? string.Format(format, pickupDate.Value) : Resources.GetString("TimeNow");
			return result;
		}

    }
}

