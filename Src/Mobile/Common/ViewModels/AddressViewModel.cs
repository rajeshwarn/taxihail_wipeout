using System.Linq;
using apcurium.MK.Common;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Extensions;
namespace apcurium.MK.Booking.Mobile.ViewModels
{
	public class AddressViewModel
	{
		

		public Address Address { get; set; }

        public string DisplayLine1
        {
            get
            {
                if ( ( Address.AddressType ==  "place" ) ||  ( Address.FriendlyName.HasValue () ) )
                {
                    return Address.FriendlyName;
                }
                else if ( Params.Get( Address.StreetNumber,Address.Street).Count ( s=> s.HasValue () ) == 0 ) 
                {
                    return Address.FullAddress;
                }
                else
                {
                    return Params.Get( Address.StreetNumber , Address.Street ).Where ( s=> s.HasValue () ).JoinBy( " " );
                }
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
                else
                {
                    return Params.Get( Address.City,Address.State, Address.ZipCode ).Where ( s=> s.HasValue () ).JoinBy( ", " );
                }
            }
        }

		public bool ShowRightArrow { get; set; }
		public bool ShowPlusSign { get; set; }
		public bool IsFirst { get; set; }
        public bool IsLast { get; set; }
		public bool IsAddNew { get; set; }
	}
}

