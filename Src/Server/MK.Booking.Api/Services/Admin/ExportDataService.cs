#region

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Enumeration.TimeZone;
using apcurium.MK.Common.Extensions;
using ServiceStack.Common.Web;
using ServiceStack.ServiceInterface;
using ServiceStack.Text;
using System.Text;
using apcurium.MK.Booking.ReadModel;

#endregion

namespace apcurium.MK.Booking.Api.Services.Admin
{
    public class ExportDataService : Service
    {
        private readonly IAccountDao _accountDao;
        private readonly IServerSettings _serverSettings;
        private readonly IReportDao _reportDao;
        private readonly IAppStartUpLogDao _appStartUpLogDao;
        private readonly IPromotionDao _promotionsDao;

        public ExportDataService(IAccountDao accountDao, IReportDao reportDao, IServerSettings serverSettings, IAppStartUpLogDao appStartUpLogDao, IPromotionDao promotionsDao)
        {
            _accountDao = accountDao;
            _reportDao = reportDao;
            _serverSettings = serverSettings;
            _appStartUpLogDao = appStartUpLogDao;
            _promotionsDao = promotionsDao;
        }

        public object Post(ExportDataRequest request)
        {
            var offset = new TimeSpan(_serverSettings.ServerData.IBS.TimeDifference);

            var startDate = request.StartDate ?? DateTime.MinValue;
            var endDate = request.EndDate.HasValue ? request.EndDate.Value.AddDays(1) : DateTime.MaxValue;// Add one day to include the current day since it ends at midnight
            var accountId = request.AccountId;

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
                               CreateDate = TimeZoneHelper.TransformToLocalTime(_serverSettings.ServerData.CompanyTimeZone, a.CreationDate).ToString("d", CultureInfo.InvariantCulture),
                               CreateTime = TimeZoneHelper.TransformToLocalTime(_serverSettings.ServerData.CompanyTimeZone, a.CreationDate).ToString("t", CultureInfo.InvariantCulture),
                               a.Settings.Name,
                               a.Settings.Phone,
                               a.Email,
                               a.DefaultCreditCard,
                               a.DefaultTipPercent,
                               a.Language,
                               a.TwitterId,
                               a.FacebookId,
                               a.HasAdminAccess,
                               a.IsConfirmed,
                               a.DisabledByAdmin,
                               a.Settings.PayBack,
                               LastLaunch = (m == null ? null : TimeZoneHelper.TransformToLocalTime(_serverSettings.ServerData.CompanyTimeZone, m.DateOccured).ToString(CultureInfo.InvariantCulture)),
                               Platform = (m == null ? null : m.Platform),
                               PlatformDetails = (m == null ? null : m.PlatformDetails),
                               ApplicationVersion = (m == null ? null : m.ApplicationVersion),
                               ServerVersion = (m == null ? null : m.ServerVersion)
                           };
                case DataType.Orders:

                    var orders = (accountId.HasValue()) ? _reportDao.GetOrderReportsByAccountId((Guid)accountId) : _reportDao.GetOrderReports(startDate, endDate);

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
                        orderReportEntry["Account.PayBack "] = orderReport.Account.PayBack;

                        orderReportEntry["Order.CompanyName"] = orderReport.Order.CompanyName;
                        orderReportEntry["Order.CompanyKey"] = orderReport.Order.CompanyKey;
                        orderReportEntry["Order.Market"] = orderReport.Order.Market;
                        orderReportEntry["Order.IBSOrderId"] = orderReport.Order.IBSOrderId.ToString();
                        orderReportEntry["Order.ChargeType"] = orderReport.Order.ChargeType;
                        orderReportEntry["Charge Account with Card on File Payment"] = orderReport.Order.IsChargeAccountPaymentWithCardOnFile.ToString();
                        orderReportEntry["Order.IsPrepaid"] = orderReport.Order.IsPrepaid.ToString();
                        orderReportEntry["Order.PickupDate"] = orderReport.Order.PickupDateTime.HasValue
                       ? orderReport.Order.PickupDateTime.Value.ToString("d", CultureInfo.InvariantCulture)
                       : string.Empty;
                        orderReportEntry["Order.PickupTime"] = orderReport.Order.PickupDateTime.HasValue
                       ? orderReport.Order.PickupDateTime.Value.ToString("t", CultureInfo.InvariantCulture)
                       : string.Empty;
                        orderReportEntry["Order.CreateDate"] = orderReport.Order.CreateDateTime.HasValue
                       ? orderReport.Order.CreateDateTime.Value.Add(offset).ToString("d", CultureInfo.InvariantCulture)
                       : string.Empty;
                        orderReportEntry["Order.CreateTime"] = orderReport.Order.CreateDateTime.HasValue
                       ? orderReport.Order.CreateDateTime.Value.Add(offset).ToString("t", CultureInfo.InvariantCulture)
                       : string.Empty;
                        orderReportEntry["Order.PickupAddress"] = orderReport.Order.PickupAddress.DisplayAddress;
                        orderReportEntry["Order.DropOffAddress"] = orderReport.Order.DropOffAddress.DisplayAddress;
                        orderReportEntry["Order.WasSwitchedToAnotherCompany"] = orderReport.Order.WasSwitchedToAnotherCompany.ToString();
                        orderReportEntry["Order.HasTimedOut"] = orderReport.Order.HasTimedOut.ToString();
                        orderReportEntry["Order.OriginalEta"] = orderReport.Order.OriginalEta.ToString();
                        orderReportEntry["Order.Error"] = orderReport.Order.Error;

