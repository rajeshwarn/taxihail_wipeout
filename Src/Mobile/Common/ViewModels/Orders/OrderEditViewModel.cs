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

namespace apcurium.MK.Booking.Mobile.ViewModels.Orders
{
	public class OrderEditViewModel: BaseViewModel, IRequestPresentationState<HomeViewModelStateRequestedEventArgs>
	{
		private readonly IOrderWorkflowService _orderWorkflowService;
		private readonly IAccountService _accountService;

        public event EventHandler<HomeViewModelStateRequestedEventArgs> PresentationStateRequested;

		public OrderEditViewModel(IOrderWorkflowService orderWorkflowService,
			IAccountService accountService)
		{
			_orderWorkflowService = orderWorkflowService;
			_accountService = accountService;
		}

		public async Task Init()
		{
			Vehicles = (await _accountService.GetVehiclesList()).Select(x => new ListItem { Id = x.ReferenceDataVehicleId, Display = x.Name }).ToArray();
			ChargeTypes = await _accountService.GetPaymentsList();

			this.Observe(_orderWorkflowService.GetAndObserveBookingSettings(), bookingSettings => BookingSettings = bookingSettings.Copy());
			this.Observe(_orderWorkflowService.GetAndObservePickupAddress(), address => PickupAddress = address.Copy());
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

//						if (this.Settings.CreditCardChargeTypeId.HasValue) 
//						{
//
//
//							if ( (orderToValidte.Settings.ChargeTypeId == _appSettings.Data.CreditCardChargeTypeId.Value)  &&
//								(!_accountService.CurrentAccount.DefaultCreditCard.HasValue ))
//							{
//								return false;
//							}
//						}
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
				return chargeType.Display; 
			}
		}
	}
}

