using System;
using NUnit.Framework;
using ServiceStack.ServiceClient.Web;
using apcurium.MK.Booking.Api.Client.Cmt;
using apcurium.MK.Booking.Api.Client.TaxiHail;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Client.Cmt.Payments.Tokenize;
using ServiceStack.Text;

namespace apcurium.CMT.Web.Tests
{
    [TestFixture]
    public class CmtPaymentServiceClientFixture 
    {

        protected const string BaseUrl = "https://payment-api.sandbox.creativemobiletechnologies.com/v2/merchants/:merchantToken/capture";



        [TestFixtureSetUp]
        public void TestFixtureSetup()
        {
        }

        [TestFixtureTearDown]
        public void TestFixtureTearDown()
        {
        }

        [SetUp]
        public void Setup()
        {
        }

        public class SampleVisa{
            public static string Number = "4012 0000 3333 0026".Replace(" ", "");
            public static string ZipCode = "00000";
            public static int AvcCvvCvv2 = 135;
            public static DateTime ExpirationDate = DateTime.Today.AddMonths(3);
        }

        public class SampleMastercard
        {
            public static string Number = "5424 1802 7979 1732".Replace(" ", "");
            public static string ZipCode = "00000";
            public static int AvcCvvCvv2 = 135;
            public static DateTime ExpirationDate = DateTime.Today.AddMonths(3);
        }

        public class SampleAmericanExpress
        {
            public static string Number = "3410 9293 659 1002".Replace(" ", "");            
            public static string ZipCode = "55555";
            public static int AvcCvvCvv2 = 1002;
            public static DateTime ExpirationDate = DateTime.Today.AddMonths(3);
        }

        public class SampleDiscover
        {
            public static string Number = "6011 0002 5950 5851".Replace(" ", "");
            public static string ZipCode = "00000";
            public static int AvcCvvCvv2 = 111;
            public static DateTime ExpirationDate = DateTime.Today.AddMonths(3);
        }

        [Test]
        public void when_tokenizing_a_credit_card()
        {
            var client = new CmtPaymentTokenizeClient();
            var response = client.Tokenize(SampleVisa.Number, SampleVisa.ExpirationDate);
            Assert.AreEqual(1, response.ResponseCode);
        }

        [Test]
        public void when_deleting_a_tokenized_credit_card()
        {
            var client = new CmtPaymentTokenizeClient();

            var token = client.Tokenize(SampleVisa.Number, SampleVisa.ExpirationDate).CardOnFileToken;

            var response = client.ForgetTokenizedCard(token);
            Assert.AreEqual(1, response.ResponseCode);
        }
    }
}
