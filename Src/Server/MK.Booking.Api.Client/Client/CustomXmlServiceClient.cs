#region

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using apcurium.MK.Common.Configuration.Impl;
using ServiceStack.Service;
using ServiceStack.ServiceHost;

#endregion

namespace apcurium.MK.Booking.Api.Client.Client
{
    public class CustomXmlServiceClient : IServiceClient, IRestClient
    {
        private static readonly Dictionary<Type, XmlSerializer> XmlSerializers = new Dictionary<Type, XmlSerializer>();

        private readonly XmlWriterSettings _xmlWriterSettings = new XmlWriterSettings
        {
            Encoding = Encoding.ASCII,
            Indent = true,
            NewLineOnAttributes = true,
        };

        public Action<WebRequest> LocalHttpWebRequestFilter;
        public TimeSpan Timeout;

        protected CustomXmlServiceClient(CmtPaymentSettings cmtSettings, bool acceptAnyCert, IWebProxy proxy = null)
        {
            BaseUrl = cmtSettings.IsSandbox
                ? cmtSettings.SandboxBaseUrl
                : cmtSettings.BaseUrl;

            if (acceptAnyCert)
            {
                //todo - Bug accept all certificates
                ServicePointManager.ServerCertificateValidationCallback = (p1, p2, p3, p4) => true;
            }
            if (proxy != null)
            {
                proxy = new WebProxy("http:\\localhost:8888");
                Proxy = proxy;
            }
        }

        public string BaseUrl { get; set; }
        public IWebProxy Proxy { get; set; }

        public TResponse Post<TResponse>(IReturn<TResponse> request)
        {
            return DoWebRequest(request, "POST");
        }

        public TResponse Delete<TResponse>(IReturn<TResponse> request)
        {
            return DoWebRequest(request, "DELETE");
        }

        public TResponse CustomMethod<TResponse>(string httpVerb, IReturn<TResponse> request)
        {
            throw new NotImplementedException();
        }

        public void CustomMethod(string httpVerb, IReturnVoid request)
        {
            throw new NotImplementedException();
        }

        public TResponse Delete<TResponse>(string relativeOrAbsoluteUrl)
        {
            throw new NotImplementedException();
        }

        public void Delete(IReturnVoid request)
        {
            throw new NotImplementedException();
        }


        public TResponse Get<TResponse>(string relativeOrAbsoluteUrl)
        {
            throw new NotImplementedException();
        }

        public void Get(IReturnVoid request)
        {
            throw new NotImplementedException();
        }

        public TResponse Get<TResponse>(IReturn<TResponse> request)
        {
            throw new NotImplementedException();
        }

        public TResponse Patch<TResponse>(string relativeOrAbsoluteUrl, object request)
        {
            throw new NotImplementedException();
        }

        public void Patch(IReturnVoid request)
        {
            throw new NotImplementedException();
        }

        public TResponse Patch<TResponse>(IReturn<TResponse> request)
        {
            throw new NotImplementedException();
        }

        public TResponse Post<TResponse>(string relativeOrAbsoluteUrl, object request)
        {
            throw new NotImplementedException();
        }

        public void Post(IReturnVoid request)
        {
            throw new NotImplementedException();
        }

        public TResponse Put<TResponse>(string relativeOrAbsoluteUrl, object request)
        {
            throw new NotImplementedException();
        }

        public void Put(IReturnVoid request)
        {
            throw new NotImplementedException();
        }

        public TResponse Put<TResponse>(IReturn<TResponse> request)
        {
            throw new NotImplementedException();
        }


        public void SendAsync<TResponse>(object request, Action<TResponse> onSuccess,
            Action<TResponse, Exception> onError)
        {
            throw new NotImplementedException();
        }

        public void DeleteAsync<TResponse>(string relativeOrAbsoluteUrl, Action<TResponse> onSuccess,
            Action<TResponse, Exception> onError)
        {
            throw new NotImplementedException();
        }

