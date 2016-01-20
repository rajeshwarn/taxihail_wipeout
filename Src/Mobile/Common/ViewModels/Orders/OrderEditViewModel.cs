using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using System.Windows.Input;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Booking.Mobile.AppServices.Orders;
using apcurium.MK.Booking.Mobile.Extensions;
using apcurium.MK.Booking.Mobile.ViewModels.Payment;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Helpers;
using apcurium.MK.Common.Extensions;
using apcurium.MK.Common.Configuration.Impl;
using apcurium.MK.Common;

namespace apcurium.MK.Booking.Mobile.ViewModels.Orders
{
	public class OrderEditViewModel: BaseViewModel
	{
		private readonly IOrderWorkflowService _orderWorkflowService;
		private readonly IAccountService _accountService;
		private readonly IPaymentService _paymentService;
		private readonly IVehicleTypeService _vehicleTypeService;
		private readonly INetworkRoamingService _networkRoamingService;

		public OrderEditViewModel(IOrderWorkflowService orderWorkflowService, IAccountService accountService, IPaymentService paymentService, IVehicleTypeService vehicleTypeService, INetworkRoamingService networkRoamingService)
		{
			_orderWorkflowService = orderWorkflowService;
			_accountService = accountService;
			_paymentService = paymentService;
			_vehicleTypeService = vehicleTypeService;
			_networkRoamingService = networkRoamingService;

			Observe(_orderWorkflowService.GetAndObserveBookingSettings(), bookingSettings => BookingSettings = bookingSettings.Copy());
			Observe(_orderWorkflowService.GetAndObservePickupAddress(), address => PickupAddress = address.Copy());
			Observe(_networkRoamingService.GetAndObserveMarketSettings(), marketSettings => MarketChanged(marketSettings));

            PhoneNumber = new PhoneNumberModel();
		}

		public async Task Init()
		{
			Vehicles = (await _vehicleTypeService.GetVehiclesList()).Select(x => new ListItem { Id = x.ReferenceDataVehicleId, Display = x.Name }).ToArray();
			ChargeTypes = (await _accountService.GetPaymentsList()).Select(x => new ListItem { Id = x.Id, Display = this.Services().Localize[x.Display] }).ToArray();
			PhoneNumber.Country = _bookingSettings.Country;
            PhoneNumber.PhoneNumber = _bookingSettings.Phone;
			RaisePropertyChanged(() => IsChargeTypesEnabled);
            RaisePropertyChanged(() => PhoneNumber);
            RaisePropertyChanged(() => SelectedCountryCode);
		}

	    private async Task MarketChanged(MarketSettings marketSettings)
	    {
			var paymentList = await _accountService.GetPaymentsList();

			if (!marketSettings.IsLocalMarket)
			{
				var paymentSettings = await _paymentService.GetPaymentSettings();
				if (paymentSettings.PaymentMode == PaymentMethod.Cmt
					|| paymentSettings.PaymentMode == PaymentMethod.RideLinqCmt)
				{
					// CoF payment option in external markets is only available with CMT payment
					paymentList.Remove(i => i.Id != Common.Enumeration.ChargeTypes.PaymentInCar.Id
						&& i.Id != Common.Enumeration.ChargeTypes.CardOnFile.Id);
				}
				else
				{
					// Only Pay in Car payment available in external markets for other payment providers
					paymentList.Remove(i => i.Id != Common.Enumeration.ChargeTypes.PaymentInCar.Id);
				}
			}

			ChargeTypes = paymentList
				.Select(x => new ListItem
					{
						Id = x.Id,
						Display = this.Services().Localize[x.Display]
					}).ToArray();
	    }

	    public bool IsChargeTypesEnabled
        {
            get
            {
				// this is in cache and set correctly when we add/update/delete credit card
				return _accountService.CurrentAccount.DefaultCreditCard == null || !Settings.DisableChargeTypeWhenCardOnFile;
            }
        }

		public PhoneNumberModel PhoneNumber { get; set; }

        public CountryCode[] CountryCodes
        {
            get
            {
                return CountryCode.CountryCodes;
            }
        }

        public CountryCode SelectedCountryCode
        {
            get
            {
                return _bookingSettings == null
                    ? default(CountryCode)
                    : CountryCode.GetCountryCodeByIndex(CountryCode.GetCountryCodeIndexByCountryISOCode(_bookingSettings.Country));
            }

            set
            {
                _bookingSettings.Country = value.CountryISOCode;
                RaisePropertyChanged();
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
                    RaisePropertyChanged(() => SelectedCountryCode);
					PhoneNumber.Country = _bookingSettings.Country;
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
                    PhoneNumber.Country = BookingSettings.Country;
                    PhoneNumber.PhoneNumber = BookingSettings.Phone;

                    if (!PhoneNumber.IsNumberPossible())
                    {
                        await this.Services().Message.ShowMessage(this.Services().Localize["UpdateBookingSettingsInvalidDataTitle"],
                            string.Format(this.Services().Localize["InvalidPhoneErrorMessage"], PhoneNumber.GetPhoneExample()));
                        return;
                    }

				    BookingSettings.Phone = PhoneHelper.GetDigitsFromPhoneNumber(BookingSettings.Phone);
					try
					{
						await _orderWorkflowService.ValidateNumberOfPassengers(BookingSettings.Passengers);
					}
					catch (OrderValidationException e)
					{
						switch (e.Error)
						{							
							case OrderValidationError.InvalidPassengersNumber:								
								this.Services().Message.ShowMessage(this.Services().Localize["InvalidPassengersNumberTitle"], this.Services().Localize["InvalidPassengersNumber"]);
								return;
						}
					}

					await _orderWorkflowService.SetBookingSettings(BookingSettings);
					await _orderWorkflowService.SetPickupAptAndRingCode(PickupAddress.Apartment, PickupAddress.RingCode);

					if (BookingSettings.ChargeTypeId == Common.Enumeration.ChargeTypes.CardOnFile.Id 
						&& _accountService.CurrentAccount.DefaultCreditCard == null)
					{
						this.Services ().Message.ShowMessage (this.Services ().Localize ["ErrorCreatingOrderTitle"], 
							this.Services ().Localize ["NoCardOnFileMessage"],
							this.Services ().Localize ["AddACardButton"], 
							() => { ShowViewModel<CreditCardAddViewModel>(new { showInstructions = true });	}, 
							this.Services ().Localize ["Cancel"], 
							() => {});

						return;
					}

					((HomeViewModel)Parent).CurrentViewState = HomeViewModelState.Review;
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
					((HomeViewModel)Parent).CurrentViewState = HomeViewModelState.Review;
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
				return _bookingSettings == null 
					? null 
					: _bookingSettings.VehicleTypeId;
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
			    return vehicle == null 
                    ? null 
                    : vehicle.Display;
			}
		}

		public int? ChargeTypeId
		{
			get
			{
				return _bookingSettings == null
                    ? null
                    : _bookingSettings.ChargeTypeId;
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
				return chargeType == null 
                    ? null 
                    : this.Services().Localize[chargeType.Display];
			}
		}
	}
}

