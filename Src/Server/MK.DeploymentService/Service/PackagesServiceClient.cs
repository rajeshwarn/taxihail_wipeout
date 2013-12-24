#region

using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;
using MK.DeploymentService.Properties;

#endregion

namespace MK.DeploymentService.Service
{
    public class PackagesServiceClient
    {
        public void UploadPackage(string filename)
        {
            var url = GetUrl();


            using (
                var client =
                    new HttpClient(new HttpClientHandler
                    {
                        Credentials = new NetworkCredential("taxihail@apcurium.com", "apcurium5200!")
                    }))
            {
                client.BaseAddress = new Uri(url);

                if (File.Exists(filename))
                {
                    var stream = new FileStream(filename, FileMode.Open);

                    var packageContent = new StreamContent(stream);
                    packageContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                    packageContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
                    {
                        FileName = Path.GetFileName(filename)
                    };
// ReSharper disable once UnusedVariable
                    var r = client.PostAsync("packages", packageContent).Result.IsSuccessStatusCode;
                }
            }
        }

        public string[] GetPackageList()
        {
            var url = GetUrl();
            using (
                var client =
                    new HttpClient(new HttpClientHandler
                    {
                        Credentials = new NetworkCredential("taxihail@apcurium.com", "apcurium5200!")
                    }))
            {
                client.BaseAddress = new Uri(url);
                var packagesName =
                    client.GetAsync(@"packages")
                        .Result.Content.ReadAsAsync<string[]>()
                        .Result;

                return packagesName;
            }
        }

        public void GetPackage(string fileName, string destinationFileName)
        {
            var url = GetUrl();

            using (
                var client =
                    new HttpClient(new HttpClientHandler
                    {
                        Credentials = new NetworkCredential("taxihail@apcurium.com", "apcurium5200!")
                    }))
            {
                client.BaseAddress = new Uri(url);
                var r = client.GetAsync(@"packages/?fileName=" + HttpUtility.UrlEncode(fileName)).Result; // + ).Result;
                using (var stream = r.Content.ReadAsStreamAsync().Result)
                {
                    Save(destinationFileName, stream);
                }
            }
        }


        public void Save(string fileName, Stream file)
        {
            var path = Path.Combine(@"c:\temp\", fileName);
            using (var fileStream = File.Create(path, (int) file.Length))
            {
                var bytesInStream = new byte[file.Length];
                file.Read(bytesInStream, 0, bytesInStream.Length);
                fileStream.Write(bytesInStream, 0, bytesInStream.Length);
                fileStream.Close();
            }
        }


        private static string GetUrl()
        {
// ReSharper disable once RedundantAssignment
            var url = Settings.Default.CustomerPortalUrl;

#if DEBUG
            url = "http://localhost:2287/api/";
#endif
            return url;
        }
    }
}