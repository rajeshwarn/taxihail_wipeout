using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace apcurium.MK.Common.Resources
{
    public class RefundPaymentResponse : BasePaymentResponse
    {
        public string Last4Digits { get; set; }
    }
}
