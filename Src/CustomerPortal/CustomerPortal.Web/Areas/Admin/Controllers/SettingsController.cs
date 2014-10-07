#region

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;
using System.Web.Routing;
using CustomerPortal.Web.Android;
using CustomerPortal.Web.Areas.Admin.Models;
using CustomerPortal.Web.Entities;
using CustomerPortal.Web.Helpers;
using CustomerPortal.Web.Services.Impl;
using MongoDB.Bson;
using MongoRepository;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Web.Script.Serialization;
using CustomerPortal.Web.Services;
using System.Globalization;

#endregion

namespace CustomerPortal.Web.Areas.Admin.Controllers
{
    [Authorize(Roles = RoleName.Admin)]
    public class SettingsController : CompanyControllerBase
    {
        private readonly KeystoreGenerator _keystoreGenerator;

        public SettingsController(IRepository<Company> repository, KeystoreGenerator keystoreGenerator)
            : base(repository)
        {
            _keystoreGenerator = keystoreGenerator;
        }

        public SettingsController()
            : this(new MongoRepository<Company>(), new KeystoreGenerator())
        {
        }

        public ActionResult SetClientSettings(string id)
        {
            var clientSettings = new[]
            {
                "AndroidSigningKeyAlias", "AndroidSigningKeyPassStorePass", "ApplicationName", "AppName",
                "CanChangeServiceUrl", "DefaultPhoneNumber", "DefaultPhoneNumberDisplay", "ErrorLog", "ErrorLogEnabled",
                "FacebookAppId", "FacebookEnabled", "GoogleMapKey", "Package", "PayByCreditCardEnabled", "SiteUrl",
                "StreetNumberScreenEnabled", "SupportEmail", "TutorialEnabled", "TwitterAccessTokenUrl",
                "TwitterAuthorizeUrl", "TwitterCallback", "TwitterConsumerKey", "TwitterConsumerSecret",
                "TwitterEnabled", "TwitterRequestTokenUrl", "ServiceUrl"
            };
            var company = Repository.GetById(id);

            foreach (var setting in company.CompanySettings)
            {
                setting.IsClientSetting = clientSettings.Contains(setting.Key);
            }

            foreach (var clientSetting in clientSettings)
            {
                var setting = company.CompanySettings.FirstOrDefault(c => c.Key == clientSetting);
                if (setting == null)
                {
                    company.CompanySettings.Add(new CompanySetting
                    {
                        Key = clientSetting,
                        Value = "",
                        IsClientSetting = true
                    });
                }
            }
            Repository.Update(company);

            return RedirectToAction("Index", new { id });
        }

        public ActionResult Reset(string id)
        {
            var source = Repository.Single(c => c.CompanyKey == "TaxiHailDemo");
            var dest = Repository.Single(c => c.Id == id);

            dest.CompanySettings = source.CompanySettings;
            Repository.Update(dest);


            return RedirectToAction("Index", new { id });
        }



