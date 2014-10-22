#region

using System.Collections.Generic;
using System.Web.Mvc;
using CustomerPortal.Web.Entities;
using CustomerPortal.Web.Models;
using Moq;
using NUnit.Framework;

#endregion

namespace CustomerPortal.Web.Test.Areas.Customer.Controllers.HomeControllerFixture
{
    [TestFixture]
    public class given_a_company : HomeControllerFixtureBase
    {
        [SetUp]
        public override void Setup()
        {
            base.Setup();
        }

        [Test]
        public void EditAppDescription_Get_asks_for_EditAppDescription_view()
        {
            // Arrange
            Company.AppDescription = "the application description";

            // Act
            var result = Sut.EditAppDescription();

            // Assert
            Assert.IsInstanceOf<ViewResult>(result);
            var viewResult = (ViewResult) result;
            Assert.IsInstanceOf<Company>(viewResult.Model);
            var model = (Company) viewResult.Model;
            Assert.AreEqual("the application description", model.AppDescription);
        }

        [Test]
        public void EditStore_Get_asks_for_EditStore_view()
        {
            var result = Sut.EditStore();

            Assert.IsInstanceOf<ViewResult>(result);
            var viewResult = (ViewResult) result;
            Assert.IsInstanceOf<StoreSettings>(viewResult.Model);
        }

        [Test]
        public void EditStore_Post_saves_settings_into_company()
        {
            // Arrange

            var model = new StoreSettingsViewModel
            {
                Keywords = "app taxi cab",
                UniqueDeviceIdentificationNumber = new List<string> {"1234567890abcdef"},
                AppStoreCredentials = new AppleStoreCredentials
                {
                    Username = "user@appstore",
                    Password = "password",
                    Team = "team"
                },
                GooglePlayCredentials = new AndroidStoreCredentials
                {
                    Username = "user@googleplay",
                    Password = "password"
                }
            };

            // Act
            Sut.EditStore(model);

            // Assert
            ServiceMock.Verify(x => x.UpdateStoreSettings(It.Is<StoreSettings>(s => s
                .Keywords == "app taxi cab"
                                                                                    &&
                                                                                    s.UniqueDeviceIdentificationNumber[0
                                                                                        ] == "1234567890abcdef")));

            ServiceMock.Verify(x => x.UpdateAppleAppStoreCredentials(It.Is<AppleStoreCredentials>(s => 
                                                                                                  s.Username == 
                                                                                                  "user@appstore"
                                                                                                  &&
                                                                                                  s.Password ==
                                                                                                  "password"
                                                                                                  &&
                                                                                                  s.Team ==
                                                                                                  "team")));

            ServiceMock.Verify(x => x.UpdateGooglePlayCredentials(It.Is<AndroidStoreCredentials>(s => 
                                                                                                  s.Username == 
                                                                                                  "user@googleplay"
                                                                                                  &&
                                                                                                  s.Password ==
                                                                                                  "password")));
        }

        [Test]
        public void Edit_Get_asks_for_Edit_view()
        {
            var result = Sut.Edit();

            Assert.IsInstanceOf<ViewResult>(result);
            var viewResult = (ViewResult) result;
            Assert.IsInstanceOf<Questionnaire>(viewResult.Model);
        }

        [Test]
        public void Edit_Post_saves_valid_data_into_company()
        {
            // Arrange

            var model = new QuestionnaireViewModel
            {
                SupportContactEmail = "help@taxihail",
                CompanyPhoneNumber = "123 456 7890",
                CompanyWebsiteUrl = "http://www.centralniagara.com",
            };

            // Act
            Sut.Edit(model);

            // Assert
            ServiceMock.Verify(x => x.UpdateQuestionnaire(It.Is<Questionnaire>(s => s
                .SupportContactEmail == "help@taxihail"
                                                                                    &&
                                                                                    s.CompanyPhoneNumber ==
                                                                                    "123 456 7890"
                                                                                    &&
                                                                                    s.CompanyWebsiteUrl ==
                                                                                    "http://www.centralniagara.com")));
        }
    }
}