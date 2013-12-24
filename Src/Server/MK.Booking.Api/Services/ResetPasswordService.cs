using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using Infrastructure.Messaging;
using ServiceStack.Common.Web;
using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Commands;
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
            if (user == null) throw new HttpError(ErrorCode.ResetPassword_AccountNotFound.ToString());

            // In case user is signed in, sign out user to force him to authenticate again
            base.RequestContext.Get<IHttpRequest>().RemoveSession();
            
            var newPassword = new PasswordService().GeneratePassword();
            var resetCommand = new Commands.ResetAccountPassword
            {
                AccountId = user.Id,
                Password = newPassword
            };

            var emailCommand = new SendPasswordResetEmail
            {
                EmailAddress = user.Email,
                Password = newPassword,
            };

            _commandBus.Send(resetCommand);
            _commandBus.Send(emailCommand);

            return new HttpResult(HttpStatusCode.OK);
        }
    }
}
