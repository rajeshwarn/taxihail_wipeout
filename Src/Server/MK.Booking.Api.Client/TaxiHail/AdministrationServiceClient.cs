#region

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Extensions;
using ServiceStack.Html;
using ServiceStack.Text;
using ServiceStack.Text.Json;
using AccountCharge = apcurium.MK.Booking.Api.Contract.Resources.AccountCharge;

#endregion

namespace apcurium.MK.Booking.Api.Client.TaxiHail
{
    public class AdministrationServiceClient : BaseServiceClient
    {
        public AdministrationServiceClient(string url, string sessionId, IPackageInfo packageInfo)
            : base(url, sessionId, packageInfo)
        {
        }

        public void GrantAdminAccess(GrantAdminRightRequest request)
        {
            var req = string.Format("/account/grantadmin");
            Client.Put<string>(req, request);
        }

        public void EnableAccount(EnableAccountByAdminRequest request)
        {
            var req = string.Format("/account/adminenable");
            Client.Put<string>(req, request);
        }

        public void DisableAccount(DisableAccountByAdminRequest request)
        {
            var req = string.Format("/account/admindisable");
            Client.Put<string>(req, request);
        }

        public IList<Address> GetDefaultFavoriteAddresses()
        {
            var req = string.Format("/admin/addresses");
            var addresses = Client.Get<IList<Address>>(req);
            return addresses;
        }

        public void AddDefaultFavoriteAddress(DefaultFavoriteAddress address)
        {
            var req = string.Format("/admin/addresses");
            Client.Post<string>(req, address);
        }

        public void UpdateDefaultFavoriteAddress(DefaultFavoriteAddress address)
        {
            var req = string.Format("/admin/addresses/{0}", address.Id);
            Client.Put<string>(req, address);
        }

        public void RemoveDefaultFavoriteAddress(Guid addressId)
        {
            var req = string.Format("/admin/addresses/{0}", addressId);
            Client.Delete<string>(req);
        }


        public void AddPopularAddress(PopularAddress address)
        {
            var req = string.Format("/admin/popularaddresses");
            Client.Post<string>(req, address);
        }

        public void UpdatePopularAddress(PopularAddress address)
        {
            var req = string.Format("/admin/popularaddresses/{0}", address.Id);
            Client.Put<string>(req, address);
        }

        public void RemovePopularAddress(Guid addressId)
        {
            var req = string.Format("/admin/popularaddresses/{0}", addressId);
            Client.Delete<string>(req);
        }

        public IList<Address> GetPopularAddresses()
        {
            var req = string.Format("/popularaddresses");
            var addresses = Client.Get<IList<Address>>(req);
            return addresses;
        }

        public IEnumerable GetAllAppSettings()
        {
            var req = string.Format("/settings");
            var address = Client.Get<IEnumerable>(req);
            return address;
        }

        public void AddOrUpdateAppSettings(ConfigurationsRequest appReq)
        {
            var req = string.Format("/settings");
            Client.Post<string>(req, appReq);
        }

        public string CreateAccountCharge(AccountChargeRequest request)
        {
            var req = string.Format("/admin/accountscharge");
            return Client.Post<string>(req, request);
        }

        public AccountCharge[]  GetAccountsCharge()
        {
            var req = string.Format("/admin/accountscharge");
            return Client.Get<AccountCharge[]>(req);
        }

        public AccountCharge GetAccountCharge(string accountNumber)
        {
            var req = string.Format("/admin/accountscharge/" + accountNumber);
            var result = Client.Get<AccountCharge>(req);
            return result;
        }

        public void UpdateAccountCharge(AccountChargeRequest request)
        {
            var req = string.Format("/admin/accountscharge");
            Client.Put<string>(req, request);
        }

        public void DeleteAccountCharge(string accountNumber)
        {
            var req = string.Format("/admin/accountscharge/" + accountNumber);
            Client.Delete<string>(req);
        }

