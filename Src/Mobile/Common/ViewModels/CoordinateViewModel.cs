using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using apcurium.MK.Booking.Mobile.Data;

namespace apcurium.MK.Booking.Mobile.ViewModels
{
    public class CoordinateViewModel
    {
        
        public CoordinateViewModel()
        {
           
        }

		public static CoordinateViewModel Create(double lat, double lon, bool changeZoom=false)
		{
			return new CoordinateViewModel 
			{ 
				Coordinate = new Coordinate 
				{ 
					Latitude = lat, 
					Longitude = lon 
				}, 
				Zoom = changeZoom ? ZoomLevel.Close : ZoomLevel.DontChange 
			};
		}

        public Coordinate Coordinate { get; set; }

        public ZoomLevel Zoom{ get; set; }
        

    }
}