                        orderReportEntry["OrderStatus.Status"] = orderReport.OrderStatus.Status.ToString();
                        orderReportEntry["OrderStatus.OrderIsCancelled"] = orderReport.OrderStatus.OrderIsCancelled.ToString();
                        orderReportEntry["OrderStatus.OrderIsCompleted"] = orderReport.OrderStatus.OrderIsCompleted.ToString();

                        orderReportEntry["Payment.Id"] = orderReport.Payment.PaymentId.ToString();
                        orderReportEntry["Payment.DriverId"] = orderReport.Payment.DriverId;
                        orderReportEntry["Payment.Medaillon"] = orderReport.Payment.Medaillon;
                        orderReportEntry["Payment.Last4Digits"] = orderReport.Payment.Last4Digits;
                        orderReportEntry["Payment.MeterAmount"] = orderReport.Payment.MeterAmount.ToString();
                        orderReportEntry["Payment.TipAmount"] = orderReport.Payment.TipAmount.ToString();
                        orderReportEntry["Payment.TotalAmountCharged"] = orderReport.Payment.TotalAmountCharged.ToString();
                        orderReportEntry["Payment.Type"] = orderReport.Payment.Type.ToString();
                        orderReportEntry["Payment.Provider"] = orderReport.Payment.Provider.ToString();
                        orderReportEntry["Payment.FirstPreAuthTransactionId"] = orderReport.Payment.FirstPreAuthTransactionId.ToSafeString();
                        orderReportEntry["Payment.TransactionId"] = orderReport.Payment.TransactionId.ToSafeString();
                        orderReportEntry["Payment.AuthorizationCode"] = orderReport.Payment.AuthorizationCode;
                        orderReportEntry["Payment.CardToken"] = orderReport.Payment.CardToken;
                        orderReportEntry["Payment.PayPalPayerId"] = orderReport.Payment.PayPalPayerId;
                        orderReportEntry["Payment.PayPalToken"] = orderReport.Payment.PayPalToken;
                        orderReportEntry["Payment.MdtTip"] = orderReport.Payment.MdtTip.ToString();
                        orderReportEntry["Payment.MdtToll"] = orderReport.Payment.MdtToll.ToString();
                        orderReportEntry["Payment.MdtFare"] = orderReport.Payment.MdtFare.ToString();
                        orderReportEntry["Payment.BookingFees"] = orderReport.Payment.BookingFees.ToString();
                        orderReportEntry["Payment.CmtPairingToken"] = orderReport.Payment.PairingToken;
                        orderReportEntry["Payment.IsPaired"] = orderReport.Payment.IsPaired.ToString();
                        orderReportEntry["Payment.WasUnpaired"] = orderReport.Payment.WasUnpaired.ToString();
                        orderReportEntry["Payment.IsCompleted"] = orderReport.Payment.IsCompleted.ToString();
                        orderReportEntry["Payment.IsCancelled"] = orderReport.Payment.IsCancelled.ToString();
                        orderReportEntry["Payment.IsRefunded"] = orderReport.Payment.IsRefunded.ToString();
                        orderReportEntry["Payment.WasChargedNoShowFee"] = orderReport.Payment.WasChargedNoShowFee.ToString();
                        orderReportEntry["Payment.WasChargedCancellationFee"] = orderReport.Payment.WasChargedCancellationFee.ToString();
                        orderReportEntry["Payment.WasChargedBookingFee"] = orderReport.Payment.WasChargedBookingFee.ToString();
                        orderReportEntry["Payment.Error"] = orderReport.Payment.Error;

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
                        orderReportEntry["VehicleInfos.DriverId"] = orderReport.VehicleInfos.DriverId;
                        orderReportEntry["VehicleInfos.DriverFirstName"] = orderReport.VehicleInfos.DriverFirstName;
                        orderReportEntry["VehicleInfos.DriverLastName"] = orderReport.VehicleInfos.DriverLastName;

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

