using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using apcurium.MK.Booking.Api.Client.TaxiHail;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Entity;
using ServiceStack.CacheAccess;
using ServiceStack.Common.Extensions;
using ServiceStack.Messaging.Rcon;


namespace apcurium.MK.Web.Areas.AdminTH.Controllers
{
    public class ImportChargeAccountController : ServiceStackController
    {
        private readonly IServerSettings _serverSettings;
        private readonly string _applicationKey;
        private AdministrationServiceClient _client;
        
        public ImportChargeAccountController(ICacheClient cache, IServerSettings serverSettings) 
            : base(cache, serverSettings)
        {
            _serverSettings = serverSettings;
            _applicationKey = serverSettings.ServerData.TaxiHail.ApplicationKey;
        }

        public ActionResult Index()
        {
            if (AuthSession.IsAuthenticated)
            {
                _client = new AdministrationServiceClient(BaseUrlAPI, SessionID, null);
                var result = ImportAccounts();
                return View(result);
            }

            return Redirect(BaseUrl);
        }

        private IEnumerable<IbsChargeAccount> GetNewChargeAccounts(IEnumerable<IbsChargeAccount> allChargeAccounts)
        {
            var ibsChargeAccounts = allChargeAccounts.ToList();

            var taxiHailChargeAccounts = _client.GetAccountsCharge();

            var newChargeAccounts = ibsChargeAccounts.Where(
                newAccount =>
                    taxiHailChargeAccounts.All(currentAccount => currentAccount.Number != newAccount.AccountNumber));

            return newChargeAccounts;
        }

        private IbsChargeAccountImportReport ImportAccounts()
        {
            var allCharges = _client.GetAllChargeAccount().ToList();
            var chargeAccountsToImport = GetNewChargeAccounts(allCharges).ToList();

            var existingAccounts = allCharges
                    .Where(x => !chargeAccountsToImport.Any(y => y.AccountNumber == x.AccountNumber)).ToList();

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
                        ErrorMessage = "Wrong answer",
                        IsRequired = toBeValidated,
                        MaxLength = length,
                        Question = caption,
                        Id = (++questionIndex),
                        Answer = "",
                        AccountId = accountId
                    });
                });

                // Needed for now: Add empty questions to obtain 8
                var count = taxiHailQuestions.Count;
                for (int i = 0; i < 8 - count; i++)
                {
                    taxiHailQuestions.Add(new AccountChargeQuestion()
                    {
                        Id = ++questionIndex,
                        IsRequired = false,
                        IsCaseSensitive = false,
                        AccountId = accountId
                    });
                }

                importedTaxiHailChargeAccounts.Add(new Common.Entity.AccountCharge()
                {
                    AccountChargeId = accountId,
                    Name = ibsChargeAccount,
                    Number = ibsChargeAccount,
                    Questions = taxiHailQuestions.ToArray()
                });

                var line = new KeyValuePair<string, string>("new",
                    string.Format("{0}", ibsChargeAccount,
                        taxiHailQuestions.Count));
                report.ReportLines.Add(line);

            });

            _client.ImportAccountCharge(new AccountChargeImportRequest()
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