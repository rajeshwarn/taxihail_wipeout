using System.Globalization;
using System.Linq;
using System.Net;
using ServiceStack.Common.Web;
using ServiceStack.ServiceInterface;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.ReadModel.Query;
using apcurium.MK.Common.Entity;

namespace apcurium.MK.Booking.Api.Services.Admin
{
    public class ExportDataService : RestServiceBase<ExportDataRequest>
    {
        private readonly IAccountDao _accountDao;
        private readonly IOrderDao _orderDao;

        public ExportDataService(IAccountDao accountDao, IOrderDao orderDao)
        {
            _accountDao = accountDao;
            _orderDao = orderDao;
        }

        public override object OnGet(ExportDataRequest request)
        {
            switch (request.Target)
            {
               case DataType.Accounts:
                    var accounts =  _accountDao.GetAll();
                    return accounts.Select(x => new{ x.Id, x.IBSAccountId, x.Settings.Name, x.Settings.Phone, x.Email, x.DefaultCreditCard, x.DefaultTipPercent, x.Language, x.TwitterId, x.FacebookId, x.IsAdmin, x.IsConfirmed } );
               break;
               case DataType.Orders:
                    var orders = _orderDao.GetAllWithAccountSummary();
                    return orders.Select(x => new
                                                  {
                                                      x.Id,
                                                      x.IBSAccountId,
                                                      x.IBSOrderId,
                                                      x.Name,
                                                      x.Phone,
                                                      x.Email,
                                                      Date = x.CreatedDate.ToString("d", CultureInfo.InvariantCulture),
                                                      Time = x.CreatedDate.ToString("t", CultureInfo.InvariantCulture),
                                                      Status = (OrderStatus)x.Status,
                                                      PickupAddress = x.PickupAddress.BookAddress,
                                                      DropOffAddress = x.DropOffAddress.BookAddress,
                                                      x.Tip,
                                                      x.Toll,
                                                      x.Fare
                                                  });
               break;
            }
            return new HttpResult(HttpStatusCode.NotFound);
        }
    }
}