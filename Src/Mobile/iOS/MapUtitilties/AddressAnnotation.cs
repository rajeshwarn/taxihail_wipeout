using MonoTouch.CoreLocation;
using MonoTouch.MapKit;
using MonoTouch.UIKit;
using apcurium.MK.Booking.Mobile.Client.Helper;

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
	    public override CLLocationCoordinate2D Coordinate {
			get { return _coordinate; }
			set { _coordinate = value; }
		}

        private readonly string _title;
		public override string Title {
			get { return _title; }
		}

        private readonly string _subtitle;
		public override string Subtitle {
			get { return _subtitle; }
		}
		
        public AddressAnnotationType AddressType { get; private set; }

        public UIImage GetImage()
        {
            return GetImage(AddressType);
        }

        public static UIImage GetImage(AddressAnnotationType addressType)
        {
            switch (addressType) 
            {
                case AddressAnnotationType.Destination:
                    return ImageHelper.ApplyColorToImage("Assets/pin_destination.png", UIColor.FromRGB(255, 0, 18));
                case AddressAnnotationType.Taxi:
                    return UIImage.FromFile("Assets/pin_cab.png");
                case AddressAnnotationType.NearbyTaxi:
                    return UIImage.FromFile("nearby.png");
                case AddressAnnotationType.NearbyTaxiCluster:
                    return UIImage.FromFile("Assets/pin_cluster.png");
                default:
                    return ImageHelper.ApplyColorToImage("hail_icon.png", UIColor.FromRGB(0, 192, 49));
            }
        }
	}
}



