using System;
using System.Linq;
using apcurium.MK.Booking.Mobile.Extensions;
using apcurium.MK.Booking.Mobile.ViewModels.Payment;
using apcurium.MK.Common.Entity;
using ServiceStack.Text;

namespace apcurium.MK.Booking.Mobile.ViewModels
{
    public class RideSettingsViewModel: BaseViewModel
    {
        private readonly BookingSettings _bookingSettings;

        public RideSettingsViewModel(string bookingSettings) : this( bookingSettings.FromJson<BookingSettings>())
        {
        }

        public RideSettingsViewModel(BookingSettings bookingSettings)
        {
            _bookingSettings = bookingSettings;


            var refDataTask = this.Services().Account.GetReferenceDataAsync();

            refDataTask.ContinueWith(result =>
                                     {
                var v = this.Services().Account.GetVehiclesList();
                _vehicules = v == null ? new ListItem[0] : v.ToArray();
				RaisePropertyChanged( ()=> Vehicles );
				RaisePropertyChanged( ()=> VehicleTypeId );
				RaisePropertyChanged( ()=> VehicleTypeName );


                var p = this.Services().Account.GetPaymentsList();
                _payments = p == null ? new ListItem[0] : p.ToArray();

				RaisePropertyChanged( ()=> Payments );
				RaisePropertyChanged( ()=> ChargeTypeId );
				RaisePropertyChanged( ()=> ChargeTypeName );
            });

        }

        public bool ShouldDisplayTipSlider
        {
            get
            {
                var setting = this.Services().Config.GetPaymentSettings();
                return setting.IsPayInTaxiEnabled || setting.PayPalClientSettings.IsEnabled;
            }
        }

        public bool ShouldDisplayCreditCards
        {
            get
            {
                var setting = this.Services().Config.GetPaymentSettings();
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
                    var account = this.Services().Account.CurrentAccount;
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
					RaisePropertyChanged();
					RaisePropertyChanged("VehicleTypeName");
                }
            }
        }

        public string VehicleTypeName
        {
            get
            {

                if (!VehicleTypeId.HasValue)
                {
                    return this.Services().Resources.GetString("NoPreference");
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
					RaisePropertyChanged("ChargeTypeName");
                }
            }
        }

        public string ChargeTypeName
        {
            get
            {
                if (!ChargeTypeId.HasValue)
                {
                    return this.Services().Resources.GetString("NoPreference");
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

        public AsyncCommand<int?> SetVehiculeType
        {
            get
            {
                return GetCommand<int?>(id =>
                {
                    VehicleTypeId = id;
                });
            }

        }

        public AsyncCommand NavigateToUpdatePassword
        {
            get
            {
                return GetCommand(() => ShowViewModel<UpdatePasswordViewModel>());
            }
        }

        public AsyncCommand<int?> SetChargeType
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

        public AsyncCommand<int?> SetCompany
        {
            get
            {
                return GetCommand<int?>(id =>
                {

                    _bookingSettings.ProviderId = id;

                });
            }
        }

        public AsyncCommand SaveCommand
        {
            get
            {
                return GetCommand(() => 
                {
                    if (ValidateRideSettings())
                    {
                        Guid? creditCard = PaymentPreferences.SelectedCreditCardId == Guid.Empty ? default(Guid?) : PaymentPreferences.SelectedCreditCardId;
                        this.Services().Account.UpdateSettings(_bookingSettings, creditCard, PaymentPreferences.Tip);
						Close(this);
                    }
                });
            }
        }

        public bool ValidateRideSettings()
        {
            if (string.IsNullOrEmpty(Name) 
                || string.IsNullOrEmpty(Phone))
            {
                this.Services().Message.ShowMessage(this.Services().Resources.GetString("UpdateBookingSettingsInvalidDataTitle"), this.Services().Resources.GetString("UpdateBookingSettingsEmptyField"));
                return false;
            }
            if (Phone.Count(Char.IsDigit) < 10)
            {
                this.Services().Message.ShowMessage(this.Services().Resources.GetString("UpdateBookingSettingsInvalidDataTitle"), this.Services().Resources.GetString("InvalidPhoneErrorMessage"));
                return false;
            }

            return true;
        }
    }
}

