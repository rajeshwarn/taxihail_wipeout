using System;
using apcurium.MK.Booking.Mobile.ViewModels;
using apcurium.MK.Booking.Api.Contract.Resources;
using ServiceStack.Text;
using apcurium.MK.Booking.Mobile.Extensions;
using Cirrious.MvvmCross.Interfaces.Commands;
using Cirrious.MvvmCross.Commands;

namespace apcurium.MK.Booking.Mobile
{
	public class RideSettingsViewModel: BaseSubViewModel<BookingSettings>
	{
        private readonly BookingSettings _bookingSettings;
		public RideSettingsViewModel (string messageId, string bookingSettings)
			:base(messageId)
		{
            this._bookingSettings = bookingSettings.FromJson<BookingSettings>();
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

        public IMvxCommand DoneCommand {
            get {
                return new MvxRelayCommand(()=> {
                    this.ReturnResult(_bookingSettings);
                });
            }
        }
	}
}

