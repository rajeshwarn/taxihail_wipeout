using MonoTouch.CoreLocation;
using MonoTouch.MapKit;

namespace apcurium.MK.Booking.Mobile.Client.MapUtitilties
{
	public enum AddressAnnotationType
	{
		Pickup = 0,
		Destination = 1,
		Taxi = 2,
        NearbyTaxi = 3,
        NearbyTaxiCluster = 4,

	}
	public class AddressAnnotation : MKAnnotation
	{
		
		public AddressAnnotation (CLLocationCoordinate2D coord, AddressAnnotationType type, string t, string s)
		{
			AddressType = type;
			_coordinate = coord;
			_title = t;
			_subtitle = s;
			
		}
		
		private CLLocationCoordinate2D _coordinate;
		private readonly string _title;
	    private readonly string _subtitle;

	    public override CLLocationCoordinate2D Coordinate {
			get { return _coordinate; }
			set { _coordinate = value; }
		}
		public override string Title {
			get { return _title; }
		}
		public override string Subtitle {
			get { return _subtitle; }
		}
		
		public AddressAnnotationType AddressType {
			get;
			private set;
		}

        public string GetImageFilename ()
        {
            return GetImageFilename(AddressType);

        }
        public static string GetImageFilename (AddressAnnotationType addressType)
        {
            switch (addressType) {
            case AddressAnnotationType.Destination:
                return "Assets/pin_destination.png";
            case AddressAnnotationType.Taxi:
                return "Assets/pin_cab.png";
            case AddressAnnotationType.NearbyTaxi:
                return "Assets/nearby-cab.png";
            case AddressAnnotationType.NearbyTaxiCluster:
                return "Assets/pin_cluster.png";
                default:
                return "Assets/pin_hail.png";
                
            }
            
        }
	
	}
}



