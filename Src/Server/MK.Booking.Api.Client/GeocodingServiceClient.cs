using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using apcurium.MK.Booking.Api.Contract.Resources;
using System.Globalization;

namespace apcurium.MK.Booking.Api.Client
{
    public class GeocodingServiceClient : BaseServiceClient
    {
        public GeocodingServiceClient(string url, AuthInfo credential)
            : base(url, credential)
        {
            Console.WriteLine( "GeocodingServiceClient");
            Console.WriteLine( _credential.Email );
            Console.WriteLine( _credential.Password );
            Console.WriteLine( "end GeocodingServiceClient");
        }


        public AddressList Search( string addressName )
        {
            var resource = string.Format("/geocode?Name={0}", addressName);
            var result = Client.Get<AddressList>(resource);
            return result;
        }


        public AddressList Search(double latitude, double longitude )
        {
            var resource = string.Format(CultureInfo.InvariantCulture, "/geocode?Lat={0}&Lng={1}", latitude, longitude );
            var result = Client.Get<AddressList>(resource);
            return result;
        }
        
    }
}
