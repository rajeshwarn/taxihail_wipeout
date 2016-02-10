using System;
using System.Windows.Input;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Booking.Mobile.Data;
using apcurium.MK.Booking.Mobile.Extensions;
using apcurium.MK.Common.Entity;
using System.Collections.Generic;
using System.ComponentModel;
using apcurium.MK.Booking.Api.Contract.Resources;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using apcurium.MK.Common.Extensions;
using apcurium.MK.Booking.Maps;
using apcurium.MK.Common.Enumeration;

namespace apcurium.MK.Booking.Mobile.ViewModels.Orders
{
    public class OrderOptionsViewModel : BaseViewModel
	{
		private readonly IOrderWorkflowService _orderWorkflowService;
		private readonly INetworkRoamingService _networkRoamingService;
		private readonly IAccountService _accountService;
		private readonly IVehicleService _vehicleService;
		private readonly IVehicleTypeService _vehicleTypeService;

		private bool _pickupInputDisabled;
		private bool _destinationInputDisabled;
		private bool _vehicleTypeInputDisabled;
		private bool _canShowRateBox = true;

		private static readonly int TimeBeforeUpdatingEtaWhenNoVehicle = 10;  // In seconds
		private DateTime? _keepEtaWhenNoVehicleStartTime = null;

		public OrderOptionsViewModel(IOrderWorkflowService orderWorkflowService, INetworkRoamingService networkRoamingService, IAccountService accountService, IVehicleService vehicleService, IVehicleTypeService vehicleTypeService)
		{
			_orderWorkflowService = orderWorkflowService;
			_networkRoamingService = networkRoamingService;
			_accountService = accountService;
			_vehicleService = vehicleService;
			_vehicleTypeService = vehicleTypeService;

			Observe (_orderWorkflowService.GetAndObserveIsDestinationModeOpened (),
				isDestinationModeOpened => {
					IsDestinationModeOpened = isDestinationModeOpened;
					OnDestinationModeOpened ();
				});
			Observe (_orderWorkflowService.GetAndObservePickupAddress (), address => PickupAddress = address);
			Observe (_orderWorkflowService.GetAndObserveDestinationAddress (), address => DestinationAddress = address);
			Observe (_orderWorkflowService.GetAndObserveAddressSelectionMode (), selectionMode => AddressSelectionMode = selectionMode);
			Observe (_orderWorkflowService.GetAndObserveEstimatedFare (), fare => EstimatedFare = fare);
			Observe (_orderWorkflowService.GetAndObserveLoadingAddress (), loading => IsLoadingAddress = loading);
			Observe (_orderWorkflowService.GetAndObserveVehicleType (), vehicleType => VehicleTypeId = vehicleType);
			Observe (_orderWorkflowService.GetAndObserveServiceType (), serviceType => ServiceType = serviceType);
			Observe (_vehicleTypeService.GetAndObserveVehiclesList(), vehicleTypes => VehicleTypesChanged(vehicleTypes)); 
			Observe (_orderWorkflowService.GetAndObserveMarketSettings(), MarketChanged);
			Observe (_orderWorkflowService.GetAndObserveMarketVehicleTypes(), marketVehicleTypes => VehicleTypesChanged(marketVehicleTypes));
			Observe (_vehicleService.GetAndObserveEta (), eta => Eta = eta);
			Observe (_vehicleService.GetAndObserveAvailableVehicles(), vehicles => _availableVehicles = vehicles);
		}

		public override void Start()
		{
			base.Start();

			Observe(ObserveHomeViewModelState(), UpdateHomeViewState);
		}

		private IObservable<HomeViewModelState> ObserveHomeViewModelState()
		{
			return Observable.FromEventPattern<PropertyChangedEventHandler, PropertyChangedEventArgs>(
					h => Parent.PropertyChanged += h,
					h => Parent.PropertyChanged -= h
				)
				.Where(args => args.EventArgs.PropertyName.Equals("CurrentViewState"))
				.Select(_ => ((HomeViewModel) Parent).CurrentViewState)
				.DistinctUntilChanged();
		}


		public void Init()
		{
			ShowDestination = false;
		}

		public void UpdateHomeViewState(HomeViewModelState state)
		{
			switch (state)
			{
				case HomeViewModelState.Review:
				case HomeViewModelState.AirportDetails:
					PickupInputDisabled = true;
					DestinationInputDisabled = true;
					VehicleTypeInputDisabled = true;
					IsDestinationSelected = false;
					IsPickupSelected = false;
					CanShowRateBox = false;
					break;
				case HomeViewModelState.PickDate:
				case HomeViewModelState.AirportPickDate:
					PickupInputDisabled  = true;
					DestinationInputDisabled = true;
					VehicleTypeInputDisabled = true;
					IsDestinationSelected = false;
					IsPickupSelected = false;
					CanShowRateBox = true;
					break;
				case HomeViewModelState.Initial:
					PickupInputDisabled = false;
					DestinationInputDisabled = false;
					VehicleTypeInputDisabled = false;
					IsDestinationSelected = AddressSelectionMode == AddressSelectionMode.DropoffSelection;
					IsPickupSelected = AddressSelectionMode == AddressSelectionMode.PickupSelection;
					CanShowRateBox = true;
					break;
			}
		}

