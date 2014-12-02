using System;
using System.Linq;
using System.Threading.Tasks;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Booking.Mobile.Extensions;
using apcurium.MK.Common.Entity;
using System.Windows.Input;

namespace apcurium.MK.Booking.Mobile.ViewModels.Orders
{
	public class OrderReviewViewModel: BaseViewModel
    {
		private readonly IOrderWorkflowService _orderWorkflowService;
		private readonly IAccountService _accountService;
        private bool _isInitialized;
        
		public OrderReviewViewModel(IOrderWorkflowService orderWorkflowService,
			IAccountService accountService)
		{
			_orderWorkflowService = orderWorkflowService;
			_accountService = accountService;
		}

        public void Init()
        {
            if (!_isInitialized)
            {
                _isInitialized = true;
                this.Observe(_orderWorkflowService.GetAndObserveBookingSettings(), settings => SettingsUpdated(settings));
                this.Observe(_orderWorkflowService.GetAndObservePickupAddress(), address => Address = address);
                this.Observe(_orderWorkflowService.GetAndObservePickupDate(), DateUpdated);
                this.Observe(_orderWorkflowService.GetAndObserveNoteToDriver(), note => Note = note);
				this.Observe(_orderWorkflowService.GetAndObservePromoCode(), code => PromoCode = code);
            }
        }

	    private async Task SettingsUpdated(BookingSettings settings)
		{
			Settings = settings;

			// this is cached, call it first
			var chargeTypes = await _accountService.GetPaymentsList();
			ChargeType = this.Services().Localize[chargeTypes.First(x => x.Id == settings.ChargeTypeId).Display];

			var vehicle = (await _accountService.GetVehiclesList()).FirstOrDefault(x => x.ReferenceDataVehicleId == settings.VehicleTypeId);
			if (vehicle != null)
			{
				VehiculeType = vehicle.Name;
			}
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
				RaisePropertyChanged(() => Apartment);
				RaisePropertyChanged(() => RingCode);
			}
		}

		public string Apartment
		{
			get{ return Address != null && Address.Apartment != null 
					? Address.Apartment
					: this.Services().Localize["NotAvailable"] ; }
		}

		public string RingCode
		{
			get{ return Address != null && Address.RingCode != null 
					? Address.RingCode
					: this.Services().Localize["NotAvailable"] ; }
		}

		private void DateUpdated(DateTime? date)
		{
			Date = date.HasValue 
				? date.Value.ToShortDateString() + " " + date.Value.ToShortTimeString()
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

		private string _promoCode;
		public string PromoCode
		{
			get { return _promoCode; }
			set
			{
				if (_promoCode != value)
				{
					_promoCode = value;
					_orderWorkflowService.SetPromoCode(value);
					RaisePropertyChanged();
				}
			}
		}

		public ICommand NavigateToPromotions
		{
			get 
			{
				return this.GetCommand(() =>
				{
					// TODO MKTAXI-2310
					ShowViewModel<HistoryListViewModel> ();
				});
			}
		}
    }
}

