using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Api.Extensions;
using apcurium.MK.Booking.Api.Services;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.IBS;
using apcurium.MK.Booking.ReadModel;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Booking.Security;
using apcurium.MK.Common;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Extensions;
using Infrastructure.Messaging;

namespace apcurium.MK.Web.Controllers.Api.Admin
{
    [RoutePrefix("api/admin/accountscharge")]
    public class AccountsChargeController : BaseApiController
    {
        private readonly IAccountChargeDao _dao;
        private readonly ICommandBus _commandBus;
        private readonly IIBSServiceProvider _ibsServiceProvider;

        public AccountsChargeController(IAccountChargeDao dao, ICommandBus commandBus, IIBSServiceProvider ibsServiceProvider)
        {
            _dao = dao;
            _commandBus = commandBus;
            _ibsServiceProvider = ibsServiceProvider;
        }

        [HttpGet, Route("{accountNumber}"), Route("{accountNumber}/{customerNumber}/{hideAnswers}")]
        public AccountChargeDetail GetAccountCharge(string accountNumber, string customerNumber, bool hideAnswers)
        {
            var isAdmin = GetSession().HasPermission(RoleName.Admin);

            var account = _dao.FindByAccountNumber(accountNumber);
            if (account == null)
            {
                throw new HttpException((int)HttpStatusCode.NotFound, "Account Not Found");
            }

            // Validate with IBS to make sure the account/customer is still active
            var ibsChargeAccount = _ibsServiceProvider.ChargeAccount().GetIbsAccount(accountNumber, customerNumber ?? "0");
            if (ibsChargeAccount == null || !ibsChargeAccount.IsValid())
            {
                throw new HttpException((int)HttpStatusCode.NotFound, "Account Not Found");
            }

            if (hideAnswers || !isAdmin)
            {
                HideAnswers(account.Questions);
            }

            var currentUser = GetSession().UserId;
            LoadCustomerAnswers(account.Questions, currentUser);

            return account;
        }

        [HttpGet, Route]
        public IList<AccountChargeDetail> GetAccountCharge()
        {
            var allAccounts = _dao.GetAll();

            var isAdmin = GetSession().HasPermission(RoleName.Admin);

            if (!isAdmin)
            {
                foreach (var account in allAccounts)
                {
                    HideAnswers(account.Questions);
                }
            }

            return allAccounts;
        }

        [HttpPost, HttpPut, Route]
        public object CreateOrUpdate(AccountChargeRequest request)
        {
            var existing = _dao.FindByAccountNumber(request.AccountNumber);
            if (existing != null)
            {
                throw new HttpException((int)HttpStatusCode.Conflict, ErrorCode.AccountCharge_AccountAlreadyExisting.ToString());
            }

            var i = 0;

            var addUpdateAccountCharge = new AddUpdateAccountCharge
            {
                AccountChargeId = Guid.NewGuid(),
                Name = request.Name,
                Number = request.AccountNumber,
                Questions = request.Questions,
                UseCardOnFileForPayment = request.UseCardOnFileForPayment,
                CompanyId = AppConstants.CompanyId
            };

            foreach (var question in request.Questions)
            {
                question.Id = i++;
                question.AccountId = addUpdateAccountCharge.AccountChargeId;
            }

            _commandBus.Send(addUpdateAccountCharge);

            return new
            {
                Id = addUpdateAccountCharge.AccountChargeId
            };
        }

        [HttpDelete, Route("{AccountNumber}")]
        public HttpResponseMessage DeleteAccountCharge(string accountNumber)
        {
            var existing = _dao.FindByAccountNumber(accountNumber);
            if (existing == null)
            {
                throw new HttpException((int)HttpStatusCode.NotFound, "Account Not Found");
            }

            var deleteAccountCharge = new DeleteAccountCharge
            {
                AccountChargeId = existing.Id,
                CompanyId = AppConstants.CompanyId
            };

            _commandBus.Send(deleteAccountCharge);

            return new HttpResponseMessage(HttpStatusCode.OK);
        }

        private void HideAnswers(IEnumerable<AccountChargeQuestion> questionsAndAnswers)
        {
            foreach (var accountChargeQuestion in questionsAndAnswers)
            {
                accountChargeQuestion.Answer = string.Empty;
            }
        }

        private void LoadCustomerAnswers(IEnumerable<AccountChargeQuestion> questionsAndAnswers, Guid userId)
        {
            var priorAnswers = _dao.GetLastAnswersForAccountId(userId);
            priorAnswers.ForEach(x =>
            {
                questionsAndAnswers.Where(q => q.Id == x.AccountChargeQuestionId && q.AccountId == x.AccountChargeId && q.SaveAnswer)
                    .ForEach(m => m.Answer = x.LastAnswer);
            });

        }
    }
}
