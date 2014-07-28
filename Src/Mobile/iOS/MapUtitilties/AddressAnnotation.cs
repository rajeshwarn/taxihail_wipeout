using MonoTouch.CoreLocation;
using MonoTouch.MapKit;
using MonoTouch.UIKit;
using apcurium.MK.Booking.Mobile.Client.Helper;
using MonoTouch.CoreGraphics;

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
        public AddressAnnotation (CLLocationCoordinate2D coord, AddressAnnotationType type, string t, string s, bool useThemeColorForIcons)
		{
			AddressType = type;
			_coordinate = coord;
			_title = t;
			_subtitle = s;
            UseThemeColorForIcons = useThemeColorForIcons;
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

        public static bool UseThemeColorForIcons { get; private set; }

        public UIImage GetImage()
        {
            return GetImage(AddressType);
        }

        public static UIImage GetImage(AddressAnnotationType addressType)
        {
            switch (addressType)
            {
                case AddressAnnotationType.Destination:
                    return UseThemeColorForIcons
                        ? ImageHelper.ApplyThemeColorToImage("destination_icon.png", CGBlendMode.Hue)
                            : UIImage.FromFile("destination_icon.png");
                case AddressAnnotationType.Taxi:
                    return ImageHelper.ApplyThemeColorToImage("taxi_icon.png", CGBlendMode.Hue);
                case AddressAnnotationType.NearbyTaxi:
                    return ImageHelper.ApplyThemeColorToImage("nearby_taxi.png", CGBlendMode.Hue);
                case AddressAnnotationType.NearbyTaxiCluster:
                    return ImageHelper.ApplyThemeColorToImage("cluster.png", CGBlendMode.Hue);
                default:
                    return UseThemeColorForIcons
                        ? ImageHelper.ApplyThemeColorToImage("hail_icon.png", CGBlendMode.Hue)
                            : UIImage.FromFile("hail_icon.png");
            }
        }
	}
}