		private AvailableVehicle[] _availableVehicles;

	    private async Task VehicleTypesChanged(IList<VehicleType> vehicleTypes)
	    {
			var isLocalMarket = await _networkRoamingService.GetAndObserveMarketSettings()
				.Select(marketSettings => marketSettings.IsLocalMarket)
				.Take(1);

            if (isLocalMarket && vehicleTypes.None())
            {
                // in local market but no vehicle types, create a fake 
                // vehicle type with the default value of reference data
                await SetDefaultVehicleType();
            }
            else
            {
                VehicleTypes = vehicleTypes;
            }

            RaisePropertyChanged(() => ShowVehicleSelection);
			RaisePropertyChanged(() => GroupVehiclesByServiceType);
	    }

	    private async Task SetDefaultVehicleType()
		{
			var data = await _accountService.GetReferenceData ();
			var defaultVehicleType = data.VehiclesList.FirstOrDefault (x => x.IsDefault ?? false);
			var defaultId = defaultVehicleType != null
                ? defaultVehicleType.Id ?? 0
                : 0;

			VehicleTypes = new List<VehicleType>
            {
				new VehicleType
                {
					LogoName = "taxi",
					Name = "TAXI",
					ReferenceDataVehicleId = defaultId
				}
			};

			VehicleTypeId = defaultId;
		}

		public bool IsDestinationModeOpened { get; set; }

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

		private ServiceType _serviceType;
		public ServiceType ServiceType
		{
			get { return _serviceType; }
			set
			{
				_serviceType = value;
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
				RaisePropertyChanged (() => GroupVehiclesByServiceType);
				RaisePropertyChanged (() => VehicleRepresentations);
				RaisePropertyChanged (() => SelectedVehicleType);
				RaisePropertyChanged (() => VehicleAndEstimateBoxIsVisible);
			}
		}

		public IList<VehicleType> VehicleRepresentations
		{
			get
			{
				if (GroupVehiclesByServiceType)
				{
					return VehicleTypes
						.Select(x => x.ServiceType)
						.Distinct()
						.Select(x => new VehicleType 
						{
							ServiceType = x,
							Name = x.ToString(),
							LogoName = x == ServiceType.Luxury 
								? "blackcar" 
								: "taxi"
						})
						.ToList();
				}
				else
				{
					return VehicleTypes.ToList();
				}
			}
		}
			
		public VehicleType SelectedVehicleType
		{
			get
            { 
				VehicleType type = null;
				if (VehicleTypeId.HasValue)
                {
					type = VehicleTypes.FirstOrDefault (x => x.ReferenceDataVehicleId == VehicleTypeId && x.ServiceType == ServiceType);
				}

				if (type == null)
                {
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

		public bool IsPickupSelected
		{
			get { return _isPickupSelected; }
			set
			{
				_isPickupSelected = value; 
				RaisePropertyChanged();
			}
		}

		public bool IsDestinationSelected
		{
			get { return _isDestinationSelected; }
			set
			{
				_isDestinationSelected = value;
				RaisePropertyChanged();
			}
		}

		public bool CanShowRateBox
		{
			get { return _canShowRateBox; }
			set
			{
				_canShowRateBox = value; 
				RaisePropertyChanged();
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

					//We need to update if the street number is accessible.
					UpdatePickupAndDestinationSelectionIfNeeded();
				}
			} 
		}


		private void UpdatePickupAndDestinationSelectionIfNeeded()
		{
			if (((HomeViewModel) Parent).CurrentViewState != HomeViewModelState.Initial)
			{
				return;
			}

			IsPickupSelected = AddressSelectionMode == AddressSelectionMode.PickupSelection;
			IsDestinationSelected = AddressSelectionMode == AddressSelectionMode.DropoffSelection;
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
				RaisePropertyChanged(() => ShowEstimate);
				RaisePropertyChanged(() => ShowEta);
				RaisePropertyChanged(() => ShowEtaInEstimate);
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
					RaisePropertyChanged ();

					if (DisplayBaseRateInfo)
					{
						RaisePropertyChanged(() => FormattedEta);
					}
				}
			}
		}

		private Direction _eta;
		private bool _isPickupSelected;
		private bool _isDestinationSelected;

		public Direction Eta
		{
			get{ return _eta; }
			set
			{ 
				_eta = value;
				RaisePropertyChanged ();
				RaisePropertyChanged(() => ShowEta);
				RaisePropertyChanged(() => ShowEtaInEstimate);
				RaisePropertyChanged(() => VehicleAndEstimateBoxIsVisible);
				RaisePropertyChanged(() => FormattedEta);

				if (!_keepEtaWhenNoVehicleStartTime.HasValue
					|| (_keepEtaWhenNoVehicleStartTime.HasValue
						&& (DateTime.Now - _keepEtaWhenNoVehicleStartTime.Value).TotalSeconds > TimeBeforeUpdatingEtaWhenNoVehicle))
				{
					RaisePropertyChanged(() => FormattedEta);
				}
			}
		}

