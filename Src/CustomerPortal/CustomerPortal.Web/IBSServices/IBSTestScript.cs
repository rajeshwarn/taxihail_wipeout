#region

using System;
using System.Linq;
using System.Text;

#endregion

namespace CustomerPortal.Web.IBSServices
{
    public class IBSTestScript
    {
        private int _defaultChargeType = 0;
        private int _defaultProvider;
        private int _defaultVehicleType = 0;

        public bool RunTest(string url, string username, string password, ref StringBuilder result)
        {
            bool isSucessFull = GetProviders(url, username, password, ref result);


            if (isSucessFull)
            {
                result.AppendLine();
                isSucessFull = isSucessFull && GetVehicleTypes(url, username, password, ref result);
                result.AppendLine();
                isSucessFull = isSucessFull && GetChargeTypes(url, username, password, ref result);
                result.AppendLine();
                isSucessFull = isSucessFull && CreatingAccount(url, username, password, ref result);
            }

            result.AppendLine();

            result.AppendLine(isSucessFull
                ? @"<h4>This IBS server is working.</h4>"
                : @"<h4>There is a problem with this IBS server. It cannot be used with TaxiHail.</h4>");

            return isSucessFull;
        }


        private bool CreatingAccount(string url, string username, string password, ref StringBuilder result)
        {
            var account = new TBookAccount3();
            account.ServiceProviderID = _defaultProvider;
            account.WEBID = Guid.NewGuid().ToString();
            account.WEBPassword = "password";
            account.LastName = "Test LastName";
            account.Phone = "5145551212";
            account.Email2 = "johntest@taxihail.com";
            account.MobilePhone = account.Phone;
            account.Address = new TWEBAddress();

            result.AppendLine("Calling SaveAccount3 for url :" + url + "IWebAccount3");

            var account3Service = new WebAccount3Service {Url = url + "IWebAccount3"};

            try
            {
                var id = account3Service.SaveAccount3(username, password, account);
                if (id > 0)
                {
                    result.AppendLine("Call sucessfull, account with id #" + id + " created");
                }
                else
                {
                    result.AppendLine("Unable to create account, no id returned, error code return = " + id);
                }
                return id > 0;
            }
            catch (Exception ex)
            {
                result.AppendLine("Error calling SaveAccount3 : " + ex.Message);
                result.AppendLine("Error details : " + ex.StackTrace);
            }
            return false;
        }

        private bool GetProviders(string url, string username, string password, ref StringBuilder result)
        {
            result.AppendLine("Calling GetProviders for url :" + url + "IStaticData");

            var staticService = new StaticDataservice {Url = url + "IStaticData"};

            try
            {
                var company = staticService.GetProviders(username, password);
                result.AppendLine("Call sucessfull, found " + company.Count() + " providers");
                if (company.Any(c => c.isDefault))
                {
                    _defaultProvider = company.First(c => c.isDefault).ProviderNum;
                    return true;
                }
                result.AppendLine("No default provider found");
                return false;
                ;
            }
            catch (Exception ex)
            {
                result.AppendLine("Error calling get providers : " + ex.Message);
                result.AppendLine("Error details : " + ex.StackTrace);
            }
            return false;
        }

        private bool GetVehicleTypes(string url, string username, string password, ref StringBuilder result)
        {
            result.AppendLine("Calling GetVehicleTypes for url :" + url + "IStaticData");

            var staticService = new StaticDataservice {Url = url + "IStaticData"};

            try
            {
                var company = staticService.GetProviders(username, password);
                int c = 0;
                foreach (var providerItem in company)
                {
                    c += staticService.GetVehicleTypes(username, password, providerItem.ProviderNum).Count();
                }

                result.AppendLine("Call sucessfull, found " + c + " vehicle types");
                return c > 0;
            }
            catch (Exception ex)
            {
                result.AppendLine("Error calling get vehcile types : " + ex.Message);
                result.AppendLine("Error details : " + ex.StackTrace);
            }
            return false;
        }

        private bool GetChargeTypes(string url, string username, string password, ref StringBuilder result)
        {
            result.AppendLine("Calling GetChargeTypes for url :" + url + "IStaticData");

            var staticService = new StaticDataservice {Url = url + "IStaticData"};

            try
            {
                var company = staticService.GetProviders(username, password);
                int c = 0;
                foreach (var providerItem in company)
                {
                    c += staticService.GetChargeTypes(username, password, providerItem.ProviderNum).Count();
                }

                result.AppendLine("Call sucessfull, found " + c + " charge types");
                return c > 0;
            }
            catch (Exception ex)
            {
                result.AppendLine("Error calling get charge types : " + ex.Message);
                result.AppendLine("Error details : " + ex.StackTrace);
            }
            return false;
        }
    }
}