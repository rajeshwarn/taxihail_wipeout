#region

using System.Net;
using System.Linq;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using Infrastructure.Messaging;
using ServiceStack.Common.Web;
using ServiceStack.ServiceInterface;
using apcurium.MK.Common.Extensions;
using System.Collections.Generic;
using apcurium.MK.Booking.ReadModel;

#endregion

namespace apcurium.MK.Booking.Api.Services
{
    public class AdminAccountManagementService : Service
    {
        private readonly ICommandBus _commandBus;
		protected IAccountDao _accountDao;

        public AdminAccountManagementService(IAccountDao dao, ICommandBus commandBus)
        {
			_accountDao = dao;
            _commandBus = commandBus;
        }

        public object Put(EnableAccountByAdminRequest request)
        {
			var account = _accountDao.FindByEmail(request.AccountEmail);
            if (account == null)
            {
                throw new HttpError(HttpStatusCode.NotFound, "Not Found");
            }

            _commandBus.Send(new EnableAccountByAdmin
            {
                AccountId = account.Id
            });
            return HttpStatusCode.OK;
        }

        public object Put(DisableAccountByAdminRequest request)
        {
			var account = _accountDao.FindByEmail(request.AccountEmail);
            if (account == null)
            {
                throw new HttpError(HttpStatusCode.NotFound, "Not Found");
            }

            _commandBus.Send(new DisableAccountByAdmin
            {
                AccountId = account.Id
            });
            return HttpStatusCode.OK;
        }

        public object Put(UnlinkAccountByAdminRequest request)
        {
			var account = _accountDao.FindByEmail(request.AccountEmail);
            if (account == null)
            {
                throw new HttpError(HttpStatusCode.NotFound, "Not Found");
            }

            _commandBus.Send(new UnlinkAccountFromIbs
            {
                AccountId = account.Id
            });
            return HttpStatusCode.OK;
        }

		public object Get(FindAccountsRequest request)
		{
			if (!request.SearchCriteria.HasValue())
			{
				throw new HttpError(HttpStatusCode.BadRequest, "SearchCriteria should have value");
			}

			var accountsWithName = _accountDao.FindByNamePattern(request.SearchCriteria);
			var accountsWithEmail = _accountDao.FindByEmailPattern(request.SearchCriteria);
			var accountsWithPhoneNumber = _accountDao.FindByPhoneNumberPattern(request.SearchCriteria);


			var foundAccounts = new List<AccountDetail>();

			foundAccounts.AddRange(accountsWithName);

			foundAccounts.AddRange(from acc2 in accountsWithEmail
								   where !(from acc1 in foundAccounts select acc1.Id).Contains(acc2.Id)
								   select acc2);

			foundAccounts.AddRange(from acc2 in accountsWithPhoneNumber
								   where !(from acc1 in foundAccounts select acc1.Id).Contains(acc2.Id)
								   select acc2);

			return foundAccounts.ToArray();
		}
    }
}