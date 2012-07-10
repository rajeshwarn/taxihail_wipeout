using System;
using System.Linq;
//using MonoTouch.CoreLocation;
using apcurium.Framework;
using apcurium.Framework.Extensions;


namespace apcurium.MK.Booking.Mobile.Data
{
	
    
	public partial class LocationData
	{

		public LocationData ()
		{
			
		}
		
		public int Id { get;set;}
		
		public bool IsNew { get;set;}
		
		public bool IsFromHistory { get;set;}
		
		public string Display { get { return Params.Get( Name, Address ).Where( s=>s.HasValue() ).JoinBy( " - " );} }
		
		
		public string Name  {  get; set; }
		
		private string _name;
		public string Address
		{get
			
			{
				return _name;
			}
			set
			{
				_name = value;
			}
		}
		public double? Longitude { get; set; }
		
		public double? Latitude { get; set; }
				
		public string Apartment { get; set; }

		public string RingCode { get; set; }
		
		public bool IsHistoricEmptyItem{ get; set; }
		
		public bool IsAddNewItem{ get; set; }
		
		public bool IsGPSDetected{ get; set; }
		
		public bool IsGPSNotAccurate{ get; set; }
		
		
		public LocationData Copy()
		{
			var copy = new LocationData();
			copy.Id = Id;
			copy.Name = Name;
			copy.Address = Address;
			copy.Longitude = Longitude;
			copy.Latitude = Latitude;
			copy.Apartment = Apartment;
			copy.RingCode = RingCode;
			copy.IsGPSDetected = IsGPSDetected;
			copy.IsGPSNotAccurate = IsGPSNotAccurate;
			
			return copy;
		}
	
		
		
		public bool IsSame( LocationData data )
		{
			return (this.Address.ToSafeString().Trim().ToLower() == data.Address.ToSafeString().Trim().ToLower() ) && (this.RingCode.ToSafeString().Trim().ToLower() == data.RingCode.ToSafeString().Trim().ToLower() ) && (this.Apartment.ToSafeString().Trim().ToLower() == data.Apartment.ToSafeString().Trim().ToLower() );
		}
		
      
	}
}

