using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;

namespace CustomerPortal.Web.Services
{
    public class GoogleApi
    {

        public CityInfo GetCityInfo(string city)
        {
            var result = new CityInfo();

            var rawJson = CallUrl(string.Format("http://maps.googleapis.com/maps/api/geocode/json?address={0}&sensor=false", city.Replace(" ", "%20")));
            
            var googleResult = JsonConvert.DeserializeObject<GResults>(rawJson);

            if ((googleResult.status == "OK") && ( googleResult.results.Count() > 0))
            {
                var r = googleResult.results.ElementAt(0); 
                result.Name = r.formatted_address;
                result.Center = new Coordinate{ Latitude = r.geometry.location.lat, Longitude = r.geometry.location.lng };
                result.SouthwestCoordinate = new Coordinate { Latitude = r.geometry.viewport.southwest.lat - 0.2, Longitude = r.geometry.viewport.southwest.lng - 0.2 };
                result.NortheastCoordinate = new Coordinate { Latitude = r.geometry.viewport.northeast.lat + 0.2, Longitude = r.geometry.viewport.northeast.lng + 0.2 };

                var rawTimeZone = CallUrl(string.Format(CultureInfo.InvariantCulture, "https://maps.googleapis.com/maps/api/timezone/json?location={0},{1}&timestamp={2}&sensor=false",  result.Center.Latitude , result.Center.Longitude, ToUnixTime(DateTime.Now)  ));
                var googleTimezone = JsonConvert.DeserializeObject<GTimezone>(rawTimeZone);
                if ( googleTimezone.status == "OK")
                {
                    result.TimeDifference = TimeSpan.FromSeconds(googleTimezone.rawOffset);
                }

            }
            return result;
        }

        
        public long ToUnixTime(DateTime date)
        {
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return Convert.ToInt64((date.ToUniversalTime() - epoch).TotalSeconds);
        }

        private string CallUrl(string url)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            try
            {
                WebResponse response = request.GetResponse();
                using (Stream responseStream = response.GetResponseStream())
                {
                    StreamReader reader = new StreamReader(responseStream, Encoding.UTF8);
                    return reader.ReadToEnd();
                }
            }
            catch (WebException ex)
            {
                WebResponse errorResponse = ex.Response;
                string errorText = "";
                using (Stream responseStream = errorResponse.GetResponseStream())
                {
                    StreamReader reader = new StreamReader(responseStream, Encoding.GetEncoding("utf-8"));
                    errorText = reader.ReadToEnd();
                    // log errorText
                }
                throw new Exception( errorText, ex );
            }
        }

    }

    public class CityInfo
    {
        public string Name { get; set; }

        public TimeSpan TimeDifference { get; set; }

        public Coordinate Center { get; set; }
        public Coordinate SouthwestCoordinate { get; set; }
        public Coordinate NortheastCoordinate { get; set; }
    }

    public class Coordinate
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }



    public class GAddressComponent
    {
        public string long_name { get; set; }
        public string short_name { get; set; }
        public List<string> types { get; set; }
    }

    public class GCoordinate
    {
        public double lat { get; set; }
        public double lng { get; set; }
    }

 
    public class GBounds
    {
        public GCoordinate northeast { get; set; }
        public GCoordinate southwest { get; set; }
    }

    
    public class GGeometry
    {
        public GBounds bounds { get; set; }
        public GCoordinate location { get; set; }
        public string location_type { get; set; }
        public GBounds viewport { get; set; }
    }

    public class GResult
    {
        public List<GAddressComponent> address_components { get; set; }
        public string formatted_address { get; set; }
        public GGeometry geometry { get; set; }
        public List<string> types { get; set; }
    }

    public class GResults
    {
        public List<GResult> results { get; set; }
        public string status { get; set; }
    }


    public class GTimezone
    {
        public double dstOffset { get; set; }
        public double rawOffset { get; set; }
        public string status { get; set; }
        public string timeZoneId { get; set; }
        public string timeZoneName { get; set; }
    }
}