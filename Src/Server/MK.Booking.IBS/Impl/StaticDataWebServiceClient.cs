#region

using System;
using System.Linq;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Diagnostic;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Extensions;

#endregion

namespace apcurium.MK.Booking.IBS.Impl
{
    public class StaticDataWebServiceClient : BaseService<StaticDataservice>, IStaticDataWebServiceClient
    {
        public StaticDataWebServiceClient(IConfigurationManager configManager, ILogger logger)
            : base(configManager, logger)
        {
        }

        public ListItem[] GetCompaniesList()
        {
            var items = new ListItem[] {};
            UseService(service =>
            {
                var u = UserNameApp;
                var p = PasswordApp;
                var companies = service.GetProviders(u, p);
                items =
                    companies.Select(
                        x => new ListItem {Display = x.ProviderName, Id = x.ProviderNum, IsDefault = x.isDefault})
                        .ToArray();
            });
            return items;
        }


        public string GetZoneByCoordinate(int? providerId, double latitude, double longitude)
        {
            var zone = "";
            var useProvider = providerId.HasValue && providerId > 0;
            var zoneByCompanyEnabled =
                ConfigManager.GetSetting("IBS.ZoneByCompanyEnabled")
                    .SelectOrDefault(bool.Parse, false);

            UseService(service =>
            {
                if (providerId != null)
                {
                    zone = useProvider && zoneByCompanyEnabled
                        ? service.GetCompanyZoneByGPS(UserNameApp, PasswordApp, providerId.Value, latitude, longitude)
                        : service.GetZoneByGPS(UserNameApp, PasswordApp, latitude, longitude);
                }
            });
            return zone;
        }


        public ListItem[] GetVehiclesList(ListItem company)
        {
            if (company == null) throw new ArgumentNullException("company");
            if (company.Id == null) throw new ArgumentException("company Id should not be null");

            var items = new ListItem[] {};
            UseService(service =>
            {
                var vehicules = service.GetVehicleTypes(UserNameApp, PasswordApp, company.Id.GetValueOrDefault());
                items =
                    vehicules.Select(
                        x => new ListItem {Display = x.Name, Id = x.ID, Parent = company, IsDefault = x.isDefault})
                        .ToArray();
            });
            return items;
        }

        public ListItem[] GetPaymentsList(ListItem company)
        {
            if (company == null) throw new ArgumentNullException("company");
            if (company.Id == null) throw new ArgumentException("company Id should not be null");

            var items = new ListItem[] {};
            UseService(service =>
            {
                var payments = service.GetChargeTypes(UserNameApp, PasswordApp, company.Id.GetValueOrDefault());
                items =
                    payments.Select(
                        x => new ListItem {Display = x.ChargeTypeName, Id = x.ChargeTypeID, Parent = company}).ToArray();
            });
            return items;
        }

        protected override string GetUrl()
        {
            return base.GetUrl() + "IStaticData";
        }
    }
}