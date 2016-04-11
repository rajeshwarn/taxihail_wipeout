using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Web.Http;
using apcurium.MK.Booking.Api.Contract.Http;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Services;
using apcurium.MK.Booking.Security;
using apcurium.MK.Common.Extensions;
using apcurium.MK.Web.Security;

namespace apcurium.MK.Web.Controllers.Api.Account
{
    public class AccountController : BaseApiController
    {
        public BookingSettingsService BookingSettingsService { get; private set; }
        public ConfirmAccountService ConfirmAccountService { get; private set; }
        public ResetPasswordService ResetPasswordService { get; private set; }
        public RegisterAccountService RegisterAccountService { get; private set; }
        public CurrentAccountService CurrentAccountService { get; private set; }
        public UpdatePasswordService UpdatePasswordService { get; private set; }
        

        public AccountController(
            BookingSettingsService bookingSettingsService,
            ConfirmAccountService confirmAccountService, 
            ResetPasswordService resetPasswordService, 
            RegisterAccountService registerAccountService, 
            CurrentAccountService currentAccountService,
            UpdatePasswordService updatePasswordService)
        {
            BookingSettingsService = bookingSettingsService;
            ConfirmAccountService = confirmAccountService;
            ResetPasswordService = resetPasswordService;
            RegisterAccountService = registerAccountService;
            CurrentAccountService = currentAccountService;
            UpdatePasswordService = updatePasswordService;
        }

        [HttpPut, Auth(Role = RoleName.Support), Route("api/v2/accounts/update/{accountId}")]
        public IHttpActionResult AccountUpdate(Guid accountId, BookingSettingsRequest bookingSettings)
        {
            BookingSettingsService.Put(new AccountUpdateRequest()
                {
                    AccountId = accountId,
                    BookingSettingsRequest = bookingSettings
                });

            return Ok();
        }

        [HttpPut, Route("api/v2/accounts/bookingsettings")]
        public IHttpActionResult AccountUpdate(BookingSettingsRequest request)
        {
            BookingSettingsService.Put(new AccountUpdateRequest()
            {
                AccountId = Session.UserId,
                BookingSettingsRequest = request
            });

            return Ok();
        }

        [HttpGet, Route("api/v2/accounts/getconfirmationcode/{email}/{countryCode}/{phoneNumber}")]
        public IHttpActionResult GetConfirmationCode(string email, string countryCode, string phoneNumber)
        {
            ConfirmAccountService.Get(new ConfirmationCodeRequest() {PhoneNumber = phoneNumber, CountryCode = countryCode, Email = email});

            return Ok();
        }

        [HttpGet, Route("api/v2/accounts/confirm/{emailAddress}/{confirmationToken}")]
        public IHttpActionResult ConfirmAccount(string emailAddress, string confirmationToken)
        {
            return ConfirmAccount(emailAddress, confirmationToken, null);
        }

        [HttpGet, Route("api/v2/accounts/confirm/{emailAddress}/{confirmationToken}/{isSmsConfirmation}")]
        public IHttpActionResult ConfirmAccount(string emailAddress, string confirmationToken, bool? isSmsConfirmation)
        {
            var result = ConfirmAccountService.Get(new ConfirmAccountRequest()
                {
                    ConfirmationToken = confirmationToken,
                    EmailAddress = emailAddress,
                    IsSMSConfirmation = isSmsConfirmation
                });

            if (!result.HasValueTrimmed())
            {
                return Ok();
            }

            var stringContent = new StringContent(result);

            stringContent.Headers.ContentType = new MediaTypeHeaderValue(MediaTypeNames.Text.Html);


            return ResponseMessage(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = stringContent
            });
        }

        [HttpGet, Auth, NoCache, Route("api/v2/accounts")]
        public IHttpActionResult GetCurrentAccount()
        {
            var result = CurrentAccountService.Get(new CurrentAccount());

            return GenerateActionResult(result);
        }

        [HttpGet, NoCache, Route("api/v2/accounts/phone/{email}")]
        public IHttpActionResult GetAccountPhone(string email)
        {
            var result = CurrentAccountService.Get(new CurrentAccountPhoneRequest() { Email = email });

            return GenerateActionResult(result);
        }

        [HttpPost, Route("api/v2/accounts/register")]
        public IHttpActionResult Register([FromBody] RegisterAccount request)
        {
            var result = RegisterAccountService.Post(request);

            return GenerateActionResult(result);
        }

        [HttpPost, Route("api/v2/accounts/resetpassword/{emailAddress}")]
        public IHttpActionResult ResetPassword(string emailAddress)
        {
            ResetPasswordService.Post(emailAddress);

            return Ok();
        }

        [HttpPost, Auth, Route("api/v2/accounts/{accountId}/updatePassword")]
        public IHttpActionResult UpdatePassword(Guid accountId, [FromBody]UpdatePassword request)
        {
            request.AccountId = accountId;

            UpdatePasswordService.Post(request);

            return Ok();
        }
    }
}
