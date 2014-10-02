#region

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Web.UI.WebControls.WebParts;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Helpers;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Extensions;
using ServiceStack.Common.Web;
using ServiceStack.ServiceInterface;

#endregion

namespace apcurium.MK.Booking.Api.Services.Admin
{
    public class ExportDataService : Service
    {
        private readonly IAccountDao _accountDao;
        private readonly IConfigurationManager _configurationManager;
        private readonly IOrderDao _orderDao;
        private readonly IAppStartUpLogDao _appStartUpLogDao;

        public ExportDataService(IAccountDao accountDao, IOrderDao orderDao, IConfigurationManager configurationManager, IAppStartUpLogDao appStartUpLogDao)
        {
            _accountDao = accountDao;
            _orderDao = orderDao;
            _configurationManager = configurationManager;
            _appStartUpLogDao = appStartUpLogDao;
        }

        public object Post(ExportDataRequest request)
        {
            var offset = new TimeSpan(_configurationManager.ServerData.IBS.TimeDifference);
            
            var startDate = request.StartDate ?? DateTime.MinValue;
            var endDate = (request.EndDate ?? DateTime.MaxValue).AddDays(1); // Add one day to include the current day since it ends at midnight

            switch (request.Target)
            {
                case DataType.Accounts:
                    var accounts = _accountDao.GetAll().Where(x => x.CreationDate >= startDate && x.CreationDate <= endDate).ToList();
                    var startUpLogs = _appStartUpLogDao.GetAll().ToList();

                    // We join each accound details with their "last launch details"
                    return from a in accounts
                            join s in startUpLogs on a.Id equals s.UserId into matchingLog
                            from m in matchingLog.DefaultIfEmpty()
                            select new
                            {
                                a.Id,
                                a.IBSAccountId,
                                CreateDate = a.CreationDate.ToLocalTime().ToString("d", CultureInfo.InvariantCulture),
                                CreateTime = a.CreationDate.ToLocalTime().ToString("t", CultureInfo.InvariantCulture),
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
                case DataType.Orders:
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
            return new HttpResult(HttpStatusCode.NotFound);
        }
    }
}