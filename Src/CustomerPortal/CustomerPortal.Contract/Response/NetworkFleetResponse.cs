using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerPortal.Contract.Response
{
    public class NetworkFleetResponse
    {
        public string CompanyName { get; set; }
        public string CompanyKey { get; set; }
        public string IbsUrl { get; set; }
        public string IbsUserName { get; set; }
        public string IbsPassword { get; set; }
    }
}
