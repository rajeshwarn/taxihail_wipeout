using System;

namespace apcurium.MK.Booking.Mobile.Data
{
    public class Coordinate
    {
        public double Latitude { get; set; }
        
        public double Longitude{ get; set; }

        public CoordinatePrecision Precision
        {
            get
            {
                var acc = Convert.ToInt32( Accuracy );
                if (acc <= (int)CoordinatePrecision.Fine)
                {
                    return CoordinatePrecision.Fine;
                }
                if (acc <= (int)CoordinatePrecision.Medium)
                {
                    return CoordinatePrecision.Medium;
                }
                if (acc <= (int)CoordinatePrecision.Coarse)
                {
                    return CoordinatePrecision.Coarse;
                }
                return CoordinatePrecision.BallPark;
            }
        }

        public double Accuracy { get; set; }

        public CoordinateRefreshTime RefreshTime
        {
            get
            {
                if (RefreshTimeInTicks <= 0)
                {
                    return CoordinateRefreshTime.ALongTimeAgo;
                }
                var time = new DateTime(RefreshTimeInTicks);
                var refreshSpan = DateTime.Now.Subtract(time);
                if (refreshSpan <= TimeSpan.FromMinutes(2))
                {
                    return CoordinateRefreshTime.Recently;
                }
                if (refreshSpan <= TimeSpan.FromMinutes(10))
                {
                    return CoordinateRefreshTime.NotRecently;
                }
                return CoordinateRefreshTime.ALongTimeAgo;
            }
        }
        
        public long RefreshTimeInTicks { get; set; }

        public bool IsUsable
        {
            get
            {
                return ((RefreshTime == CoordinateRefreshTime.Recently) && (Precision == CoordinatePrecision.Fine));                
            }

        }


    }
}