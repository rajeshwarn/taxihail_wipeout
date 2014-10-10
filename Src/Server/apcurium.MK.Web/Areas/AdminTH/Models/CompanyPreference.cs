using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace apcurium.MK.Web.Areas.AdminTH.Models
{
    public class CompanyPreference
    {
            public string CompanyId { get; set; }
            public bool CanAccept { get; set; }
            public bool CanDispatch { get; set; }
    }
}
