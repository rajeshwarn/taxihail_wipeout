using System;
using apcurium.MK.Booking.Mobile.ViewModels;
using apcurium.MK.Booking.Api.Contract.Resources;
using ServiceStack.Text;
using apcurium.MK.Booking.Mobile.Extensions;
using Cirrious.MvvmCross.Interfaces.Commands;
using Cirrious.MvvmCross.Commands;
using Cirrious.MvvmCross.Interfaces.ServiceProvider;
using apcurium.MK.Booking.Mobile.AppServices;
using Cirrious.MvvmCross.ExtensionMethods;
using apcurium.MK.Common.Entity;
using System.Linq;
using apcurium.MK.Booking.Mobile.ViewModels.Payment;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace apcurium.MK.Booking.Mobile
{
	public class RideSettingsViewModel: BaseViewModel,
        IMvxServiceConsumer<IAccountService>
	{
        private readonly BookingSettings _bookingSettings;
        private readonly IAccountService _accountService;
		public RideSettingsViewModel (string bookingSettings)
		{
            this._bookingSettings = bookingSettings.FromJson<BookingSettings>();
            _accountService = this.GetService<IAccountService>();
            
            _vehicules = _accountService.GetVehiclesList().ToArray();
            _payments = _accountService.GetPaymentsList().ToArray();

            var account = _accountService.CurrentAccount;
            var paymentInformation = new PaymentInformation {
                CreditCardId = account.DefaultCreditCard,
                TipAmount = account.DefaultTipAmount,
                TipPercent = account.DefaultTipPercent,
            };
            PaymentPreferences = new PaymentDetailsViewModel(Guid.NewGuid().ToString(), paymentInformation);
		}

        public override void Restart ()
        {
            base.Restart ();
            PaymentPreferences.LoadCreditCards();
        }

        public PaymentDetailsViewModel PaymentPreferences {
            get;
            private set;
        }


        private NullableListItem[] _vehicules;
        public NullableListItem[] Vehicles {
            get {
                return _vehicules;
            }
        }

        private ListItem[] _payments;
        public ListItem[] Payments {
            get {
                return _payments;
            }
        }

        public int? VehicleTypeId {
			get {
				return _bookingSettings.VehicleTypeId;
			}
			set {
				if(value != _bookingSettings.VehicleTypeId){
					_bookingSettings.VehicleTypeId = value;
                    FirePropertyChanged("VehicleTypeId");
                    FirePropertyChanged("VehicleTypeName");
				}
			}
        }

        public string VehicleTypeName {
            get {
                var vehicle = this.Vehicles.FirstOrDefault(x=>x.Id == VehicleTypeId);
                if(vehicle == null) return null;
                return vehicle.Display;
            }
        }

        public int ChargeTypeId {
            get {
                return _bookingSettings.ChargeTypeId;
            }
			set {
				if(value != _bookingSettings.ChargeTypeId){
					_bookingSettings.ChargeTypeId = value;
                    FirePropertyChanged("ChargeTypeId");
                    FirePropertyChanged("ChargeTypeName");
				}
			}
        }

        public string ChargeTypeName {
            get {
                var chargeType = this.Payments.FirstOrDefault(x=>x.Id == ChargeTypeId);
                if(chargeType == null) return null;
                return chargeType.Display; 
            }
        }

        public string Name {
            get {
                return _bookingSettings.Name;
            }
            set {
                if(value != _bookingSettings.Name)
                {
                    _bookingSettings.Name = value;
                    FirePropertyChanged("Name");
                }
            }
        }

        public string Phone {
            get {
                return _bookingSettings.Phone;
            }
            set {
                if(value != _bookingSettings.Phone)
                {
                    _bookingSettings.Phone = value;
                    FirePropertyChanged("Phone");
                }
            }
        }

        public IMvxCommand SetVehiculeType {
            get {
                return GetCommand<int>(id =>
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
            get{
                return GetCommand<int>(id =>
                {

                    ChargeTypeId = id;

                });
            }
        }

        public IMvxCommand SetCompany
        {
            get{
                return GetCommand<int>(id =>
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
					if(ValidateRideSettings() && PaymentPreferences.ValidatePaymentSettings())
					{
                        var tipAmount = PaymentPreferences.IsTipInPercent ? default(double?) : PaymentPreferences.TipDouble;
                        var tipPercent = PaymentPreferences.IsTipInPercent ? PaymentPreferences.TipDouble : default(double?);
                        Guid? creditCard = PaymentPreferences.SelectedCreditCardId == Guid.Empty ? default(Guid?) : PaymentPreferences.SelectedCreditCardId;
                        _accountService.UpdateSettings (_bookingSettings, creditCard, tipAmount, tipPercent);
                        Close();
					}
                });
            }
        }

		private bool ValidateRideSettings()
		{
			if (string.IsNullOrEmpty(Name) 
			    || string.IsNullOrEmpty(Phone))
			{
                base.MessageService.ShowMessage(Resources.GetString("UpdateBookingSettingsInvalidDataTitle"), Resources.GetString("UpdateBookingSettingsEmptyField"));
				return false;
			}
            if ( Phone.Count(x => Char.IsDigit(x)) < 10 )
            {
                MessageService.ShowMessage(Resources.GetString("UpdateBookingSettingsInvalidDataTitle"), Resources.GetString("InvalidPhoneErrorMessage"));
                return false;
            }

			return true;
		}

	}
}