                case DataType.Promotions:
                    var exportedPromotions = new List<Dictionary<string, string>>();

                    var promotions = _promotionsDao.GetAll().Where(x => (!x.StartDate.HasValue || (x.StartDate.HasValue && x.StartDate.Value <= endDate))
                       && (!x.EndDate.HasValue || (x.EndDate.HasValue && x.EndDate.Value >= startDate))).ToArray();

                    for (int i = 0; i < promotions.Length; i++)
                    {
                        var promo = new Dictionary<string, string>();
                        promo["Name"] = promotions[i].Name;
                        promo["Description"] = promotions[i].Description;
                        promo["StartDate"] = promotions[i].StartDate.HasValue ? promotions[i].StartDate.Value.ToString("d", CultureInfo.InvariantCulture) : null;
                        promo["StartTime"] = promotions[i].StartTime.HasValue ? promotions[i].StartTime.Value.ToString("t", CultureInfo.InvariantCulture) : null;
                        promo["EndDate"] = promotions[i].EndDate.HasValue ? promotions[i].EndDate.Value.ToString("d", CultureInfo.InvariantCulture) : null;
                        promo["EndTime"] = promotions[i].EndTime.HasValue ? promotions[i].EndTime.Value.ToString("t", CultureInfo.InvariantCulture) : null;
                        var days = (string[])JsonSerializer.DeserializeFromString(promotions[i].DaysOfWeek, typeof(string[]));
                        var daysOfWeek = Enum.GetNames(typeof(System.DayOfWeek));

                        var daysText = new StringBuilder();

                        for (int i1 = 0; i1 < daysOfWeek.Length; i1++)
                        {
                            if (days.Contains(daysOfWeek[i1]))
                            {
                                if (daysText.Length > 0)
                                {
                                    daysText.Append(", ");
                                }

                                daysText.Append(CultureInfo.InvariantCulture.DateTimeFormat.DayNames[i1]);
                            }
                        }

                        promo["Days"] = daysText.ToString();
                        promo["Applies To"] = ((promotions[i].AppliesToCurrentBooking ? "Current booking" : "") + (promotions[i].AppliesToFutureBooking ? " Future booking" : "")).TrimStart();
                        promo["Discount"] = promotions[i].DiscountValue.ToString() + (promotions[i].DiscountType == Common.Enumeration.PromoDiscountType.Cash ? " $" : " %");

                        if (promotions[i].TriggerSettings.Type != Common.Enumeration.PromotionTriggerTypes.CustomerSupport)
                        {
                            promo["Maximum Usage Per User"] = promotions[i].MaxUsagePerUser.ToString();
                            promo["Maximum Usage"] = promotions[i].MaxUsage.ToString();
                        }

                        promo["Promo Code"] = promotions[i].Code;
                        promo["Published Start Date"] = promotions[i].PublishedStartDate.HasValue ? promotions[i].PublishedStartDate.Value.ToString("d", CultureInfo.InvariantCulture) : null;
                        promo["Published End Date"] = promotions[i].PublishedEndDate.HasValue ? promotions[i].PublishedEndDate.Value.ToString("d", CultureInfo.InvariantCulture) : null;

                        switch (promotions[i].TriggerSettings.Type)
                        {
                            case Common.Enumeration.PromotionTriggerTypes.AccountCreated:
                                promo["Trigger"] = "Account created";
                                break;

                            case Common.Enumeration.PromotionTriggerTypes.AmountSpent:
                                promo["Trigger"] = "Amount spent " + promotions[i].TriggerSettings.AmountSpent.ToString(); ;
                                break;

                            case Common.Enumeration.PromotionTriggerTypes.CustomerSupport:
                                promo["Trigger"] = "Customer support";
                                break;

                            case Common.Enumeration.PromotionTriggerTypes.RideCount:
                                promo["Trigger"] = "Ride count " + promotions[i].TriggerSettings.RideCount.ToString();
                                break;
                        }

                        exportedPromotions.Add(promo);
                    }

                    return exportedPromotions;
            }

            return new HttpResult(HttpStatusCode.NotFound);
        }
    }
}