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
		public bool CreateNewVersion(string companyId, string versionNumber, string websiteUrl, string ipaFileName, FileStream ipaFile, string apkFileName, FileStream apkFile)
		{
			var data = new
			{
				CompanyId = companyId,
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
					return result.StatusCode.Equals(HttpStatusCode.OK);
				}
			}
		}
	}
}