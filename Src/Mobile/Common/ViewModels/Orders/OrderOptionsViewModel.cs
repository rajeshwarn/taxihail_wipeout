using System;
using System.Windows.Input;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Booking.Mobile.Data;
using apcurium.MK.Booking.Mobile.Extensions;
using apcurium.MK.Common.Entity;
using System.Collections.Generic;
using apcurium.MK.Booking.Api.Contract.Resources;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using apcurium.MK.Common.Extensions;

namespace apcurium.MK.Booking.Mobile.ViewModels.Orders
{
	public class OrderOptionsViewModel : BaseViewModel, IRequestPresentationState<HomeViewModelStateRequestedEventArgs>
	{
		private readonly IOrderWorkflowService _orderWorkflowService;
		private readonly IAccountService _accountService;

        public event EventHandler<HomeViewModelStateRequestedEventArgs> PresentationStateRequested;

		public OrderOptionsViewModel(IOrderWorkflowService orderWorkflowService, IAccountService accountService)
		{
			_orderWorkflowService = orderWorkflowService;
			_accountService = accountService;

			this.Observe(_orderWorkflowService.GetAndObservePickupAddress(), address => PickupAddress = address);
			this.Observe(_orderWorkflowService.GetAndObserveDestinationAddress(), address => DestinationAddress = address);
			this.Observe(_orderWorkflowService.GetAndObserveAddressSelectionMode(), selectionMode => AddressSelectionMode = selectionMode);
			this.Observe(_orderWorkflowService.GetAndObserveEstimatedFare(), fare => EstimatedFare = fare);
			this.Observe(_orderWorkflowService.GetAndObserveLoadingAddress(), loading => IsLoadingAddress = loading);
			this.Observe(_orderWorkflowService.GetAndObserveVehicleType(), vehicleType => VehicleTypeId = vehicleType);
		}

		public async Task Init()
		{
			var list = await _accountService.GetVehiclesList();

			if (list.None ()) 
			{
				SetDefaultVehicleType ();
			} else {
				VehicleTypes = list;
			}
		}

		async Task SetDefaultVehicleType ()
		{
			var data = await _accountService.GetReferenceData ();
			var defaultVehicleType = data.VehiclesList.FirstOrDefault (x => x.IsDefault.Value);
			var defaultId = defaultVehicleType != null ? defaultVehicleType.Id.Value : 0;
			VehicleTypes = new List<VehicleType> {
				new VehicleType {
					LogoName = "taxi",
					Name = "TAXI",
					ReferenceDataVehicleId = defaultId
				}
			};
			VehicleTypeId = defaultId;
		}

		private int? _vehicleTypeId;
		public int? VehicleTypeId
		{
			get { return _vehicleTypeId; }
			set
			{
				_vehicleTypeId = value;
				RaisePropertyChanged();
				RaisePropertyChanged(() => SelectedVehicleType);
			}
		}

		private IEnumerable<VehicleType> _vehicleTypes = new List<VehicleType>();
		public IEnumerable<VehicleType> VehicleTypes
		{
			get
			{
				return _vehicleTypes;
			}
			set
			{
				_vehicleTypes = value ?? new List<VehicleType>();

				RaisePropertyChanged ();
				RaisePropertyChanged (() => SelectedVehicleType);
				RaisePropertyChanged (() => VehicleAndEstimateBoxIsVisible);
			}
		}
			
		public VehicleType SelectedVehicleType
		{
			get { 
				VehicleType type = null;
				if (VehicleTypeId.HasValue) {
					type = VehicleTypes.FirstOrDefault (x => x.ReferenceDataVehicleId == VehicleTypeId); 
				}

				if (type == null) {
					type = VehicleTypes.FirstOrDefault (); 
				}
				return type;
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

		private Address _destinationAddress;
		public Address DestinationAddress
		{
			get { return _destinationAddress; }
			set
			{
				if (value != _destinationAddress)
				{
					_destinationAddress = value;
					RaisePropertyChanged();
				}
			}
		}

		private AddressSelectionMode _addressSelectionMode;
		public AddressSelectionMode AddressSelectionMode
		{
			get { return _addressSelectionMode; }
			set 
			{ 
				if (value != _addressSelectionMode)
				{
					_addressSelectionMode = value;
					RaisePropertyChanged();
					OnAddressSelectionModeChanged();
				}
			} 
		}

		private bool _isLoadingAddress;
		public bool IsLoadingAddress
		{
			get
			{ 
				return _isLoadingAddress;
			}
			private set
			{
				if (_isLoadingAddress != value)
				{
					_isLoadingAddress = value;
					RaisePropertyChanged();
				}
			}
		}

		private bool _showDestination;
		public bool ShowDestination
		{
			get
			{ 
				return _showDestination;
			}
			private set
			{
				_showDestination = value && !Settings.HideDestination;
				RaisePropertyChanged();
				RaisePropertyChanged(() => VehicleAndEstimateBoxIsVisible);
				RaisePropertyChanged (() => ShowEstimate);
			}
		}

		private string _estimatedFare;
		public string EstimatedFare
		{
			get { return _estimatedFare; }
			set
			{
				if (value != _estimatedFare)
				{
					_estimatedFare = value;
					RaisePropertyChanged();
				}
			}
		}

		public bool VehicleAndEstimateBoxIsVisible
		{
			get { return VehicleTypes.Count() > 1 || ShowEstimate; }
		}

		public bool ShowEstimate
		{
			get { return ShowDestination && Settings.ShowEstimate; }
		}

        public ICommand SetAddress
        {
            get
            {
				return this.GetCommand<Address>(async address => {
					await _orderWorkflowService.SetAddress(address);
                });
            }
        }

		public ICommand SetVehicleType
		{
			get
			{
				return this.GetCommand<VehicleType>(vehicleType => {
					_orderWorkflowService.SetVehicleType(vehicleType.ReferenceDataVehicleId);
				});
			}
		}

		public ICommand ShowSearchAddress
		{
			get
			{
				return this.GetCommand(() =>
				{
                    PresentationStateRequested.Raise(this, new HomeViewModelStateRequestedEventArgs(HomeViewModelState.AddressSearch));
				});
			}
		}

		void OnAddressSelectionModeChanged()
		{
			ShowDestination = AddressSelectionMode == AddressSelectionMode.DropoffSelection;
            if (!ShowDestination)
            {
                _orderWorkflowService.ClearDestinationAddress();
            }
		}
	}
}

