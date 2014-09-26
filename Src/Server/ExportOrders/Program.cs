using apcurium.MK.Booking.Api.Client.TaxiHail;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Contract.Security;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ExportTool
{
    class Program
    {
        const string CONFIG_FILE_NAME = "export.cfg";
        const string FILE_DATE_FORMAT = "yyyy-dd-M--HH-mm-ss";

        private static Dictionary<string, string> _configItems;
        private static TimeSpan _offset;
        private static DateTime _startDate;
        private static DateTime _endDate;
        private static AuthenticationData _token;
        private static string _url;
        static void Main(string[] args)
        {
            string[] config;

            try
            {
                config = File.ReadAllLines(CONFIG_FILE_NAME);
            }
            catch (Exception e)
            {
                Console.WriteLine(string.Format("Please configure the Export Tool in {0} as following:", CONFIG_FILE_NAME));

                var initialConfig = string.Format(@"ExportPath=C:\My\Export\Path{0}ApiUrl=http://Url/To/Api{0}Username=admin_username{0}Password=admin_password",
                    Environment.NewLine);

                Console.WriteLine(initialConfig);
                File.WriteAllText(CONFIG_FILE_NAME, initialConfig);

                return;
            }

            _configItems = config.GroupBy(s => s.Replace(s.Split('=')[0] + "=", ""), x => x.Split('=')[0])
                      .ToDictionary(g => g.First(), g => g.Key);

            if (!Directory.Exists(_configItems["ExportPath"]))
            {
                try
                {
                    Directory.CreateDirectory(_configItems["ExportPath"]);
                }
                catch
                {
                    Console.WriteLine(string.Format("Verify your Export Path in {0}", CONFIG_FILE_NAME));
                    return;
                }
            }

            _url = _configItems["ApiUrl"];

            _startDate = DateTime.Now.Subtract(TimeSpan.FromDays(1));
            _endDate = DateTime.Now;

            try
            {
                var auth = new AuthServiceClient(_url, null, null);
                var response = auth.Authenticate(_configItems["Username"], _configItems["Password"]);
                response.Wait();
                _token = response.Result;
            }
            catch
            {
                Console.WriteLine(string.Format("Verify your admin credentials in {0}", CONFIG_FILE_NAME));
                return;
            }

            ExportOrders();
        }

        private static void ExportOrders()
        {
            try
            {
                var path = (_configItems["ExportPath"] + @"\").Replace(@"\\", @"\");
                var timestamp = DateTime.Now.ToString(FILE_DATE_FORMAT);
                var accountsFile = path + "account_" + timestamp + ".txt";
                var ordersFile = path + "orders_" + timestamp + ".txt";

                var client = new ExportDataServiceClient(_url, _token.SessionId, null);

                var orders = Export(client, DataType.Orders);
                var accounts = Export(client, DataType.Accounts);

                File.WriteAllText(ordersFile, orders);
                File.WriteAllText(accountsFile, accounts);
                
            }
            catch (Exception e)
            {
                Console.WriteLine("Problem when trying to export data");
                return;
            }
        }

        private static string Export(ExportDataServiceClient client, DataType type)
        {
            var request = new ExportDataRequest() { StartDate = DateTime.Now.Subtract(TimeSpan.FromDays(1)), EndDate = DateTime.Now, Target = type };
            var resultTask = client.GetOrders(request);
            resultTask.Wait();
            return resultTask.Result.ToString();
        }
    }
}

