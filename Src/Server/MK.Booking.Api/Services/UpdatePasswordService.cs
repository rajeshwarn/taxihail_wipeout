#region

using System.Net;
using System.Web;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Api.Extensions;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Booking.Security;
using Infrastructure.Messaging;

#endregion

namespace apcurium.MK.Booking.Api.Services
{
    public class UpdatePasswordService : BaseApiService
    {
        private readonly ICommandBus _commandBus;
        private readonly IAccountDao _dao;

        public UpdatePasswordService(ICommandBus commandBus, IAccountDao dao)
        {
            _commandBus = commandBus;
            _dao = dao;
        }

        public void Post(UpdatePassword request)
        {
            var user = _dao.FindById(request.AccountId);

            if (user == null)
            {
                throw new HttpException((int)HttpStatusCode.NotFound, "Account not found");
            }
            if (!string.IsNullOrEmpty(user.FacebookId) || !string.IsNullOrEmpty(user.TwitterId))
            {
                throw new HttpException((int)HttpStatusCode.Unauthorized, "Facebook or Twitter account cannot update password");
            }

            if (!new PasswordService().IsValid(request.CurrentPassword, request.AccountId.ToString(), user.Password))
            {
                throw new HttpException(ErrorCode.UpdatePassword_NotSame.ToString());
            }


            var updateCommand = new UpdateAccountPassword
            {
                AccountId = user.Id,
                Password = request.NewPassword
            };


            _commandBus.Send(updateCommand);

            // logout
            Session.RemoveSessionIfNeeded();
        }
    }
}