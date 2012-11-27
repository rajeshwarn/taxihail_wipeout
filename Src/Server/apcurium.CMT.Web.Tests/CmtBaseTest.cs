using ServiceStack.Text;
using apcurium.MK.Booking.Api.Client.Cmt;
using apcurium.MK.Booking.Api.Client.TaxiHail;
using apcurium.MK.Booking.Api.Contract.Resources;
using System;

namespace apcurium.CMT.Web.Tests
{
    public class CmtBaseTest
    {
        protected const string BaseUrl = "https://mobile-sandbox.cmtapi.com/v1/";
        protected Account TestAccount { get; set; }
        protected string TestAccountPassword { get { return "password1"; } }
        protected string SessionId { get; set; }

        protected CmtAuthCredentials Credentials = new CmtAuthCredentials();

        public virtual void TestFixtureSetup()
        {
            JsConfig.EmitCamelCaseNames = true;
            JsConfig.DateHandler = JsonDateHandler.ISO8601;
            Credentials = new CmtAuthCredentials { ConsumerKey = "AH7j9KweF235hP", ConsumerSecret = "K09JucBn23dDrehZa"};
        }

        public virtual void Setup()
        {

        }

        public virtual void TestFixtureTearDown()
        {
           
        }

        protected  string GetTempEmail()
        {
            var email = string.Format("testemail.{0}@apcurium.com", Guid.NewGuid().ToString().Replace("-", ""));
            return email;
        }

        protected Account CreateAndAuthenticateTestAccount()
        {
            var newAccount = new AccountServiceClient(BaseUrl, null).CreateTestAccount();
            var authResponse = new AuthServiceClient(BaseUrl, null).Authenticate(newAccount.Email, TestAccountPassword);
            SessionId = authResponse.SessionId;
            return newAccount;
        }
        
    }
}
