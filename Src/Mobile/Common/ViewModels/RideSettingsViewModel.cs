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
			set {
				if(value != _bookingSettings.VehicleTypeId){
					_bookingSettings.VehicleTypeId = value;
				}
			}
        }

        public int ChargeTypeId {
            get {
                return _bookingSettings.ChargeTypeId;
            }
			set {
				if(value != _bookingSettings.ChargeTypeId){
					_bookingSettings.ChargeTypeId = value;
				}
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

        public int Passengers {
            get {
                return _bookingSettings.Passengers;
            }
            set {
                if(value != _bookingSettings.Passengers)
                {
                    _bookingSettings.Passengers = value;
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

        public IMvxCommand SaveCommand
        {
            get
            {
                return new MvxRelayCommand(() => 
                                           {
					if(ValidateRideSettings())
					{
                    	ReturnResult(_bookingSettings);
					}
					else
					{
                		base.MessageService.ShowMessage(Resources.GetString("UpdateBookingSettingsInvalidDataTitle"), Resources.GetString("UpdateBookingSettingsEmptyField"));
					}
                });
            }
        }

		private bool ValidateRideSettings()
		{
			if (string.IsNullOrEmpty(Name) 
			    || string.IsNullOrEmpty(Phone)
			    || Passengers <= 0)
			{
				return false;
			}
			return true;
		}

	}
}

