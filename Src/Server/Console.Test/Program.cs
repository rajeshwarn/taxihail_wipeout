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
        static void Main(string[] args)
        {
            var service = new AccountServiceClient(@"http://localhost:6900/", null);
            //var service = new AccountServiceClient(@"http://project.apcurium.com/apcurium.MK.Web.csproj_deploy/api/", null);
            service.RegisterAccount(new RegisterAccount { AccountId = Guid.NewGuid(), Email = "aaa@aaa.com", FirstName = "AAAA", LastName = "BBBB", Password = "VVVVV", Phone = "555-555-6666"});
        }
    }
}