        public void GetAsync<TResponse>(string relativeOrAbsoluteUrl, Action<TResponse> onSuccess,
            Action<TResponse, Exception> onError)
        {
            throw new NotImplementedException();
        }

        public void PostAsync<TResponse>(string relativeOrAbsoluteUrl, object request, Action<TResponse> onSuccess,
            Action<TResponse, Exception> onError)
        {
            throw new NotImplementedException();
        }

        public void PutAsync<TResponse>(string relativeOrAbsoluteUrl, object request, Action<TResponse> onSuccess,
            Action<TResponse, Exception> onError)
        {
            throw new NotImplementedException();
        }

        public void SetCredentials(string userName, string password)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public void SendOneWay(string relativeOrAbsoluteUrl, object request)
        {
            throw new NotImplementedException();
        }

        public void SendOneWay(object request)
        {
            throw new NotImplementedException();
        }

        public TResponse PostFile<TResponse>(string relativeOrAbsoluteUrl, Stream fileToUpload, string fileName,
            string mimeType)
        {
            throw new NotImplementedException();
        }

        public TResponse PostFile<TResponse>(string relativeOrAbsoluteUrl, FileInfo fileToUpload, string mimeType)
        {
            throw new NotImplementedException();
        }

        public TResponse PostFileWithRequest<TResponse>(string relativeOrAbsoluteUrl, Stream fileToUpload,
            string fileName, object request)
        {
            throw new NotImplementedException();
        }

        public TResponse PostFileWithRequest<TResponse>(string relativeOrAbsoluteUrl, FileInfo fileToUpload,
            object request)
        {
            throw new NotImplementedException();
        }

        public void Send(IReturnVoid request)
        {
            throw new NotImplementedException();
        }

        public TResponse Send<TResponse>(IReturn<TResponse> request)
        {
            throw new NotImplementedException();
        }

        public TResponse Send<TResponse>(object request)
        {
            throw new NotImplementedException();
        }

        #region IRestClientAsync implementation

        void IRestClientAsync.GetAsync<TResponse>(IReturn<TResponse> request, Action<TResponse> onSuccess,
            Action<TResponse, Exception> onError)
        {
            throw new NotImplementedException();
        }

        void IRestClientAsync.DeleteAsync<TResponse>(IReturn<TResponse> request, Action<TResponse> onSuccess,
            Action<TResponse, Exception> onError)
        {
            throw new NotImplementedException();
        }

        void IRestClientAsync.PostAsync<TResponse>(IReturn<TResponse> request, Action<TResponse> onSuccess,
            Action<TResponse, Exception> onError)
        {
            throw new NotImplementedException();
        }

        void IRestClientAsync.PutAsync<TResponse>(IReturn<TResponse> request, Action<TResponse> onSuccess,
            Action<TResponse, Exception> onError)
        {
            throw new NotImplementedException();
        }

        void IRestClientAsync.CustomMethodAsync<TResponse>(string httpVerb, IReturn<TResponse> request,
            Action<TResponse> onSuccess, Action<TResponse, Exception> onError)
        {
            throw new NotImplementedException();
        }

        void IRestClientAsync.CancelAsync()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IRestClient implementation

        TResponse IRestClient.Get<TResponse>(IReturn<TResponse> request)
        {
            throw new NotImplementedException();
        }

        TResponse IRestClient.Get<TResponse>(string relativeOrAbsoluteUrl)
        {
            throw new NotImplementedException();
        }

        TResponse IRestClient.Delete<TResponse>(IReturn<TResponse> request)
        {
            throw new NotImplementedException();
        }

        TResponse IRestClient.Delete<TResponse>(string relativeOrAbsoluteUrl)
        {
            throw new NotImplementedException();
        }

        TResponse IRestClient.Post<TResponse>(IReturn<TResponse> request)
        {
            throw new NotImplementedException();
        }