        public ActionResult InitWebTheme(string id)
        {
            var templateCompany = Repository.First(c => c.CompanyKey == "TaxiHailDemo");
            var sourceFiles = new WebThemeFilesManager(templateCompany.Id).GetAll();

            var destination = new WebThemeFilesManager(id).GetFolderPath();
            if (!Directory.Exists(destination))
            {
                Directory.CreateDirectory(destination);
            }

            foreach (var sourceFile in sourceFiles)
            {

                var fileName = Path.GetFileName(sourceFile);
                if (fileName.ToLower() != "logo.png")
                {
                    var destFile = Path.Combine(destination, fileName);

                    if (!System.IO.File.Exists(destFile))
                    {
                        System.IO.File.Copy(sourceFile, destFile);
                    }
                }



            }

            var logo = new AssetsManager(id).GetAll().FirstOrDefault(f => Path.GetFileName(f).ToLower() == "logo.png");
            if (!string.IsNullOrEmpty(logo))
            {
                var logoDestination = Path.Combine(destination, "logo.png");
                if (!System.IO.File.Exists(logoDestination))
                {
                    System.IO.File.Copy(logo, logoDestination);
                }
            }


            logo = new AssetsManager(id).GetAll().FirstOrDefault(f => Path.GetFileName(f).ToLower() == "logo_1_5.png");
            if (!string.IsNullOrEmpty(logo))
            {
                var logoDestination = Path.Combine(destination, "logo.png");
                if (!System.IO.File.Exists(logoDestination))
                {
                    System.IO.File.Copy(logo, logoDestination);
                }
            }

            var emailLogo = new AssetsManager(id).GetAll().FirstOrDefault(f => Path.GetFileName(f).ToLower() == "email_logo.png");
            if (!string.IsNullOrEmpty(emailLogo))
            {
                var logoDestination = Path.Combine(destination, "email_logo.png");
                if (!System.IO.File.Exists(logoDestination))
                {
                    System.IO.File.Copy(emailLogo, logoDestination);
                }
            }

            return RedirectToAction("Index", "Home", new { area = "Customer", companyId = id });
        }
        [HttpPost]
        public ActionResult InitWithCity(string id, string city = null)
        {
            return RedirectToAction("Init", new { id = id, city = city });

        }
        public ActionResult Init(string id, string city = null)
        {
            CityInfo cityInfo = null;
            if (!string.IsNullOrEmpty(city))
            {
                cityInfo = new GoogleApi().GetCityInfo(city);
            }

            var company = Repository.GetById(id);
            if (company == null) return HttpNotFound();


            var model = new InitSettingsModel { Company = company };

            model.CityInfo = cityInfo == null ? "" : cityInfo.Name;

            model.Settings = new Dictionary<string, Value>();

            model.Settings.Add("Direction.FlateRate", new Value(
                company.Application.FlagDropRate.HasValue ? company.Application.FlagDropRate.Value.ToString() : "2.25", false));

            if (company.Application.UnitOfLength == UnitOfLength.Kilometers)
            {
                model.Settings.Add("Direction.RatePerKm", new Value(
                    company.Application.MileageRate.HasValue ? company.Application.MileageRate.Value.ToString() : "1.25", false));
            }
            else
            {
                model.Settings.Add("Direction.RatePerKm", new Value(
                    company.Application.MileageRate.HasValue
                        ? (Convert.ToDouble(company.Application.MileageRate.Value) * 0.390625).ToString()
                        : "1.25", false)); // Convertion in the questionnaire is invalid, this fixes the issue
            }
            model.Settings.Add("DistanceFormat", new Value(
                company.Application.UnitOfLength == UnitOfLength.Kilometers ? "KM" : "MILE", false));

            model.Settings.Add("GeoLoc.DefaultLatitude", new Value(cityInfo != null ? cityInfo.Center.Latitude.ToString(CultureInfo.InstalledUICulture) : "", true));
            model.Settings.Add("GeoLoc.DefaultLongitude", new Value(cityInfo != null ? cityInfo.Center.Longitude.ToString(CultureInfo.InstalledUICulture) : "", true));

            if (cityInfo == null)
            {
                model.Settings.Add("GeoLoc.SearchFilter", new Value(@"{0},ottawa,on,canada&region=ca", true));
                model.Settings.Add("Client.LowerLeftLatitude", new Value("0", true));
                model.Settings.Add("Client.LowerLeftLongitude", new Value("0", true));
                model.Settings.Add("Client.UpperRightLatitude", new Value("0", true));
                model.Settings.Add("Client.UpperRightLongitude", new Value("0", true));
            }
            else
            {
                model.Settings.Add("GeoLoc.SearchFilter", new Value(@"{0}" + string.Format(",{0}&bounds={1},{2}|{3},{4}", cityInfo.Name.Replace(" ", "+"), cityInfo.SouthwestCoordinate.Latitude, cityInfo.SouthwestCoordinate.Longitude, cityInfo.NortheastCoordinate.Latitude, cityInfo.NortheastCoordinate.Longitude), true));
                model.Settings.Add("Client.LowerLeftLatitude", new Value(cityInfo.SouthwestCoordinate.Latitude.ToString(CultureInfo.InvariantCulture), true));
                model.Settings.Add("Client.LowerLeftLongitude", new Value(cityInfo.SouthwestCoordinate.Longitude.ToString(CultureInfo.InvariantCulture), true));
                model.Settings.Add("Client.UpperRightLatitude", new Value(cityInfo.NortheastCoordinate.Latitude.ToString(CultureInfo.InvariantCulture), true));
                model.Settings.Add("Client.UpperRightLongitude", new Value(cityInfo.NortheastCoordinate.Longitude.ToString(CultureInfo.InvariantCulture), true));
            }

            model.Settings.Add("DefaultPhoneNumber", new Value(
                string.IsNullOrEmpty(company.Application.CompanyPhoneNumber)
                    ? ""
                    : company.Application.CompanyPhoneNumber.Replace("-", ""), true));
            model.Settings.Add("DefaultPhoneNumberDisplay", new Value(company.Application.CompanyPhoneNumber, true));

            //company.CompanyKey

            model.Settings.Add("APNS.ProductionCertificatePath", new Value(
                string.Format("../../Certificates/{0}.p12", company.CompanyKey), false));

            model.Settings.Add("GCM.PackageName", new Value(string.Format("com.apcurium.MK.{0}", company.CompanyKey), false));
            model.Settings.Add("Receipt.Note", new Value("Thank You!<br>" + company.Application.AppName, false));

            if (cityInfo == null)
            {
                model.Settings.Add("IBS.TimeDifference", new Value("0", true));
                
            }
            else
            {
                model.Settings.Add("IBS.TimeDifference", new Value((-1 * (TimeSpan.FromHours(-5).Ticks - cityInfo.TimeDifference.Ticks)).ToString(), true));
                
            }


            model.Settings.Add("TaxiHail.ApplicationName", new Value(company.Application.AppName, true));
            model.Settings.Add("TaxiHail.ApplicationKey", new Value(company.CompanyKey, true));
            model.Settings.Add("TaxiHail.AccentColor", new Value("#0057a3", false));
            model.Settings.Add("TaxiHail.EmailFontColor", new Value("#000", false));
            model.Settings.Add("TaxiHail.SiteName", new Value(company.CompanyKey, true));

            model.Settings.Add("AndroidSigningKeyAlias", new Value("MK" + company.CompanyKey, false));
            model.Settings.Add("AndroidSigningKeyPassStorePass", new Value(string.Format("mk{0}0001.", company.CompanyKey), false));

            model.Settings.Add("ApplicationName", new Value(company.Application.AppName, true));
            model.Settings.Add("AppName", new Value(company.Application.AppName, true));



            model.Settings.Add("Package", new Value("com.apcurium.MK." + company.CompanyKey, false));

            model.Settings.Add("Client.AboutUsUrl", new Value(company.Application.AboutUsLink, true));


            model.Settings.Add("SupportEmail", new Value(company.Application.SupportContactEmail, true));
            model.Settings.Add("Client.SupportEmail", new Value(company.Application.SupportContactEmail, true));


            return View(model);
        }



        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Init(string id, InitSettingsModel model)
        {
            var company = Repository.GetById(id);
            company.CompanySettings = new List<CompanySetting>();


            var path = HostingEnvironment.MapPath("~/assets/DefaultSettings/Common.json");
            if (System.IO.File.Exists(path))
            {
                var json = System.IO.File.ReadAllText(path);
                var settings = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);

                var d = new Dictionary<string, Value>();
                foreach (var setting in settings)
                {
                    d.Add(setting.Key, new Value(setting.Value, true));
                }

                AddOrUpdateSettings(d, company);
            }

