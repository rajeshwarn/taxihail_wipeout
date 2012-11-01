﻿using System.IO;
using apcurium.MK.Booking.Api.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using apcurium.MK.Booking.Api.Contract.Requests;
using System.Threading;

namespace PopularAddressLoader
{
    class Program
    {
        static void Main(string[] args)
        {
            string url = @"http://staging.taxihail.biz:8181/taxiworld/Api/";
            //string url = @"http://localhost/apcurium.MK.Web/Api/";

            Console.ReadKey();
            var auth = new AuthServiceClient(url, null);
            var token = auth.Authenticate("taxihail@apcurium.com", "1l1k3B4n4n@");
            var c = new PopularAddressesServiceClient(url, token.SessionId);


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

                var a = new PopularAddress() {FriendlyName = string.Format("Train Station ({1})", values[0], values[1]), Latitude = lat, Longitude = lng, FullAddress = string.Format("{0} Station", values[0], values[1])};
                Console.WriteLine(a.FriendlyName);
                list.Add(a);

            }



            foreach (var popularAddress in list)
            {
                Console.WriteLine( popularAddress.FriendlyName );
                
                c.Add(popularAddress);
                Thread.Sleep(1000);
            }
                        

        }
    }
}
