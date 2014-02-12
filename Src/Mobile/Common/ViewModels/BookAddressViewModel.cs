using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using apcurium.MK.Booking.Mobile.Messages;
using apcurium.MK.Common.Extensions;
using System.Threading;
using apcurium.MK.Common.Entity;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Booking.Mobile.Extensions;
using ServiceStack.Text;
using apcurium.MK.Common;
using System.Windows.Input;

namespace apcurium.MK.Booking.Mobile.ViewModels
{
    // TODO: We don't need a full MvvmCross view model here
    // Just need NotifyPropertyChanged and this.GetCommand
    public class BookAddressViewModel : BaseViewModel
    {
        private CancellationTokenSource _cancellationToken;
        private bool _isExecuting;
        private Func<Address> _getAddress;
        private Action<Address> _setAddress;
        private string _id;
        private string _searchingTitle;

        public event EventHandler AddressChanged;
        public event EventHandler AddressCleared;

		public void Init(Func<Address> getAddress, Action<Address> setAddress)
        {
            _getAddress = getAddress;
            _setAddress = setAddress;
            _id = Guid.NewGuid().ToString();
            _searchingTitle = this.Services().Localize["AddressSearchingText"];
        }

        public string AddressLine2
        {
            get
            {
                var addressDisplay = "";
                var adr = _getAddress();
                if (adr != null)
                {
                    if ((adr.AddressType == "place") || (Params.Get(adr.City, adr.State, adr.ZipCode).Count(s => s.HasValue()) == 0))
                    {
                        addressDisplay =  adr.FullAddress;
                    }
                    else
                    {
                        addressDisplay =  Params.Get(adr.City, adr.State, adr.ZipCode).Where(s => s.HasValue()).JoinBy(", ");
                    }
                }
                return addressDisplay;
            }
        }

        public string EmptyAddressPlaceholder { get; set; }

        public string AddressLine1
        {
            get
            {
				RaisePropertyChanged(() => IsPlaceHolder);                
                return GetAddress().HasValue() ? GetAddress() : EmptyAddressPlaceholder;
            }
        }

        private string GetAddress()
        {
            var adr = _getAddress();
            if (adr == null)
            {
                return "";
            }
            if (adr.BuildingName.HasValue())
            {
                return Params.Get(adr.BuildingName, adr.Street).Where(s => s.HasValue() && s.Trim().HasValue()).JoinBy(", ");
            }
            return Params.Get(adr.StreetNumber, adr.Street).Any(s => s.HasValue() && s.Trim().HasValue())
                 ? Params.Get(adr.StreetNumber, adr.Street).Where(s => s.HasValue() && s.Trim().HasValue()).JoinBy(" ") 
                 : adr.FullAddress;
        }



        public bool IsPlaceHolder
        {
            get
            {
                return Model.FullAddress.IsNullOrEmpty();
            }
        }

        public Address Model { get { return _getAddress(); } set { _setAddress(value); } }       

    }
}
