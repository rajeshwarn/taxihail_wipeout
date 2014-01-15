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

namespace DeploymentServiceTools
{
	public class CustomerPortalRepository
    {
		private HttpClient client;

		public CustomerPortalRepository()
		{

			client = new HttpClient ();
			client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", 
				Convert.ToBase64String(
					System.Text.ASCIIEncoding.ASCII.GetBytes(
						string.Format("{0}:{1}", 
							ConfigurationManager.AppSettings["CustomerPortalUsername"], 
							ConfigurationManager.AppSettings["CustomerPortalPassword"]))));
			client.BaseAddress = new Uri(ConfigurationManager.AppSettings ["CustomerPortalUrl"]);
		}

		public string CreateNewVersion(string companyKey, string versionNumber, string websiteUrl, string ipaFileName, FileStream ipaFile, string apkFileName, FileStream apkFile)
		{
			versionNumber = versionNumber.Replace("[Bitbucket]", string.Empty).Replace(" ", string.Empty);

			if (websiteUrl.Contains ("staging.taxihail.com")) {
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

				if (!string.IsNullOrWhiteSpace (ipaFileName) && ipaFile != null) {
					var ipaContent = new StreamContent(ipaFile);
					ipaContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
					ipaContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment") { FileName = ipaFileName };
					multipartFormDataContent.Add(ipaContent);
				}

				if (!string.IsNullOrWhiteSpace (apkFileName) && apkFile != null) {
					var apkContent = new StreamContent(apkFile);
					apkContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
					apkContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment") { FileName = apkFileName };
					multipartFormDataContent.Add(apkContent);
				}

				var result = client.PostAsync("admin/version", multipartFormDataContent).Result;

				var message = string.Empty;
				if (result.IsSuccessStatusCode) {
					message = string.Format ("Version {0} created for company {1}", versionNumber, companyKey);
				} else {
					message = string.Format("Version could not be created: HttpError: {0}", result.Content.ReadAsStringAsync ().Result);
				}

				return message;
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

			var result = await client.PostAsync("admin/appleDevCenter/downloadProfile", urlEncodedContent);
			if (result.IsSuccessStatusCode) 
			{
				try
				{
					var userPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
					var filename = GetFilename(result);
					var savePath = Path.Combine(userPath, "Library/MobileDevice/Provisioning Profiles", filename);

					if(File.Exists(savePath))
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
				catch(Exception e) 
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
				catch {
					return result.ToString();
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