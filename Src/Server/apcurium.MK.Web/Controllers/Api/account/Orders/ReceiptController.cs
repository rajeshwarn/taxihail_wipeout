using System;
using System.Threading.Tasks;
using System.Web.Http;
using apcurium.MK.Booking.Api.Services;
using apcurium.MK.Booking.IBS;
using apcurium.MK.Booking.Maps;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Booking.Security;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Web.Security;
using Infrastructure.Messaging;

namespace apcurium.MK.Web.Controllers.Api.Account.Orders
{
    [RoutePrefix("api/account/orders/")]
    public class ReceiptController : BaseApiController
    {
        public SendReceiptService SendReceiptService { get; }

        public ReceiptController(ICommandBus commandBus,
            IIBSServiceProvider ibsServiceProvider,
            IOrderDao orderDao,
            IOrderPaymentDao orderPaymentDao,
            ICreditCardDao creditCardDao,
            IAccountDao accountDao,
            IPromotionDao promotionDao,
            IReportDao reportDao,
            IServerSettings serverSettings,
            IGeocoding geocoding)
        {
            SendReceiptService = new SendReceiptService(commandBus, ibsServiceProvider, orderDao, orderPaymentDao, creditCardDao, accountDao, promotionDao, reportDao, serverSettings, geocoding, Logger);
        }

        [HttpPost, Auth, Route("{orderId}/sendreceipt")]
        public async Task<IHttpActionResult> SendReceiptForOrder(Guid orderId)
        {
            await SendReceiptService.Post(orderId, string.Empty);

            return Ok();
        }

        [HttpPost, Auth(Role = RoleName.Support), Route("{orderId}/sendreceipt/{recipientEmail}")]
        public async Task<IHttpActionResult> SendReceiptForOrderToRecipientEmail(Guid orderId, string recipientEmail)
        {
            await SendReceiptService.Post(orderId, recipientEmail);

            return Ok();
        }
    }
}
