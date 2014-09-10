﻿using System;
using System.Collections.Generic;
using System.Net;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Booking.Security;
using apcurium.MK.Common;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Extensions;
using Infrastructure.Messaging;
using ServiceStack.Common.Web;
using ServiceStack.ServiceInterface;
using ServiceStack.ServiceInterface.Auth;

namespace apcurium.MK.Booking.Api.Services
{
    public class AccountsChargeService : Service
    {
        private readonly IAccountChargeDao _dao;
        private readonly ICommandBus _commandBus;

        public AccountsChargeService(IAccountChargeDao dao, ICommandBus commandBus)
        {
            _commandBus = commandBus;
            _dao = dao;
        }

        public object Get(AccountChargeRequest request)
        {
            bool isAdmin = SessionAs<AuthUserSession>().HasPermission(RoleName.Admin);

            if (!request.Number.HasValue())
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
                var account = _dao.FindByAccountNumber(request.Number);
                if (account == null)
                {
                    throw new HttpError(HttpStatusCode.NotFound, "Account Not Found");
                }

                if (request.HideAnswers || !isAdmin)
                {
                    HideAnswers(account.Questions);
                }

                return account;
            }
        }

        private void HideAnswers( IEnumerable<AccountChargeQuestion> questionsAndAnswers)
        {
            foreach (var accountChargeQuestion in questionsAndAnswers)
            {
                accountChargeQuestion.Answer = string.Empty;
            }
        }

        public object Post(AccountChargeRequest request)
        {
            var existing = _dao.FindByAccountNumber(request.Number);
            if (existing != null)
            {
                throw new HttpError(HttpStatusCode.Conflict, ErrorCode.AccountCharge_AccountAlreadyExisting.ToString());
            }

            var i = 0;
            
            var addUpdateAccountCharge = new AddUpdateAccountCharge
            {
                AccountChargeId = Guid.NewGuid(),
                Name = request.Name,
                Number = request.Number,
                Questions = request.Questions,
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
            var existing = _dao.FindByAccountNumber(request.Number);
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
                Number = request.Number,
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
            var existing = _dao.FindByAccountNumber(request.Number);
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