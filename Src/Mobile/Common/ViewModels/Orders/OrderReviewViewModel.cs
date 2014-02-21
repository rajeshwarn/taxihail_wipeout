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
		bool _hasShowWarnings;
        
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

		public async void ReviewStart()
        {
			if (!_hasShowWarnings)
			{
				await ShowFareEstimateAlertDialogIfNecessary();
				await PreValidateOrder();
			}
        }

		private async Task PreValidateOrder()
		{
			var validationInfo = await _orderWorkflowService.ValidateOrder();
			if (validationInfo.HasWarning)
			{
				_hasShowWarnings = true;
				this.Services().Message.ShowMessage(this.Services().Localize["WarningTitle"], 
					validationInfo.Message, 
					this.Services().Localize["Continue"], 
					delegate{}, 
					this.Services().Localize["Cancel"], 
					() => { ChangePresentation(new HomeViewModelPresentationHint(HomeViewModelState.Initial));
					});
			}
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

		async Task ShowFareEstimateAlertDialogIfNecessary()
		{
			if (await _orderWorkflowService.ShouldWarnAboutEstimate())
			{
				_hasShowWarnings = true;
				this.Services().Message.ShowMessage(this.Services().Localize["WarningEstimateTitle"], this.Services().Localize["WarningEstimate"],
					"Ok", delegate{ },
					this.Services().Localize["WarningEstimateDontShow"], () => this.Services().Cache.Set("WarningEstimateDontShow", "yes"));

			}
		}
    }
}

