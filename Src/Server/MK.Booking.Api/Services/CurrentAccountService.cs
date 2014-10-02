#region

using System;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Common.Configuration;
using AutoMapper;
using Infrastructure.Messaging;
using ServiceStack.ServiceInterface;

#endregion

namespace apcurium.MK.Booking.Api.Services
{
    public class CurrentAccountService : Service
    {
        public CurrentAccountService(IAccountDao dao, IConfigurationManager configurationManager)
        {
            Dao = dao;
            ConfigurationManager = configurationManager;
        }

        protected IAccountDao Dao { get; set; }
        protected IConfigurationManager ConfigurationManager { get; set; }
        
        public object Get(CurrentAccount request)
        {
            var session = this.GetSession();
            var account = Dao.FindById(new Guid(session.UserAuthId));

            var currentAccount = Mapper.Map<CurrentAccountResponse>(account);

            currentAccount.Settings.ChargeTypeId = account.Settings.ChargeTypeId ?? ConfigurationManager.ServerData.DefaultBookingSettings.ChargeTypeId;
            currentAccount.Settings.VehicleTypeId = account.Settings.VehicleTypeId ?? ConfigurationManager.ServerData.DefaultBookingSettings.VehicleTypeId;
            currentAccount.Settings.ProviderId = account.Settings.ProviderId ?? ConfigurationManager.ServerData.DefaultBookingSettings.ProviderId;

            return currentAccount;
        }
    }
}