using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using System.Windows.Input;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Booking.Mobile.Extensions;
using apcurium.MK.Common.Entity;
using apcurium.MK.Booking.Mobile.ViewModels.Payment;

namespace apcurium.MK.Booking.Mobile.ViewModels.Orders
{
	public class OrderEditViewModel: BaseViewModel, IRequestPresentationState<HomeViewModelStateRequestedEventArgs>
	{
		private readonly IOrderWorkflowService _orderWorkflowService;
		private readonly IAccountService _accountService;
	    private bool _isInitialized;

        public event EventHandler<HomeViewModelStateRequestedEventArgs> PresentationStateRequested;

		public OrderEditViewModel(IOrderWorkflowService orderWorkflowService,
			IAccountService accountService)
		{
			_orderWorkflowService = orderWorkflowService;
			_accountService = accountService;
		}

		public async Task Init()
		{
		    if (!_isInitialized)
		    {
		        _isInitialized = true;
                Vehicles = (await _accountService.GetVehiclesList()).Select(x => new ListItem { Id = x.ReferenceDataVehicleId, Display = x.Name }).ToArray();
                ChargeTypes = (await _accountService.GetPaymentsList()).Select(x => new ListItem { Id = x.Id, Display = this.Services().Localize[x.Display] }).ToArray();
                RaisePropertyChanged(() => IsChargeTypesEnabled);

                this.Observe(_orderWorkflowService.GetAndObserveBookingSettings(), bookingSettings => BookingSettings = bookingSettings.Copy());
                this.Observe(_orderWorkflowService.GetAndObservePickupAddress(), address => PickupAddress = address.Copy());
		    }
		}

	    public bool IsChargeTypesEnabled
        {
            get
            {
				// this is in cache and set correctly when we add/update/delete credit card
				return !_accountService.CurrentAccount.DefaultCreditCard.HasValue 
					|| !Settings.DisableChargeTypeWhenCardOnFile;
            }
        }

		private BookingSettings _bookingSettings;
		public BookingSettings BookingSettings
		{
			get { return _bookingSettings; }
			set
			{
				if (value != _bookingSettings)
				{
					_bookingSettings = value;
					RaisePropertyChanged();
					RaisePropertyChanged(() => VehicleTypeId);
					RaisePropertyChanged(() => VehicleTypeName);
					RaisePropertyChanged(() => ChargeTypeId);
					RaisePropertyChanged(() => ChargeTypeName);
				}
			}
		}

		private Address _pickupAddress;
		public Address PickupAddress
		{
			get { return _pickupAddress; }
			set
			{
				if (value != _pickupAddress)
				{
					_pickupAddress = value;
					RaisePropertyChanged();
				}
			}
		}

		public ICommand Save
		{
			get
			{
				return this.GetCommand(async () =>
				{
					await _orderWorkflowService.SetBookingSettings(BookingSettings);
					await _orderWorkflowService.SetPickupAptAndRingCode(PickupAddress.Apartment, PickupAddress.RingCode);

					if ((BookingSettings.ChargeTypeId == apcurium.MK.Common.Enumeration.ChargeTypes.CardOnFile.Id)  &&
						(!_accountService.CurrentAccount.DefaultCreditCard.HasValue))
					{
						this.Services ().Message.ShowMessage (this.Services ().Localize ["ErrorCreatingOrderTitle"], 
							this.Services ().Localize ["NoCardOnFileMessage"],
							this.Services ().Localize ["AddACardButton"], 
							() => { ShowViewModel<CreditCardAddViewModel>(new { showInstructions = true });	}, 
							this.Services ().Localize ["Cancel"], 
							() => {});

						return;
					}
                    PresentationStateRequested.Raise(this, new HomeViewModelStateRequestedEventArgs(HomeViewModelState.Review));
				});
			}
		}

		public ICommand Cancel
		{
			get
			{
				return this.GetCommand(async () =>
				{
					var bookingSettings = await _orderWorkflowService.GetAndObserveBookingSettings().Take(1).ToTask();
					var pickupAddress = await _orderWorkflowService.GetAndObservePickupAddress().Take(1).ToTask();

					BookingSettings = bookingSettings.Copy();
					PickupAddress = pickupAddress.Copy();
					
                    PresentationStateRequested.Raise(this, new HomeViewModelStateRequestedEventArgs(HomeViewModelState.Review));
				});
			}
		}

		private IEnumerable<ListItem> _vehicles;
		public IEnumerable<ListItem> Vehicles
		{
			get
			{
				return _vehicles;
			}
			set
			{
				_vehicles = value ?? new List<ListItem>();
				RaisePropertyChanged();
				RaisePropertyChanged(() => VehicleTypeId);
				RaisePropertyChanged(() => VehicleTypeName);
			}
		}

		private IEnumerable<ListItem> _chargeTypes;
		public IEnumerable<ListItem> ChargeTypes
		{
			get
			{
				return _chargeTypes;
			}
			set
			{
				_chargeTypes = value ?? new List<ListItem>();
				RaisePropertyChanged();
				RaisePropertyChanged(() => ChargeTypeId);
				RaisePropertyChanged(() => ChargeTypeName);
			}
		}

		public int? VehicleTypeId
		{
			get
			{
				return _bookingSettings.VehicleTypeId;
			}
			set
			{
				if (value != _bookingSettings.VehicleTypeId)
				{
					_bookingSettings.VehicleTypeId = value;
					RaisePropertyChanged();
					RaisePropertyChanged(() => VehicleTypeName);
				}
			}
		}

		public string VehicleTypeName
		{
			get
			{
				if (!VehicleTypeId.HasValue)
				{
					return this.Services().Localize["NoPreference"];
				}

				if (Vehicles == null)
				{
					return null;
				}

				var vehicle = Vehicles.FirstOrDefault(x => x.Id == VehicleTypeId);
				if (vehicle == null)
					return null;
				return vehicle.Display;
			}
		}

		public int? ChargeTypeId
		{
			get
			{
				return _bookingSettings.ChargeTypeId;
			}
			set
			{
				if (value != _bookingSettings.ChargeTypeId)
				{
					_bookingSettings.ChargeTypeId = value;
					RaisePropertyChanged();
					RaisePropertyChanged(() => ChargeTypeName);
				}
			}
		}

		public string ChargeTypeName
		{
			get
			{
				if (!ChargeTypeId.HasValue)
				{
					return this.Services().Localize["NoPreference"];
				}

				if (ChargeTypes == null)
				{
					return null;
				}

				var chargeType = ChargeTypes.FirstOrDefault(x => x.Id == ChargeTypeId);
				if (chargeType == null)
					return null;
				return this.Services().Localize[chargeType.Display]; 
			}
		}
	}
}

