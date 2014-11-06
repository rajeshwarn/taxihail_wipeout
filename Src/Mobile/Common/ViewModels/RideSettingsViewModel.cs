using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Booking.Mobile.Extensions;
using apcurium.MK.Booking.Mobile.ViewModels.Payment;
using apcurium.MK.Common.Configuration.Impl;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Enumeration;
using ServiceStack.ServiceClient.Web;
using ServiceStack.Text;

namespace apcurium.MK.Booking.Mobile.ViewModels
{
	public class RideSettingsViewModel: PageViewModel
    {
		private readonly IAccountService _accountService;
		private readonly IPaymentService _paymentService;
	    private readonly IAccountPaymentService _accountPaymentService;
	    private readonly IOrderWorkflowService _orderWorkflowService;

        private BookingSettings _bookingSettings;
	    private ClientPaymentSettings _paymentSettings;

		public RideSettingsViewModel(IAccountService accountService, 
			IPaymentService paymentService,
            IAccountPaymentService accountPaymentService,
			IOrderWorkflowService orderWorkflowService)
		{
			_orderWorkflowService = orderWorkflowService;
			_paymentService = paymentService;
		    _accountPaymentService = accountPaymentService;
		    _accountService = accountService;
		}

		public async void Init(string bookingSettings)
        {
			using (this.Services ().Message.ShowProgress ())
			{
				_bookingSettings = bookingSettings.FromJson<BookingSettings>();
			    _paymentSettings = await _paymentService.GetPaymentSettings();

				var p = await _accountService.GetPaymentsList();
				_payments = p == null ? new ListItem[0] : p.Select(x => new ListItem { Id = x.Id, Display = this.Services().Localize[x.Display] }).ToArray();
				
                RaisePropertyChanged(() => Payments );
				RaisePropertyChanged(() => ChargeTypeId );
				RaisePropertyChanged(() => ChargeTypeName );
				RaisePropertyChanged(() => IsChargeTypesEnabled);
                RaisePropertyChanged(() => IsChargeAccountPaymentEnabled);

				// this should be called last since it calls the server, we don't want to slow down other controls
				var v = await _accountService.GetVehiclesList();
				_vehicules = v == null ? new ListItem[0] : v.Select(x => new ListItem { Id = x.ReferenceDataVehicleId, Display = x.Name }).ToArray();
				RaisePropertyChanged(() => Vehicles );
				RaisePropertyChanged(() => VehicleTypeId );
				RaisePropertyChanged(() => VehicleTypeName );
			}
		}

        public bool ShouldDisplayTip
        {
            get
            {
                return _paymentSettings.IsPayInTaxiEnabled || _paymentSettings.PayPalClientSettings.IsEnabled;
            }
        }

	    public bool IsChargeTypesEnabled
	    {
	        get
            {
                return !_accountService.CurrentAccount.DefaultCreditCard.HasValue || !Settings.DisableChargeTypeWhenCardOnFile;
            }
	    }

	    public bool IsChargeAccountPaymentEnabled
	    {
	        get
	        {
                return _paymentSettings.IsChargeAccountPaymentEnabled;
	        }
	    }

        private PaymentDetailsViewModel _paymentPreferences;
        public PaymentDetailsViewModel PaymentPreferences
        {
            get
            {
                if (_paymentPreferences == null)
                {
					_paymentPreferences = Container.Resolve<PaymentDetailsViewModel>();
					_paymentPreferences.Start();
                }
                return _paymentPreferences;
            }
        }

        private ListItem[] _vehicules;
        public ListItem[] Vehicles
        {
            get
            {
                return _vehicules;
            }
        }

        private ListItem[] _payments;
        public ListItem[] Payments
        {
            get
            {
				return _payments;
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
					_orderWorkflowService.SetVehicleType (value);
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

                if (Payments == null)
                {
                    return null;
                }
                                
                var chargeType = Payments.FirstOrDefault(x => x.Id == ChargeTypeId);
                if (chargeType == null)
                    return null;
				return this.Services().Localize[chargeType.Display]; 
            }
        }

        public string Name
        {
            get
            {
                return _bookingSettings.Name;
            }
            set
            {
                if (value != _bookingSettings.Name)
                {
                    _bookingSettings.Name = value;
					RaisePropertyChanged();
                }
            }
        }

