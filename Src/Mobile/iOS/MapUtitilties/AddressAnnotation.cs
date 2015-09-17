using CoreLocation;
using MapKit;
using UIKit;
using apcurium.MK.Booking.Mobile.Client.Helper;
using System.Windows.Input;

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
        private static readonly UIColor Red = UIColor.FromRGB(255, 0, 23);
        private static readonly UIColor Green = UIColor.FromRGB(30, 192, 34);

		public AddressAnnotation(CLLocationCoordinate2D coord, AddressAnnotationType type, string t, string s, bool useThemeColorForIcons, bool showSubtitleOnPin, string vehicleTypeLogoName = null)
		{
			AddressType = type;
			_coordinate = coord;
			_title = t;
			_subtitle = s;
            UseThemeColorForIcons = useThemeColorForIcons;
			ShowSubtitleOnPin = showSubtitleOnPin;
            _vehicleTypeLogoName = vehicleTypeLogoName;
		}
		
		private CLLocationCoordinate2D _coordinate;
        private string _vehicleTypeLogoName;

		public bool ShowSubtitleOnPin = true;

	    public override CLLocationCoordinate2D Coordinate 
        {
			get { return _coordinate; }
		}

        public override void SetCoordinate(CLLocationCoordinate2D value)
        {
            _coordinate = value;
        }

        public ICommand HideMedaillonsCommand
        {
            get;
            set; 
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
            return GetImage(AddressType, _vehicleTypeLogoName);
        }

        public static UIImage GetImage(AddressAnnotationType addressType, string vehicleTypeLogoName = null)
        {
            const string defaultIconName = "taxi";

            switch (addressType)
            {
                case AddressAnnotationType.Destination:
                    return UseThemeColorForIcons
                        ? ImageHelper.ApplyThemeColorToMapIcon("destination_icon.png", true)
                        : ImageHelper.ApplyColorToMapIcon("destination_icon.png", Red, true);
                case AddressAnnotationType.Taxi:
                    return ImageHelper.ApplyThemeColorToMapIcon("taxi_icon.png", true);
                case AddressAnnotationType.NearbyTaxi:
                    return ImageHelper.ApplyThemeColorToMapIcon(string.Format("nearby_{0}.png", vehicleTypeLogoName ?? defaultIconName), false);
                case AddressAnnotationType.NearbyTaxiCluster:
                    return ImageHelper.ApplyThemeColorToMapIcon(string.Format("cluster_{0}.png", vehicleTypeLogoName ?? defaultIconName), false);
                default:
                    return UseThemeColorForIcons
                        ? ImageHelper.ApplyThemeColorToMapIcon("hail_icon.png", true)
                        : ImageHelper.ApplyColorToMapIcon("hail_icon.png", Green, true);
            }
        }
	}
}