#region

using System;
using System.Collections;
using System.Collections.Generic;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Common.Entity;

#endregion

namespace apcurium.MK.Booking.Api.Client.TaxiHail
{
    public class AdministrationServiceClient : BaseServiceClient
    {
        public AdministrationServiceClient(string url, string sessionId, string userAgent)
            : base(url, sessionId, userAgent)
        {
        }

        public void GrantAdminAccess(GrantAdminRightRequest request)
        {
            var req = string.Format("/account/grantadmin");
            Client.Put<string>(req, request);
        }

        public void EnableAccount(EnableAccountByAdminRequest request)
        {
            var req = string.Format("/account/adminenable");
            Client.Put<string>(req, request);
        }

        public void DisableAccount(DisableAccountByAdminRequest request)
        {
            var req = string.Format("/account/admindisable");
            Client.Put<string>(req, request);
        }

        public IList<Address> GetDefaultFavoriteAddresses()
        {
            var req = string.Format("/admin/addresses");
            var addresses = Client.Get<IList<Address>>(req);
            return addresses;
        }

        public void AddDefaultFavoriteAddress(DefaultFavoriteAddress address)
        {
            var req = string.Format("/admin/addresses");
            Client.Post<string>(req, address);
        }

        public void UpdateDefaultFavoriteAddress(DefaultFavoriteAddress address)
        {
            var req = string.Format("/admin/addresses/{0}", address.Id);
            Client.Put<string>(req, address);
        }

        public void RemoveDefaultFavoriteAddress(Guid addressId)
        {
            var req = string.Format("/admin/addresses/{0}", addressId);
            Client.Delete<string>(req);
        }


        public void AddPopularAddress(PopularAddress address)
        {
            var req = string.Format("/admin/popularaddresses");
            Client.Post<string>(req, address);
        }

        public void UpdatePopularAddress(PopularAddress address)
        {
            var req = string.Format("/admin/popularaddresses/{0}", address.Id);
            Client.Put<string>(req, address);
        }

        public void RemovePopularAddress(Guid addressId)
        {
            var req = string.Format("/admin/popularaddresses/{0}", addressId);
            Client.Delete<string>(req);
        }

        public IList<Address> GetPopularAddresses()
        {
            var req = string.Format("/popularaddresses");
            var addresses = Client.Get<IList<Address>>(req);
            return addresses;
        }

        public IEnumerable GetAllAppSettings()
        {
            var req = string.Format("/settings");
            var address = Client.Get<IEnumerable>(req);
            return address;
        }

        public void AddOrUpdateAppSettings(ConfigurationsRequest appReq)
        {
            var req = string.Format("/settings");
            Client.Post<string>(req, appReq);
        }
    }
}