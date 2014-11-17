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
using apcurium.MK.Booking.Maps;
using Cirrious.CrossCore;
using apcurium.MK.Booking.Mobile.Infrastructure;

namespace apcurium.MK.Booking.Mobile.ViewModels.Orders
{
	public class OrderOptionsViewModel : BaseViewModel, IRequestPresentationState<HomeViewModelStateRequestedEventArgs>
	{
		private readonly IOrderWorkflowService _orderWorkflowService;
		private readonly IAccountService _accountService;
		private readonly IVehicleService _vehicleService;
        public event EventHandler<HomeViewModelStateRequestedEventArgs> PresentationStateRequested;

	    private string _market;

		public OrderOptionsViewModel(IOrderWorkflowService orderWorkflowService, IAccountService accountService, IVehicleService vehicleService)
		{
			_orderWorkflowService = orderWorkflowService;
			_accountService = accountService;
			_vehicleService = vehicleService;

			Observe(_orderWorkflowService.GetAndObservePickupAddress(), address => PickupAddress = address);
			Observe(_orderWorkflowService.GetAndObserveDestinationAddress(), address => DestinationAddress = address);
			Observe(_orderWorkflowService.GetAndObserveAddressSelectionMode(), selectionMode => AddressSelectionMode = selectionMode);
			Observe(_orderWorkflowService.GetAndObserveEstimatedFare(), fare => EstimatedFare = fare);
			Observe(_orderWorkflowService.GetAndObserveLoadingAddress(), loading => IsLoadingAddress = loading);
			Observe(_orderWorkflowService.GetAndObserveVehicleType(), vehicleType => VehicleTypeId = vehicleType);
            Observe(_orderWorkflowService.GetAndObserveMarket(), market => _market = market);
			Observe(_vehicleService.GetAndObserveEta(), eta => Eta = eta);
		}

		public async Task Init()
		{
		    Start();
		}

	    public async Task Start()
	    {
            var list = await _accountService.GetVehiclesList();

            if (list.None())
            {
                await SetDefaultVehicleType();
            }
            else
            {
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
				RaisePropertyChanged (() => ShowVehicleSelection);
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
				_showDestination = value;
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

		private Direction _eta;
		public Direction Eta
		{
			get{ return _eta; }
			set
			{ 
				_eta = value;
				RaisePropertyChanged ();
				RaisePropertyChanged(() => ShowEta);
				RaisePropertyChanged(() => VehicleAndEstimateBoxIsVisible);
				RaisePropertyChanged(() => FormattedEta);
			}
		}

		public string FormattedEta
		{
			get
			{
				if (Eta.IsValidEta()) 
				{
					if (Eta.Duration > 30) 
					{
						return this.Services ().Localize ["EtaNotAvailable"];
					} 
					else 
					{
						var durationUnit = Eta.Duration <= 1 ? this.Services ().Localize ["EtaDurationUnit"] : this.Services ().Localize ["EtaDurationUnitPlural"];
						return string.Format (this.Services ().Localize ["Eta"], Eta.FormattedDistance, Eta.Duration, durationUnit);
					}
				}

				return string.Empty;
			}
		}

		public bool VehicleAndEstimateBoxIsVisible
		{
			get { return ShowVehicleSelection || ShowEstimate || ShowEta; }
		}

		public bool ShowEta
		{
			get
			{
				return Settings.ShowEta && Eta != null;
			}
		}

		public bool ShowEstimate
		{
			get { return ShowDestination && Settings.ShowEstimate; }
		}

		public bool ShowVehicleSelection
		{
			get { return (VehicleTypes.Count() > 1) && Settings.VehicleTypeSelectionEnabled && !_market.HasValue(); }
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

		private void OnAddressSelectionModeChanged()
		{
			ShowDestination = AddressSelectionMode == AddressSelectionMode.DropoffSelection;
            if (!ShowDestination)
            {
                _orderWorkflowService.ClearDestinationAddress();
            }
		}
	}
}

