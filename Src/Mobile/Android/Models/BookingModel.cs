using System;
using Android.OS;
using apcurium.MK.Booking.Mobile.Data;

namespace apcurium.MK.Booking.Mobile.Client.Models
{
	public class BookingModel
    {
		public BookingModel()
       	{
			Data = new BookingInfoData();
	    }
		
		public BookingInfoData Data {
			get;
			set;
		}
		
        public string Origin { 
			get { return Data.PickupLocation.Address; }
			set { Data.PickupLocation.Address = value; }
		}
        public string Destination {
			get { return Data.DestinationLocation.Address; }
			set { Data.DestinationLocation.Address = value; }
		}
        public string Appartment {
			get { return Data.PickupLocation.Apartment; }
			set { Data.PickupLocation.Apartment = value; }
		}
        public string RingCode {
			get { return Data.PickupLocation.RingCode; }
			set { Data.PickupLocation.RingCode = value; }
		}
		        
		
		public string Distance {
			get { 
				var distance = Data.GetDistance();
				return distance.HasValue ? Math.Round( distance.Value / 1000, 1 ).ToString() + "km" :  AppContext.Current.App.GetString( Resource.String.NotAvailable ); }
		}  
		
		public string Price {
			get { 
				var price = Data.GetPrice( Data.GetDistance() );
				return price.HasValue ? price.ToString() : AppContext.Current.App.GetString( Resource.String.NotAvailable );

			}
		}	
	}
}
