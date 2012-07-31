using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using System.Web;
using ServiceStack.ServiceClient.Web;
using apcurium.MK.Booking.Api.Client;
using apcurium.MK.Booking.Api.Contract.Requests;

namespace apcurium.MK.Web
{
    /// <summary>
    /// Summary description for PingomCheck
    /// </summary>
    public class PingdomCheck : IHttpHandler
    {
        private const int PingdomTestAccount = 77;
        public void ProcessRequest(HttpContext context)
        {
            var baseUrl = new Uri(context.Request.Url, VirtualPathUtility.ToAbsolute("~/api")).ToString();
            var name = Guid.NewGuid().ToString();
            var client = new AccountServiceClient(baseUrl);

            var account = client.GetTestAccount(PingdomTestAccount);
            if(account == null)
            {
                // Wait for testAccount to be created and try again
                Thread.Sleep(5000);
                account = client.GetTestAccount(PingdomTestAccount);
            }
            new AuthServiceClient(baseUrl).Authenticate(account.Email, "password1");

            client.UpdateBookingSettings(new BookingSettingsRequest
            {
                Name = name
            });

            Thread.Sleep(5000);

            account = client.GetMyAccount();

            if(account.Settings.Name != name)
            {
                throw new Exception("Pingom check failed");
            }

            context.Response.ContentType = "text/plain";
            context.Response.Write("Pingom check succeeded");
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }
}