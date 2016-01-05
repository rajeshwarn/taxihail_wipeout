using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using apcurium.MK.Booking.Mobile.Infrastructure;

namespace apcurium.MK.Booking.Mobile.Client.PlatformIntegration
{
    public class BraintreeDropinViewService : IBraintreeDropinViewService
    {
        public Task<string> ShowDropinView(string clientToken)
        {
			throw new Exception();
        }
    }
}
