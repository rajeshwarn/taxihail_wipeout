using System.Linq;
using apcurium.MK.Booking.Mobile.Framework.Extensions;
using apcurium.MK.Booking.Mobile.ViewModels;
using apcurium.MK.Common.Entity;
using ServiceStack.Text;
using Cirrious.MvvmCross.Interfaces.Commands;
using apcurium.MK.Booking.Mobile.Messages;
using apcurium.MK.Common;
using TinyMessenger;
using TinyIoC;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Booking.Mobile.Extensions;
using System.Threading.Tasks;

namespace apcurium.MK.Booking.Mobile
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
                FirePropertyChanged(() => StreetNumberOrBuildingName);
            }
            _token = MessengerHub.Subscribe<AddressSelected> (OnAddressSelected, selected => selected.OwnerId == _ownerId);
        }

        public int NumberOfCharAllowed
        {
            get{

                    
                var max =  TinyIoCContainer.Current.Resolve<ICacheService>().Get<string>( "Client.NumberOfCharInRefineAddress");
                Task.Factory.SafeStartNew( () => TinyIoCContainer.Current.Resolve<ICacheService>().Set<string>( "Client.NumberOfCharInRefineAddress", TinyIoCContainer.Current.Resolve<IConfigurationManager>().GetSetting( "Client.NumberOfCharInRefineAddress" )));
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
                FirePropertyChanged (() => StreetNumberOrBuildingName);
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

        public IMvxCommand NavigateToSearch {
            get {
                return GetCommand(() =>
                 {
                    MessengerHub.Unsubscribe<AddressSelected> ( _token );
                    _token.Dispose ();
                    _token = null;

                    RequestNavigate<AddressSearchViewModel> (new { search = "", ownerId = _ownerId , places = "false"});                                       
                    RequestClose( this );
                });
            }
        }

        public IMvxCommand NavigateToPlaces {
            get {
                return GetCommand(() =>
                                  {
                    MessengerHub.Unsubscribe<AddressSelected> ( _token );
                    _token.Dispose ();
                    _token = null; 
                    RequestNavigate<AddressSearchViewModel> (new { search = "", ownerId = _ownerId, places = "true" });                                       
                    RequestClose( this );
                });
            }
        }

        public IMvxCommand DeleteAddressCommand {
            get {
                return GetCommand(() =>
                                  {

                    MessengerHub.Publish(new AddressSelected(this, null,_ownerId,false));                                                        
                    RequestClose( this );
                });
            }
        }



        void OnAddressSelected (AddressSelected obj)
        {
            RequestClose(this);
        }

        public IMvxCommand SaveCommand
        {
            get
            {
                return GetCommand(() => 
                {
                    Model.UpdateStreetOrNumberBuildingName(StreetNumberOrBuildingName);
                    MessengerHub.Publish(new AddressSelected(this, Model,_ownerId,true));                    
                });
            }
        }
    }
}

