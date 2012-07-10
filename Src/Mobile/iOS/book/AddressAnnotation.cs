using System;
using MonoTouch.MapKit;
using MonoTouch.CoreLocation;

namespace TaxiMobileApp
{
	public enum AddressAnnotationType
	{
		Pickup = 0,
		Destination = 1,
		Taxi = 2,
	}
	public class AddressAnnotation : MKAnnotation
	{
			/// <summary>
		/// Need this constructor to set the fields, since the public
		/// interface of this class is all READ-ONLY
		/// <summary>
		public AddressAnnotation (CLLocationCoordinate2D coord, AddressAnnotationType type, string t, string s) : base()
		{
			AddressType = type;
			_coordinate = coord;
			_title = t;
			_subtitle = s;
			
		}
		
		private CLLocationCoordinate2D _coordinate;
		private string _title, _subtitle;
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
	
	}
}



