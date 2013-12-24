using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using AutoMapper;
using ServiceStack.ServiceInterface;
using apcurium.MK.Booking.ReadModel.Query;
using apcurium.MK.Booking.Api.Contract.Requests;
using ServiceStack.ServiceInterface.Auth;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Common.Configuration;

namespace apcurium.MK.Booking.Api.Services
{
    public class CurrentAccountService : RestServiceBase<CurrentAccount>
    {

        public CurrentAccountService(IAccountDao dao, IConfigurationManager configurationManager)
        {
            Dao = dao;
            ConfigurationManager = configurationManager;
        }

        protected IAccountDao Dao { get; set; }
        protected IConfigurationManager ConfigurationManager { get; set; }


        public override object OnGet(CurrentAccount request)
        {
            var session = this.GetSession();
            var account = Dao.FindById(new Guid(session.UserAuthId));

            var currentAccount = Mapper.Map<CurrentAccountResponse>(account);

            currentAccount.Settings.ChargeTypeId = account.Settings.ChargeTypeId ?? ParseToNullable(ConfigurationManager.GetSetting("DefaultBookingSettings.ChargeTypeId"));
            currentAccount.Settings.VehicleTypeId = account.Settings.VehicleTypeId ?? ParseToNullable(ConfigurationManager.GetSetting("DefaultBookingSettings.VehicleTypeId"));
            currentAccount.Settings.ProviderId = account.Settings.ProviderId ?? ParseToNullable(ConfigurationManager.GetSetting("DefaultBookingSettings.ProviderId"));

            return currentAccount;
        }

        private int? ParseToNullable(string val)
        {
            int result;
            if (int.TryParse(val, out result))
            {
                return result;
            }
            else
            {
                return default(int?);
            }
        }
    }
}
