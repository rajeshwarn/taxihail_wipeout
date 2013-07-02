
namespace MK.Booking.PayPal.Test
{
    public class PayPalSandboxCredentials: IPayPalCredentials
    {
        public string Username {
            get { return "vincent.costel-facilitator_api1.gmail.com"; }
        }
        public string Password {
            get { return "1372362468"; }
        }
        public string Signature {
            get { return "AFcWxV21C7fd0v3bYYYRCpSSRl31ADYXGX.gPsewqg6pNBBa9JL5zoCL"; }
        }
    }
}
