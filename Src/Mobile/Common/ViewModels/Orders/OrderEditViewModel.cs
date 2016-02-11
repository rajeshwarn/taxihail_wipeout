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
using ChargeTypesEnum = apcurium.MK.Common.Enumeration.ChargeTypes;

namespace apcurium.MK.Booking.Mobile.ViewModels.Orders
{
	public class OrderEditViewModel: BaseViewModel
	{
		private readonly IOrderWorkflowService _orderWorkflowService;
		private readonly IAccountService _accountService;
		private readonly IPaymentService _paymentService;
		private readonly INetworkRoamingService _networkRoamingService;

		public OrderEditViewModel(IOrderWorkflowService orderWorkflowService, IAccountService accountService, IPaymentService paymentService, INetworkRoamingService networkRoamingService)
		{
			_orderWorkflowService = orderWorkflowService;
			_accountService = accountService;
			_paymentService = paymentService;
			_networkRoamingService = networkRoamingService;

            _chargeTypes = new ListItem[0];

			Observe(_orderWorkflowService.GetAndObserveBookingSettings(), bookingSettings => BookingSettings = bookingSettings.Copy());
			Observe(_orderWorkflowService.GetAndObservePickupAddress(), address => PickupAddress = address.Copy());
			Observe(_networkRoamingService.GetAndObserveMarketSettings(), marketSettings => MarketChanged(marketSettings).FireAndForget());

            PhoneNumber = new PhoneNumberModel();
		}

		public async Task Init()
		{
            var paymentList = await _accountService.GetPaymentsList();

            var localize = this.Services().Localize;

            ChargeTypes = (paymentList ?? new ListItem[0])
                .Select(x => new ListItem { Id = x.Id, Display = localize[x.Display] })
                .ToArray();

            PhoneNumber.Country = _bookingSettings.Country;
            PhoneNumber.PhoneNumber = _bookingSettings.Phone;
            RaisePropertyChanged(() => PhoneNumber);
            RaisePropertyChanged(() => SelectedCountryCode);
		}

	    private async Task MarketChanged(MarketSettings marketSettings)
	    {
			var paymentList = await _accountService.GetPaymentsList();

	        paymentList = paymentList ?? new ListItem[0];

            var paymentSettings = await _paymentService.GetPaymentSettings();

            var isCmt = paymentSettings.PaymentMode == PaymentMethod.Cmt ||
                        paymentSettings.PaymentMode == PaymentMethod.RideLinqCmt;

	        paymentList = isCmt
	            ? HandlePaymentInCarForCmt(paymentList, marketSettings)
	            : EnforceExternalMarketPaymentInCarIfNeeded(paymentList, marketSettings);


	        var localize = this.Services().Localize;

            ChargeTypes = paymentList
				.Select(x => new ListItem
				{
					Id = x.Id,
					Display = localize[x.Display]
				})
                .ToArray();
            

	        HandleChargeTypeSelectionAccess(marketSettings.IsLocalMarket);
	    }

        private IList<ListItem> EnforceExternalMarketPaymentInCarIfNeeded(IList<ListItem> paymentList, MarketSettings market)
        {
            if (market.IsLocalMarket)
            {
                return paymentList;
            }

            return paymentList
                .Where(paymentMethod => paymentMethod.Id == Common.Enumeration.ChargeTypes.PaymentInCar.Id)
                .ToArray();
        }

		private IList<ListItem> EnsurePaymentInCarAvailableIfNeeded(IList<ListItem> paymentList, MarketSettings market)
		{
			if (paymentList.None(x => x.Id == ChargeTypesEnum.PaymentInCar.Id))
			{
				paymentList.Insert(0, ChargeTypesEnum.PaymentInCar);
			}

			return paymentList;
		}

        private IList<ListItem> HandlePaymentInCarForCmt(IList<ListItem> paymentList, MarketSettings market)
	    {
            if (!market.IsLocalMarket)
            {
				return market.DisableOutOfAppPayment
					? paymentList
					: EnsurePaymentInCarAvailableIfNeeded(paymentList, market);
            }

            paymentList.Remove(x => x.Id == Common.Enumeration.ChargeTypes.PaymentInCar.Id);

            if (ChargeTypeId == Common.Enumeration.ChargeTypes.PaymentInCar.Id)
            {
                ChargeTypeId = null;
            }

            return paymentList;
        }

	    private void HandleChargeTypeSelectionAccess(bool isLocalMarket)
	    {
            // We ignore the DisableChargeTypeWhenCardOnFile when on external market because the override in marketSetting will decide if we can change the charge type.
	        if (!isLocalMarket)
	        {
                IsChargeTypesEnabled = ChargeTypes.Length > 1;
                return;
	        }
            
            // If the setting DisableChargeTypeWhenCardOnFile is true, prevent changing the chargetype to something else then credit card.
            var isChargeTypeLocked = _accountService.CurrentAccount.DefaultCreditCard != null && Settings.DisableChargeTypeWhenCardOnFile;

            IsChargeTypesEnabled = !isChargeTypeLocked && ChargeTypes.Length > 1;
        }

	    public bool IsChargeTypesEnabled
	    {
	        get { return _isChargeTypesEnabled; }
	        set
	        {
	            _isChargeTypesEnabled = value;
	            RaisePropertyChanged();
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

		private ListItem[] _chargeTypes;
	    private bool _isChargeTypesEnabled;

	    public ListItem[] ChargeTypes
		{
			get
			{
				return _chargeTypes;
			}
			set
			{
				_chargeTypes = value ?? new ListItem[0];
				RaisePropertyChanged();
				RaisePropertyChanged(() => ChargeTypeId);
				RaisePropertyChanged(() => ChargeTypeName);
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

