using System;
using apcurium.MK.Booking.Mobile.AppServices.Orders;
using apcurium.MK.Common.Entity;
using apcurium.MK.Booking.Mobile.AppServices;
using System.Threading.Tasks;
using System.Linq;
using apcurium.MK.Booking.Mobile.Extensions;

namespace apcurium.MK.Booking.Mobile.ViewModels.Orders
{
	public class OrderReviewViewModel: ChildViewModel
    {
		readonly OrderWorkflowService _orderWorkflowService;
		readonly IAccountService _accountService;
        
		public OrderReviewViewModel(OrderWorkflowService orderWorkflowService,
									IAccountService accountService)
		{
			_orderWorkflowService = orderWorkflowService;
			_accountService = accountService;

			this.Observe(orderWorkflowService.GetAndObserveBookingSettings(), (settings) => SettingsUpdated(settings));
			this.Observe(orderWorkflowService.GetAndObservePickupAddress(), address => Address = address);
			this.Observe(orderWorkflowService.GetAndObservePickupDate(), DateUpdated);
		}

		private async Task SettingsUpdated(BookingSettings settings)
		{
			Settings = settings;
			var list = await _accountService.GetVehiclesList();
			VehiculeType = list.First(x => x.Id == settings.VehicleTypeId).Display;
			list = await _accountService.GetPaymentsList();
			ChargeType = list.First(x => x.Id == settings.ChargeTypeId).Display;
		}

		private BookingSettings _settings;
		public BookingSettings Settings
		{
			get { return _settings; }
			set
			{
				if (value != _settings)
				{
					_settings = value;
					RaisePropertyChanged();
				}
			}
		}

		private Address _address;
		public Address Address
		{
			get { return _address; }
			set
			{
				if (value != _address)
				{
					_address = value;
					RaisePropertyChanged();
					RaisePropertyChanged("Apartment");
					RaisePropertyChanged("RingCode");
				}
			}
		}

		public string Apartment
		{
			get{ return Address != null && Address.Apartment != null ? 
								Address.Apartment
								: this.Services().Localize["NotAvailable"] ; }
		}

		public string RingCode
		{
			get{ return Address != null && Address.RingCode != null ? 
						Address.RingCode
						: this.Services().Localize["NotAvailable"] ; }
		}

		private void DateUpdated(DateTime? date)
		{
			Date = date.HasValue ?
			       date.Value.ToShortDateString() + " " + date.Value.ToLongTimeString()
			       : this.Services().Localize["TimeNow"];
		}

		private string _date;
		public string Date
		{
			get{ return _date; }
			set
			{
				_date = value;
				RaisePropertyChanged();
			}
		}

		private string _vehiculeType;
		public string VehiculeType
		{
			get{ return _vehiculeType; }
			set
			{
				_vehiculeType = value;
				RaisePropertyChanged();
			}
		}

		private string _chargeType;
		public string ChargeType
		{
			get{ return _chargeType; }
			set
			{
				_chargeType = value;
				RaisePropertyChanged();
			}
		}
    }
}

