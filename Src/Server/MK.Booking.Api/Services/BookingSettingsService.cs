﻿#region

using System;
using System.Net;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.IBS;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Enumeration;
using apcurium.MK.Common.Extensions;
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
        private readonly IIBSServiceProvider _ibsServiceProvider;
        private readonly IServerSettings _serverSettings;

        public BookingSettingsService(IAccountChargeDao accountChargeDao, ICommandBus commandBus, IIBSServiceProvider ibsServiceProvider, IServerSettings serverSettings)
        {
            _accountChargeDao = accountChargeDao;
            _commandBus = commandBus;
            _ibsServiceProvider = ibsServiceProvider;
            _serverSettings = serverSettings;
        }

        public object Put(BookingSettingsRequest request)
        {
            var isChargeAccountEnabled = _serverSettings.GetPaymentSettings().IsChargeAccountPaymentEnabled;
            // Validate account number if charge account is enabled and ChargeType mode is set to Account
            if (isChargeAccountEnabled && request.ChargeTypeId == ChargeTypes.Account.Id && !string.IsNullOrWhiteSpace(request.AccountNumber))
            {
                if (!request.CustomerNumber.HasValue())
                {
                    throw new HttpError(HttpStatusCode.Forbidden, ErrorCode.AccountCharge_InvalidAccountNumber.ToString());
                }

                // Validate locally that the account exists
                var account = _accountChargeDao.FindByAccountNumber(request.AccountNumber);
                if (account == null)
                {
                    throw new HttpError(HttpStatusCode.Forbidden, ErrorCode.AccountCharge_InvalidAccountNumber.ToString());
                }

                // Validate with IBS to make sure the account/customer is still active
                var ibsChargeAccount = _ibsServiceProvider.ChargeAccount().GetIbsAccount(request.AccountNumber, request.CustomerNumber);
                if (!ibsChargeAccount.IsValid())
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