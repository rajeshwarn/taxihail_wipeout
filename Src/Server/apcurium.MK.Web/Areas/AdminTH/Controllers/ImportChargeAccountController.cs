using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.IBS;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Common;
using apcurium.MK.Common.Caching;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Extensions;
using AutoMapper;
using Infrastructure.Messaging;


namespace apcurium.MK.Web.Areas.AdminTH.Controllers
{
    public class ImportChargeAccountController : ApcuriumServiceController
    {
        private readonly IAccountChargeDao _dao;
        private readonly IIBSServiceProvider _ibsServiceProvider;
        private readonly ICommandBus _commandBus;

        public ImportChargeAccountController(ICacheClient cache, IServerSettings serverSettings,
                                            IAccountChargeDao dao, IIBSServiceProvider ibsServiceProvider,
                                            ICommandBus commandBus)
            : base(cache, serverSettings)
        {
            _dao = dao;
            _ibsServiceProvider = ibsServiceProvider;
            _commandBus = commandBus;
        }

        public ActionResult Index()
        {
            if (AuthSession.IsAuthenticated())
            {
                try
                {
                    var result = ImportAccounts();
                    return View(result);
                }
                catch (Exception ex)
                {
                    ViewBag.Error = "Cannot import accounts: " + ex.Message;
                    return View();
                }
            }

            return Redirect(BaseUrl);
        }

        private IEnumerable<IbsChargeAccount> GetNewChargeAccounts(List<IbsChargeAccount> ibsAccounts)
        {
            var taxiHailChargeAccounts = _dao.GetAll();

            var newChargeAccounts = ibsAccounts.Where(
                ibsChargeAccount =>
                    taxiHailChargeAccounts.All(currentAccount => currentAccount.Number != ibsChargeAccount.AccountNumber));

            return newChargeAccounts;
        }

        private IbsChargeAccountImportReport ImportAccounts()
        {
            var accountsFromIbs = _ibsServiceProvider.ChargeAccount().GetAllAccount();
            var ibsAccounts = new List<IbsChargeAccount>();
            Mapper.Map(accountsFromIbs, ibsAccounts);

            var chargeAccountsToImport = GetNewChargeAccounts(ibsAccounts).ToList();

            var existingAccounts = ibsAccounts
                    .Where(x => chargeAccountsToImport.All(y => y.AccountNumber != x.AccountNumber))
                    .ToList();

            var chargeAccounNumbers = chargeAccountsToImport
                .ToArray()
                .Select(x => x.AccountNumber)
                .Distinct();

            var report = new IbsChargeAccountImportReport();

            var importedTaxiHailChargeAccounts = new List<Common.Entity.AccountCharge>();

            chargeAccounNumbers.ForEach(ibsChargeAccount =>
            {
                var questions = chargeAccountsToImport
                    .Where(x => x.AccountNumber == ibsChargeAccount)
                    .SelectMany(x => x.Prompts.Where(p => !string.IsNullOrWhiteSpace(p.Caption)));
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
                        Id = questionIndex++,
                        Answer = "",
                        AccountId = accountId
                    });
                });

                // Needed for now: Add empty questions to obtain 8
                var count = taxiHailQuestions.Count;
                for (int i = 0; i < 8 - count; i++)
                {
                    taxiHailQuestions.Add(new AccountChargeQuestion
                    {
                        Id = questionIndex++,
                        IsRequired = false,
                        IsCaseSensitive = false,
                        AccountId = accountId
                    });
                }

                importedTaxiHailChargeAccounts.Add(new Common.Entity.AccountCharge
                {
                    AccountChargeId = accountId,
                    Name = ibsChargeAccount,
                    Number = ibsChargeAccount,
                    Questions = taxiHailQuestions.ToArray()
                });

                var line = new KeyValuePair<string, string>("new", ibsChargeAccount);
                report.ReportLines.Add(line);
            });

            _commandBus.Send(new ImportAccountCharge
            {
                AccountCharges = importedTaxiHailChargeAccounts.ToArray(),
                CompanyId = AppConstants.CompanyId
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