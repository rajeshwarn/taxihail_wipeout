using System;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Booking.Mobile.Extensions;
using apcurium.MK.Common.Entity;
using System.Windows.Input;
using apcurium.MK.Common.Extensions;

namespace apcurium.MK.Booking.Mobile.ViewModels.Orders
{
	public class OrderReviewViewModel: BaseViewModel
    {
		private readonly IOrderWorkflowService _orderWorkflowService;
		private readonly IAccountService _accountService;
        
		public OrderReviewViewModel(IOrderWorkflowService orderWorkflowService,	IAccountService accountService)
		{
			_orderWorkflowService = orderWorkflowService;
			_accountService = accountService;

			Observe(_orderWorkflowService.GetAndObserveBookingSettings(), settings => SettingsUpdated(settings));
			Observe(_orderWorkflowService.GetAndObservePickupAddress(), address => Address = address);
			Observe(_orderWorkflowService.GetAndObservePickupDate(), DateUpdated);
            //We are throttling to prevent cases where we can cause the app to become unresponsive after typing fast.
			Observe(_orderWorkflowService.GetAndObserveNoteToDriver().Throttle(TimeSpan.FromMilliseconds(500)), note => Note = note);
            Observe(_orderWorkflowService.GetAndObservePromoCode(), code => PromoCode = code);
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
			get 
			{ 
				return _vehiculeType.HasValue()
					? _vehiculeType
					: this.Services().Localize["NotAvailable"]; 
			}
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
                    RaisePropertyChanged(() => PromotionButtonText);
				}
			}
		}

		public ICommand NavigateToPromotions
		{
			get 
			{
				return this.GetCommand(() =>
				{
					ShowViewModel<PromotionViewModel>();
				});
			}
		}

	    public string PromotionButtonText
	    {
	        get
	        {
	            if (_promoCode.HasValue())
	            {
                    return string.Format("{0} {1}", this.Services().Localize["PromoCodeLabel"], PromoCode);
	            }
                return this.Services().Localize["PromotionButton"];
	        }
	    }
    }
}

