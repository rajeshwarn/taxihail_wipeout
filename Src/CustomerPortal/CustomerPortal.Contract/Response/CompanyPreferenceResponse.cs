using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CustomerPortal.Contract.Resources;

namespace CustomerPortal.Contract.Response
{
    public class CompanyPreferenceResponse
    {
        public CompanyPreference CompanyPreference { get; set; }
        public bool CompanyAllowDispatch { get; set; }
    }
}
