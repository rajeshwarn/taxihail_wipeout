using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Web.Http;
using System.Web.Http.Controllers;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Services;
using apcurium.MK.Booking.Email;
using apcurium.MK.Booking.IBS;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Booking.Security;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Extensions;
using apcurium.MK.Web.Security;
using Infrastructure.Messaging;

namespace apcurium.MK.Web.Controllers.Api.Account
{
    [RoutePrefix("account")]
    public class AccountController : BaseApiController
    {
        private readonly BookingSettingsService _bookingSettingsService;
        private readonly ConfirmAccountService _confirmAccountService;

        public AccountController(IAccountDao accountDao, IServerSettings serverSettings,
            IAccountChargeDao accountChargeDao, ICommandBus commandBus, IIBSServiceProvider ibsServiceProvider,
            ITemplateService templateService)
        {
            _bookingSettingsService = new BookingSettingsService(accountChargeDao, accountDao, commandBus, ibsServiceProvider, serverSettings);

            _confirmAccountService = new ConfirmAccountService(commandBus,accountDao,templateService,serverSettings);
        }

        protected override void Initialize(HttpControllerContext controllerContext)
        {
            base.Initialize(controllerContext);

            PrepareApiServices(_bookingSettingsService, _confirmAccountService);
        }

        [HttpPut, Auth(Role = RoleName.Support), Route("update/{accountId}")]
        public IHttpActionResult AccountUpdate(Guid accountId, BookingSettingsRequest bookingSettings)
        {
            _bookingSettingsService.Put(new AccountUpdateRequest()
                {
                    AccountId = accountId,
                    BookingSettingsRequest = bookingSettings
                });

            return Ok();
        }

        [HttpPut, Route("bookingsettings")]
        public IHttpActionResult AccountUpdate(BookingSettingsRequest request)
        {
            _bookingSettingsService.Put(new AccountUpdateRequest()
            {
                AccountId = GetSession().UserId,
                BookingSettingsRequest = request
            });

            return Ok();
        }

        [HttpGet, Route("getconfirmationcode/{email}/{countryCode}/{phoneNumber}")]
        public IHttpActionResult GetConfirmationCode(string email, string countryCode, string phoneNumber)
        {
            _confirmAccountService.Get(new ConfirmationCodeRequest() {PhoneNumber = phoneNumber, CountryCode = countryCode, Email = email});

            return Ok();
        }

        [HttpGet, Route("confirm/{emailAddress}/{confirmationToken}/{isSmsConfirmation:bool?}")]
        public HttpResponseMessage ConfirmAccount(string emailAddress, string confirmationToken, bool? isSmsConfirmation)
        {
            var result = _confirmAccountService.Get(new ConfirmAccountRequest()
                {
                    ConfirmationToken = confirmationToken,
                    EmailAddress = emailAddress,
                    IsSMSConfirmation = isSmsConfirmation
                });

            if (!result.HasValueTrimmed())
            {
                return new HttpResponseMessage(HttpStatusCode.OK);
            }

            var stringContent = new StringContent(result);

            stringContent.Headers.ContentType = new MediaTypeHeaderValue(MediaTypeNames.Text.Html);

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = stringContent
            };
        }

    }
}
