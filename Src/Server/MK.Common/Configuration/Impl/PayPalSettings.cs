using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace apcurium.MK.Common.Configuration.Impl
{
    public class PayPalSettings
    {
        public PayPalSettings()
        {

        }

        public PayPalSettings(Guid id)
        {
            Id = id;
            SandboxCredentials = new PayPalCredentials();
            Credentials = new PayPalCredentials();
            IsSandbox = true;
        }

  

        [Key]
        public Guid Id { get; set; }

        public bool IsEnabled { get; set; }
        public bool IsSandbox {get; set;}
        public PayPalCredentials SandboxCredentials { get; set; }
        public PayPalCredentials Credentials { get; set; }
    }
}
