#region

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web.Helpers;
using ExtendedMongoMembership;

#endregion

namespace CustomerPortal.Web.Entities
{
    public static class ChangeStatusEmail
    {
        public static void SendEmail(string oldStat, string newStat, MongoSession session, DateTime date, string company, string poNumber)
        {
            var isDisabled = (bool) new AppSettingsReader().GetValue("DisableEmailNotification", typeof (bool));
            if (isDisabled)
            {
                return;
            }

            if (oldStat != newStat)
            {
                var users = session.Users;
                var emails = new List<string>();
                foreach (var user in users)
                {
                    emails.AddRange(from userrole in user.Roles where userrole.RoleName == "admin" select user.UserName);
                }

                try
                {
                    var subject = String.Format("TaxiHail Customer Portal Message : {0} status has changed to {1}.",
                        company,
                        newStat);
                    var body = String.Format("Status change for {0} on {1}. Status changed from {2} to {3}.",
                        company, date, oldStat, newStat);

                    if ( string.IsNullOrWhiteSpace(poNumber) )
                    {
                        body += System.Environment.NewLine + "No purchase order in customer portal for this company.";
                    }
                    else 
                    {
                        body += System.Environment.NewLine + "PO #" + poNumber ;
                    }

                    foreach (var email in emails)
                    {
                        WebMail.Send(email, subject, body);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }

        public static void SendEmail(string oldStat, string newStat, MongoSession session, DateTime date, string company,
            string note, string poNumber)
        {
            var isDisabled = (bool) new AppSettingsReader().GetValue("DisableEmailNotification", typeof (bool));
            if (isDisabled)
            {
                return;
            }

            if (oldStat != newStat)
            {
                var users = session.Users;
                var emails = new List<string>();
                foreach (var user in users)
                {
                    emails.AddRange(from userrole in user.Roles where userrole.RoleName == "admin" select user.UserName);
                }

                try
                {
                    var subject = String.Format("{0} status has changed to {1}.", company,
                        newStat);
                    var body = String.Format("Status change for {0} on {1}. Status changed from {2} to {3}. {4} ",
                        company, date, oldStat, newStat, note);

                    if (string.IsNullOrWhiteSpace(poNumber))
                    {
                        body += System.Environment.NewLine + "No purchase order in customer portal for this company.";
                    }
                    else
                    {
                        body += System.Environment.NewLine + "PO #" + poNumber;
                    }


                    foreach (var email in emails)
                    {
                        WebMail.Send(email, subject, body);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }
    }
}