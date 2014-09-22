using System.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using apcurium.MK.Booking.Api.Client.TaxiHail;
using apcurium.MK.Booking.ReadModel.Query;
using System.Globalization;
using apcurium.MK.Booking.Database;
using apcurium.MK.Booking.Api.Contract.Security;
using CsvHelper;

namespace ExportTool
{
    class Program
    {
        const string CONFIG_FILE_NAME = "export.cfg";

        private static Dictionary<string, string> _configItems;
        private static TimeSpan _offset;
        private static DateTime _startDate;
        private static DateTime _endDate;

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

                var initialConfig = string.Format(@"ConnectionString=Data Source=YOUR_SERVER;Initial Catalog=YOUR_CATALOG;Integrated Security=True; MultipleActiveResultSets=True{0}ApiUrl=http://Url/To/Api{0}Username=admin_username{0}Password=admin_password{0}IBS.TimeDifference=0{0}ExportPath=C:\My\Export\Path{0}FileDateFormat=yyyy-dd-M--HH-mm-ss",
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

            string url = _configItems["ApiUrl"];

            var ibsServerTimeDifference = long.Parse(_configItems["IBS.TimeDifference"]);

            _offset = new TimeSpan(ibsServerTimeDifference);
            _startDate = DateTime.Now.Subtract(TimeSpan.FromDays(10));
            _endDate = DateTime.Now;

            Mapping.RegisterMaps();

            try
            {
                var tryContext = new BookingDbContext(_configItems["ConnectionString"]);
                tryContext.Dispose();
            }
            catch
            {
                Console.WriteLine(string.Format("Verify your connection string in {0}", CONFIG_FILE_NAME));
                return;
            }

            Task<AuthenticationData> token;

            try
            {
                var auth = new AuthServiceClient(url, null, null);
                token = auth.Authenticate(_configItems["Username"], _configItems["Password"]);
            }
            catch
            {
                Console.WriteLine(string.Format("Verify your admin credentials in {0}", CONFIG_FILE_NAME));
                return;
            }

            ExportOrders();
            ExportAccount();
        }

