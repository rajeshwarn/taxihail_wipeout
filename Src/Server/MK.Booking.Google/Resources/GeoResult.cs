using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using apcurium.MK.Common.Extensions;

namespace apcurium.MK.Booking.Google.Resources
{
    public enum ResultStatus
    {
        UNKNOWN,
        OK,
        ZERO_RESULTS,
        OVER_QUERY_LIMIT,
        REQUEST_DENIED,
        INVALID_REQUEST,

    }


    public class Viewport
    {
        public Location Northeast { get; set; }
        public Location Southwest { get; set; }
    }





    public class Bounds
    {
        public Location Northeast { get; set; }
        public Location Southwest { get; set; }
    }

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

    public enum AddressComponentType
    {
        Street_address,
        Route,
        Intersection,
        Political,
        Country,
        Administrative_area_level_1,
        Administrative_area_level_2,
        Administrative_area_level_3,
        Colloquial_area,
        Locality,
        Sublocality,
        Neighborhood,
        Premise,
        Subpremise,
        Postal_code,
        Natural_feature,
        Airport,
        Park,
        Bus_station,
        Transit_station,
        Point_of_interest,
        Post_box,
        Street_number,
        Floor,
        Room,
        Unkown,
    }

    public class AddressComponent
    {
        public string Long_name { get; set; }
        public string Short_name { get; set; }

        public List<string> Types { get; set; }

        public IEnumerable<AddressComponentType> AddressComponentTypes { get { return Types.Select(type => type.ToEnum<AddressComponentType>(true, AddressComponentType.Unkown)); } }


    }




   

    public class GeoObj
    {
        public List<AddressComponent> Address_components { get; set; }
        public string Formatted_address { get; set; }
        public Geometry Geometry { get; set; }

        public List<string> Types { get; set; }
        public IEnumerable<AddressComponentType> AddressComponentTypes { get { return Types.Select(type => type.ToEnum<AddressComponentType>(true, AddressComponentType.Unkown)); } }
    }

    public class GeoResult
    {
        public ResultStatus Status { get; set; }
        public List<GeoObj> Results { get; set; }
    }
}