        public void ImportAccountCharge(AccountChargeImportRequest request)
        {
            var req = string.Format("/admin/accountscharge/import");
            Client.Post<string>(req, request);
        }

        public IbsChargeAccount GetChargeAccount(string accountNumber, string customerNumber)
        {
            var req = string.Format("/admin/ibschargeaccount/{0}/{1}", accountNumber, customerNumber);
            var result = Client.Get<IbsChargeAccount>(req);
            return result;
        }

        public IbsChargeAccountValidation ValidateChargeAccount(IbsChargeAccountValidationRequest validationRequest)
        {
            var req = string.Format("/admin/ibschargeaccount/");
            var result = Client.Post<IbsChargeAccountValidation>(req, validationRequest);
            return result;
        }

        public IEnumerable<IbsChargeAccount> GetAllChargeAccount()
        {
            var req = string.Format("/admin/ibschargeaccount/all");
            var result = Client.Get<IEnumerable<IbsChargeAccount>>(req);
            return result;
        }

        public IEnumerable<IbsChargeAccount> GetNewChargeAccounts()
        {
            var req = string.Format("/admin/ibschargeaccount/all");
            var ibsChargeAccounts = Client.Get<IEnumerable<IbsChargeAccount>>(req);

            var taxiHailChargeAccounts = GetAccountsCharge();

            var newChargeAccounts = ibsChargeAccounts.Where(
                newAccount =>
                    taxiHailChargeAccounts.All(currentAccount => currentAccount.Number != newAccount.AccountNumber));

            return newChargeAccounts;
        }

        public IbsChargeAccountImportReport ImportAccounts()
        {
            var chargeAccountsToImport = GetNewChargeAccounts().ToList();
            
            var existingAccounts = GetAllChargeAccount().ToList().Except(chargeAccountsToImport);
            
            var chargeAccounNumbers = chargeAccountsToImport.ToArray().Select(x => x.AccountNumber).Distinct();
            
            var report = new IbsChargeAccountImportReport();
            
            var importedTaxiHailChargeAccounts = new List<Common.Entity.AccountCharge>();

            chargeAccounNumbers.ForEach(ibsChargeAccount =>
            {
                var questions =
                    chargeAccountsToImport.Where(x => x.AccountNumber == ibsChargeAccount).SelectMany(x => x.Prompts);
                var taxiHailQuestions = new List<AccountChargeQuestion>();
                var questionIndex = 0;
                var accountId = Guid.NewGuid();

                questions.ForEach(ibsQuestion =>
                {
                    var caption = ibsQuestion.Caption;
                    var length = ibsQuestion.Length;
                    var toBeValidated = ibsQuestion.ToBeValidated;

                    taxiHailQuestions.Add(new AccountChargeQuestion
                    {
                        IsCaseSensitive = false,
                        ErrorMessage = "",
                        IsRequired = toBeValidated,
                        MaxLength = length,
                        Question = caption,
                        Id = ++questionIndex,
                        Answer = "",
                        AccountId = accountId
                    });
                });

                importedTaxiHailChargeAccounts.Add(new Common.Entity.AccountCharge()
                {
                    AccountChargeId = accountId,
                    Name = ibsChargeAccount,
                    Number = ibsChargeAccount,
                    Questions = taxiHailQuestions.ToArray()
                });

                var line = new KeyValuePair<string, string>("new",
                    string.Format("{0} ({1} questions)", ibsChargeAccount,
                        taxiHailQuestions.Count));
                report.ReportLines.Add(line);

            });

            // POST
            ImportAccountCharge(new AccountChargeImportRequest()
            {
                AccountCharges = importedTaxiHailChargeAccounts.ToArray()
            });

            existingAccounts.ForEach(existing =>
            {
                var existingLine = new KeyValuePair<string, string>("existing",
                    string.Format("{0} (Customer {1}) Already Existing", existing.AccountNumber, existing.CustomerNumber));

                report.ReportLines.Add(existingLine);
            });

            return report;
        }
    }
}