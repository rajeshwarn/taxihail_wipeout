using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Api.Contract.Requests;
using ServiceStack.ServiceInterface.Auth;
using System.Net;
using ServiceStack.ServiceClient.Web;
using System.Threading;

namespace apcurium.MK.Web.ConsoleTest
{
    class Program
    {
        static void Main(string[] args)
        {
            

            
            //var email = string.Format( "testemail.{0}@apcurium.com", Guid.NewGuid().ToString().Replace("-", "" ) );

            //var account = client.Send<Account>( new RegisterAccount { Email = email, FirstName = "Test", LastName = "Account", Id = Guid.NewGuid(), Password = "password1" });

            //Thread.Sleep(5000);
    
            //var authResponse = client.Send<AuthResponse>( new Auth {
            //    UserName = email,
            //    Password = "password1", 
            //    RememberMe = true, 
            //    provider = CredentialsAuthProvider.Name 
            //});

            //var accountLoaded = client.Get<Account>("/account/me");


            //client = new ServiceStack.ServiceClient.Web.JsonServiceClient("http://localhost.:6900/api");

            //container = new CookieContainer();
            //container.Add(new Uri("http://localhost.:6900/api"), new Cookie("ss-id", authResponse.SessionId));            
            
            //ServiceClientBase.HttpWebRequestFilter = req =>
            //{
            //    req.CookieContainer = container;
            //};


            //var accountLoaded2 = client.Get<Account>("/account/me");
            


            
        }
    }
}
