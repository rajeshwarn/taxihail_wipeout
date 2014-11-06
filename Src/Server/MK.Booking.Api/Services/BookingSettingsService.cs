#region

using System;
using System.Net;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using AutoMapper;
using Infrastructure.Messaging;
using ServiceStack.Common.Web;
using ServiceStack.ServiceInterface;

#endregion

namespace apcurium.MK.Booking.Api.Services
{
    public class BookingSettingsService : Service
    {
        private readonly IAccountChargeDao _accountChargeDao;
        private readonly ICommandBus _commandBus;

        public BookingSettingsService(IAccountChargeDao accountChargeDao, ICommandBus commandBus)
        {
            _accountChargeDao = accountChargeDao;
            _commandBus = commandBus;
        }

        public object Put(BookingSettingsRequest request)
        {
            // Validate account number
            if (!string.IsNullOrWhiteSpace(request.AccountNumber))
            {
                var chargeAccount = _accountChargeDao.FindByAccountNumber(request.AccountNumber);
                if (chargeAccount == null)
                {
                    throw new HttpError(HttpStatusCode.Forbidden, ErrorCode.AccountCharge_InvalidAccountNumber.ToString());
                }
            }

            var command = new UpdateBookingSettings();
            Mapper.Map(request, command);

            command.AccountId = new Guid(this.GetSession().UserAuthId);

            _commandBus.Send(command);

            return new HttpResult(HttpStatusCode.OK);
        }
    }
}