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

namespace apcurium.MK.Booking.Mobile
{
	public class RideSettingsViewModel: BaseSubViewModel<BookingSettings>,
        IMvxServiceConsumer<IAccountService>
	{
        private readonly BookingSettings _bookingSettings;
		public RideSettingsViewModel (string messageId, string bookingSettings)
			:base(messageId)
		{
            this._bookingSettings = bookingSettings.FromJson<BookingSettings>();
            var accountService = this.GetService<IAccountService>();

            _vehicules = accountService.GetVehiclesList().ToArray();
            _payments = accountService.GetPaymentsList().ToArray();
		}

        private ListItem[] _vehicules;
        public ListItem[] Vehicles {
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

        public int VehicleTypeId {
            get {
                return _bookingSettings.VehicleTypeId;
            }
        }

        public int ChargeTypeId {
            get {
                return _bookingSettings.ChargeTypeId;
            }
        }

        private string _name;
        public string Name {
            get {
                return _bookingSettings.Name;
            }
            set {
                if(value != _name)
                {
                    _name = value;
                    FirePropertyChanged("Name");
                }
            }
        }

        private string _phone;
        public string Phone {
            get {
                return _bookingSettings.Phone;
            }
            set {
                if(value != _phone)
                {
                    _phone = value;
                    FirePropertyChanged("Phone");
                }
            }
        }

        private int _passengers;
        public int Passengers {
            get {
                return _bookingSettings.Passengers;
            }
            set {
                if(value != _passengers)
                {
                    _passengers = value;
                    FirePropertyChanged("Passengers");
                }
            }
        }

        public IMvxCommand SetVehiculeType {
            get {
                return new MvxRelayCommand<int>(id=>{

                    _bookingSettings.VehicleTypeId = id;

                });
            }

        }
        
        public IMvxCommand SetChargeType
        {
            get{
                return new MvxRelayCommand<int>(id=>{

                    _bookingSettings.ChargeTypeId = id;

                });
            }
        }

        public IMvxCommand SetCompany
        {
            get{
                return new MvxRelayCommand<int>(id=>{

                    _bookingSettings.ProviderId = id;

                });
            }
        }
	}
}

