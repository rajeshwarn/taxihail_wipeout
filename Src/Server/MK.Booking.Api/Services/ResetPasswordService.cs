using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using Infrastructure.Messaging;
using ServiceStack.Common.Web;
using ServiceStack.ServiceInterface;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.ReadModel.Query;
using apcurium.MK.Booking.Security;

namespace apcurium.MK.Booking.Api.Services
{
    public class ResetPasswordService : RestServiceBase<ResetPassword>
    {

        private readonly ICommandBus _commandBus;
        private readonly IAccountDao _dao;

        public ResetPasswordService(ICommandBus commandBus, IAccountDao dao)
        {
            _commandBus = commandBus;
            _dao = dao;
        }

        public override object OnPost(ResetPassword request)
        {
            var user = _dao.FindByEmail(request.EmailAddress);
            if (user == null) throw HttpError.NotFound("Account not found");

            var newPassword = new PasswordService().GeneratePassword();
            var command = new Commands.ResetAccountPassword();
            command.AccountId = user.Id;
            command.Password = newPassword;
            _commandBus.Send(command);

            return new HttpResult(HttpStatusCode.OK);
        }
    }
}
