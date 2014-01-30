using System;
using apcurium.MK.Common.Entity;
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

		public void Init(string address)
		{
			_address = new Address();
			IsNew = true;

			if (!string.IsNullOrEmpty (address))
			{
				_address = address.FromJson<Address>();
				IsNew = false;
			} 
		}

        public bool ShowRingCodeField {
            get
            {
				return this.Services().Settings.ShowRingCodeField;
            }
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
					RaisePropertyChanged();
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
					RaisePropertyChanged();
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
					RaisePropertyChanged();
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
					RaisePropertyChanged();
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
					RaisePropertyChanged();
					RaisePropertyChanged(() => RebookIsAvailable);
                }
            }
        }

		public bool RebookIsAvailable {
			get { return !IsNew && !this.Services().Settings.HideRebookOrder; }
		}

        public AsyncCommand ValidateAddress
        {
            get {
                return GetCommand(() =>
                {
                    this.Services().Message.ShowProgress(true);
                    var task = Task.Factory.StartNew(() => this.Services().Geoloc.ValidateAddress(_address.FullAddress))
                        .HandleErrors();
                    task.ContinueWith(t => this.Services().Message.ShowProgress(false));
                    task.ContinueWith(t=>{
                        var location = t.Result;
                        if ((location == null) || string.IsNullOrWhiteSpace(location.FullAddress) || !location.HasValidCoordinate ()) {
                            this.Services().Message.ShowMessage(this.Services().Localize["InvalidAddressTitle"], this.Services().Localize["InvalidAddressMessage"]);
                            return;
                        }

                        InvokeOnMainThread (() =>
                        {
							location.CopyTo(_address);
							RaisePropertyChanged(()=>BookAddress);
                        });
                        
                    }, TaskContinuationOptions.OnlyOnRanToCompletion);
                });
            }
        }

        public void StopValidatingAddresses ()
        {
            _validateAddressCancellationTokenSource.Cancel();
        }

        public AsyncCommand SaveAddress
        {
            get {
                return GetCommand(() =>
                {
                    if (!ValidateFields()) return;
                    var progressShowing = true;
                    this.Services().Message.ShowProgress(true);
                    try {
                        var location = this.Services().Geoloc.ValidateAddress(_address.FullAddress);
                        if ((location == null) || string.IsNullOrWhiteSpace(location.FullAddress) || !location.HasValidCoordinate ()) {
                            this.Services().Message.ShowMessage(this.Services().Localize["InvalidAddressTitle"], this.Services().Localize["InvalidAddressMessage"]);
                            return;
                        }
                    
                        location.CopyTo( _address );

						RaisePropertyChanged (() => BookAddress );

                        this.Services().Account.UpdateAddress(_address);

                        this.Services().Message.ShowProgress(false);
						progressShowing = false;
						Close(this);
                    
                    } catch (Exception ex) {
                        Logger.LogError (ex);
                    } finally {
                        if (progressShowing) this.Services().Message.ShowProgress(false);
                    }
                });
            }
        }

        public AsyncCommand DeleteAddress
        {
            get {
                return GetCommand(() =>
                {
                    this.Services().Message.ShowProgress(true);
                
                    try {
                        if (_address.IsHistoric) {
                            this.Services().Account.DeleteHistoryAddress(_address.Id);
                        } else {
                            this.Services().Account.DeleteFavoriteAddress(_address.Id);
                        }
							Close (this);
                    } catch (Exception ex) {
                        Logger.LogError (ex);
                    } finally {
                        this.Services().Message.ShowProgress(false);
                    }
                });
            }
        }

        public AsyncCommand RebookOrder
		{
            get
            {
                return GetCommand(() =>
				{
	                 var order = new Order {PickupAddress = _address};

	                 var account = this.Services().Account.CurrentAccount;
	                 order.Settings = account.Settings;
	                 var serialized = JsonSerializer.SerializeToString(order);
					 ShowViewModel<BookViewModel>(new { order = serialized });
				});
			}
		}

        private bool ValidateFields()
        {
            if (string.IsNullOrWhiteSpace(BookAddress))
            {
                this.Services().Message.ShowMessage(this.Services().Localize["InvalidAddressTitle"], this.Services().Localize["InvalidAddressMessage"]);
                return false;
            }
            if (string.IsNullOrWhiteSpace(FriendlyName))
            {
                this.Services().Message.ShowMessage(this.Services().Localize["SaveAddressEmptyFieldTitle"], this.Services().Localize["SaveAddressEmptyFieldMessage"]);
                return false;
            }
            return true;
        }
	}
}

