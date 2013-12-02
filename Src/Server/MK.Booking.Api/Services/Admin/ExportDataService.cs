using System;
using System.Globalization;
using System.Linq;
using System.Net;
using apcurium.MK.Booking.Api.Helpers;
using ServiceStack.Common.Web;
using ServiceStack.ServiceInterface;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.ReadModel.Query;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Extensions;
using System.Collections.Generic;
using ServiceStack.Redis.Support;

namespace apcurium.MK.Booking.Api.Services.Admin
{
    public class ExportDataService : RestServiceBase<ExportDataRequest>
    {
        private readonly IAccountDao _accountDao;
        private readonly IOrderDao _orderDao;
        private readonly IConfigurationManager _configurationManager;

        public ExportDataService(IAccountDao accountDao, IOrderDao orderDao, IConfigurationManager configurationManager)
        {
            _accountDao = accountDao;
            _orderDao = orderDao;
            _configurationManager = configurationManager;
        }

        public override object OnGet(ExportDataRequest request)
        {
            var ibsServerTimeDifference = _configurationManager.GetSetting("IBS.TimeDifference").SelectOrDefault(long.Parse, 0);
            var offset = new TimeSpan(ibsServerTimeDifference);

            switch (request.Target)
            {
                case DataType.Accounts:
                    var accounts = _accountDao.GetAll();
                    return accounts.Select(x => new
                    {
                        x.Id,
                        x.IBSAccountId,
                        CreateDate = x.CreationDate.ToLocalTime().ToString("d", CultureInfo.InvariantCulture),
                        CreateTime = x.CreationDate.ToLocalTime().ToString("t", CultureInfo.InvariantCulture),
                        x.Settings.Name,
                        x.Settings.Phone,
                        x.Email,
                        x.DefaultCreditCard,
                        x.DefaultTipPercent,
                        x.Language,
                        x.TwitterId,
                        x.FacebookId,
                        x.IsAdmin,
                        x.IsConfirmed,
                        x.DisabledByAdmin
                    });
                    break;
                case DataType.Orders:
                    var orders = _orderDao.GetAllWithAccountSummary();
                    return orders.Select(x =>
                        {
                            var operatingSystem = UserAgentParser.GetOperatingSystem(x.UserAgent);
                            var phone = string.IsNullOrWhiteSpace(x.Phone) ? "" : x.Phone.ToSafeString();
                            var transactionId = string.IsNullOrEmpty(x.TransactionId) ||
                                                (x.TransactionId.Trim().Length <= 1)
                                                    ? ""
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

                            excelResult["Payment Meter Amount"] = x.PaymentMeterAmount.ToString();
                            excelResult["Payment Tip Amount"] = x.PaymentTipAmount.ToString();
                            excelResult["Payment Total Amount"] = x.PaymentTotalAmount.ToString();
                            excelResult["Payment Type"] = x.PaymentType.ToString();
                            excelResult["Payment Provider"] = x.PaymentProvider.ToString();
                            excelResult["Transaction Id"] = transactionId;
                            excelResult["Authorization Code"] = x.AuthorizationCode;
                            excelResult["Card Token"] = x.CardToken;


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

                            foreach (var rate in x.Rating)
                            {
                                excelResult[rate.Key] = rate.Value;
                            }

                            return excelResult;

                        }).ToList();
                    break;
            }
            return new HttpResult(HttpStatusCode.NotFound);
        }
    }
}