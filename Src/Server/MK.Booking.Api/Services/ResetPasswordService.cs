using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using Infrastructure.Messaging;
using ServiceStack.Common.Web;
using ServiceStack.ServiceInterface;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Security;

namespace apcurium.MK.Booking.Api.Services
{
    public class ResetPasswordService : RestServiceBase<ResetPassword>
    {

        private ICommandBus _commandBus;

        public ResetPasswordService(ICommandBus commandBus)
        {
            _commandBus = commandBus;
        }

        public override object OnPost(ResetPassword request)
        {
            if (!request.AccountId.Equals(new Guid(this.GetSession().UserAuthId)))
            {
                throw HttpError.Unauthorized("Unauthorized");
            }
            var service = new PasswordService();
            var command = new Commands.ResetAccountPassword();
            command.AccountId = request.AccountId;
            command.Password = service.GeneratePassword();
            _commandBus.Send(command);

            return new HttpResult(HttpStatusCode.OK);
        }
    }
}
