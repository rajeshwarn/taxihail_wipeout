using System;
using Cirrious.MvvmCross.Interfaces.Commands;
using apcurium.MK.Common.Entity;
using Cirrious.MvvmCross.Interfaces.ViewModels;
using ServiceStack.Text;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Mobile.Extensions;
using System.Threading.Tasks;
using System.Threading;

namespace apcurium.MK.Booking.Mobile.ViewModels
{
	public class LocationDetailViewModel: BaseViewModel
	{
	    readonly CancellationTokenSource _validateAddressCancellationTokenSource = new CancellationTokenSource();

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

        public bool ShowRingCodeField {
            get
            {
                return ConfigurationManager.GetSetting( "Client.ShowRingCodeField" ) != "false" ;
            }
            
        }
		private readonly Address _address;
		
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
                    FirePropertyChanged(() => IsNew);
					FirePropertyChanged(() => RebookIsAvailable);
                }
            }
        }

		public bool RebookIsAvailable {
			get {
				var setting = ConfigurationManager.GetSetting("Client.HideRebookOrder");
				return !IsNew && !bool.Parse(string.IsNullOrWhiteSpace(setting) ? bool.FalseString : setting);
			}
		}

        public IMvxCommand ValidateAddress {
            get {
                return GetCommand(() =>
                {
                    MessageService.ShowProgress(true);
                    var task = Task.Factory.StartNew(()=> GeolocService.ValidateAddress (_address.FullAddress))
                        .HandleErrors();
                    task.ContinueWith(t=> MessageService.ShowProgress(false));
                    task.ContinueWith(t=>{
                        var location = t.Result;
                        if ((location == null) || string.IsNullOrWhiteSpace(location.FullAddress) || !location.HasValidCoordinate ()) {
                            MessageService.ShowMessage (Resources.GetString("InvalidAddressTitle"), Resources.GetString("InvalidAddressMessage"));
                            return;
                        }

                        InvokeOnMainThread (() =>
                        {
							location.CopyTo(_address);
							FirePropertyChanged(()=>BookAddress);
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
                    var progressShowing = true;
                    MessageService.ShowProgress(true);
                    try {
						var location = GeolocService.ValidateAddress (_address.FullAddress);
                        if ((location == null) || string.IsNullOrWhiteSpace(location.FullAddress) || !location.HasValidCoordinate ()) {
                            MessageService.ShowMessage (Resources.GetString("InvalidAddressTitle"), Resources.GetString("InvalidAddressMessage"));
                            return;
                        }
                    
                        location.CopyTo( _address );

                        FirePropertyChanged (() => BookAddress );                         

                        AccountService.UpdateAddress(_address);

						MessageService.ShowProgress(false);
						progressShowing = false;
						Close();
                    
                    } catch (Exception ex) {
                        Logger.LogError (ex);
                    } finally {

                        if(progressShowing) MessageService.ShowProgress(false);
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
                            AccountService.DeleteHistoryAddress (_address.Id);
                        } else {
                            AccountService.DeleteFavoriteAddress(_address.Id);
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
                 var order = new Order {PickupAddress = _address};
    
                 var account = AccountService.CurrentAccount;
                 order.Settings = account.Settings;
                 var serialized = JsonSerializer.SerializeToString(order);
				 RequestNavigate<BookViewModel>(new { order = serialized }, true, MvxRequestedBy.UserAction);
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