            AddOrUpdateSettings(model.Settings, company);

            UpdateCompany(company);

            return RedirectToAction("Index", "Home", new { area = "Customer", companyId = id });

        }

        private static void AddOrUpdateSettings(Dictionary<string, Value> settings, Company company)
        {
            foreach (var setting in settings)
            {
                if (company.CompanySettings.Any(s => s.Key == setting.Key))
                {
                    company.CompanySettings.Single(s => s.Key == setting.Key).Value = setting.Value.StringValue;
                }
                else
                {
                    company.CompanySettings.Add(new CompanySetting { Key = setting.Key, Value = setting.Value.StringValue });
                }
            }
        }



        public ActionResult Edit(string id)
        {
            var company = Repository.GetById(id);

            if (company == null) return HttpNotFound();

            return PartialView(company);
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Edit(string id, FormCollection form)
        {
            var company = Repository.GetById(id);
            foreach (var key in form.AllKeys.Where(k => k.StartsWith("KEY_")))
            {
                var value = form[key];
                var k = key.Substring(4, key.Length - 4);

                company.CompanySettings.First(s => s.Key == k).Value = value;

            }

            if (form.AllKeys.Contains("NKEY1_Key") && !string.IsNullOrWhiteSpace(form["NKEY1_Key"]))
            {
                var key = form["NKEY1_Key"];
                var value = form["NKEY1_Value"];
                var client = form["NKEY1_Client"];
                company.CompanySettings.Add(new CompanySetting
                {
                    Key = key,
                    Value = value,
                    IsClientSetting = client.Contains("true")
                });
            }

            if (form.AllKeys.Contains("NKEY2_Key") && !string.IsNullOrWhiteSpace(form["NKEY2_Key"]))
            {
                var key = form["NKEY2_Key"];
                var value = form["NKEY2_Value"];
                var client = form["NKEY2_Client"];
                company.CompanySettings.Add(new CompanySetting
                {
                    Key = key,
                    Value = value,
                    IsClientSetting = client.Contains("true")
                });
            }


            UpdateCompany(company);
            return RedirectToAction("Index", "Home", new { area = "Customer", companyId = id });
        }

        private void UpdateCompany(Company company)
        {

            var clientSettings = new[]
            {
                "ApplicationName", "AppName",
                "CanChangeServiceUrl", "DefaultPhoneNumber", "DefaultPhoneNumberDisplay", "ErrorLog", "ErrorLogEnabled",
                "FacebookAppId", "FacebookEnabled", "TwitterAccessTokenUrl",
                "TwitterAuthorizeUrl", "TwitterCallback", "TwitterConsumerKey", "TwitterConsumerSecret",
                "TwitterEnabled", "TwitterRequestTokenUrl", "ServiceUrl"
            };

            foreach (var setting in company.CompanySettings)
            {
                if (clientSettings.Contains(setting.Key))
                {
                    setting.IsClientSetting = true;
                }
            }

            company.CompanySettings.OrderBy(x => x.IsClientSetting).ThenBy(x => x.Key);

            Repository.Update(company);
        }

        public ActionResult DefaultSettings()
        {
            var settings = new MongoRepository<DefaultCompanySetting>();

            return View(settings.OrderBy(s => s.Id));
        }

        public ActionResult ApplyDefaultsToCompanies()
        {
            var defaultSettings = new MongoRepository<DefaultCompanySetting>();
            var companies = new MongoRepository<Company>();

            foreach (var company in companies)
            {
                foreach (var setting in company.CompanySettings)
                {
                    setting.IsClientSetting = false;
                }

                foreach (var defaultSetting in defaultSettings)
                {
                    if (defaultSetting.Id.Contains("Facebook"))
                    {
                        defaultSetting.Key.ToString();
                    }
                    var setting = company.CompanySettings.SingleOrDefault(c => c.Key == defaultSetting.Id);
                    if (setting != null)
                    {
                        company.CompanySettings.Remove(setting);
                    }
                }

                var clientKeys = new string[]
                {
                    "ApplicationName", "ServiceUrl", "CanChangeServiceUrl", "ErrorLog", "ErrorLogEnabled", "FacebookAppId",
                    "FacebookEnabled", "ServiceUrlTest", "TwitterAccessTokenUrl", "TwitterAuthorizeUrl",
                    "TwitterCallback", "TwitterConsumerKey", "TwitterConsumerSecret", "TwitterEnabled",
                    "TwitterRequestTokenUrl"
                };

                foreach (var clientKey in clientKeys)
                {
                    var setting = company.CompanySettings.SingleOrDefault(c => c.Key == clientKey);
                    if (setting == null)
                    {
                        if (!defaultSettings.Any(s => s.Id == clientKey))
                        {
                            company.CompanySettings.Add(new CompanySetting { Key = clientKey, IsClientSetting = true });
                        }
                    }
                    else
                    {
                        setting.IsClientSetting = true;
                    }

                }
                companies.Update(company);
            }
            return RedirectToAction("DefaultSettings");
        }




        public ActionResult Delete(string id, string key, bool isClient)
        {
            var company = Repository.GetById(id);
            if (company == null) return HttpNotFound();
            company.CompanySettings.Remove(company.CompanySettings.Single(s => s.Key == key && s.IsClientSetting == isClient));
            Repository.Update(company);
            TempData["message"] = String.Format("Setting with the key '{0}' was successfully deleted from {1}",
                key, company.CompanyName);

            return RedirectToAction("Index", "Home", new { area = "Customer", companyId = id });
        }


        public ActionResult WebTheme(string id)
        {

            var company = Repository.GetById(id);
            if (company == null) return HttpNotFound();

            var model = new FileModel
            {
                CompanyName = company.CompanyName,
                Files = new WebThemeFilesManager(id).GetAll().Select(Path.GetFileName)
            };
            model.Company = company;

            return PartialView(model);
        }

        public ActionResult Color(string id)
        {

            var company = Repository.GetById(id);
            if (company == null) return HttpNotFound();


            var model = new ColorModel
            {
                Company = company,
                CompanyColor = string.IsNullOrEmpty(company.Style.CompanyColor) ? "" : company.Style.CompanyColor.TrimStart('#'),
                TitleColor = string.IsNullOrEmpty(company.Style.TitleColor) ? "" : company.Style.TitleColor.TrimStart('#'),
                MenuColor = string.IsNullOrEmpty(company.Style.MenuColor) ? "" : company.Style.MenuColor.TrimStart('#'),
                WebAccentColor = string.IsNullOrEmpty(company.Style.WebAccentColor) ? "" : company.Style.WebAccentColor.TrimStart('#'),
                LoginColor = string.IsNullOrEmpty(company.Style.LoginColor) ? "" : company.Style.LoginColor.TrimStart('#'),
                EmailFontColor = string.IsNullOrEmpty(company.Style.EmailFontColor) ? "" : company.Style.EmailFontColor.TrimStart('#'),
            };
            model.Company = company;

            return PartialView(model);
        }

        [HttpPost]
        public ActionResult Color(string id, ColorModel model)
        {
            var company = Repository.GetById(id);
            if (company == null) return HttpNotFound();

            if (!string.IsNullOrEmpty(model.WebAccentColor))
            {
                new WebThemeFilesManager(id).SetAccentColor(ColorHelper.ColorFromHex("#FF" + model.WebAccentColor));
                company.Style.WebAccentColor = "#" + model.WebAccentColor;
            }
            if (!string.IsNullOrEmpty(model.EmailFontColor))
            {

                new WebThemeFilesManager(id).SetEmailFontColor(ColorHelper.ColorFromHex("#00" + model.EmailFontColor));
                company.Style.EmailFontColor = "#" + model.EmailFontColor;
            }

            if (!string.IsNullOrEmpty(model.CompanyColor))
            {
                new AssetsManager(id).SetStyleNavigationBarColor(
                    ColorHelper.ColorFromHex("#FF" + model.CompanyColor));

                company.Style.CompanyColor = "#" + model.CompanyColor;
            }

            if (!string.IsNullOrEmpty(model.MenuColor))
            {
                company.Style.MenuColor = "#" + model.MenuColor;
            }

            if (!string.IsNullOrEmpty(model.TitleColor))
            {

                new AssetsManager(id).SetStyleNavigationTitleBarColor(
                    ColorHelper.ColorFromHex("#FF" + model.TitleColor));

                company.Style.TitleColor = "#" + model.TitleColor;
            }

            if (!string.IsNullOrEmpty(model.LoginColor))
            {
                company.Style.LoginColor = "#" + model.LoginColor;
            }

          

            Repository.Update(company);




            return RedirectToAction("Index", "Home", new { area = "Customer", companyId = id });
        }





        public ActionResult Assets(string id)
        {
            var company = Repository.GetById(id);
            if (company == null) return HttpNotFound();

            var model = new FileModel
            {
                CompanyName = company.CompanyName,
                Files = new AssetsManager(id).GetAll().Select(Path.GetFileName)
            };
            model.Company = company;

            return PartialView(model);
        }

        [HttpPost]
        public ActionResult Assets(string id, string type, IEnumerable<HttpPostedFileBase> files)
        {
            if (files != null)
            {
                var company = Repository.GetById(id);
                if (company == null) return HttpNotFound();

                var fileManager = GetFileManager(type, id);

                foreach (HttpPostedFileBase file in files)
                {
                    fileManager.Save(file);
                }
            }

            return RedirectToAction("Index", "Home", new { area = "Customer", companyId = id });
        }

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);

