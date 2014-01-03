#region

using System.Net;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Booking.Security;
using Infrastructure.Messaging;
using ServiceStack.Common.Web;
using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;

#endregion

namespace apcurium.MK.Booking.Api.Services
{
    public class UpdatePasswordService : Service
    {
        private readonly ICommandBus _commandBus;
        private readonly IAccountDao _dao;

        public UpdatePasswordService(ICommandBus commandBus, IAccountDao dao)
        {
            _commandBus = commandBus;
            _dao = dao;
        }

        public object Post(UpdatePassword request)
        {
            var user = _dao.FindById(request.AccountId);
            if (user == null) throw HttpError.NotFound("Account not found");
            if (!string.IsNullOrEmpty(user.FacebookId) || !string.IsNullOrEmpty(user.TwitterId))
                throw HttpError.Unauthorized("Facebook or Twitter account cannot update password");
            if (!new PasswordService().IsValid(request.CurrentPassword, request.AccountId.ToString(), user.Password))
                throw new HttpError(ErrorCode.UpdatePassword_NotSame.ToString());


            var udpateCommand = new UpdateAccountPassword
            {
                AccountId = user.Id,
                Password = request.NewPassword
            };


            _commandBus.Send(udpateCommand);

            // logout
            RequestContext.Get<IHttpRequest>().RemoveSession();
            return new HttpResult(HttpStatusCode.OK);
        }
    }
}