		public string FormattedEta
		{
			get
			{
				if (!DisplayBaseRateInfo)
				{
					if (_availableVehicles == null || !_availableVehicles.Any ())
					{
						_keepEtaWhenNoVehicleStartTime = DateTime.Now;
						return this.Services ().Localize ["EtaNoTaxiAvailable"];
					}

					if (Eta == null || (Eta != null && !Eta.IsValidEta ()))
					{
						return this.Services ().Localize ["EtaNoTaxiAvailable"];
					}

					if (Eta.Duration > 30)
					{
						return this.Services ().Localize ["EtaNotAvailable"];
					}

					_keepEtaWhenNoVehicleStartTime = null;

					var durationUnit = Eta.Duration <= 1 ? this.Services ().Localize ["EtaDurationUnit"] : this.Services ().Localize ["EtaDurationUnitPlural"];

					var serviceType = SelectedVehicleType.ServiceType;

					return Eta.Duration == 0
                    ? this.Services ().Localize ["EtaLessThanAMinute"]
                            : string.Format(this.Services().Localize["Eta" + (serviceType == ServiceType.Luxury ? "_Luxury" : "")], 
								Eta.Duration, durationUnit);
				} else
				{
					return EstimatedFare;
				}

			}
		}

		public bool VehicleAndEstimateBoxIsVisible
		{
		    get
		    {
                return ShowVehicleSelection || ShowEstimate;
		    }
		}

		public bool ShowEta
		{
			get
			{
				return (Settings.ShowEta && FormattedEta.HasValue() && !ShowEstimate) || DisplayBaseRateInfo;
			}
		}

		public bool ShowEtaInEstimate
		{
			get { return (Settings.ShowEta && FormattedEta.HasValue()) && !DisplayBaseRateInfo; }
		}

		public bool ShowEstimate
		{
			get { return (ShowDestination && Settings.ShowEstimate) && !DisplayBaseRateInfo; }
		}

		public bool DisplayBaseRateInfo
		{
			get { return true; } // TODO: Settings.DisplayBaseRateInfo
		}

		public bool BaseRateToggled { get; set; }

		public bool GroupVehiclesByServiceType 
		{
			get { return VehicleTypes.Select(x => x.ServiceType).Distinct().Count() > 1; }
		}

		public bool ShowVehicleSelection
		{
			get { return (VehicleTypes.Count() > 1) && Settings.VehicleTypeSelectionEnabled; }
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
				return this.GetCommand<VehicleSelectionModel>(model => {

					if (!VehicleTypeInputDisabled) 
					{
						var vehicle = (GroupVehiclesByServiceType && !model.IsSubMenuSelection)
							? VehicleTypes.First(x => x.ServiceType == model.VehicleType.ServiceType)
							: model.VehicleType;
						ServiceType = vehicle.ServiceType;
						_orderWorkflowService.SetVehicle(vehicle.ReferenceDataVehicleId, vehicle.ServiceType);
					}
				});
			}
		}

		public ICommand ShowPickUpSearchAddress
		{
			get
			{
				return this.GetCommand(() =>
				{
				    if (AddressSelectionMode == AddressSelectionMode.DropoffSelection)
				    {
				        _orderWorkflowService.ToggleBetweenPickupAndDestinationSelectionMode();
				    }

					((HomeViewModel)Parent).CurrentViewState = HomeViewModelState.AddressSearch;
				});
			}
		}

        public ICommand ShowDestinationSearchAddress
        {
            get
            {
                return this.GetCommand(() =>
                {
                    if (AddressSelectionMode == AddressSelectionMode.PickupSelection)
                    {
                        _orderWorkflowService.ToggleBetweenPickupAndDestinationSelectionMode();
                    }

					((HomeViewModel)Parent).CurrentViewState = HomeViewModelState.AddressSearch;
                });
            }
        }

		public bool PickupInputDisabled
		{
			get { return _pickupInputDisabled; }
			set
			{
				_pickupInputDisabled = value;
				RaisePropertyChanged();
			}
		}

		public bool DestinationInputDisabled
		{
			get { return _destinationInputDisabled; }
			set
			{
				_destinationInputDisabled = value;
				RaisePropertyChanged();
			}
		}

		public bool VehicleTypeInputDisabled
		{
			get { return _vehicleTypeInputDisabled; }
			set
			{
				_vehicleTypeInputDisabled = value; 
				RaisePropertyChanged();
			}
		}

		private void OnDestinationModeOpened()
		{
			if (AddressSelectionMode == AddressSelectionMode.None
				&& !ShowDestination
				&& !IsDestinationModeOpened)
			{
				// First launch
				return;
			}

			ShowDestination = !ShowDestination;

			if (!ShowDestination)
			{
				_orderWorkflowService.ClearDestinationAddress();

				if (AddressSelectionMode == AddressSelectionMode.DropoffSelection)
				{
					_orderWorkflowService.ToggleBetweenPickupAndDestinationSelectionMode();
				}
			}
		}

		public class VehicleSelectionModel
		{
			public VehicleType VehicleType { get; set; }
			public bool IsSubMenuSelection { get; set; }
		}
	}
}

