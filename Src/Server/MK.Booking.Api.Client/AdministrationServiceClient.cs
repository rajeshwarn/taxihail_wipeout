using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Common.Entity;

namespace apcurium.MK.Booking.Api.Client
{
    public class AdministrationServiceClient : BaseServiceClient
    {
        public AdministrationServiceClient(string url, string sessionId) : base(url, sessionId)
        {
        }

        public void GrantAdminAccess(GrantAdminRightRequest request)
        {
            var req = string.Format("/account/grantadmin");
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
            var response = Client.Post<string>(req, address);
        }

        public void UpdateDefaultFavoriteAddress(DefaultFavoriteAddress address)
        {
            var req = string.Format("/admin/addresses/{0}", address.Id);
            var response = Client.Put<string>(req, address);
        }

        public void RemoveDefaultFavoriteAddress(Guid addressId)
        {
            var req = string.Format("/admin/addresses/{0}", addressId);
            var response = Client.Delete<string>(req);
        }


        public void AddPopularAddress(PopularAddress address)
        {
            var req = string.Format("/admin/popularaddresses");
            var response = Client.Post<string>(req, address);
        }

        public void UpdatePopularAddress(PopularAddress address)
        {
            var req = string.Format("/admin/popularaddresses/{0}", address.Id);
            var response = Client.Put<string>(req, address);
        }

        public void RemovePopularAddress(Guid addressId)
        {
            var req = string.Format("/admin/popularaddresses/{0}", addressId);
            var response = Client.Delete<string>(req);
        }

        public IList<Address> GetPopularAddresses()
        {
            var req = string.Format("/popularaddresses");
            var addresses = Client.Get<IList<Address>>(req);
            return addresses;
        }

        public IEnumerable GetAllAppSettings()
        {
            var req = string.Format("/settings/");
            var address = Client.Get<IEnumerable>(req);
            return address;
        }

        public string AddAppSettings(ConfigurationsRequest appReq)
        {
            var req = string.Format("/settings/");
            return Client.Post<string>(req, appReq);
        }

        public string UpdateAppSettings(ConfigurationsRequest appReq)
        {
            var req = string.Format("/settings/{0}", appReq.Key);
            return Client.Put<string>(req, appReq);
            
        }

        public void CreateRate(Rates rate)
        {
            var req = string.Format("/admin/rates");
            var response = Client.Post<string>(req, rate);
        }

        public void DeleteRate(Guid rateId)
        {
            var req = string.Format("/admin/rates/" + rateId);
            var response = Client.Delete<string>(req);
        }

        public IList<Rates> GetRates()
        {
            var req = string.Format("/admin/rates");
            return Client.Get<IList<Rates>>(req);
        }

        
    }
}
