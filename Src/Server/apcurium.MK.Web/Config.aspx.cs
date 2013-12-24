#region

using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using apcurium.MK.Booking.Database;
using apcurium.MK.Common.Extensions;

#endregion

namespace apcurium.MK.Web
{
    public partial class Config : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            LoginPanel.Visible = (string) Session["IsAuth"] != true.ToString();
            SettingsPanel.Visible = (string) Session["IsAuth"] == true.ToString();

            if ((string) Session["IsAuth"] == true.ToString())
            {
                var datatable = new DataTable();

                var includeKeys = new[]
                {
                    "DefaultBookingSettings.ChargeTypeId", "DefaultBookingSettings.NbPassenger"
                    , "DefaultBookingSettings.ProviderId", "DefaultBookingSettings.VehicleTypeId"
                    , "Direction.FlateRate", "Direction.RatePerKm"
                    , "Email.NoReply", "DefaultBookingSettings.NbPassenger"
                    , "IBS.AutoDispatch", "IBS.WebServicesPassword"
                    , "IBS.WebServicesUrl", "IBS.WebServicesUserName"
                    , "Smtp.Credentials.Password", "Smtp.Credentials.Username"
                    , "Smtp.DeliveryMethod", "Smtp.EnableSsl"
                    , "Smtp.Host", "Smtp.Port"
                    , "Smtp.UseDefaultCredentials", "TaxiHail.SiteName"
                };


                var connectionString = new BookingDbContext("DbContext.Booking").Database.Connection.ConnectionString;
                var keys = includeKeys.Select(k => string.Format("'{0}'", k));

                using (
                    var adapter =
                        new SqlDataAdapter(
                            "select * from Config.AppSettings where [key] in (" + keys.JoinBy(", ") + ")",
                            connectionString))
                {
                    adapter.Fill(datatable);
                }

                configList.DataSource = datatable;
                configList.DataBind();
            }
        }

        protected void cmdLogin_Click(object sender, EventArgs e)
        {

// ReSharper disable LocalizableElement
            if ((txtUsername.Text == "taxihailconfiguser") 
                    && (txtPassword.Text == "taxihail@123456."))
            {
// ReSharper restore LocalizableElement
                Session["IsAuth"] = true.ToString();
                Response.Redirect("config.aspx");
            }
        }

        protected void cmdSave_Click(object sender, EventArgs e)
        {
            var connectionString = new BookingDbContext("DbContext.Booking").Database.Connection.ConnectionString;

            foreach (DataListItem ri in configList.Items)
            {
                if (ri.ItemType == ListItemType.Item || ri.ItemType == ListItemType.AlternatingItem)
                {
                    var label = (Label) ri.FindControl("KeyLabel");
                    var value = (TextBox) ri.FindControl("ValueText");
                    if ((label != null) && (value != null))
                    {
                        using (var conn = new SqlConnection(connectionString))
                        {
                            conn.Open();
                            using (var command = conn.CreateCommand())
                            {
                                command.CommandText = "update Config.AppSettings set [Value] = '" +
                                                      value.Text.Replace("'", "''") + "' where [key] ='" +
                                                      label.Text.Replace("'", "''") + "'";
                                command.ExecuteNonQuery();
                            }
                            conn.Close();
                        }
                    }
                }
            }

            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();
                using (var command = conn.CreateCommand())
                {
                    command.CommandText = "delete from Cache.Items where [key] ='IBS.StaticData'";
                    command.ExecuteNonQuery();
                }
                conn.Close();
            }
        }


        public static Control FindControlIterative(Control root, string clientID)
        {
            foreach (Control control in root.Controls)
            {
                if (control.ClientID == clientID)
                {
                    return control;
                }
                if (control.HasControls())
                {
                    var ctl = FindControlIterative(control, clientID);
                    if (ctl != null)
                    {
                        return ctl;
                    }
                }
            }
            return null;
        }

        protected void cmdRedirect_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/Api/ReferenceData");
        }
    }
}