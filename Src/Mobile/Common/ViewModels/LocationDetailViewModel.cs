using System;
using apcurium.MK.Booking.Mobile.ViewModels;
using Cirrious.MvvmCross.Interfaces.Commands;
using Cirrious.MvvmCross.Commands;
using apcurium.MK.Common.Entity;
using TinyIoC;
using Cirrious.MvvmCross.Interfaces.ViewModels;
using ServiceStack.Text;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Mobile.AppServices;
using Cirrious.MvvmCross.Interfaces.ServiceProvider;
using Cirrious.MvvmCross.ExtensionMethods;
using apcurium.MK.Booking.Mobile.Extensions;

namespace apcurium.MK.Booking.Mobile
{
	public class LocationDetailViewModel: BaseViewModel,
        IMvxServiceConsumer<IGeolocService>,
        IMvxServiceConsumer<IAccountService>
	{
        IAccountService _accountService;

		public LocationDetailViewModel (string address)
		{
            _address = address.FromJson<Address>();
            IsNew = false;
		}

        public LocationDetailViewModel ()
        {
            _address = new Address();
            IsNew = true;
        }

        protected override void Initialize ()
        {
            base.Initialize ();
            _accountService = this.GetService<IAccountService>();
        }

		private Address _address;
		
        public string FullAddress {
            get {
                return _address.FullAddress;
            }
            set {
                if(value != _address.FullAddress)
                {
                    _address.FullAddress = value;
                    FirePropertyChanged("FullAddress");
                }
            }
        }

        public string Apartment {
            get {
                return _address.Apartment;
            }
            set {
                if(value != _address.Apartment)
                {
                    _address.Apartment = value;
                    FirePropertyChanged("Apartment");
                }
            }
        }

        public string RingCode {
            get {
                return _address.RingCode;
            }
            set {
                if(value != _address.RingCode)
                {
                    _address.RingCode = value;
                    FirePropertyChanged("RingCode");
                }
            }
        }

        public string FriendlyName {
            get {
                return _address.FriendlyName;
            }
            set {
                if(value != _address.FriendlyName)
                {
                    _address.FriendlyName = value;
                    FirePropertyChanged("FriendlyName");
                }
            }
        }

        private bool _isNew;
        public bool IsNew {
            get {
                return _isNew;
            }
            set {
                if(value != _isNew)
                {
                    _isNew = value;
                    FirePropertyChanged("IsNew");
                }
            }
        }

        public IMvxCommand ValidateAddress {
            get {
                return new MvxRelayCommand(()=> {
                    MessageService.ShowProgress(true);
                    try {
                        var location = this.GetService<IGeolocService> ().ValidateAddress (FullAddress);
                        if ((location == null) || string.IsNullOrWhiteSpace(location.FullAddress) || !location.HasValidCoordinate ()) {
                            MessageService.ShowMessage (Resources.GetString("InvalidAddressTitle"), Resources.GetString("InvalidAddressMessage"));
                            return;
                        }
                        
                        InvokeOnMainThread (() =>
                                            {
                            FullAddress = location.FullAddress;
                            _address.Latitude = location.Latitude;
                            _address.Longitude = location.Longitude;
                            
                        });
                        
                    } catch (Exception ex) {
                        Logger.LogError (ex);
                    } finally {
                        MessageService.ShowProgress(false);
                    }
                });
            }
        }

        public IMvxCommand SaveAddress {
            get {

                return new MvxRelayCommand(()=> {
                
                    if (!ValidateFields()) return;
            
                    MessageService.ShowProgress(true);
                    try {
                        var location = this.GetService<IGeolocService> ().ValidateAddress (FullAddress);
                        if ((location == null) || string.IsNullOrWhiteSpace(location.FullAddress) || !location.HasValidCoordinate ()) {
                            MessageService.ShowMessage (Resources.GetString("InvalidAddressTitle"), Resources.GetString("InvalidAddressMessage"));
                            return;
                        }
                    
                        InvokeOnMainThread (() =>
                        {
                            FullAddress = location.FullAddress;
                            location.FriendlyName = _address.FriendlyName;
                            location.Apartment = location.Apartment;
                            location.RingCode = location.RingCode;
                            location.Id = _address.Id;
                            _accountService.UpdateAddress(location);     
                            Close();
                        });
                    
                    } catch (Exception ex) {
                        Logger.LogError (ex);
                    } finally {
                        MessageService.ShowProgress(false);
                    }
                });
            }
        }

        public IMvxCommand DeleteAddress {
            get {
                return new MvxRelayCommand (() => {

                    MessageService.ShowProgress (true);
                
                    try {
                        if (_address.IsHistoric) {
                            TinyIoCContainer.Current.Resolve<IAccountService> ().DeleteHistoryAddress (_address.Id);
                        } else {
                            TinyIoCContainer.Current.Resolve<IAccountService> ().DeleteFavoriteAddress (_address.Id);
                        }
                        Close ();
                    } catch (Exception ex) {
                        Logger.LogError (ex);
                    } finally {
                        MessageService.ShowProgress (false);
                    }
           
            
                });
            }
        
        }

		public IMvxCommand RebookOrder
		{
			get { return new MvxRelayCommand(()=>
				                                 {
                 var order = new Order();
                 order.PickupAddress = _address;
                 var account = TinyIoCContainer.Current.Resolve<IAccountService>().CurrentAccount;
                 order.Settings = account.Settings;
                 var serialized = JsonSerializer.SerializeToString(order);
				 RequestNavigate<BookViewModel>(new { order = serialized }, clearTop: true, requestedBy: MvxRequestedBy.UserAction);
				});
			}
		}

        private bool ValidateFields()
        {
            if (string.IsNullOrWhiteSpace(FullAddress))
            {
                MessageService.ShowMessage(Resources.GetString("InvalidAddressTitle"), Resources.GetString("InvalidAddressMessage"));
                return false;
            }
            if (string.IsNullOrWhiteSpace(FriendlyName))
            {
                MessageService.ShowMessage(Resources.GetString("SaveAddressEmptyFieldTitle"), Resources.GetString("SaveAddressEmptyFieldMessage"));
                return false;
            }
            return true;
            
        }
	}
}

