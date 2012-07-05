using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using apcurium.MK.Common.Extensions;

namespace apcurium.MK.Booking.Api.Services.GoogleApi
{
 

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
        
        public IEnumerable<AddressComponentType> AddressComponentTypes { get { return Types.Select( type=> type.ToEnum<AddressComponentType>( true, AddressComponentType.Unkown ) );}}


    }



  
    public class Geometry
    {
        public Location Location { get; set; }
        public string Location_type { get; set; }
        public Viewport Viewport { get; set; }
        public Bounds Bounds { get; set; }
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
