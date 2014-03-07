using System;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Common.Entity;
using ServiceStack.Text;
using System.Windows.Input;
using System.Linq;
using apcurium.MK.Booking.Mobile.Infrastructure;
using System.Threading.Tasks;
using System.Collections.Generic;
using apcurium.MK.Booking.Mobile.PresentationHints;
using apcurium.MK.Booking.Mobile.Extensions;
using System.Reactive.Threading.Tasks;
using System.Reactive.Linq;

namespace apcurium.MK.Booking.Mobile.ViewModels.Orders
{
	public class OrderEditViewModel: ChildViewModel
	{
		private readonly IOrderWorkflowService _orderWorkflowService;
		private readonly IAccountService _accountService;

		public OrderEditViewModel(IOrderWorkflowService orderWorkflowService,
			IAccountService accountService)
		{
			_orderWorkflowService = orderWorkflowService;
			_accountService = accountService;
		}

		public async Task Init()
		{
			Vehicles = await _accountService.GetVehiclesList();
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
					_orderWorkflowService.SetBookingSettings(BookingSettings);
					await _orderWorkflowService.SetPickupAptAndRingCode(PickupAddress.Apartment, PickupAddress.RingCode);
					ChangePresentation(new HomeViewModelPresentationHint(HomeViewModelState.Review));
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
					
					ChangePresentation(new HomeViewModelPresentationHint(HomeViewModelState.Review));
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
				_vehicles = value == null 
				            ? new List<ListItem>() 
				            : value;
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
				_chargeTypes = value == null 
				            ? new List<ListItem>() 
				            : value;
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

