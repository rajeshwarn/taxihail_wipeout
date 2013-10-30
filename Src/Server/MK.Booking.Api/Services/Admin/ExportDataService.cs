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
                    var accounts =  _accountDao.GetAll();
                    return accounts.Select(x => new{ 
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
                        x.DisabledByAdmin} );
               break;
               case DataType.Orders:
                    var orders = _orderDao.GetAllWithAccountSummary();
                    return orders.Select(x =>
                    {
                        var userAgent = UserAgentParser.GetOperatingSystem(x.UserAgent);                            
                        return new
                        {
                            x.Id,
                            x.IBSAccountId,
                            x.IBSOrderId,
                            x.Name,
                            x.Phone,
                            x.Email,
                            PickupDate = x.PickupDate.ToString("d", CultureInfo.InvariantCulture),
                            PickupTime = x.PickupDate.ToString("t", CultureInfo.InvariantCulture),
                            CreateDate = x.CreatedDate.Add(offset).ToString("d", CultureInfo.InvariantCulture),
                            CreateTime = x.CreatedDate.Add(offset).ToString("t", CultureInfo.InvariantCulture),
                            Status = (OrderStatus) x.Status,
                            PickupAddress = x.PickupAddress.DisplayAddress,
                            DropOffAddress = x.DropOffAddress.DisplayAddress,
                            x.Tip,
                            x.Toll,
                            x.Fare,
                            userAgent,
                            x.UserAgent
                        };

                    });
               break;
            }
            return new HttpResult(HttpStatusCode.NotFound);
        }
    }
}