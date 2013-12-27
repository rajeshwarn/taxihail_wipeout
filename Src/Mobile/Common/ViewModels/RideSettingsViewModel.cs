using System;
using apcurium.MK.Booking.Mobile.ViewModels;
using ServiceStack.Text;
using Cirrious.MvvmCross.Interfaces.Commands;
using Cirrious.MvvmCross.Interfaces.ServiceProvider;
using apcurium.MK.Booking.Mobile.AppServices;
using Cirrious.MvvmCross.ExtensionMethods;
using apcurium.MK.Common.Entity;
using System.Linq;
using apcurium.MK.Booking.Mobile.ViewModels.Payment;

namespace apcurium.MK.Booking.Mobile
{
    public class RideSettingsViewModel: BaseViewModel,
        IMvxServiceConsumer<IAccountService>
    {
        private readonly BookingSettings _bookingSettings;
        private readonly IAccountService _accountService;

        public RideSettingsViewModel(string bookingSettings) : this( bookingSettings.FromJson<BookingSettings>())
        {
        }

        public RideSettingsViewModel(BookingSettings bookingSettings)
        {
            _bookingSettings = bookingSettings;
            _accountService = this.GetService<IAccountService>();

            var refDataTask = _accountService.GetReferenceDataAsync();

            refDataTask.ContinueWith(result =>
                                     {
                var v = _accountService.GetVehiclesList();
                _vehicules = v == null ? new ListItem[0] : v.ToArray();
                FirePropertyChanged( ()=> Vehicles );
                FirePropertyChanged( ()=> VehicleTypeId );
                FirePropertyChanged( ()=> VehicleTypeName );


                var p = _accountService.GetPaymentsList();
                _payments = p == null ? new ListItem[0] : p.ToArray();

                FirePropertyChanged( ()=> Payments );
                FirePropertyChanged( ()=> ChargeTypeId );
                FirePropertyChanged( ()=> ChargeTypeName );
            });

        }

        public bool ShouldDisplayTipSlider
        {
            get
            {
                var setting = ConfigurationManager.GetPaymentSettings();
                return setting.IsPayInTaxiEnabled || setting.PayPalClientSettings.IsEnabled;
            }
        }

        public bool ShouldDisplayCreditCards
        {
            get
            {
                var setting = ConfigurationManager.GetPaymentSettings();
                return setting.IsPayInTaxiEnabled;
            }
        }
        public override void Restart()
        {
            base.Restart();
            PaymentPreferences.LoadCreditCards();
        }

        private PaymentDetailsViewModel _paymentPreferences;

        public PaymentDetailsViewModel PaymentPreferences
        {
            get
            {
                if (_paymentPreferences == null)
                {
                    var account = _accountService.CurrentAccount;
                    var paymentInformation = new PaymentInformation
                    {
                        CreditCardId = account.DefaultCreditCard,
                        TipPercent = account.DefaultTipPercent,
                    };

                    _paymentPreferences = new PaymentDetailsViewModel(Guid.NewGuid().ToString(), paymentInformation);
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
                    FirePropertyChanged("VehicleTypeId");
                    FirePropertyChanged("VehicleTypeName");
                }
            }
        }

        public string VehicleTypeName
        {
            get
            {

                if (!VehicleTypeId.HasValue)
                {
                    return Resources.GetString("NoPreference");
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
                    FirePropertyChanged("ChargeTypeId");
                    FirePropertyChanged("ChargeTypeName");
                }
            }
        }

        public string ChargeTypeName
        {
            get
            {
                if (!ChargeTypeId.HasValue)
                {
                    return Resources.GetString("NoPreference");
                }


                if (Payments == null)
                {
                    return null;
                }
                                
                var chargeType = Payments.FirstOrDefault(x => x.Id == ChargeTypeId);
                if (chargeType == null)
                    return null;
                return chargeType.Display; 
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
                    FirePropertyChanged("Name");
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
                    FirePropertyChanged("Phone");
                }
            }
        }

        public IMvxCommand SetVehiculeType
        {
            get
            {
                return GetCommand<int?>(id =>
                {
                    VehicleTypeId = id;
                });
            }

        }

        public IMvxCommand NavigateToUpdatePassword
        {
            get
            {
                return GetCommand(() => RequestNavigate<UpdatePasswordViewModel>());
            }
        }

        public IMvxCommand SetChargeType
        {
            get
            {
                return GetCommand<int?>(id =>
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

        public IMvxCommand SetCompany
        {
            get
            {
                return GetCommand<int?>(id =>
                {

                    _bookingSettings.ProviderId = id;

                });
            }
        }

        public IMvxCommand SaveCommand
        {
            get
            {
                return GetCommand(() => 
                {
                    if (ValidateRideSettings())
                    {
                        Guid? creditCard = PaymentPreferences.SelectedCreditCardId == Guid.Empty ? default(Guid?) : PaymentPreferences.SelectedCreditCardId;
                        _accountService.UpdateSettings(_bookingSettings, creditCard, PaymentPreferences.Tip);
                        Close();
                    }
                });
            }
        }

        public bool ValidateRideSettings()
        {
            if (string.IsNullOrEmpty(Name) 
                || string.IsNullOrEmpty(Phone))
            {
                base.MessageService.ShowMessage(Resources.GetString("UpdateBookingSettingsInvalidDataTitle"), Resources.GetString("UpdateBookingSettingsEmptyField"));
                return false;
            }
            if (Phone.Count(Char.IsDigit) < 10)
            {
                MessageService.ShowMessage(Resources.GetString("UpdateBookingSettingsInvalidDataTitle"), Resources.GetString("InvalidPhoneErrorMessage"));
                return false;
            }

            return true;
        }
    }
}

