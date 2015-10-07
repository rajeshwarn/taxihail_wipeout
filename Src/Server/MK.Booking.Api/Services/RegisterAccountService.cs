using System;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Api.Helpers;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Extensions;
using AutoMapper;
using Infrastructure.Messaging;
using ServiceStack.Common.Web;
using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;
using RegisterAccount = apcurium.MK.Booking.Api.Contract.Requests.RegisterAccount;
using apcurium.MK.Common;
using apcurium.MK.Common.Helpers;

namespace apcurium.MK.Booking.Api.Services
{
    public class RegisterAccountService : Service
    {
        private readonly IAccountDao _accountDao;
        private readonly ICommandBus _commandBus;
        private readonly IServerSettings _serverSettings;
        private readonly Resources.Resources _resources;

        public RegisterAccountService(ICommandBus commandBus, IAccountDao accountDao, IServerSettings serverSettings)
        {
            _commandBus = commandBus;
            _accountDao = accountDao;
            _serverSettings = serverSettings;
            _resources = new Resources.Resources(serverSettings);
        }

        public object Post(RegisterAccount request)
        {
            // Ensure user is not signed in
            RequestContext.Get<IHttpRequest>().RemoveSession();

            if (_accountDao.FindByEmail(request.Email) != null)
            {
                throw new HttpError(ErrorCode.CreateAccount_AccountAlreadyExist.ToString());
            }

            CountryCode countryCode = CountryCode.GetCountryCodeByIndex(CountryCode.GetCountryCodeIndexByCountryISOCode(request.Country));

			if (PhoneHelper.IsNumberPossible(countryCode, request.Phone))
            {
                request.Phone = PhoneHelper.GetDigitsFromPhoneNumber(request.Phone);
            }
            else
            {
                throw new HttpError(string.Format(_resources.Get("PhoneNumberFormat"), countryCode.GetPhoneExample()));
            }

            if (request.FacebookId.HasValue())
            {
                // Facebook registration
                if (_accountDao.FindByFacebookId(request.FacebookId) != null)
                {
                    throw new HttpError(ErrorCode.CreateAccount_AccountAlreadyExist.ToString());
                }
                    
                var command = new RegisterFacebookAccount();

                Mapper.Map(request, command);
                command.Id = Guid.NewGuid();

                _commandBus.Send(command);

                return new Account {Id = command.AccountId};
            }
            if (request.TwitterId.HasValue())
            {
                // Twitter registration
                if (_accountDao.FindByTwitterId(request.TwitterId) != null)
                {
                    throw new HttpError(ErrorCode.CreateAccount_AccountAlreadyExist.ToString());
                }
                    
                var command = new RegisterTwitterAccount();

                Mapper.Map(request, command);
                command.Id = Guid.NewGuid();

                _commandBus.Send(command);

                return new Account {Id = command.AccountId};
            }
            else
            {
                // Normal registration
                var accountActivationDisabled = _serverSettings.ServerData.AccountActivationDisabled;
                var smsConfirmationEnabled = _serverSettings.ServerData.SMSConfirmationEnabled;

                var confirmationToken = smsConfirmationEnabled 
                    ? GenerateActivationCode() 
                    : Guid.NewGuid().ToString();

                var command = new Commands.RegisterAccount();

                Mapper.Map(request, command);
                command.Id = Guid.NewGuid();

                command.ConfimationToken = confirmationToken;
                command.AccountActivationDisabled = accountActivationDisabled;
                _commandBus.Send(command);

                if (!accountActivationDisabled)
                {
                    if (smsConfirmationEnabled
                        && (request.ActivationMethod == null  
                                || request.ActivationMethod == ActivationMethod.Sms))
                    {
                        _commandBus.Send(new SendAccountConfirmationSMS
                        {
                            ClientLanguageCode = command.Language,
                            Code = confirmationToken,
                            CountryCode = command.Country,
                            PhoneNumber = command.Phone
                        });
                    }
                    else
                    {
                        _commandBus.Send(new SendAccountConfirmationEmail
                        {
                            ClientLanguageCode = command.Language,
                            EmailAddress = command.Email,
                            ConfirmationUrl =
                                new Uri(string.Format("/api/account/confirm/{0}/{1}", command.Email, confirmationToken), UriKind.Relative),
                        });
                    }
                }

                return new Account {Id = command.AccountId};
            }
        }

        private string GenerateActivationCode()
        {
            var random = new Random(DateTime.Now.Second);
            var strPass = "";
            for (var x = 0; x <= 4; x++)
            {
                var p = random.Next(0, 9);
                strPass += p;
            }
            return strPass;
        }
    }
}