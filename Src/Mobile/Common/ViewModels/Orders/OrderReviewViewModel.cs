using System;
using apcurium.MK.Booking.Mobile.AppServices.Orders;
using apcurium.MK.Common.Entity;
using apcurium.MK.Booking.Mobile.AppServices;
using System.Threading.Tasks;
using System.Linq;
using apcurium.MK.Booking.Mobile.Extensions;
using apcurium.MK.Booking.Mobile.PresentationHints;
using System.Windows.Input;

namespace apcurium.MK.Booking.Mobile.ViewModels.Orders
{
	public class OrderReviewViewModel: ChildViewModel
    {
		readonly IOrderWorkflowService _orderWorkflowService;
        
		public OrderReviewViewModel(IOrderWorkflowService orderWorkflowService)
		{
			_orderWorkflowService = orderWorkflowService;
		}

		public void Init()
		{
			this.Observe(_orderWorkflowService.GetAndObserveBookingSettings(), (settings) => SettingsUpdated(settings));
			this.Observe(_orderWorkflowService.GetAndObservePickupAddress(), address => Address = address);
			this.Observe(_orderWorkflowService.GetAndObservePickupDate(), DateUpdated);
			this.Observe(_orderWorkflowService.GetAndObserveNoteToDriver(), note => Note = note);
		}

		private async Task SettingsUpdated(BookingSettings settings)
		{
			Settings = settings;
			var list = await this.Services().Account.GetVehiclesList();
			VehiculeType = list.First(x => x.Id == settings.VehicleTypeId).Display;
			list = await this.Services().Account.GetPaymentsList();
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
				_address = value;
				RaisePropertyChanged();
				RaisePropertyChanged("Apartment");
				RaisePropertyChanged("RingCode");
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
			       date.Value.ToShortDateString() + " " + date.Value.ToShortTimeString()
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

		private string _note;
		public string Note
		{
			get { return _note; }
			set
			{
				if (_note != value)
				{
					_note = value;
					_orderWorkflowService.SetNoteToDriver(value);
					RaisePropertyChanged();
				}
			}
		}


    }
}

