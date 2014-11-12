#region

using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.IBS;
using apcurium.MK.Common.Configuration;
using AutoMapper;
using ServiceStack.ServiceInterface;
using System.Collections.Generic;

#endregion

namespace apcurium.MK.Booking.Api.Services
{
    public class IbsChargeAccountService : Service
    {
        private readonly IIBSServiceProvider _ibsServiceProvider;
        private readonly IServerSettings _serverSettings;

        public IbsChargeAccountService(IIBSServiceProvider ibsServiceProvider, IServerSettings serverSettings)
        {
            _ibsServiceProvider = ibsServiceProvider;
            _serverSettings = serverSettings;
            
        }

        public IbsChargeAccount Get(IbsChargeAccountRequest request)
        {
            // TODO: When debugging, verify if at this point I need the company key in the request or the service 

            var accountFromIbs = _ibsServiceProvider.ChargeAccount().GetIbsAccount(request.AccountNumber, request.CustomerNumber);
            var account = new IbsChargeAccount();

            Mapper.Map(accountFromIbs, account);

            return account;
        }

        public IbsChargeAccountValidation Post(IbsChargeAccountValidationRequest validationRequest)
        {
            var ibsAccountValidation = _ibsServiceProvider.ChargeAccount().ValidateIbsChargeAccount(validationRequest.Prompts, validationRequest.AccountNumber, validationRequest.CustomerNumber);
            var accountValidation = new IbsChargeAccountValidation();

            Mapper.Map(ibsAccountValidation, accountValidation);

            return accountValidation;
        }

        public IEnumerable<IbsChargeAccount> Get()
        {
            var accountsFromIbs = _ibsServiceProvider.ChargeAccount().GetAllAccount();
            var accounts = new List<IbsChargeAccount>();

            Mapper.Map(accountsFromIbs, accounts);

            return accounts;
        }
    }
}