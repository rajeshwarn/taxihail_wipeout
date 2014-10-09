#region

using System;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using HtmlAgilityPack;

#endregion

namespace Cupertino
{
    public class BrowserSession
    {
        private readonly WebProxy _proxy;
        private MemoryStream _download = new MemoryStream();
        private HtmlDocument _htmlDoc;
        private bool _isDownload;
        private bool _isPost;
        private string _lastResponseString;
        private string _referer;

        public BrowserSession()
        {
        }

        public BrowserSession(string proxyHost, int proxyPort)
        {
            _proxy = new WebProxy(proxyHost, proxyPort);
        }

        /// <summary>
        /// System.Net.CookieCollection. Provides a collection container for instances of Cookie class 
        /// </summary>
        public CookieCollection Cookies { get; set; }

        /// <summary>
        /// Provide a key-value-pair collection of form elements 
        /// </summary>
        public FormElementCollection FormElements { get; set; }

        /// <summary>
        /// Makes a HTTP GET request to the given URL
        /// </summary>
        public string Get(string url)
        {
            _isPost = false;
            _referer = null;
//            CreateWebRequestObject().Load(url);
            CreateWebRequestObject().Load(url, "GET", _proxy, null);
            _referer = url;
            return _htmlDoc.DocumentNode.InnerHtml;
        }

        /// <summary>
        /// Makes a HTTP POST request to the given URL
        /// </summary>
        public string Post(string url)
        {
            _isPost = true;
//            CreateWebRequestObject().Load(url, "POST");
            CreateWebRequestObject().Load(url, "POST", _proxy, null);
            try
            {
                var result = _htmlDoc.DocumentNode.InnerHtml;
                return result;
            }
            catch (Exception)
            {
                return _lastResponseString;
            }
        }

        public Stream GetDownload(string url)
        {
            _isPost = false;
            _isDownload = true;
            CreateWebRequestObject().Load(url, "GET", _proxy, null);
            return _download;
        }

        /// <summary>
        /// Creates the HtmlWeb object and initializes all event handlers. 
        /// </summary>
        private HtmlWeb CreateWebRequestObject()
        {
            var web = new HtmlWeb();
            web.UseCookies = true;
            web.PreRequest = OnPreRequest;
            web.PostResponse = OnAfterResponse;
            web.PreHandleDocument = OnPreHandleDocument;
            return web;
        }

        /// <summary>
        /// Event handler for HtmlWeb.PreRequestHandler. Occurs before an HTTP request is executed.
        /// </summary>
        protected bool OnPreRequest(HttpWebRequest request)
        {
            // to remove the "Expect: Continue" taking the place of "Keep-Alive"
            request.ServicePoint.Expect100Continue = false;
            request.KeepAlive = true;
            var sp = request.ServicePoint;
            var prop = sp.GetType().GetProperty("HttpBehaviour", BindingFlags.Instance | BindingFlags.NonPublic);
            prop.SetValue(sp, (byte) 0, null);

            request.UserAgent = "Mac Safari";
            request.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";
            //request.Headers[HttpRequestHeader.AcceptEncoding] = "gzip, deflate";
            request.Headers[HttpRequestHeader.AcceptLanguage] = "en-US,en;q=0.5";
            request.Referer = _referer;

            AddCookiesTo(request); // Add cookies that were saved from previous requests
            if (_isPost) AddPostDataTo(request); // We only need to add post data on a POST request

            return true;
        }

        /// <summary>
        /// Event handler for HtmlWeb.PostResponseHandler. Occurs after a HTTP response is received
        /// </summary>
        protected void OnAfterResponse(HttpWebRequest request, HttpWebResponse response)
        {
            _lastResponseString = null;
            _download = new MemoryStream();
            SaveCookiesFrom(request, response);
            if (response != null && (_isDownload || response.ContentType.Contains("json")))
            {
                var remoteStream = response.GetResponseStream();

                if (_isDownload)
                {
                    remoteStream.CopyTo(_download);
                }

                if (response.ContentType.Contains("json"))
                {
                    var sr = new StreamReader(remoteStream);
                    _lastResponseString = sr.ReadToEnd();
                }
            }
        }

        /// <summary>
        /// Event handler for HtmlWeb.PreHandleDocumentHandler. Occurs before a HTML document is handled
        /// </summary>
        protected void OnPreHandleDocument(HtmlDocument document)
        {
            SaveHtmlDocument(document);
        }

        /// <summary>
        /// Assembles the Post data and attaches to the request object
        /// </summary>
        private void AddPostDataTo(HttpWebRequest request)
        {
            string payload = FormElements.AssemblePostPayload();
            byte[] buff = Encoding.UTF8.GetBytes(payload.ToCharArray());
            request.ContentLength = buff.Length;
            request.ContentType = "application/x-www-form-urlencoded";
            Stream reqStream = request.GetRequestStream();
            reqStream.Write(buff, 0, buff.Length);
        }

        /// <summary>
        /// Add cookies to the request object
        /// </summary>
        private void AddCookiesTo(HttpWebRequest request)
        {
            if (Cookies != null && Cookies.Count > 0)
            {
                // ugly hack otherwise cookies are not sent across subdomains
                for (int i = 0; i < Cookies.Count; i++)
                {
                    var cookie = Cookies[i];
                    cookie.Version = 0;
                }
                request.CookieContainer.Add(Cookies);
            }
        }

        /// <summary>
        /// Saves cookies from the response object to the local CookieCollection object
        /// </summary>
        private void SaveCookiesFrom(HttpWebRequest request, HttpWebResponse response)
        {
            //save the cookies ;)
            if (request.CookieContainer.Count > 0 || response.Cookies.Count > 0)
            {
                if (Cookies == null)
                {
                    Cookies = new CookieCollection();
                }

                Cookies.Add(request.CookieContainer.GetCookies(request.RequestUri));
                Cookies.Add(response.Cookies);
            }
        }

        /// <summary>
        /// Saves the form elements collection by parsing the HTML document
        /// </summary>
        private void SaveHtmlDocument(HtmlDocument document)
        {
            _htmlDoc = document;
            FormElements = new FormElementCollection(_htmlDoc);
        }
    }
}