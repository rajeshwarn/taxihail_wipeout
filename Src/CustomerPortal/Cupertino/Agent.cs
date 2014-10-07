#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Cupertino.Model;
using HtmlAgilityPack;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

#endregion

namespace Cupertino
{
    public class Agent
    {
        private readonly BrowserSession _browserSession;

        public Agent()
        {
//	        _browserSession = new BrowserSession("192.168.12.118", 8888); //fiddler hook
            _browserSession = new BrowserSession();
        }

        public LoginResponse Login(string username, string password, string team)
        {
            HtmlNode.ElementsFlags["option"] = HtmlElementFlag.Closed;

            var response = new LoginResponse();

            var htmlContent =
                _browserSession.Get(
                    "https://daw.apple.com/cgi-bin/WebObjects/DSAuthWeb.woa/wa/login?&appIdKey=891bd3417a7776362562d2197f89480a8547b108fd934911bcbea0110d07f757&path=%2F%2Fmembercenter%2Flogin.action");
            var actionUrl = GetPostUrl(htmlContent);
            _browserSession.FormElements["theAccountName"] = username;
            _browserSession.FormElements["theAccountPW"] = password;
            htmlContent = _browserSession.Post("https://daw.apple.com" + actionUrl);

            if (IsLoginPage(htmlContent))
            {
                response.ErrorMessage = "Invalid login information";
                return response;
            }

            //verify the metadata of the response to see the redirect url
            var redirectUrl = GetRedirectUrl(htmlContent);
            htmlContent = _browserSession.Get(redirectUrl);

            if (IsSelectTeamPage(htmlContent))
            {
                if (string.IsNullOrWhiteSpace(team))
                {
                    response.ErrorMessage = "Missing team information";
                    return response;
                }

                htmlContent = SelectTeam(htmlContent, team);
                if (htmlContent == null)
                {
                    response.ErrorMessage = "Team does not exist for this user";
                    return response;
                }

                if (IsSelectTeamPage(htmlContent))
                {
                    response.ErrorMessage = "Problem selecting team";
                    return response;
                }
            }

            response.IsSuccessful = true;

            return response;
        }

        public DownloadProfileResponse DownloadProfile(string username, string password, string team, string appId, bool adHoc)
        {
            var response = new DownloadProfileResponse();

            var loginResponse = Login(username, password, team);
            if (loginResponse.IsSuccessful)
            {
                string errorMessage;
                var profile = GetProfile(appId, adHoc, out errorMessage);
                if (profile == null)
                {
                    response.ErrorMessage = errorMessage;
                    return response;
                }

                response.FileStream = _browserSession.GetDownload(profile.DownloadUrl);
                response.FileStream.Position = 0;
                response.FileName = profile.UUID + ".mobileprovision"; // we use the UUID so we can just put the file in ~/Libray/MobileDevice/Provisioning Profiles/ folder to install it
                response.IsSuccessful = true;
            }

            response.ErrorMessage = loginResponse.ErrorMessage;
            return response;
        }

        public GetDevicesOfProfileResponse GetDevicesOfProfile(string username, string password, string team, string appId)
        {
            var response = new GetDevicesOfProfileResponse();

            var loginResponse = Login(username, password, team);
            if (loginResponse.IsSuccessful)
            {
                string errorMessage;
                var profile = GetProfile(appId, true, out errorMessage);
                if (profile == null)
                {
                    response.ErrorMessage = errorMessage;
                    return response;
                }

                var htmlContent = _browserSession.Get(profile.EditUrl);
                var doc = new HtmlDocument();
                doc.LoadHtml(htmlContent);
                var devices = doc.DocumentNode.SelectNodes("//dd[@class='selectDevices']//div[@class='rows']//div//input");
                var checkedDevices = devices.Where(x => x.GetAttributeValue("checked", null) == "");
                var selectedDeviceIds = checkedDevices.Select(x => x.GetAttributeValue("value", ""));
                
                var registeredDevices = GetRegisteredDevices();
                response.DeviceUDIDs = registeredDevices.Devices.Where(x => selectedDeviceIds.Contains(x.DeviceId)).Select(x => x.DeviceNumber);
                response.IsSuccessful = true;
            }

            response.ErrorMessage = loginResponse.ErrorMessage;
            return response;
        }