        private static void ExportOrders()
        {
            try
            {
                var orderDao = new OrderDao(() => new BookingDbContext(_configItems["ConnectionString"]));

                var orders = orderDao.GetAllWithAccountSummary();

                var exportedOrders = orders.Where(x => x.CreatedDate >= _startDate && x.CreatedDate <= _endDate)
                    .Select(x =>
                    {
                        var operatingSystem = x.UserAgent;
                        var phone = string.IsNullOrWhiteSpace(x.Phone) ? "" : x.Phone.Trim();
                        var transactionId = string.IsNullOrEmpty(x.TransactionId) ||
                                            (x.TransactionId.Trim().Length <= 1)
                            ? string.Empty
                            : "Auth: " + (string.IsNullOrEmpty(x.TransactionId) ? "" : x.TransactionId.Trim());

                        var excelResult = new Dictionary<string, string>();

                        excelResult["Id"] = x.Id.ToString();
                        excelResult["IBS Account Id"] = x.IBSAccountId.ToString(CultureInfo.InvariantCulture);
                        excelResult["IBS Order Id"] = x.IBSOrderId.ToString();
                        excelResult["Name"] = x.Name;
                        excelResult["Phone"] = phone;
                        excelResult["Email"] = x.Email;
                        excelResult["Pickup Date"] = x.PickupDate.ToString("d", CultureInfo.InvariantCulture);
                        excelResult["Pickup Time"] = x.PickupDate.ToString("t", CultureInfo.InvariantCulture);
                        excelResult["Create Date"] = x.CreatedDate.Add(_offset)
                            .ToString("d", CultureInfo.InvariantCulture);
                        excelResult["Create Time"] = x.CreatedDate.Add(_offset)
                            .ToString("t", CultureInfo.InvariantCulture);
                        excelResult["Status"] = x.Status.ToString(CultureInfo.InvariantCulture);
                        excelResult["Pickup Address"] = x.PickupAddress.DisplayAddress;
                        excelResult["Drop Off Address"] = x.DropOffAddress.DisplayAddress;
                        excelResult["Mdt Tip"] = x.MdtTip.ToString();
                        excelResult["Mdt Toll"] = x.MdtToll.ToString();
                        excelResult["Mdt Fare"] = x.MdtFare.ToString();

                        excelResult["Charge Type"] = x.ChargeType;

                        excelResult["Payment Meter Amount"] = x.PaymentMeterAmount.ToString();
                        excelResult["Payment Tip Amount"] = x.PaymentTipAmount.ToString();
                        excelResult["Payment Total Amount"] = x.PaymentTotalAmount.ToString();
                        excelResult["Payment Type"] = x.PaymentType.ToString();
                        excelResult["Payment Provider"] = x.PaymentProvider.ToString();
                        excelResult["Transaction Id"] = transactionId;
                        excelResult["Authorization Code"] = x.AuthorizationCode;
                        excelResult["Card Token"] = x.CardToken;
                        excelResult["Account Card Token"] = x.AccountDefaultCardToken;

                        excelResult["PayPal Payer Id"] = x.PayPalPayerId;
                        excelResult["PayPal Token"] = x.PayPalToken;
                        excelResult["Is Cancelled"] = x.IsCancelled.ToString();
                        excelResult["Is Completed"] = x.IsCompleted.ToString();

                        excelResult["Vehicle Number"] = x.VehicleNumber;
                        excelResult["Vehicle Type"] = x.VehicleType;
                        excelResult["Vehicle Make"] = x.VehicleMake;
                        excelResult["Vehicle Model"] = x.VehicleModel;
                        excelResult["Vehicle Color"] = x.VehicleColor;
                        excelResult["Vehicle Registration"] = x.VehicleRegistration;
                        excelResult["Driver First Name"] = x.DriverFirstName;
                        excelResult["Driver Last Name"] = x.DriverLastName;

                        excelResult["Operating System"] = operatingSystem;
                        excelResult["User Agent"] = x.UserAgent;
                        excelResult["ClientVersion"] = x.ClientVersion;

                        foreach (var rate in x.Rating)
                        {
                            excelResult[rate.Key] = rate.Value;
                        }

                        return excelResult;
                    });

                var resultOrders = exportedOrders.OrderBy(order => order["Create Date"])
                                     .ThenBy(order => order["Create Time"]).ToList();

                SerializeDictionaryToFile(resultOrders as List<Dictionary<string, string>>, new FileStream(_configItems["ExportPath"] + @"\" + "orders_" + DateTime.Now.ToString(_configItems["FileDateFormat"]) + ".txt", FileMode.CreateNew));
            }
            catch
            {
                Console.WriteLine("Problem when trying to export orders");
                return;
            }
        }

        private static void ExportAccount()
        {
            try
            {
                var accountDao = new AccountDao(() => new BookingDbContext(_configItems["ConnectionString"]));
                var appStartUpLogDao = new AppStartUpLogDao(() => new BookingDbContext(_configItems["ConnectionString"]));

                var accounts = accountDao.GetAll().ToList().Where(x => x.CreationDate >= _startDate && x.CreationDate <= _endDate).ToList();
                var startUpLogs = appStartUpLogDao.GetAll().ToList();

                var accountResults = from a in accounts
                                     join s in startUpLogs on a.Id equals s.UserId into matchingLog
                                     from m in matchingLog.DefaultIfEmpty()
                                     select new
                                     {
                                         a.Id,
                                         a.IBSAccountId,
                                         CreateDate = a.CreationDate,
                                         CreateTime = a.CreationDate,
                                         a.Settings.Name,
                                         a.Settings.Phone,
                                         a.Email,
                                         a.DefaultCreditCard,
                                         a.DefaultTipPercent,
                                         a.Language,
                                         a.TwitterId,
                                         a.FacebookId,
                                         a.IsAdmin,
                                         a.IsConfirmed,
                                         a.DisabledByAdmin,
                                         LastLaunch = (m == null ? null : m.DateOccured.ToLocalTime().ToString(CultureInfo.InvariantCulture)),
                                         Platform = (m == null ? null : m.Platform),
                                         PlatformDetails = (m == null ? null : m.PlatformDetails),
                                         ApplicationVersion = (m == null ? null : m.ApplicationVersion),
                                         ServerVersion = (m == null ? null : m.ServerVersion)
                                     };

                var exportedAccount = accountResults.ToList().Where(x => x.CreateDate >= _startDate && x.CreateDate <= _endDate)
                        .Select(x =>
                        {
                            var excelResult = new Dictionary<string, string>();
                            excelResult["Id"] = x.Id.ToString();
                            excelResult["IBS Account Id"] = x.IBSAccountId.ToString(CultureInfo.InvariantCulture);
                            excelResult["Create Date"] = x.CreateDate.Add(_offset)
                                .ToString("d", CultureInfo.InvariantCulture);
                            excelResult["Create Time"] = x.CreateDate.Add(_offset)
                                .ToString("t", CultureInfo.InvariantCulture);
                            excelResult["Name"] = x.Name;
                            excelResult["Phone"] = x.Phone;
                            excelResult["Email"] = x.Email;
                            excelResult["Default Credit Card"] = x.DefaultCreditCard.ToString();
                            excelResult["Default Tip Percent"] = x.DefaultTipPercent.ToString();
                            excelResult["Language"] = x.Language;
                            excelResult["Twitter"] = x.TwitterId;
                            excelResult["Facebook"] = x.FacebookId;
                            excelResult["Is Admin"] = x.IsAdmin.ToString();
                            excelResult["Is Confirmed"] = x.IsConfirmed.ToString();
                            excelResult["Disabled By Admin"] = x.DisabledByAdmin.ToString();
                            excelResult["Last Launch"] = x.LastLaunch;
                            excelResult["Platform"] = x.Platform;
                            excelResult["Platform Details"] = x.PlatformDetails;
                            excelResult["Application Version"] = x.ApplicationVersion;
                            excelResult["Server Version"] = x.ServerVersion;
                            return excelResult;
                        });

                SerializeDictionaryToFile(exportedAccount.ToList<Dictionary<string, string>>(), new FileStream(_configItems["ExportPath"] + @"\" + "accounts_" + DateTime.Now.ToString(_configItems["FileDateFormat"]) + ".txt", FileMode.CreateNew));
            }
            catch (Exception e)
            {
                Console.WriteLine("Problem when trying to export accounts");
                return;
            }
        }

        private static void SerializeDictionaryToFile(List<Dictionary<string, string>> response, FileStream stream)
        {
            if (response == null)
            {
                Console.WriteLine("There was no result for the specified period.");
                return;
            }

            using (var writer = new CsvWriter((new StreamWriter(stream))))
            {
                var orderedColumns = response.OrderByDescending(x => x.Keys.Count);
                var columns = orderedColumns.Any() ? orderedColumns.First() : new Dictionary<string, string>();

                string columnsName = null;

                foreach (var column in columns)
                {
                    if (columnsName == null)
                        columnsName = "\"" + column.Key + "\"";
                    else
                        columnsName += "," + "\"" + column.Key + "\"";
                }

                writer.WriteField(columnsName, false);
                writer.NextRecord();

                foreach (var line in response)
                {
                    string columnValue = null;
                    foreach (var column in line)
                    {
                        if (columnValue == null)
                            columnValue = "\"" + column.Value + "\"";
                        else
                            columnValue += "," + "\"" + column.Value + "\"";
                    }
                    writer.WriteField(columnValue, false);
                    writer.NextRecord();
                }
            }
        }
    }
}