        public string Phone
        {
            get
            {
                return _bookingSettings.Phone;
            }
            set
            {
                if (value != _bookingSettings.Phone)
                {
                    _bookingSettings.Phone = value;
					RaisePropertyChanged();
                }
            }
        }

		public string AccountNumber
		{
			get
			{
				return _bookingSettings.AccountNumber;
			}
			set
			{
				if (value != _bookingSettings.AccountNumber)
				{
					_bookingSettings.AccountNumber = value;
					RaisePropertyChanged();
				}
			}
		}

		public ICommand SetVehiculeType
        {
            get
            {
                return this.GetCommand<int?>(id =>
                {
                    VehicleTypeId = id;
                });
            }
        }

		public ICommand NavigateToUpdatePassword
        {
            get
            {
                return this.GetCommand(() => ShowViewModel<UpdatePasswordViewModel>());
            }
        }

		public ICommand SetChargeType
        {
            get
            {
                return this.GetCommand<int?>(id =>
                {
                    ChargeTypeId = id;
                });
            }
        }

        public int? ProviderId
        {
            get
            { 
                return _bookingSettings.ProviderId;
            }
        }

		public ICommand SetCompany
        {
            get
            {
                return this.GetCommand<int?>(id =>
                {
                    _bookingSettings.ProviderId = id;
                });
            }
        }

		public ICommand SaveCommand
        {
            get
            {
                return this.GetCommand(async () => 
                {
					using (this.Services ().Message.ShowProgress ())
					{
                        var creditCard = PaymentPreferences.SelectedCreditCardId == Guid.Empty
                                ? default(Guid?)
                                : PaymentPreferences.SelectedCreditCardId;

                        if (await ValidateRideSettings(creditCard))
					    {
					        try
					        {
					            await _accountService.UpdateSettings(_bookingSettings, creditCard, PaymentPreferences.Tip);
                                Close(this);
					        }
					        catch (WebServiceException ex)
					        {
					            switch (ex.ErrorCode)
					            {
					                case "AccountCharge_InvalidAccountNumber":
					                    this.Services()
					                        .Message.ShowMessage(this.Services().Localize["UpdateBookingSettingsInvalidDataTitle"],
					                            this.Services().Localize["UpdateBookingSettingsInvalidAccount"]);
					                    break;
					                default:
					                    this.Services()
					                        .Message.ShowMessage(this.Services().Localize["UpdateBookingSettingsInvalidDataTitle"],
					                            this.Services().Localize["UpdateBookingSettingsGenericError"]);
					                    break;
					            }
					        }
					    }
					}
                });
            }
        }

        public async Task<bool> ValidateRideSettings(Guid? creditCard)
        {
            if (string.IsNullOrEmpty(Name) || string.IsNullOrEmpty(Phone))
            {
                this.Services().Message.ShowMessage(this.Services().Localize["UpdateBookingSettingsInvalidDataTitle"], this.Services().Localize["UpdateBookingSettingsEmptyField"]);
                return false;
            }
            if (Phone.Count(Char.IsDigit) < 10)
            {
                this.Services().Message.ShowMessage(this.Services().Localize["UpdateBookingSettingsInvalidDataTitle"], this.Services().Localize["InvalidPhoneErrorMessage"]);
                return false;
            }
            if (ChargeTypeId == ChargeTypes.Account.Id && string.IsNullOrWhiteSpace(AccountNumber))
            {
                this.Services().Message.ShowMessage(this.Services().Localize["UpdateBookingSettingsInvalidDataTitle"], this.Services().Localize["UpdateBookingSettingsEmptyAccount"]);
                return false;
            }
            if (ChargeTypeId == ChargeTypes.Account.Id)
            {
                try
                {
                    var chargeAccount = await _accountPaymentService.GetAccountCharge(AccountNumber);
                    if (chargeAccount.UseCardOnFileForPayment && creditCard == default(Guid?))
                    {
                        this.Services().Message.ShowMessage(this.Services().Localize["UpdateBookingSettingsInvalidDataTitle"],
                            this.Services().Localize["UpdateBookingSettingsInvalidCoF"]);
                    }
                }
                catch (Exception ex)
                {
                    this.Services().Message.ShowMessage(this.Services().Localize["UpdateBookingSettingsInvalidDataTitle"],
                        this.Services().Localize["UpdateBookingSettingsInvalidAccount"]);
                }
                
            }

            return true;
        }
    }
}