        private ProvisioningProfile GetProfile(string appId, bool adHoc, out string errorMessage)
        {
            errorMessage = string.Empty;

            var htmlContent =
                    _browserSession.Get(
                        "https://developer.apple.com/account/ios/profile/profileList.action?type=production");

            var jsonDataUrl =
                Regex.Match(htmlContent, "profileDataURL = \"([^\"]*)\"")
                    .Value.Replace("profileDataURL = ", string.Empty)
                    .Replace("\"", string.Empty); // '/profileDataURL = "([^"]*)"/'
            jsonDataUrl += "&type=production";

            if (jsonDataUrl == "&type=production")
            {
                errorMessage = "There was a problem with the Apple Developer Center, please try again";
                return null;
            }

            var jsonContent = _browserSession.Post(jsonDataUrl);
            var response = JsonConvert.DeserializeObject<ProvisioningProfileList>(jsonContent);

            var profiles = response.ProvisioningProfiles.Where(x => x.AppId.Identifier == appId);
            if (!profiles.Any())
            {
                errorMessage = string.Format("No profile corresponding to identifier {0} found", appId);
                return null;
            }

            var activeProfiles = profiles.Where(x => x.Status == "Active").ToList();
            if (!activeProfiles.Any())
            {
                errorMessage = string.Format("No active profile corresponding to identifier {0} found", appId);
                return null;
            }

            ProvisioningProfile profile;
            if (adHoc)
            {
                var adHocProfiles = activeProfiles.Where(x => x.DeviceCount != 0).ToList();
                if (adHocProfiles.Count() > 1)
                {
                    errorMessage = string.Format("More than one active profile corresponding to identifier {0} found", appId);
                    return null;
                }

                profile = adHocProfiles.FirstOrDefault();
            }
            else
            {
                var prodProfiles = activeProfiles.Where(x => x.DeviceCount == 0).ToList();
                if (prodProfiles.Count() > 1)
                {
                    errorMessage = string.Format("More than one active profile corresponding to identifier {0} found", appId);
                    return null;
                }

                profile = prodProfiles.FirstOrDefault();
            }

            if (profile == null)
            {
                errorMessage = string.Format("No active profile for {1} corresponding to identifier {0} found", appId, adHoc ? "AdHoc" : "AppStore");
                return null;
            }

            return profile;
        }

        private RegisteredDevices GetRegisteredDevices()
        {
            var htmlContent =
                _browserSession.Get(
                    "https://developer.apple.com/account/ios/device/deviceList.action");

            var jsonDataUrl =
                Regex.Match(htmlContent, "deviceDataURL = \"([^\"]*)\"")
                    .Value.Replace("deviceDataURL = ", string.Empty)
                    .Replace("\"", string.Empty); // '/profileDataURL = "([^"]*)"/'

            var jsonContent = _browserSession.Post(jsonDataUrl);
            return JsonConvert.DeserializeObject<RegisteredDevices>(jsonContent);
        }

        private string SelectTeam(string htmlContent, string team)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(htmlContent);
            var form = doc.DocumentNode.SelectSingleNode("//form[@name='saveTeamSelection']");
            var actionUrl = form.GetAttributeValue("action", "");
            HtmlNode selectedTeam = null;
            try
            {
                var teams = doc.DocumentNode.SelectNodes("//select[@id='teams']//option");
                selectedTeam = teams.FirstOrDefault(x => x.InnerText.Contains(team));
            }
            catch (Exception)
            {
                var teams = doc.DocumentNode.SelectNodes("//p[@class='team-value']//label[@class='label-primary']");
                var selectedTeamLabel = teams.FirstOrDefault(x => x.InnerText.Contains(team));
                if (selectedTeamLabel != null)
                {
                    var selectedTeamId = selectedTeamLabel.GetAttributeValue("for", "");
                    selectedTeam =
                        doc.DocumentNode.SelectSingleNode(string.Format("//p[@class='team-value']//input[@id='{0}']",
                            selectedTeamId));
                }
            }

            if (selectedTeam == null)
            {
                return null;
            }

            _browserSession.FormElements["memberDisplayId"] = selectedTeam.Attributes["value"].Value;
            htmlContent = _browserSession.Post("https://developer.apple.com" + actionUrl);

            return htmlContent;
        }

        private string GetRedirectUrl(string htmlContent)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(htmlContent);
            var select = doc.DocumentNode.SelectNodes("//meta[contains(@content, 'URL')]");
            try
            {
                return select[0].Attributes["content"].Value.Split('=')[1];
            }
            catch
            {
                return string.Empty;
            }
        }

        private bool IsLoginPage(string htmlContent)
        {
            return GetTitle(htmlContent).Contains("Sign in with your Apple ID");
        }

        private bool IsSelectTeamPage(string htmlContent)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(htmlContent);
            var form = doc.DocumentNode.SelectSingleNode("//form[@name='saveTeamSelection']");

            return form != null;
        }

        private string GetPostUrl(string htmlContent)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(htmlContent);
            var form = doc.DocumentNode.SelectSingleNode("//form");
            return form.GetAttributeValue("action", "");
        }

        private string GetTitle(string htmlContent)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(htmlContent);
            var title = doc.DocumentNode.SelectSingleNode("//title");
            return title.InnerText;
        }
    }
}