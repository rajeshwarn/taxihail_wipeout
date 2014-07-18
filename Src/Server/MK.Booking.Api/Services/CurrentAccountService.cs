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

            currentAccount.Settings.ChargeTypeId = account.Settings.ChargeTypeId ??
                                                   ParseToNullable(
                                                       ConfigurationManager.GetSetting(
                                                           "DefaultBookingSettings.ChargeTypeId"));
            currentAccount.Settings.VehicleTypeId = account.Settings.VehicleTypeId ??
                                                    ParseToNullable(
                                                        ConfigurationManager.GetSetting(
                                                            "DefaultBookingSettings.VehicleTypeId"));
            currentAccount.Settings.ProviderId = account.Settings.ProviderId ??
                                                 ParseToNullable(
                                                     ConfigurationManager.GetSetting("DefaultBookingSettings.ProviderId"));

            return currentAccount;
        }

        private int? ParseToNullable(string val)
        {
            int result;
            if (int.TryParse(val, out result))
            {
                return result;
            }
            return default(int?);
        }
    }
}