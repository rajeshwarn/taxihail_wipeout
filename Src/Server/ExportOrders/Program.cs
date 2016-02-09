using apcurium.MK.Booking.Api.Client.TaxiHail;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Contract.Security;
using ExportTool.Properties;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ExportTool
{
    class Program
    {
       
        const string FILE_DATE_FORMAT = "yyyy-dd-M--HH-mm-ss";

        private static TimeSpan _offset;
        private static DateTime _startDate;
        private static DateTime _endDate;
        private static AuthenticationData _token;
        private static string _url;
        static void Main(string[] args)
        {
            if ( !Directory.Exists( Settings.Default.ExportPath ) )
            {
                Console.WriteLine(string.Format("Export path doesn't exist : {0}", Settings.Default.ExportPath ));

                return;
            }

            _url = Settings.Default.ApiUrl ;

            _startDate = DateTime.Now.Subtract(TimeSpan.FromDays(1));
            _endDate = DateTime.Now;

            try
            {
                var auth = new AuthServiceClient(_url, null, null, null, null);
                var response = auth.Authenticate(Settings.Default.Username, Settings.Default.Password);
                response.Wait();
                _token = response.Result;
            }
            catch
            {
                Console.WriteLine(string.Format("Verify your admin credentials, user {0} and password {1} are invalid", Settings.Default.Username , Settings.Default.Password ));
                return;
            }

            ExportOrders();
        }

        private static void ExportOrders()
        {
            try
            {
                var path = (Settings.Default.ExportPath + @"\").Replace(@"\\", @"\");
                var timestamp = DateTime.Now.ToString(FILE_DATE_FORMAT);
                var accountsFile = path + "account_" + timestamp + ".csv";
                var ordersFile = path + "orders_" + timestamp + ".csv";

                var client = new ExportDataServiceClient(_url, _token.SessionId, null, null);

                var orders = Export(client, DataType.Orders);
                var accounts = Export(client, DataType.Accounts);

                File.WriteAllText(ordersFile, orders);
                File.WriteAllText(accountsFile, accounts);
                
            }
            catch (Exception e)
            {
                
                Console.WriteLine("Problem when trying to export data");
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
                return;
            }
        }

        private static string Export(ExportDataServiceClient client, DataType type)
        {
            var start = DateTime.Now.Date.Subtract(TimeSpan.FromDays(1)).Subtract(TimeSpan.FromSeconds(1));
            var end = DateTime.Now.Date.Subtract(TimeSpan.FromSeconds(1));
            
            var request = new ExportDataRequest() { StartDate = start , EndDate = end , Target = type };
            var resultTask = client.GetOrders(request);
            resultTask.Wait();
            return resultTask.Result.ToString();
        }
    }
}

