using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.IBS;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Booking.Security;
using apcurium.MK.Common;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Extensions;
using Infrastructure.Messaging;

namespace apcurium.MK.Booking.Api.Services
{
    public class AccountsChargeService : Service
    {
        private readonly IAccountChargeDao _dao;
        private readonly ICommandBus _commandBus;
        private readonly IIBSServiceProvider _ibsServiceProvider;

        public AccountsChargeService(IAccountChargeDao dao, ICommandBus commandBus, IIBSServiceProvider ibsServiceProvider)
        {
            _commandBus = commandBus;
            _ibsServiceProvider = ibsServiceProvider;
            _dao = dao;
        }

        public object Get(AccountChargeRequest request)
        {
            bool isAdmin = SessionAs<AuthUserSession>().HasPermission(RoleName.Admin);

            if (!request.AccountNumber.HasValue())
            {
                var allAccounts = _dao.GetAll();

                if (request.HideAnswers || !isAdmin)
                {
                    foreach (var account in allAccounts)
                    {
                        HideAnswers(account.Questions);
                    }
                }
                return allAccounts;
            }
            else
            {
                // Validate locally that the account exists
                var account = _dao.FindByAccountNumber(request.AccountNumber);
                if (account == null)
                {
                    throw new HttpError(HttpStatusCode.NotFound, "Account Not Found");
                }

                // Validate with IBS to make sure the account/customer is still active
                var ibsChargeAccount = _ibsServiceProvider.ChargeAccount().GetIbsAccount(request.AccountNumber, request.CustomerNumber ?? "0");
                if (ibsChargeAccount == null || !ibsChargeAccount.IsValid())
                {
                    throw new HttpError(HttpStatusCode.NotFound, "Account Not Found");
                }

                if (request.HideAnswers || !isAdmin)
                {
                    HideAnswers(account.Questions);
                }

                var currentUser = new Guid(this.GetSession().UserAuthId);
                LoadCustomerAnswers(account.Questions, currentUser);

                return account;
            }
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
            IEnumerable<AccountChargeQuestionAnswer> priorAnswers = _dao.GetLastAnswersForAccountId(userId);
            priorAnswers.ForEach(x =>
            {
                var matches = questionsAndAnswers.Where(q => q.Id == x.AccountChargeQuestionId && q.AccountId == x.AccountChargeId && q.SaveAnswer);
                if (matches != null)
                {
                    matches.ForEach(m => m.Answer = x.LastAnswer);
                }
            });

        }

        public object Post(AccountChargeRequest request)
        {
            var existing = _dao.FindByAccountNumber(request.AccountNumber);
            if (existing != null)
            {
                throw new HttpError(HttpStatusCode.Conflict, ErrorCode.AccountCharge_AccountAlreadyExisting.ToString());
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

        public object Put(AccountChargeRequest request)
        {
            var existing = _dao.FindByAccountNumber(request.AccountNumber);
            if (existing != null
                && existing.Id != request.Id)
            {
                throw new HttpError(HttpStatusCode.Conflict, ErrorCode.AccountCharge_AccountAlreadyExisting.ToString());
            }

            var i = 0;
            foreach (var question in request.Questions)
            {
                question.Id = i++;
                question.AccountId = request.Id;
            }
            var addUpdateAccountCharge = new AddUpdateAccountCharge
            {
                AccountChargeId = request.Id,
                Name = request.Name,
                Number = request.AccountNumber,
                UseCardOnFileForPayment = request.UseCardOnFileForPayment,
                Questions = request.Questions,
                CompanyId = AppConstants.CompanyId
            };

            _commandBus.Send(addUpdateAccountCharge);

            return new
            {
                Id = addUpdateAccountCharge.AccountChargeId
            };
        }

        public object Delete(AccountChargeRequest request)
        {
            var existing = _dao.FindByAccountNumber(request.AccountNumber);
            if (existing == null)
            {
                throw new HttpError(HttpStatusCode.NotFound, "Account Not Found");
            }

            var deleteAccountCharge = new DeleteAccountCharge
            {
                AccountChargeId = existing.Id,
                CompanyId = AppConstants.CompanyId
            };

            _commandBus.Send(deleteAccountCharge);

            return new HttpResult(HttpStatusCode.OK, "OK");
        }
    }
}