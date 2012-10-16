using apcurium.MK.Booking.Api.Contract;
using apcurium.MK.Booking.Api.Contract.Requests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using apcurium.MK.Common.Extensions;

namespace apcurium.MK.Web
{
    public partial class ApiDoc : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            var c = new ContractDocument();
            var report = c.Build();

            foreach (var contractDocumentReport in report)
            {
                Response.Write("<strong>Resource : " + contractDocumentReport.Path + "</strong>");
                Response.Write("<br>");
                foreach (var verb in contractDocumentReport.VerbList)
                {
                    Response.Write("&nbsp;&nbsp;Verb : " + verb );
                    Response.Write("<br>");
                }
                Response.Write("<br>");
                                               

            }
            
        }
    }
}