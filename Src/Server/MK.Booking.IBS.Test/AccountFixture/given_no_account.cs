#region

using System;
using NUnit.Framework;

#endregion

namespace MK.Booking.IBS.Test.AccountFixture
{
    [TestFixture]
    public class given_no_account
    {
        [SetUp]
        public void Setup()
        {
            Sut = new WebAccount3Service
            {
                Url = "http://apcuriumibs:6928/XDS_IASPI.DLL/soap/IWebAccount3"
            };
        }

        protected WebAccount3Service Sut;


        [Test]
        public void when_creating_an_account()
        {
            // Arrange
            var accountId = Guid.NewGuid().ToString().Substring(0, 5);
            var email = "vincent.costel@apcurium.com";
            var account = new TBookAccount3
            {
                WEBID = accountId,
                Address = new TWEBAddress(),
                Email2 = email,
                Title = "",
                FirstName = "Apcurium",
                LastName = "Test",
                Phone = "5141234569",
                MobilePhone = "5141234569",
                WEBPassword = "123456"
            };

            // Act
            var ibsAcccountId = Sut.SaveAccount3("taxi", "test", account);

            // Assert
            Assert.Greater(ibsAcccountId, 0);
        }
    }
}