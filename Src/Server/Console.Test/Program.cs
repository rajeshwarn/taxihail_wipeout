using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using apcurium.MK.Booking.Api.Client;
using apcurium.MK.Booking.Api.Contract.Requests;

namespace Console.Test
{
    class Program
    {
        private const string BaseUrl = @"http://project.apcurium.com/apcurium.MK.Web.csproj_deploy/api/";
        static void Main(string[] args)
        {            
            //var service = new AccountServiceClient(@"http://project.apcurium.com/apcurium.MK.Web.csproj_deploy/api/", null);
            //service.RegisterAccount(new RegisterAccount { AccountId = Guid.NewGuid(), Email = "aaa@aaa.com", Name = "AAAA", Password = "VVVVV", Phone = "555-555-6666"});
            var auth = new AuthServiceClient(BaseUrl, null).Authenticate("aaa@aaa.com", "VVVVV");
            var service = new AccountServiceClient(BaseUrl, auth.SessionId);
            var account = service.GetMyAccount();
            account.ToString();
            //service.RegisterAccount(new RegisterAccount { AccountId = Guid.NewGuid(), Email = "aaa@aaa.com", Name = "AAAA", Password = "VVVVV", Phone = "555-555-6666" });
        }
    }
}
