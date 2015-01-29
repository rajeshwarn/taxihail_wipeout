#region

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Extensions;
using ServiceStack.Common.Web;
using ServiceStack.ServiceInterface;
using ServiceStack.Text;

#endregion

namespace apcurium.MK.Booking.Api.Services.Admin
{
    public class ExportDataService : Service
    {
        private readonly IAccountDao _accountDao;
        private readonly IServerSettings _serverSettings;
        private readonly IReportDao _reportDao;
        private readonly IAppStartUpLogDao _appStartUpLogDao;

        public ExportDataService(IAccountDao accountDao, IReportDao reportDao, IServerSettings serverSettings, IAppStartUpLogDao appStartUpLogDao)
        {
            _accountDao = accountDao;
            _reportDao = reportDao;
            _serverSettings = serverSettings;
            _appStartUpLogDao = appStartUpLogDao;
        }

        public object Post(ExportDataRequest request)
        {
            var offset = new TimeSpan(_serverSettings.ServerData.IBS.TimeDifference);
            
            var startDate = request.StartDate ?? DateTime.MinValue;
            var endDate = request.EndDate.HasValue ? request.EndDate.Value.AddDays(1) : DateTime.MaxValue;// Add one day to include the current day since it ends at midnight

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

                    var orders = _reportDao.GetAll(startDate, endDate);
                    var exportedOrderReports = new List<Dictionary<string, string>>();
                    
                    orders.ForEach(orderReport =>
                        {
                            var orderReportEntry = new Dictionary<string, string>();
                            
                            orderReportEntry["Account.AccountId"] = orderReport.Account.AccountId.ToString();
                            orderReportEntry["Account.Name"] = orderReport.Account.Name.Trim();
                            orderReportEntry["Account.Phone"] = orderReport.Account.Phone.Trim();
                            orderReportEntry["Account.Email"] = orderReport.Account.Email.Trim();
                            orderReportEntry["Account.IBSAccountId"] = orderReport.Account.IBSAccountId.ToString();
                            orderReportEntry["Account.DefaultCardToken "] = orderReport.Account.DefaultCardToken.ToString();

                            orderReportEntry["Order.CompanyName"] = orderReport.Order.CompanyName;
                            orderReportEntry["Order.IBSOrderId"] = orderReport.Order.IBSOrderId.ToString();
                            orderReportEntry["Order.ChargeType"] = orderReport.Order.ChargeType;
                            orderReportEntry["Order.PickupDate"] = orderReport.Order.PickupDateTime.Value.ToString("d", CultureInfo.InvariantCulture);
                            orderReportEntry["Order.PickupTime"] = orderReport.Order.PickupDateTime.Value.ToString("t", CultureInfo.InvariantCulture);
                            orderReportEntry["Order.CreateDate"] = orderReport.Order.CreateDateTime.Value.Add(offset).ToString("d", CultureInfo.InvariantCulture);
                            orderReportEntry["Order.CreateTime"] = orderReport.Order.CreateDateTime.Value.Add(offset).ToString("t", CultureInfo.InvariantCulture);
                            orderReportEntry["Order.PickupAddress"] = orderReport.Order.PickupAddress.DisplayAddress;
                            orderReportEntry["Order.DropOffAddress "] = orderReport.Order.DropOffAddress.DisplayAddress;

                            orderReportEntry["OrderStatus.Status"] = orderReport.OrderStatus.Status.ToString();
                            orderReportEntry["OrderStatus.OrderIsCancelled"] = orderReport.OrderStatus.OrderIsCancelled.ToString();
                            orderReportEntry["OrderStatus.OrderIsCompleted"] = orderReport.OrderStatus.OrderIsCompleted.ToString();

                            orderReportEntry["Payment.MeterAmount"] = orderReport.Payment.MeterAmount.ToString();
                            orderReportEntry["Payment.TipAmount"] = orderReport.Payment.TipAmount.ToString();
                            orderReportEntry["Payment.TotalAmountCharged"] = orderReport.Payment.TotalAmountCharged.ToString();
                            orderReportEntry["Payment.Type"] = orderReport.Payment.Type.ToString();
                            orderReportEntry["Payment.Provider"] = orderReport.Payment.Provider.ToString();
                            orderReportEntry["Payment.TransactionId"] = orderReport.Payment.TransactionId.ToSafeString();
                            orderReportEntry["Payment.AuthorizationCode"] = orderReport.Payment.AuthorizationCode;
                            orderReportEntry["Payment.CardToken"] = orderReport.Payment.CardToken;
                            orderReportEntry["Payment.PalPayerId"] = orderReport.Payment.PalPayerId;
                            orderReportEntry["Payment.PayPalToken"] = orderReport.Payment.PayPalToken;
                            orderReportEntry["Payment.MdtTip"] = orderReport.Payment.MdtTip.ToString();
                            orderReportEntry["Payment.MdtToll"] = orderReport.Payment.MdtToll.ToString();
                            orderReportEntry["Payment.MdtFare"] = orderReport.Payment.MdtFare.ToString();

                            orderReportEntry["Promotion.Code"] = orderReport.Promotion.Code;
                            orderReportEntry["Promotion.WasApplied"] = orderReport.Promotion.WasApplied.ToString();
                            orderReportEntry["Promotion.WasRedeemed"] = orderReport.Promotion.WasRedeemed.ToString();
                            orderReportEntry["Promotion.SavedAmount"] = orderReport.Promotion.SavedAmount.ToString();

                            orderReportEntry["VehicleInfos.Number"] = orderReport.VehicleInfos.Number;
                            orderReportEntry["VehicleInfos.Type"] = orderReport.VehicleInfos.Type;
                            orderReportEntry["VehicleInfos.Make"] = orderReport.VehicleInfos.Make;
                            orderReportEntry["VehicleInfos.Model"] = orderReport.VehicleInfos.Model;
                            orderReportEntry["VehicleInfos.Color"] = orderReport.VehicleInfos.Color;
                            orderReportEntry["VehicleInfos.Registration"] = orderReport.VehicleInfos.Registration;
                            orderReportEntry["VehicleInfos.DriverFirstName"] = orderReport.VehicleInfos.DriverFirstName;
                            orderReportEntry["VehicleInfos.DriverLastName"] = orderReport.VehicleInfos.DriverLastName;
                            orderReportEntry["VehicleInfos.WasConfirmed"] = orderReport.VehicleInfos.WasConfirmed.ToString();

                            orderReportEntry["Client.OperatingSystem"] = orderReport.Client.OperatingSystem;
                            orderReportEntry["Client.UserAgent"] = orderReport.Client.UserAgent;
                            orderReportEntry["Client.Version"] = orderReport.Client.Version;

                            var rating = (JsonSerializer.DeserializeFromString(orderReport.Rating, typeof(Dictionary<string, string>)) as Dictionary<string, string>) ?? new Dictionary<string, string>();
                            
                            foreach (var rate in rating)
                            {
                                orderReportEntry["Rating." + rate.Key] = rate.Value;
                            }
                            
                            exportedOrderReports.Add(orderReportEntry);
                        });

                    return exportedOrderReports;
            }
            return new HttpResult(HttpStatusCode.NotFound);
        }
    }
}