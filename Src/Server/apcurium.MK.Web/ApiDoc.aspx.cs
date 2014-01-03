#region

using System;
using System.Web.UI;
using apcurium.MK.Booking.Api.Contract;

#endregion

namespace apcurium.MK.Web
{
    public partial class ApiDoc : Page
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
                    Response.Write("&nbsp;&nbsp;Verb : " + verb);
                    Response.Write("<br>");
                }
                Response.Write("<br>");
            }
        }
    }
}