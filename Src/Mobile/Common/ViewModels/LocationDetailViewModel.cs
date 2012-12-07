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
using System.Threading;
using System.Threading.Tasks;

namespace apcurium.MK.Booking.Mobile
{
	public class LocationDetailViewModel: BaseViewModel,
        IMvxServiceConsumer<IGeolocService>,
        IMvxServiceConsumer<IAccountService>
	{
        IAccountService _accountService;

        CancellationTokenSource _validateAddressCancellationTokenSource = new CancellationTokenSource();

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
		
        public string BookAddress {
            get {
                return _address.BookAddress;
            }
            set {
                if(value != _address.FullAddress)
                {
                    _address.FullAddress = value;
                    FirePropertyChanged("BookAddress");
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
                return GetCommand(() =>
                {
                    MessageService.ShowProgress(true);
                    var task = Task.Factory.StartNew(()=>{
						return this.GetService<IGeolocService> ().ValidateAddress (_address.FullAddress);
                    }, _validateAddressCancellationTokenSource.Token)
                        .HandleErrors();
                    task.ContinueWith(t=> {
                                MessageService.ShowProgress(false);
                    });
                    task.ContinueWith(t=>{
                        var location = t.Result;
                        if ((location == null) || string.IsNullOrWhiteSpace(location.FullAddress) || !location.HasValidCoordinate ()) {
                            MessageService.ShowMessage (Resources.GetString("InvalidAddressTitle"), Resources.GetString("InvalidAddressMessage"));
                            return;
                        }

                        InvokeOnMainThread (() =>
                                            {
							location.CopyTo(_address);
							FirePropertyChanged("BookAddress");
                            
                        });
                        
                    }, TaskContinuationOptions.OnlyOnRanToCompletion);

                });
            }
        }

        public void StopValidatingAddresses ()
        {
            _validateAddressCancellationTokenSource.Cancel();
        }

        public IMvxCommand SaveAddress {
            get {

                return GetCommand(() =>
                {
                
                    if (!ValidateFields()) return;
            
                    MessageService.ShowProgress(true);
                    try {
						var location = this.GetService<IGeolocService> ().ValidateAddress (_address.FullAddress);
                        if ((location == null) || string.IsNullOrWhiteSpace(location.FullAddress) || !location.HasValidCoordinate ()) {
                            MessageService.ShowMessage (Resources.GetString("InvalidAddressTitle"), Resources.GetString("InvalidAddressMessage"));
                            return;
                        }
                    
                        InvokeOnMainThread (() =>
                        {
                            BookAddress = location.FullAddress;
                            location.FriendlyName = _address.FriendlyName;
                            location.Apartment = _address.Apartment;
                            location.RingCode = _address.RingCode;
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
                return GetCommand(() =>
                {

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
            get
            {
                return GetCommand(() =>
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
            if (string.IsNullOrWhiteSpace(BookAddress))
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

