using System.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using apcurium.MK.Booking.Api.Client.TaxiHail;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.ReadModel.Query;
using System.Threading;
using apcurium.MK.Common.Entity;
using System.Globalization;

namespace ExportOrders
{
    class Program
    {
        static void Main(string[] args)
        {
            //string url = @"http://staging.taxihail.biz:8181/taxiworld/Api/";
            string url = @"http://localhost/apcurium.MK.Web/Api/";

            //var ibsServerTimeDifference =
               // _configurationManager.GetSetting("IBS.TimeDifference").SelectOrDefault(long.Parse, 0);
            //var offset = new TimeSpan(ibsServerTimeDifference);
            var offset = TimeSpan.FromMilliseconds(0);
            var startDate = DateTime.Now.Subtract(TimeSpan.FromDays(1));
            var endDate = DateTime.Now;
            
            //var param = GetParamsFromArgs(args);
            var connectionString = ""; //new ConnectionStringSettings("MkWeb", param.MkWebConnectionString);
            var accounts = new AccountDao(() => new BookingDbContext(connectionString.ConnectionString));

            var auth = new AuthServiceClient(url, null);
            var token = auth.Authenticate("taxihail@apcurium.com", "1l1k3B4n4n@");

            var orders = _orderDao.GetAllWithAccountSummary();

            var exportedOrders = orders.Where(x => x.CreatedDate >= startDate && x.CreatedDate <= endDate)
                .Select(x =>
                {
                    var operatingSystem = x.UserAgent.GetOperatingSystem();
                    var phone = string.IsNullOrWhiteSpace(x.Phone) ? "" : x.Phone.ToSafeString();
                    var transactionId = string.IsNullOrEmpty(x.TransactionId) ||
                                        (x.TransactionId.Trim().Length <= 1)
                        ? string.Empty
                        : "Auth: " + x.TransactionId.ToSafeString();

                    var excelResult = new Dictionary<string, string>();

                    excelResult["Id"] = x.Id.ToString();
                    excelResult["IBS Account Id"] = x.IBSAccountId.ToString(CultureInfo.InvariantCulture);
                    excelResult["IBS Order Id"] = x.IBSOrderId.ToString();
                    excelResult["Name"] = x.Name;
                    excelResult["Phone"] = phone;
                    excelResult["Email"] = x.Email;
                    excelResult["Email"] = x.Email;
                    excelResult["Pickup Date"] = x.PickupDate.ToString("d", CultureInfo.InvariantCulture);
                    excelResult["Pickup Time"] = x.PickupDate.ToString("t", CultureInfo.InvariantCulture);
                    excelResult["Create Date"] = x.CreatedDate.Add(offset)
                        .ToString("d", CultureInfo.InvariantCulture);
                    excelResult["Create Time"] = x.CreatedDate.Add(offset)
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

            return exportedOrders.OrderBy(order => order["Create Date"])
                                 .ThenBy(order => order["Create Time"]).ToList();


        }
    }
}

