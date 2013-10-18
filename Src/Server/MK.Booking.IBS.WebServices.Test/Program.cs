using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MK.Booking.IBS.WebServices.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            CallGetVehicleTypes("http://mk.drivelinq.com:6929/XDS_IASPI.DLL/soap/");
            CallGetVehicleTypes("http://mk.drivelinq.com:6928/XDS_IASPI.DLL/soap/");
            Console.ReadLine();
        }


        private static void CallGetZoneForCompany(string baseUrl)
        {

            Console.WriteLine("Calling CallGetZoneForCompany web service " + baseUrl);


            var staticService = new StaticDataservice { Url = baseUrl + "IStaticData" };


            try
            {
                var company = staticService.GetProviders("taxi", "test");

                foreach (var co in company)
                {
                    var zone = staticService.GetCompanyZoneByGPS("taxi", "test", co.ProviderNum, 45.42159, -75.6477);

                    Console.WriteLine(zone);


                }


            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }


        private static void CallGetVehicleTypes(string baseUrl)
        {

            Console.WriteLine("Calling CallGetVehicleTypes web service " + baseUrl);


            var staticService = new StaticDataservice { Url = baseUrl + "IStaticData" };


            try
            {
                var company = staticService.GetProviders("taxi", "test");

                foreach (var co in company)
                {
                    var vehicles = staticService.GetVehicleTypes("taxi", "test", Convert.ToInt32(co.ProviderNum));


                    foreach (var v in vehicles)
                    {
                        Console.WriteLine(v.Name);
                    }

                }


            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }


        private static void CallGetCompanyZoneByGPS(string baseUrl)
        {

            Console.WriteLine("Calling CallGetCompanyZoneByGPS web service " + baseUrl);


            var staticService = new StaticDataservice { Url = baseUrl + "IStaticData" };


            try
            {
                var company = staticService.GetProviders("taxi", "test").FirstOrDefault(p => p.isDefault);

                var zone = staticService.GetCompanyZoneByGPS("taxi", "test", company.ProviderNum, 45.3417, -75.9233);

                if (string.IsNullOrEmpty(zone))
                {
                    Console.WriteLine("Call made sucesfully but no zone found");
                }
                else
                {
                    Console.WriteLine("Zone found " + zone);

                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }


        private static void CallGetAvailableVehicles(string baseUrl)
        {

            Console.WriteLine("Calling CallGetAvailableVehicles web service " + baseUrl);


            var orderService = new WebOrder7Service { Url = baseUrl + "IWebOrder_7" };


            try
            {
                var vehicles = orderService.GetAvailableVehicles("taxi", "test", 45.3417, -75.9233, 10000, 10000);

                if (vehicles.Count() == 0)
                {
                    Console.WriteLine("Call made sucesfully but no vehicle found");
                }
                foreach (var c in vehicles)
                {
                    Console.WriteLine(string.Format("Vehicle found #{0} , Postion {1} / {2}", c.VehicleNumber, c.Latitude, c.Longitude));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
