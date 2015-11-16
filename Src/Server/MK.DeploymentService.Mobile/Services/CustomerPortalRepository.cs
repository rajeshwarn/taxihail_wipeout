using System;
using System.Configuration;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using System.Collections.Generic;
using MK.DeploymentService.Mobile;
using log4net;
using ServiceStack.Common.Web;
using System.Linq;
using System.Threading.Tasks;
using MK.DeploymentService.Mobile.Helper;

namespace DeploymentServiceTools
{
	public class CustomerPortalRepository
    {
		public string CreateNewVersion(string companyKey, string versionNumber, string websiteUrl, DeployInfo deployment)
		{
			versionNumber = versionNumber.Replace("[Bitbucket]", string.Empty).Replace(" ", string.Empty);

			if (websiteUrl.Contains ("staging.taxihail.com")) 
            {
				versionNumber = versionNumber + ".staging";
			}

			var data = new
			{
				CompanyKey = companyKey,
				VersionNumber = versionNumber,
				WebsiteUrl = websiteUrl.Replace ("/api/", string.Empty)
			};

			using (var multipartFormDataContent = new MultipartFormDataContent())
			{
				multipartFormDataContent.Add(new StringContent(JsonConvert.SerializeObject(data)), "data");

				if (deployment.iOSAdhocFileExist) {
					var ipaContent = new StreamContent(deployment.GetiOSAdhocStream());
					ipaContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
					ipaContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment") { FileName = deployment.iOSAdhocFileName };
					multipartFormDataContent.Add(ipaContent);
				}

				if (deployment.iOSAppStoreFileExist) {
					var ipaAppStoreContent = new StreamContent(deployment.GetiOSAppStoreStream());
					ipaAppStoreContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
					ipaAppStoreContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment") { FileName = deployment.iOSAppStoreFileName };
					multipartFormDataContent.Add(ipaAppStoreContent);
				}

				if (deployment.AndroidApkFileExist) {
					var apkContent = new StreamContent(deployment.GetAndroidApkStream());
					apkContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
					apkContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment") { FileName = deployment.AndroidApkFileName };
					multipartFormDataContent.Add(apkContent);
				}

			    if (deployment.CallboxApkFileExist)
			    {
                    var apkContent = new StreamContent(deployment.GetCallboxApkStream());
                    apkContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                    apkContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment") { FileName = deployment.CallboxApkFileName };
                    multipartFormDataContent.Add(apkContent);
                }

                using (var client = CustomerPortalHttpClientProvider.Get())
                {
                    var result = client.PostAsync("admin/version", multipartFormDataContent).Result;

                    return result.IsSuccessStatusCode
                        ? string.Format("Version {0} created for company {1}", versionNumber, companyKey)
                        : string.Format("Version could not be created: HttpError: {0}", result.Content.ReadAsStringAsync().Result);
                }
			}
		}

		public async Task<string> DownloadProfile(string appleUsername, string applePassword, string appleTeam, string appId, bool isAdHoc)
		{
			var urlEncodedContent = new FormUrlEncodedContent(new Dictionary<string, string>
				{
					{"Username", appleUsername},
					{"Password", applePassword},
					{"Team", appleTeam},
					{"AppId", appId},
					{"AdHoc", isAdHoc.ToString()}
				});

            using (var client = CustomerPortalHttpClientProvider.Get())
            {
                var result = await client.PostAsync("admin/appleDevCenter/downloadProfile", urlEncodedContent);
                if (result.IsSuccessStatusCode)
                {
                    try
                    {
                        var userPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                        var filename = GetFilename(result);
                        var savePath = Path.Combine(userPath, "Library/MobileDevice/Provisioning Profiles", filename);

                        if (File.Exists(savePath))
                        {
                            return string.Format("Provisioning profile {0} for {1} already exists", filename, appId);
                        }

                        using (Stream contentStream = await result.Content.ReadAsStreamAsync(),
                            stream = new FileStream(savePath, FileMode.Create, FileAccess.Write, FileShare.None))
                        {
                            await contentStream.CopyToAsync(stream);
                        }

                        return string.Format("Downloaded and installed profile {0} for {1}", filename, appId);
                    }
                    catch (Exception e)
                    {
                        return string.Format("Could not download/install provisioning profile, continuing...{0}{1}", Environment.NewLine, e);
                    }
                }
                else
                {
                    try
                    {
                        return string.Format("Downloading profile: StatusCode:{0} Message:{1}", result.StatusCode, result.Content.ReadAsAsync<HttpError>().Result.Message);
                    }
                    catch
                    {
                        return result.ToString();
                    }
                }
            }
		}

		private string GetFilename(HttpResponseMessage result)
		{
			string fileName = string.Empty;
			var contentDisposition = result.Headers.GetValues("Content-Disposition").First();
			if (!string.IsNullOrEmpty(contentDisposition))
			{
				string lookFor = "filename=";
				int index = contentDisposition.IndexOf(lookFor, StringComparison.CurrentCultureIgnoreCase);
				if (index >= 0)
					fileName = contentDisposition.Substring(index + lookFor.Length);
			}

			return fileName;
		}
	}
}