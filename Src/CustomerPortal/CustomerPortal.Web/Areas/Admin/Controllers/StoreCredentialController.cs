using System.Linq;
using System.Web.Mvc;
using CustomerPortal.Web.Areas.Admin.Models;
using CustomerPortal.Web.Entities;
using MongoRepository;
using System.Net;
using System.IO;
using System;

namespace CustomerPortal.Web.Areas.Admin.Controllers
{
    public class StoreCredentialController : Controller
    {
        private readonly MongoRepository<Company> repository;

        public StoreCredentialController()
        {
            repository = new MongoRepository<Company>();
        }

        //
        // GET: /Admin/Apple/

        public ActionResult Index()
        {
            return View(repository.Where(x=>x.Status != AppStatus.DemoSystem ).OrderBy(x => x.CompanyName).Select( c=> new StoreInfoModel{ Company = c}).ToArray());
        }



        public ActionResult RefreshStoreAvailability()
        {

            foreach (var company in repository.Where(c=>c.Status != AppStatus.DemoSystem))
            {
                if ( company.CompanySettings.Any(s=>s.Key.ToLower() == "package" ) )
                {
                    CheckIfAppExist(company);

                    if ( CheckIfAppExist(company ) )
                    {
                        company.Store.AndroidStoreUrl = "https://play.google.com/store/apps/details?id=" + company.CompanySettings.Single(s => s.Key.ToLower() == "package").Value;
                        repository.Update(company);
                    }
                    
                }

                if ( company.CompanySettings.Any(s=>s.Key.ToLower() == "package" )  )
                {
                    var app = GetAppleAppInfo( company.CompanySettings.Single(s => s.Key.ToLower() == "package" ).Value );
                    if ( app != null )
                    {
                        company.Store.AppleStoreId = app.trackId.ToString();
                        company.Store.AppleStoreUrl = app.trackViewUrl;
                        company.Store.PublishedDate = DateTime.Parse( app.releaseDate.Split('T')[0] );
                        repository.Update(company);
                    }

                }
            }
            
            return RedirectToAction("Index");
        }

        private static bool CheckIfAppExist(Company company)
        {
            try
            {
                var encode = System.Text.Encoding.GetEncoding("utf-8");
                string str = "https://play.google.com/store/apps/details?id=" + company.CompanySettings.Single(s => s.Key.ToLower() == "package").Value;
                var wrquest = (HttpWebRequest)WebRequest.Create(str);
                var getresponse = (HttpWebResponse)wrquest.GetResponse();
                var objStream = getresponse.GetResponseStream();
                var objSR = new StreamReader(objStream, encode, true);
                string strResponse = objSR.ReadToEnd();
                return true;
            }
            catch( WebException ex )
            {
                return false;
            }
        }


        public AppInfo GetAppleAppInfo(string bundleId)
        {
            try
            {
                var encode = System.Text.Encoding.GetEncoding("utf-8");
                var searchByBundleId = "http://itunes.apple.com/en/lookup?bundleId=" + bundleId;
                var wrquest = (HttpWebRequest)WebRequest.Create(searchByBundleId);
                var getresponse = (HttpWebResponse)wrquest.GetResponse();
                var objStream = getresponse.GetResponseStream();
                var objSR = new StreamReader(objStream, encode, true);
                string strResponse = objSR.ReadToEnd();

                var result = Newtonsoft.Json.JsonConvert.DeserializeObject<AppleSearchResult>(strResponse);

                if (result.resultCount == 1)
                {
                    return result.results.First();
                }

                return null;
            }
            catch (WebException ex)
            {
                return null;
            }
        }

        public ActionResult CheckAppleCredentials(string id)
        {
            var company = repository.GetById(id);
            CheckAppleCredentials(company);

            return RedirectToAction("Index");
        }
        //
        // GET: /Admin/Apple/Details/5

        private void CheckAppleCredentials(Company company)
        {
            var a = new Cupertino.Agent();

            var errors = company.Errors.Keys.Where(k => k.Contains("iOSError")).ToArray();
            foreach (var error in errors)
            {
                company.Errors.Remove(error);
            }

            if (!string.IsNullOrWhiteSpace(company.AppleAppStoreCredentials.Username) &&
                !string.IsNullOrWhiteSpace(company.AppleAppStoreCredentials.Password))
            {
                var r = a.Login(company.AppleAppStoreCredentials.Username, company.AppleAppStoreCredentials.Password, company.AppleAppStoreCredentials.Team);
                if (r.IsSuccessful)
                {
                    if (company.CompanySettings.Any(s => s.Key == "Package"))
                    {
                        var response = a.GetDevicesOfProfile(company.AppleAppStoreCredentials.Username,
                                                             company.AppleAppStoreCredentials.Password,
                                                             company.AppleAppStoreCredentials.Team,
                                                             company.CompanySettings.First(s => s.Key == "Package").Value);

                        if (!response.IsSuccessful)
                        {
                            company.Errors.Add("iOSError.ResponseErrorMessage", response.ErrorMessage);
                        }
                        else
                        {
                            foreach (var device in new MongoRepository<IosDevice>())
                            {
                                if (!response.DeviceUDIDs.Contains(device.Id))
                                {
                                    if (!company.Errors.ContainsKey("iOSError.MissingDefault" + device.Id))
                                    {
                                        company.Errors.Add("iOSError.MissingDefault" + device.Id, "Missing Default iOS device  : " + device.Name);
                                    }
                                }
                            }

                            foreach (var device in company.Store.UniqueDeviceIdentificationNumber)
                            {
                                if (!response.DeviceUDIDs.Contains(device))
                                {
                                    if (!company.Errors.ContainsKey("iOSError.MissingCustomer" + device))
                                    {
                                        company.Errors.Add("iOSError.MissingCustomer" + device, "Missing Customer iOS device  : " + device);
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        company.Errors.Add("iOSError.NoAppId", "No iOS AppId");
                    }
                }
                else
                {
                    company.Errors.Add("iOSError.InvalidLogin", "Cannot login to Apple : " + r.ErrorMessage);
                }
            }
            else
            {
                company.Errors.Add("iOSError.NotLoginInfo", "No Apple login info");
            }

            repository.Update(company);
        }
    }
}
