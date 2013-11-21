using System;
using System.Configuration;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;

namespace DeploymentServiceTools
{
	public class CustomerPortalRepository
    {
		public string CreateNewVersion(string companyKey, string versionNumber, string websiteUrl, string ipaFileName, FileStream ipaFile, string apkFileName, FileStream apkFile)
		{
			versionNumber = versionNumber.Replace("[Bitbucket]", string.Empty).Replace(" ", string.Empty);

			var data = new
			{
				CompanyKey = companyKey,
				VersionNumber = versionNumber,
				WebsiteUrl = websiteUrl
			};

			using (var client = new HttpClient())
			{
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

					var url = Path.Combine (ConfigurationManager.AppSettings ["CustomerPortalAPIUrl"], "admin/version");
					var result = client.PostAsync(url, multipartFormDataContent).Result;

					var message = string.Empty;
					if (result.IsSuccessStatusCode) {
						message = string.Format ("Version {0} created for company {1}", versionNumber, companyKey);
					} else {
						message = string.Format("Version could not be created: HttpError: {0}", result.Content.ReadAsStringAsync ().Result);
					}

					return message;
				}
			}
		}
	}
}