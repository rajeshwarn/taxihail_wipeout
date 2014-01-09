using System.Linq;
using System.Threading.Tasks;
using apcurium.MK.Booking.Mobile.Extensions;
using apcurium.MK.Booking.Mobile.Framework.Extensions;
using apcurium.MK.Booking.Mobile.Messages;
using apcurium.MK.Common;
using apcurium.MK.Common.Entity;
using ServiceStack.Text;
using TinyMessenger;

namespace apcurium.MK.Booking.Mobile.ViewModels
{
    public class BookStreetNumberViewModel : BaseViewModel
    {
        private readonly string _ownerId;
        private TinyMessageSubscriptionToken _token;
        public BookStreetNumberViewModel (string ownerId, string address)
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
            get{


                var max = this.Services().Cache.Get<string>("Client.NumberOfCharInRefineAddress");
                Task.Factory.SafeStartNew(() => this.Services().Cache.Set("Client.NumberOfCharInRefineAddress", this.Services().Config.GetSetting("Client.NumberOfCharInRefineAddress")));
                int m;
                if ( int.TryParse( max , out m ) )
                {
                    return m;
                }
                return 10;
            }

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

        public AsyncCommand NavigateToSearch
        {
            get {
                return GetCommand(() =>
                 {
                     this.Services().MessengerHub.Unsubscribe<AddressSelected>(_token);
                    _token.Dispose ();
                    _token = null;

                    RequestNavigate<AddressSearchViewModel> (new { search = "", ownerId = _ownerId , places = "false"});                                       
                    RequestClose( this );
                });
            }
        }

        public AsyncCommand NavigateToPlaces
        {
            get {
                return GetCommand(() =>
                                  {
                    this.Services().MessengerHub.Unsubscribe<AddressSelected>(_token);
                    _token.Dispose ();
                    _token = null; 
                    RequestNavigate<AddressSearchViewModel> (new { search = "", ownerId = _ownerId, places = "true" });                                       
                    RequestClose( this );
                });
            }
        }

        public AsyncCommand DeleteAddressCommand
        {
            get {
                return GetCommand(() =>
                                  {
                    this.Services().MessengerHub.Publish(new AddressSelected(this, null, _ownerId, false));                                                        
                    RequestClose( this );
                });
            }
        }



        void OnAddressSelected (AddressSelected obj)
        {
            RequestClose(this);
        }

        public AsyncCommand SaveCommand
        {
            get
            {
                return GetCommand(() => 
                {
                    Model.UpdateStreetOrNumberBuildingName(StreetNumberOrBuildingName);
                    this.Services().MessengerHub.Publish(new AddressSelected(this, Model, _ownerId, true));                    
                });
            }
        }
    }
}

