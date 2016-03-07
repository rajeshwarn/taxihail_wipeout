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
using System.Text;
using System.Web;

#endregion

namespace apcurium.MK.Booking.Api.Services.Admin
{
    public class ExportDataService : BaseApiService
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
            var endDate = request.EndDate.HasValue 
                ? request.EndDate.Value.AddDays(1) // Add one day to include the current day since it ends at midnight
                : DateTime.MaxValue;

            var accountId = request.AccountId;

            switch (request.Target)
            {
                case DataType.Accounts:
                    return PrepareAccountData(startDate, endDate);
                case DataType.Orders:
                    return PrepareOrdersData(accountId, startDate, endDate, offset);
                case DataType.Promotions:
                    return PreparePromotionsData(endDate, startDate);
                default:
                    throw new HttpException((int)HttpStatusCode.NotFound, "Not found");
            }
        }

        private object PreparePromotionsData(DateTime endDate, DateTime startDate)
        {
            var exportedPromotions = new List<Dictionary<string, string>>();

            var promotions = _promotionsDao.GetAll().Where(x => (!x.StartDate.HasValue || (x.StartDate.Value <= endDate)) && (!x.EndDate.HasValue || (x.EndDate.Value >= startDate))).ToArray();

            foreach (var promotion in promotions)
            {
                var promo = new Dictionary<string, string>
                {
                    ["Name"] = promotion.Name, ["Description"] = promotion.Description, ["StartDate"] = promotion.StartDate.HasValue ? promotion.StartDate.Value.ToString("d", CultureInfo.InvariantCulture) : null, ["StartTime"] = promotion.StartTime.HasValue ? promotion.StartTime.Value.ToString("t", CultureInfo.InvariantCulture) : null, ["EndDate"] = promotion.EndDate.HasValue ? promotion.EndDate.Value.ToString("d", CultureInfo.InvariantCulture) : null, ["EndTime"] = promotion.EndTime.HasValue ? promotion.EndTime.Value.ToString("t", CultureInfo.InvariantCulture) : null
                };

                var days = promotion.DaysOfWeek.FromJsonSafe<string[]>();
                var daysOfWeek = Enum.GetNames(typeof (DayOfWeek));

                var daysText = new StringBuilder();

                for (var i1 = 0; i1 < daysOfWeek.Length; i1++)
                {
                    if (!days.Contains(daysOfWeek[i1]))
                    {
                        continue;
                    }
                    if (daysText.Length > 0)
                    {
                        daysText.Append(", ");
                    }

                    daysText.Append(CultureInfo.InvariantCulture.DateTimeFormat.DayNames[i1]);
                }

                promo["Days"] = daysText.ToString();
                promo["Applies To"] = ((promotion.AppliesToCurrentBooking ? "Current booking" : "") + (promotion.AppliesToFutureBooking ? " Future booking" : "")).TrimStart();
                promo["Discount"] = promotion.DiscountValue + (promotion.DiscountType == Common.Enumeration.PromoDiscountType.Cash ? " $" : " %");

                if (promotion.TriggerSettings.Type != Common.Enumeration.PromotionTriggerTypes.CustomerSupport)
                {
                    promo["Maximum Usage Per User"] = promotion.MaxUsagePerUser.ToString();
                    promo["Maximum Usage"] = promotion.MaxUsage.ToString();
                }

                promo["Promo Code"] = promotion.Code;
                promo["Published Start Date"] = promotion.PublishedStartDate.HasValue ? promotion.PublishedStartDate.Value.ToString("d", CultureInfo.InvariantCulture) : null;
                promo["Published End Date"] = promotion.PublishedEndDate.HasValue ? promotion.PublishedEndDate.Value.ToString("d", CultureInfo.InvariantCulture) : null;

                switch (promotion.TriggerSettings.Type)
                {
                    case Common.Enumeration.PromotionTriggerTypes.AccountCreated:
                        promo["Trigger"] = "Account created";
                        break;

                    case Common.Enumeration.PromotionTriggerTypes.AmountSpent:
                        promo["Trigger"] = "Amount spent " + promotion.TriggerSettings.AmountSpent;
                        break;

                    case Common.Enumeration.PromotionTriggerTypes.CustomerSupport:
                        promo["Trigger"] = "Customer support";
                        break;

                    case Common.Enumeration.PromotionTriggerTypes.RideCount:
                        promo["Trigger"] = "Ride count " + promotion.TriggerSettings.RideCount;
                        break;
                }

                exportedPromotions.Add(promo);
            }

            return exportedPromotions;
        }

        private object PrepareOrdersData(Guid? accountId, DateTime startDate, DateTime endDate, TimeSpan offset)
        {
            var orders = accountId.HasValue ? _reportDao.GetOrderReportsByAccountId(accountId.Value) : _reportDao.GetOrderReports(startDate, endDate);

            var exportedOrderReports = new List<Dictionary<string, string>>();

            orders.ForEach(orderReport =>
            {
                var orderReportEntry = new Dictionary<string, string>
                {
                    ["Account.AccountId"] = orderReport.Account.AccountId.ToString(),
                    ["Account.Name"] = orderReport.Account.Name.Trim(),
                    ["Account.Phone"] = orderReport.Account.Phone.Trim(),
                    ["Account.Email"] = orderReport.Account.Email.Trim(),
                    ["Account.IBSAccountId"] = orderReport.Account.IBSAccountId.ToString(),
                    ["Account.DefaultCardToken "] = orderReport.Account.DefaultCardToken.ToString(),
                    ["Account.PayBack "] = orderReport.Account.PayBack,
                    ["Order.CompanyName"] = orderReport.Order.CompanyName,
                    ["Order.CompanyKey"] = orderReport.Order.CompanyKey,
                    ["Order.Market"] = orderReport.Order.Market,
                    ["Order.IBSOrderId"] = orderReport.Order.IBSOrderId.ToString(),
                    ["Order.ChargeType"] = orderReport.Order.ChargeType,
                    ["Charge Account with Card on File Payment"] =
                        orderReport.Order.IsChargeAccountPaymentWithCardOnFile.ToString(),
                    ["Order.IsPrepaid"] = orderReport.Order.IsPrepaid.ToString(),
                    ["Order.PickupDate"] = orderReport.Order.PickupDateTime.HasValue
                            ? orderReport.Order.PickupDateTime.Value.ToString("d", CultureInfo.InvariantCulture)
                            : string.Empty,
                    ["Order.PickupTime"] = orderReport.Order.PickupDateTime.HasValue
                            ? orderReport.Order.PickupDateTime.Value.ToString("t", CultureInfo.InvariantCulture)
                            : string.Empty,
                    ["Order.CreateDate"] = orderReport.Order.CreateDateTime.HasValue
                            ? orderReport.Order.CreateDateTime.Value.Add(offset)
                                .ToString("d", CultureInfo.InvariantCulture)
                            : string.Empty,
                    ["Order.CreateTime"] = orderReport.Order.CreateDateTime.HasValue
                            ? orderReport.Order.CreateDateTime.Value.Add(offset).ToString("t", CultureInfo.InvariantCulture)
                            : string.Empty,
                    ["Order.PickupAddress"] = orderReport.Order.PickupAddress.DisplayAddress,
                    ["Order.DropOffAddress"] = orderReport.Order.DropOffAddress.DisplayAddress,
                    ["Order.WasSwitchedToAnotherCompany"] = orderReport.Order.WasSwitchedToAnotherCompany.ToString(),
                    ["Order.HasTimedOut"] = orderReport.Order.HasTimedOut.ToString(),
                    ["Order.OriginalEta"] = orderReport.Order.OriginalEta.ToString(),
                    ["Order.Error"] = orderReport.Order.Error,
                    ["OrderStatus.Status"] = orderReport.OrderStatus.Status.ToString(),
                    ["OrderStatus.OrderIsCancelled"] = orderReport.OrderStatus.OrderIsCancelled.ToString(),
                    ["OrderStatus.OrderIsCompleted"] = orderReport.OrderStatus.OrderIsCompleted.ToString(),
                    ["Payment.Id"] = orderReport.Payment.PaymentId.ToString(),
                    ["Payment.DriverId"] = orderReport.Payment.DriverId,
                    ["Payment.Medallion"] = orderReport.Payment.Medallion,
                    ["Payment.Last4Digits"] = orderReport.Payment.Last4Digits.IsNullOrEmpty()
                            ? string.Empty
                            : string.Format("'{0}'", orderReport.Payment.Last4Digits),
                    ["Payment.MeterAmount"] = orderReport.Payment.MeterAmount.ToString(),
                    ["Payment.TipAmount"] = orderReport.Payment.TipAmount.ToString(),
                    ["Payment.TotalAmountCharged"] = orderReport.Payment.TotalAmountCharged.ToString(),
                    ["Payment.Type"] = orderReport.Payment.Type.ToString(),
                    ["Payment.Provider"] = orderReport.Payment.Provider.ToString(),
                    ["Payment.FirstPreAuthTransactionId"] = orderReport.Payment.FirstPreAuthTransactionId.ToSafeString(),
                    ["Payment.TransactionId"] = orderReport.Payment.TransactionId.ToSafeString(),
                    ["Payment.AuthorizationCode"] = orderReport.Payment.AuthorizationCode,
                    ["Payment.CardToken"] = orderReport.Payment.CardToken,
                    ["Payment.PayPalPayerId"] = orderReport.Payment.PayPalPayerId,
                    ["Payment.PayPalToken"] = orderReport.Payment.PayPalToken,
                    ["Payment.MdtTip"] = orderReport.Payment.MdtTip.ToString(),
                    ["Payment.MdtToll"] = orderReport.Payment.MdtToll.ToString(),
                    ["Payment.MdtFare"] = orderReport.Payment.MdtFare.ToString(),
                    ["Payment.BookingFees"] = orderReport.Payment.BookingFees.ToString(),
                    ["Payment.CmtPairingToken"] = orderReport.Payment.PairingToken,
                    ["Payment.IsPaired"] = orderReport.Payment.IsPaired.ToString(),
                    ["Payment.WasUnpaired"] = orderReport.Payment.WasUnpaired.ToString(),
                    ["Payment.IsCompleted"] = orderReport.Payment.IsCompleted.ToString(),
                    ["Payment.IsCancelled"] = orderReport.Payment.IsCancelled.ToString(),
                    ["Payment.IsRefunded"] = orderReport.Payment.IsRefunded.ToString(),
                    ["Payment.WasChargedNoShowFee"] = orderReport.Payment.WasChargedNoShowFee.ToString(),
                    ["Payment.WasChargedCancellationFee"] = orderReport.Payment.WasChargedCancellationFee.ToString(),
                    ["Payment.WasChargedBookingFee"] = orderReport.Payment.WasChargedBookingFee.ToString(),
                    ["Payment.Error"] = orderReport.Payment.Error,
                    ["Promotion.Code"] = orderReport.Promotion.Code,
                    ["Promotion.WasApplied"] = orderReport.Promotion.WasApplied.ToString(),
                    ["Promotion.WasRedeemed"] = orderReport.Promotion.WasRedeemed.ToString(),
                    ["Promotion.SavedAmount"] = orderReport.Promotion.SavedAmount.ToString(),
                    ["VehicleInfos.Number"] = orderReport.VehicleInfos.Number,
                    ["VehicleInfos.Type"] = orderReport.VehicleInfos.Type,
                    ["VehicleInfos.Make"] = orderReport.VehicleInfos.Make,
                    ["VehicleInfos.Model"] = orderReport.VehicleInfos.Model,
                    ["VehicleInfos.Color"] = orderReport.VehicleInfos.Color,
                    ["VehicleInfos.Registration"] = orderReport.VehicleInfos.Registration,
                    ["VehicleInfos.DriverId"] = orderReport.VehicleInfos.DriverId,
                    ["VehicleInfos.DriverFirstName"] = orderReport.VehicleInfos.DriverFirstName,
                    ["VehicleInfos.DriverLastName"] = orderReport.VehicleInfos.DriverLastName,
                    ["Client.OperatingSystem"] = orderReport.Client.OperatingSystem,
                    ["Client.UserAgent"] = orderReport.Client.UserAgent,
                    ["Client.Version"] = orderReport.Client.Version
                };

                var rating = orderReport.Rating.FromJsonSafe<Dictionary<string, string>>() ?? new Dictionary<string, string>();

                foreach (var rate in rating)
                {
                    orderReportEntry["Rating." + rate.Key] = rate.Value;
                }

                exportedOrderReports.Add(orderReportEntry);
            });

            return exportedOrderReports;
        }

        private object PrepareAccountData(DateTime startDate, DateTime endDate)
        {
            var accounts = _accountDao.GetAll().Where(x => x.CreationDate >= startDate && x.CreationDate <= endDate).ToList();
            var startUpLogs = _appStartUpLogDao.GetAll().ToList();

            // We join each account details with their "last launch details"
            return from a in accounts
                join s in startUpLogs on a.Id equals s.UserId into matchingLog
                from m in matchingLog.DefaultIfEmpty()
                select new
                {
                    a.Id, a.IBSAccountId, CreateDate = TimeZoneHelper.TransformToLocalTime(_serverSettings.ServerData.CompanyTimeZone, a.CreationDate).ToString("d", CultureInfo.InvariantCulture), CreateTime = TimeZoneHelper.TransformToLocalTime(_serverSettings.ServerData.CompanyTimeZone, a.CreationDate).ToString("t", CultureInfo.InvariantCulture), a.Settings.Name, a.Settings.Phone, a.Email, a.DefaultCreditCard, a.DefaultTipPercent, a.Language, a.TwitterId, a.FacebookId, a.HasAdminAccess, a.IsConfirmed, a.DisabledByAdmin, a.Settings.PayBack, LastLaunch = (m == null ? null : TimeZoneHelper.TransformToLocalTime(_serverSettings.ServerData.CompanyTimeZone, m.DateOccured).ToString(CultureInfo.InvariantCulture)), Platform = (m == null ? null : m.Platform), PlatformDetails = (m == null ? null : m.PlatformDetails), ApplicationVersion = (m == null ? null : m.ApplicationVersion), ServerVersion = (m == null ? null : m.ServerVersion)
                };
        }
    }
}