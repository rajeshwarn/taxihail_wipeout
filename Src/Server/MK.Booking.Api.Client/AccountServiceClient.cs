using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ServiceStack.Common.Web;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Api.Contract.Requests;

namespace apcurium.MK.Booking.Api.Client
{
    public class AccountServiceClient : BaseServiceClient
    {
        public AccountServiceClient(string url, AuthInfo credential)
            : base(url, credential)
        {
        }


        public Account GetMyAccount( )
        {
            var result = Client.Get<Account>("/account/me");
            return result;
        }
        
        public Account GetTestAccount(int index)
        {
            var result = Client.Get<Account>("/account/test/" + index.ToString());
            return result;
        }

        public Guid RegisterAccount(RegisterAccount account)
        {
            var result = Client.Post<Account>("/account/register",account);
            return result.Id;
        }

        public AddressList GetFavoriteAddresses(Guid accountId)
        {
            var req = string.Format("/accounts/{0}/addresses", accountId.ToString());
            var addresses = Client.Get<AddressList>(req);
            return addresses;
        }

        public void AddFavoriteAddress(SaveFavoriteAddress address)
        {
            var req = string.Format("/account/{0}/addresses", address.AccountId);
            var response = Client.Post<string>(req, address);
        }

    }
}
