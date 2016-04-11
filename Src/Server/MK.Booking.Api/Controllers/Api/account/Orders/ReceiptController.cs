using System;
using System.Threading.Tasks;
using System.Web.Http;
using apcurium.MK.Booking.Api.Services;
using apcurium.MK.Booking.Security;
using apcurium.MK.Web.Security;

namespace apcurium.MK.Web.Controllers.Api.Account.Orders
{
    public class ReceiptController : BaseApiController
    {
        public SendReceiptService SendReceiptService { get; private set; }

        public ReceiptController(SendReceiptService sendReceiptService)
        {
            SendReceiptService = sendReceiptService;
        }

        [HttpPost, Auth, Route("api/v2/accounts/orders/{orderId}/sendreceipt")]
        public async Task<IHttpActionResult> SendReceiptForOrder(Guid orderId)
        {
            await SendReceiptService.Post(orderId, string.Empty);

            return Ok();
        }

        [HttpPost, Auth(Role = RoleName.Support), Route("api/v2/accounts/orders/{orderId}/sendreceipt/{recipientEmail}")]
        public async Task<IHttpActionResult> SendReceiptForOrderToRecipientEmail(Guid orderId, string recipientEmail)
        {
            await SendReceiptService.Post(orderId, recipientEmail);

            return Ok();
        }
    }
}
