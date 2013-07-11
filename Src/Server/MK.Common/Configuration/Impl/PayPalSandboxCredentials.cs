
using System.ComponentModel.DataAnnotations.Schema;

namespace apcurium.MK.Common.Configuration.Impl
{
    public class PayPalCredentials
    {
        public PayPalCredentials()
        {
            Username = "vincent.costel-facilitator_api1.gmail.com";
            Password = "1372362468";
            Signature = "AFcWxV21C7fd0v3bYYYRCpSSRl31ADYXGX.gPsewqg6pNBBa9JL5zoCL";
        }

        public string Username { get; set; }
        public string Password { get; set; }
        public string Signature { get; set; }


    }
}