        TResponse IRestClient.Post<TResponse>(string relativeOrAbsoluteUrl, object request)
        {
            throw new NotImplementedException();
        }

        TResponse IRestClient.Put<TResponse>(IReturn<TResponse> request)
        {
            throw new NotImplementedException();
        }

        TResponse IRestClient.Put<TResponse>(string relativeOrAbsoluteUrl, object request)
        {
            throw new NotImplementedException();
        }

        TResponse IRestClient.Patch<TResponse>(IReturn<TResponse> request)
        {
            throw new NotImplementedException();
        }

        TResponse IRestClient.Patch<TResponse>(string relativeOrAbsoluteUrl, object request)
        {
            throw new NotImplementedException();
        }

        TResponse IRestClient.PostFile<TResponse>(string relativeOrAbsoluteUrl, FileInfo fileToUpload, string mimeType)
        {
            throw new NotImplementedException();
        }

        TResponse IRestClient.CustomMethod<TResponse>(string httpVerb, IReturn<TResponse> request)
        {
            throw new NotImplementedException();
        }

        public HttpWebResponse Head(IReturn request)
        {
            throw new NotImplementedException();
        }

        public HttpWebResponse Head(string relativeOrAbsoluteUrl)
        {
            throw new NotImplementedException();
        }

        #endregion

        private static XmlSerializer GetOrCreate(Type t)
        {
            if (!XmlSerializers.ContainsKey(t))
            {
                XmlSerializers.Add(t, new XmlSerializer(t, ""));
            }
            return XmlSerializers[t];
        }

        public string GetEndpoint(object obj)
        {
            var routeAttribute =
                (RouteAttribute)
                    Attribute.GetCustomAttributes(obj.GetType()).FirstOrDefault(att => att is RouteAttribute);
            var path = "";
            if (routeAttribute != null)
            {
                path = routeAttribute.Path;
            }

            var toSwitch = path.Split('{').Skip(1).Select(item => item.Split('}').First()).ToArray();

            foreach (var property in obj.GetType().GetProperties())
            {
                var match = toSwitch.FirstOrDefault(str => str == property.Name);
                if (!string.IsNullOrEmpty(match))
                {
                    var value = property.GetValue(obj, null);
                    if (value == null)
                    {
                        value = "";
                    }
                    path = path.Replace("{" + match + "}", value.ToString());
                }
            }

            return path;
        }

        private TResponse DoWebRequest<TResponse>(IReturn<TResponse> request, string method = "GET")
        {
            var requestStream = new MemoryStream();
            var xmlWriter = XmlWriter.Create(requestStream, _xmlWriterSettings);
            var serializer = GetOrCreate(request.GetType());

            serializer.Serialize(xmlWriter, request);

            requestStream.Position = 0;

            var webRequest = WebRequest.Create(BaseUrl + GetEndpoint(request));
            webRequest.Method = method;
            webRequest.ContentType = "application/xml";
            //webRequest.ContentLength = requestStream.Length;
            if (Proxy != null)
            {
                webRequest.Proxy = Proxy;
            }

            LocalHttpWebRequestFilter(webRequest);
            using (var webRequestStream = webRequest.GetRequestStream())
            {
                requestStream.WriteTo(webRequestStream);
            }

            var webResponse = webRequest.GetResponse();
#if DEBUG

            var stream = new MemoryStream();
            var responseStream = webResponse.GetResponseStream();
            if (responseStream != null) responseStream.CopyTo(stream);
            stream.Position = 0;
            new StreamReader(stream).ReadToEnd();
            stream.Position = 0;

            var responseObject = (TResponse) GetOrCreate(typeof (TResponse)).Deserialize(stream);
#else
            var responseObject = (TResponse)GetOrCreate(typeof(TResponse)).Deserialize(webResponse.GetResponseStream());
#endif


            return responseObject;
        }
    }
}