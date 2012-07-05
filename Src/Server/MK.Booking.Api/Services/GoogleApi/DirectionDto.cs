using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace apcurium.MK.Booking.Api.Services.GoogleApi
{
 
    public class Distance
    {
        public string Text { get; set; }
        public int Value { get; set; }
    }

    public class Duration
    {
        public string Text { get; set; }
        public int Value { get; set; }
    }

 
    public class Polyline
    {
        public string Points { get; set; }
    }

    public class Step
    {
        public Distance Distance { get; set; }
        public Duration Duration { get; set; }
        public Location End_location { get; set; }
        public string Html_instructions { get; set; }
        public Polyline Polyline { get; set; }
        public Location Start_location { get; set; }
        public string Travel_mode { get; set; }
    }

    public class Leg
    {
        public Distance Distance { get; set; }
        public Duration Duration { get; set; }
        public string End_address { get; set; }
        public Location End_location { get; set; }
        public string Start_address { get; set; }
        public Location Start_location { get; set; }
        public List<Step> Steps { get; set; }
        
    }

    public class OverviewPolyline
    {
        public string Points { get; set; }
    }

    public class Route
    {
        public Bounds Bounds { get; set; }
        public string Copyrights { get; set; }
        public List<Leg> Legs { get; set; }
        public OverviewPolyline Overview_polyline { get; set; }
        public string Summary { get; set; }     
    }

    public class DirectionResult
    {
        public List<Route> Routes { get; set; }
        public ResultStatus Status { get; set; }
    }

}
