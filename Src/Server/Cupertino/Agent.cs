using System;
using System.Net.Http;
using HtmlAgilityPack;

namespace Cupertino
{
	public class Agent
	{
		private string Username = "apcurium";
		private string Password = "Apcurium52!";
		//private string Team = "";

		public void Get(string url)
		{
			using (var client = new HttpClient())
			{
				client.DefaultRequestHeaders.Add("user-agent", "Mac Safari");

				var response = client.GetAsync(url).Result.EnsureSuccessStatusCode();

				var responseBodyAsText = response.Content.ReadAsStringAsync().Result;

				if (IsLoginPage (responseBodyAsText)) 
				{
					Login (url);
					Get (url);
				}

				if (IsSelectTeamPage (responseBodyAsText)) 
				{
					//post form with team info
				}

//				using (var multipartFormDataContent = new MultipartFormDataContent())
//				{
//					multipartFormDataContent.Add(new StringContent(JsonConvert.SerializeObject(data)), "data");
//
//					var ipaContent = new StreamContent(ipaFile);
//					ipaContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
//					ipaContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment") { FileName = ipaFileName };
//					multipartFormDataContent.Add(ipaContent);
//
//					var apkContent = new StreamContent(apkFile);
//					apkContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
//					apkContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment") { FileName = apkFileName };
//					multipartFormDataContent.Add(apkContent);
//
//					var result = client.PostAsync(Path.Combine(ConfigurationManager.AppSettings["CustomerPortalAPIUrl"], "/admin/version"), multipartFormDataContent).Result;
//					return result.StatusCode.Equals(HttpStatusCode.OK);
//				}
			}
		}

		private bool IsLoginPage(string htmlContent)
		{
			return GetTitle(htmlContent).Contains("Sign in with your Apple ID");
		}

		private bool IsSelectTeamPage(string htmlContent)
		{
			return GetTitle(htmlContent).Contains("Select Your Team");
		}

		public void Login(string url)
		{
			var browserSession = new BrowserSession ();
			var test = browserSession.Get (url);
			var test2 = GetPostUrl (test);
			browserSession.FormElements ["theAccountName"] = Username;
			browserSession.FormElements ["theAccountPW"] = Password;
			var response = browserSession.Post("https://daw.apple.com" + test2);
			var test3 = browserSession.Get ("https://developer.apple.com/devcenter/selectTeam.action");
			var a = true;
		}

		private string GetPostUrl(string htmlContent)
		{
			var doc = new HtmlDocument();
			doc.LoadHtml (htmlContent);
			var form = doc.DocumentNode.SelectSingleNode("//form");
			return form.GetAttributeValue ("action", "");
		}

		private string GetTitle(string htmlContent)
		{
			var doc = new HtmlDocument();
			doc.LoadHtml (htmlContent);
			var title = doc.DocumentNode.SelectSingleNode("//title");
			return title.InnerText;
		}
	}
}

