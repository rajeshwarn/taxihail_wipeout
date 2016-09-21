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
using System;
using apcurium.MK.Common.Extensions;
using apcurium.MK.Common.Configuration;

#endregion

namespace apcurium.MK.Booking.Api.Services
{
	public class ResetPasswordService : Service
	{
		private readonly ICommandBus _commandBus;
		private readonly IAccountDao _dao;
		private readonly IServerSettings _serverSettings;

		public ResetPasswordService(ICommandBus commandBus, IAccountDao dao, IServerSettings serverSettings)
		{
			_commandBus = commandBus;
			_dao = dao;
			_serverSettings = serverSettings;
		}

		public object Post(ResetPassword request)
		{
			var user = _dao.FindByEmail(request.EmailAddress);
			if (user == null)
			{
				throw new HttpError(ErrorCode.ResetPassword_AccountNotFound.ToString());
			}

			if (!string.IsNullOrEmpty(user.FacebookId))
			{
				throw new HttpError(ErrorCode.ResetPassword_FacebookAccount.ToString());
			}

			if (!string.IsNullOrEmpty(user.TwitterId))
			{
				throw new HttpError(ErrorCode.ResetPassword_TwitterAccount.ToString());
			}

			var currentSession = this.GetSession();

			var currentUserId = currentSession.UserAuthId.HasValueTrimmed()
				? new Guid(currentSession.UserAuthId)
				: Guid.Empty;

			if (user.Id == currentUserId)
			{
				// In case user is signed in, sign out user to force him to authenticate again
				base.RequestContext.Get<IHttpRequest>().RemoveSession();
			}

			var newPassword = new PasswordService().GeneratePassword();
			var resetCommand = new ResetAccountPassword
			{
				AccountId = user.Id,
				Password = newPassword
			};

			_commandBus.Send(resetCommand);

			if (_serverSettings.ServerData.SendPasswordResetAsSMSEnabled)
			{
				var smsCommand = new SendPasswordResetSMS
				{
					ClientLanguageCode = user.Language,
					CountryCode = user.Settings.Country,
					PhoneNumber = user.Settings.Phone,
					Password = newPassword
				};

				_commandBus.Send(smsCommand);
			}
			else
			{

				var emailCommand = new SendPasswordResetEmail
				{
					ClientLanguageCode = user.Language,
					EmailAddress = user.Email,
					Password = newPassword,
				};

				_commandBus.Send(emailCommand);
			}

			return new HttpResult(HttpStatusCode.OK);
		}
	}
}