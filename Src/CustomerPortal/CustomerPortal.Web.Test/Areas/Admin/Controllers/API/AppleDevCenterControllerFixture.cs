#region

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Web.Http;
using CustomerPortal.Web.Areas.Admin.Controllers.API;
using CustomerPortal.Web.Areas.Admin.Models;
using NUnit.Framework;

#endregion

namespace CustomerPortal.Web.Test.Areas.Admin.Controllers.API
{
    [TestFixture]
    public class AppleDevCenterControllerFixture
    {
        [SetUp]
        public void Setup()
        {
            client = new HttpClient {BaseAddress = new Uri("http://localhost:2287/api/")};
            Sut = new AppleDevCenterController();
        }

        private AppleDevCenterController Sut { get; set; }
        private HttpClient client;

        [Test]
        public void When_downloading_adhoc_profile_we_get_file()
        {
            var data = new DownloadProvisioningProfileRequest
            {
                Username = "apcurium",
                Password = "Apcurium52!",
                Team = "MoveOn",
                AppId = "com.moveonsoftware.timeon",
                AdHoc = true
            };

            var urlEncodedContent = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                {"Username", data.Username},
                {"Password", data.Password},
                {"Team", data.Team},
                {"AppId", data.AppId},
                {"AdHoc", data.AdHoc.ToString()}
            });

            var result = client.PostAsync("admin/appleDevCenter/downloadProfile", urlEncodedContent).Result;
            try
            {
                Console.WriteLine(result.Content.ReadAsAsync<HttpError>().Result.Message);
            }
            catch (Exception)
            {
            }
            Assert.IsTrue(result.IsSuccessStatusCode);
            Assert.AreEqual("81025B30-7AC0-4E7D-AD62-FEB816A58E99.mobileprovision", result.Content.Headers.ContentDisposition.FileName);
            Assert.Greater(result.Content.Headers.ContentLength, 0, "Content-Length is 0");
        }

        [Test]
        public void When_downloading_appstore_profile_we_get_file()
        {
            var data = new DownloadProvisioningProfileRequest
            {
                Username = "apcurium",
                Password = "Apcurium52!",
                Team = "MoveOn",
                AppId = "com.moveonsoftware.timeon",
                AdHoc = false
            };

            var urlEncodedContent = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                {"Username", data.Username},
                {"Password", data.Password},
                {"Team", data.Team},
                {"AppId", data.AppId},
                {"AdHoc", data.AdHoc.ToString()}
            });

            var result = client.PostAsync("admin/appleDevCenter/downloadProfile", urlEncodedContent).Result;
            try
            {
                Console.WriteLine(result.Content.ReadAsAsync<HttpError>().Result.Message);
            }
            catch (Exception)
            {
            }
            Assert.IsTrue(result.IsSuccessStatusCode);
            Assert.AreEqual("TimeOn_AppStore.mobileprovision", result.Content.Headers.ContentDisposition.FileName);
            Assert.Greater(result.Content.Headers.ContentLength, 0, "Content-Length is 0");
        }

        [Test]
        public void When_using_bad_credentials_expects_bad_request()
        {
            var data = new TestAppleDevCenterLoginRequest
            {
                Username = "apcurium",
                Password = "abc123456"
            };

            var urlEncodedContent =
                new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    {"Username", data.Username},
                    {"Password", data.Password},
                    {"Team", data.Team}
                });

            var result = client.PostAsync("admin/appleDevCenter/testLogin", urlEncodedContent).Result;
            Console.WriteLine(result);

            Assert.IsFalse(result.IsSuccessStatusCode);
            Assert.AreEqual("Invalid login information", result.Content.ReadAsAsync<HttpError>().Result.Message);
        }

        [Test]
        public void When_using_good_credentials_expects_ok()
        {
            var data = new TestAppleDevCenterLoginRequest
            {
                Username = "apcurium",
                Password = "Apcurium52!",
                Team = "Baventure Group Inc."
            };

            var urlEncodedContent =
                new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    {"Username", data.Username},
                    {"Password", data.Password},
                    {"Team", data.Team}
                });

            var result = client.PostAsync("admin/appleDevCenter/testLogin", urlEncodedContent).Result;
            Console.WriteLine(result);

            Assert.IsTrue(result.IsSuccessStatusCode);
        }

        [Test]
        public void When_using_good_credentials_with_bad_team_expects_bad_request()
        {
            var data = new TestAppleDevCenterLoginRequest
            {
                Username = "apcurium",
                Password = "Apcurium52!",
                Team = "BOB"
            };

            var urlEncodedContent =
                new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    {"Username", data.Username},
                    {"Password", data.Password},
                    {"Team", data.Team}
                });

            var result = client.PostAsync("admin/appleDevCenter/testLogin", urlEncodedContent).Result;
            Console.WriteLine(result);

            Assert.IsFalse(result.IsSuccessStatusCode);
            Assert.AreEqual("Team does not exist for this user", result.Content.ReadAsAsync<HttpError>().Result.Message);
        }

        [Test]
        public void When_using_good_credentials_with_no_team_when_team_is_required_expects_bad_request()
        {
            var data = new TestAppleDevCenterLoginRequest
            {
                Username = "apcurium",
                Password = "Apcurium52!"
            };

            var urlEncodedContent =
                new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    {"Username", data.Username},
                    {"Password", data.Password},
                    {"Team", data.Team}
                });

            var result = client.PostAsync("admin/appleDevCenter/testLogin", urlEncodedContent).Result;
            Console.WriteLine(result);

            Assert.IsFalse(result.IsSuccessStatusCode);
            Assert.AreEqual("Missing team information", result.Content.ReadAsAsync<HttpError>().Result.Message);
        }
    }
}