            var companyId = (string)filterContext.RouteData.Values["companyId"];
            filterContext.ToString();
        }


        public ActionResult Image(string id, string filename, string type)
        {
            string filepath;
            var fileManager = GetFileManager(type, id);

            if (!fileManager.Exists(filename, out filepath))
            {
                return HttpNotFound();
            }

            var mimeType = MimeMapping.GetMimeMapping(filepath);
            return File(filepath, mimeType);
        }

        [HttpPost]
        public ActionResult DeleteFile(string id, string path, string type)
        {
            var company = Repository.GetById(id);
            if (company == null) return HttpNotFound();
            if (path != null)
            {
                GetFileManager(type, id).Delete(path);
            }

            return RedirectToAction("Index", "Home", new { area = "Customer", companyId = id });
        }

        public ActionResult DeleteSelected(string id, string[] path, string type)
        {
            var company = Repository.GetById(id);
            if (company == null) return HttpNotFound();
            if (path != null && path.Length != 0)
            {
                for (int i = 0; i < path.Length; i++)
                {
                    GetFileManager(type, id).Delete(path[i]);
                }

            }
            return RedirectToAction("Index", "Home", new { area = "Customer", companyId = id });
        }

        public ActionResult GenerateKeystore(string type, string id)
        {
            try
            {
                _keystoreGenerator.Generate(id, GetFileManager(type, id).GetFolderPath());
            }
            catch (Exception e)
            {
                TempData["warning"] = e.Message;
            }

            return RedirectToAction("Index", "Home", new { area = "Customer", companyId = id });
        }

        private FileManagerBase GetFileManager(string type, string id)
        {
            switch (type)
            {
                case "assets":
                    return new AssetsManager(id);
                case "webtheme":
                    return new WebThemeFilesManager(id);
            }
            throw new ArgumentException("file manager type not recognized");
        }
    }
}