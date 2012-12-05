using System;
using apcurium.MK.Booking.Mobile.ViewModels;
using apcurium.MK.Common.Entity;
using ServiceStack.Text;
using apcurium.Framework.Extensions;
using Cirrious.MvvmCross.Interfaces.Commands;
using Cirrious.MvvmCross.Commands;
using apcurium.MK.Booking.Mobile.Messages;

namespace apcurium.MK.Booking.Mobile
{
    public class BookStreetNumberViewModel : BaseViewModel
    {
        string _ownerId;

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
            MessengerHub.Subscribe<AddressSelected> (OnAddressSelected, selected => selected.OwnerId == _ownerId);
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

        public IMvxCommand NavigateToSearch {
            get {
                return new MvxRelayCommand (() =>
                 {
                    RequestNavigate<AddressSearchViewModel> (new { search = Model.BookAddress, ownerId = _ownerId });                    
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
                return new MvxRelayCommand(() => 
                {
                    Model.UpdateStreetOrNumberBuildingName(StreetNumberOrBuildingName);
                    MessengerHub.Publish(new AddressSelected(this, Model,_ownerId));                    
                });
            }
        }
    }
}

