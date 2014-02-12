using System.Linq;
using System.Threading.Tasks;
using apcurium.MK.Booking.Mobile.Extensions;
using apcurium.MK.Booking.Mobile.Framework.Extensions;
using apcurium.MK.Booking.Mobile.Messages;
using apcurium.MK.Common;
using apcurium.MK.Common.Entity;
using ServiceStack.Text;
using TinyMessenger;
using System.Windows.Input;

namespace apcurium.MK.Booking.Mobile.ViewModels
{
    public class BookStreetNumberViewModel : BaseViewModel
    {
        private string _ownerId;
        private TinyMessageSubscriptionToken _token;
		public void Init(string ownerId, string address)
        {
            _ownerId = ownerId;
            if (address != null) {
                Model = JsonSerializer.DeserializeFromString<Address>(address);
                if(Model.StreetNumber.HasValue())
                {
                    _streetNumberOrBuildingName = Model.StreetNumber;
                }

                if(Model.BuildingName.HasValue())
                {
                    _streetNumberOrBuildingName = Model.BuildingName;
                }
				RaisePropertyChanged(() => StreetNumberOrBuildingName);
            }
            _token = this.Services().MessengerHub.Subscribe<AddressSelected>(OnAddressSelected, selected => selected.OwnerId == _ownerId);
        }

        public int NumberOfCharAllowed
        {
			get{ return Settings.NumberOfCharInRefineAddress;}
        }

        string _streetNumberOrBuildingName;
        public string StreetNumberOrBuildingName {
            get {
                return _streetNumberOrBuildingName;
            }
            set {
                _streetNumberOrBuildingName = value;
				RaisePropertyChanged ();
            }
        }

        Address _model;
        public Address Model {
            get {
                return _model;
            }
            set {
                _model = value;
            }
        }

        public string StreetCity
        {
            get{
                return Params.Get(  _model.Street, _model.City ).Where( s=>s.HasValue() ).JoinBy( ", " );
            }
        }

		public ICommand NavigateToSearch
        {
            get {
                return this.GetCommand(() =>
                 {
                     this.Services().MessengerHub.Unsubscribe<AddressSelected>(_token);
                    _token.Dispose ();
                    _token = null;

                    ShowViewModel<AddressSearchViewModel> (new { search = "", ownerId = _ownerId , places = "false"});                                       
                    Close( this );
                });
            }
        }

		public ICommand NavigateToPlaces
        {
            get {
                return this.GetCommand(() =>
                                  {
                    this.Services().MessengerHub.Unsubscribe<AddressSelected>(_token);
                    _token.Dispose ();
                    _token = null; 
                    ShowViewModel<AddressSearchViewModel> (new { search = "", ownerId = _ownerId, places = "true" });                                       
                    Close( this );
                });
            }
        }

		public ICommand DeleteAddressCommand
        {
            get {
                return this.GetCommand(() =>
                                  {
                    this.Services().MessengerHub.Publish(new AddressSelected(this, null, _ownerId, false));                                                        
                    Close( this );
                });
            }
        }



        void OnAddressSelected (AddressSelected obj)
        {
            Close(this);
        }

		public ICommand SaveCommand
        {
            get
            {
                return this.GetCommand(() => 
                {
                    Model.UpdateStreetOrNumberBuildingName(StreetNumberOrBuildingName);
                    this.Services().MessengerHub.Publish(new AddressSelected(this, Model, _ownerId, true));                    
                });
            }
        }
    }
}

