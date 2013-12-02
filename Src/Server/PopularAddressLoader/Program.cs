using System.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using apcurium.MK.Booking.Api.Client.TaxiHail;
using apcurium.MK.Booking.Api.Contract.Requests;
using System.Threading;
using apcurium.MK.Common.Entity;

namespace PopularAddressLoader
{
    class Program
    {
        static void Main(string[] args)
        {
            //string url = @"http://staging.taxihail.biz:8181/taxiworld/Api/";
            string url = @"http://localhost/apcurium.MK.Web/Api/";
            //string url = @"http://services.taxihail.com/Thriev/Api/";


            var auth = new AuthServiceClient(url, null, "Test");
            var token = auth.Authenticate("taxihail@apcurium.com", "1l1k3B4n4n@");

            //var direction = new DirectionsServiceClient(url, token.SessionId, "");
            //var di = direction.GetDirectionDistance(51.434028, -0.526826, 51.5035709, -0.199753);

            //di.Price.ToString();

            var c = new PopularAddressesServiceClient(url, token.SessionId, "Test");



            var reader = new StreamReader(File.OpenRead(@"D:\Dropbox\Mobile Knowledge\train.csv"));

            var list = new List<PopularAddress>();

            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                var values = line.Split(',');

                double lat;
                double lng;
                if (!double.TryParse(values[2], out lat))
                {
                    throw new InvalidDataException();
                }

                if (!double.TryParse(values[3], out lng))
                {
                    throw new InvalidDataException();
                }

                var a = new PopularAddress()
                {
                    Address = new Address { FriendlyName = string.Format("Train Station ({1})", values[0], values[1]), Latitude = lat, Longitude = lng, FullAddress = string.Format("{0} Station", values[0], values[1]) }
                };
                Console.WriteLine(a.Address.FriendlyName);
                list.Add(a);

            }



            foreach (var popularAddress in list)
            {
                Console.WriteLine(popularAddress.Address.FriendlyName);

                c.Add(popularAddress);
                Thread.Sleep(1000);
            }


        }
    }
}
