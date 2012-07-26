using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using Infrastructure.Messaging;
using ServiceStack.Common.Web;
using ServiceStack.ServiceInterface;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.ReadModel.Query;

namespace apcurium.MK.Booking.Api.Services
{
    public class ConfirmAccountService : RestServiceBase<ConfirmAccountRequest>
    {
        private readonly IAccountDao _accountDao;
        private ICommandBus _commandBus;

        public ConfirmAccountService(ICommandBus commandBus, IAccountDao accountDao)
        {
            _accountDao = accountDao;
            _commandBus = commandBus;
        }

        public override object OnGet(ConfirmAccountRequest request)
        {
            var account = _accountDao.FindByEmail(request.EmailAddress);
            if(account == null) throw new HttpError(HttpStatusCode.NotFound, "Not Found");

            _commandBus.Send(new ConfirmAccount
            {
                AccountId = account.Id,
                ConfimationToken = request.ConfirmationToken
            });

            return new HttpResult(new FileInfo(Path.Combine(AssemblyDirectory, "AccountConfirmationSuccess.html")));
        }

        static public string AssemblyDirectory
        {
            get
            {
                string codeBase = Assembly.GetExecutingAssembly().CodeBase;
                UriBuilder uri = new UriBuilder(codeBase);
                string path = Uri.UnescapeDataString(uri.Path);
                return Path.GetDirectoryName(path);
            }
        }
    }
}
