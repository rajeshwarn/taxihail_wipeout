#region

using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Booking.Security;
using Infrastructure.Messaging;
using System;
using System.Web;
using apcurium.MK.Booking.Api.Extensions;

#endregion

namespace apcurium.MK.Booking.Api.Services
{
   public class ResetPasswordService : BaseApiService
   {
      private readonly ICommandBus _commandBus;
      private readonly IAccountDao _dao;

      public ResetPasswordService(ICommandBus commandBus, IAccountDao dao)
      {
         _commandBus = commandBus;
         _dao = dao;
      }

      public void Post(string emailAddress)
      {
         var user = _dao.FindByEmail(emailAddress);
         if (user == null)
         {
            throw new HttpException(ErrorCode.ResetPassword_AccountNotFound.ToString());
         }

         if (!string.IsNullOrEmpty(user.FacebookId))
         {
            throw new HttpException(ErrorCode.ResetPassword_FacebookAccount.ToString());
         }

         if (!string.IsNullOrEmpty(user.TwitterId))
         {
            throw new HttpException(ErrorCode.ResetPassword_TwitterAccount.ToString());
         }

         var currentUserId = Session.IsAuthenticated()
             ? Session.UserId
             : Guid.Empty;

         if (user.Id == currentUserId)
         {
            // In case user is signed in, sign out user to force him to authenticate again
            Session.RemoveSessionIfNeeded();
         }

         var newPassword = new PasswordService().GeneratePassword();
         var resetCommand = new ResetAccountPassword
         {
            AccountId = user.Id,
            Password = newPassword
         };

         var emailCommand = new SendPasswordResetEmail
         {
            ClientLanguageCode = user.Language,
            EmailAddress = user.Email,
            Password = newPassword,
         };

         _commandBus.Send(resetCommand);
         _commandBus.Send(emailCommand);
      }
   }
}