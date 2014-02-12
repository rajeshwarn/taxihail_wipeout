using System.Linq;
using apcurium.MK.Common;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Extensions;
namespace apcurium.MK.Booking.Mobile.ViewModels
{
	public class AddressViewModel : BaseViewModel
	{
        public Address Address { get; set; }
        public bool ShowRightArrow { get; set; }
        public bool ShowPlusSign { get; set; }
        public string Icon { get; set; }	
        public bool IsAddNew { get; set; }

        private bool _isFirst;
        private bool _isLast;

        public string DisplayLine1
        {
            get
            {
                if ( ( Address.AddressType ==  "place" ) ||  ( Address.FriendlyName.HasValue () ) )
                {
                    return Address.FriendlyName;
                }
                return Params.Get( Address.StreetNumber,Address.Street).Count ( s=> s.HasValue () ) == 0 
                    ? Address.FullAddress
                    : Params.Get( Address.StreetNumber , Address.Street ).Where ( s=> s.HasValue () ).JoinBy( " " );
            }
        }

        public string DisplayLine2
        {
            get
            {
                if ( ( Address.AddressType ==  "place" ) || ( Params.Get( Address.City,Address.State, Address.ZipCode ).Count ( s=> s.HasValue () ) == 0 ) || ( Address.FriendlyName.HasValue () ) )
                {
                    return Address.FullAddress;
                }
                return Params.Get( Address.City,Address.State, Address.ZipCode ).Where ( s=> s.HasValue () ).JoinBy( ", " );
            }
        }

		
        public bool IsFirst { get{return _isFirst;} 
            set 
            {
                _isFirst = value;
				RaisePropertyChanged();
            }
        }

        public bool IsLast { get{return _isLast;} 
            set 
            {
                _isLast = value;
				RaisePropertyChanged();
            }
        }

		public AddressType Type { get; set; }

		public bool IsSearchResult { get; set; }
	}

	public enum AddressType
	{
		Favorites,
		Places,
		History,
	}
}

