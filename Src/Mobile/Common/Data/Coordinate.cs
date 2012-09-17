using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;



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
                else if (acc <= (int)CoordinatePrecision.Medium)
                {
                    return CoordinatePrecision.Medium;
                }
                else if (acc <= (int)CoordinatePrecision.Coarse)
                {
                    return CoordinatePrecision.Coarse;
                }
                else
                {
                    return CoordinatePrecision.BallPark;
                }
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
                else
                {
                    var time = new DateTime(RefreshTimeInTicks);
                    var refreshSpan = DateTime.Now.Subtract(time);
                    if (refreshSpan <= TimeSpan.FromMinutes(2))
                    {
                        return CoordinateRefreshTime.Recently;
                    }
                    else if (refreshSpan <= TimeSpan.FromMinutes(10))
                    {
                        return CoordinateRefreshTime.NotRecently;
                    }
                    else
                    {
                        return CoordinateRefreshTime.ALongTimeAgo;
                    }
                }
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