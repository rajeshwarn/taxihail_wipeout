using System;
using System.Linq;
using apcurium.MK.Booking.Mobile.Infrastructure;
using ServiceStack.Text;
using System.IO;
using apcurium.MK.Common.Configuration;
using TinyIoC;

namespace apcurium.MK.Booking.Mobile.Settings
{
    public class AppSettings : IAppSettings
    {
        private readonly AppSettingsData _data;

        public AppSettings ()
        {
            using (var stream = GetType().Assembly.GetManifestResourceStream(GetType ().Assembly.GetManifestResourceNames().FirstOrDefault(x => x.Contains("Settings.json")))) 
			{
			    if (stream != null)
			        using (var reader = new StreamReader(stream)) 
			        {
			            string serializedData = reader.ReadToEnd ();
			            _data = JsonSerializer.DeserializeFromString<AppSettingsData> (serializedData);
			        }
			}
        }

        public bool ErrorLogEnabled {
            get { return true; }
        }

        public string ErrorLog {
            get { 
                string path = Environment.GetFolderPath (Environment.SpecialFolder.Personal);
                return  Path.Combine (path, "errorlog.txt"); 
            }
        }
    
        public string ApplicationName { get { return _data.ApplicationName; } }

		public string DefaultServiceUrl { get { return "http://services.taxihail.com/{0}/api/"; } }

		public bool CanChangeServiceUrl { get { return _data.CanChangeServiceUrl; } }

        public string ServiceUrl {
            get {
                string url;
                try {
                    url = TinyIoCContainer.Current.Resolve<ICacheService> ().Get<string> ("TaxiHail.ServiceUrl");
                } catch {
                    return _data.ServiceUrl;
                }
                if (string.IsNullOrEmpty (url)) {
                    
                    return _data.ServiceUrl;
                }
                return url;
            }
            set {
                if (CanChangeServiceUrl) {
                    TinyIoCContainer.Current.Resolve<IConfigurationManager> ().Reset ();

                    if (string.IsNullOrEmpty (value)) {
                        TinyIoCContainer.Current.Resolve<ICacheService> ().Clear ("TaxiHail.ServiceUrl");
                    } else if (value.ToLower ().StartsWith ("http")) {
                        TinyIoCContainer.Current.Resolve<ICacheService> ().Set<string> ("TaxiHail.ServiceUrl", value);
                    } else {
                        TinyIoCContainer.Current.Resolve<ICacheService> ().Set<string> ("TaxiHail.ServiceUrl", string.Format (DefaultServiceUrl, value));
                    }
                }
            }
        }

        public bool TwitterEnabled {
            get { return _data.TwitterEnabled; }
        }

        public string TwitterConsumerKey {
            get { return _data.TwitterConsumerKey; }
        }

        public string TwitterCallback {
            get { return _data.TwitterCallback; }
        }

        public string TwitterConsumerSecret {
            get { return _data.TwitterConsumerSecret; }
        }

        public string TwitterRequestTokenUrl {
            get { return _data.TwitterRequestTokenUrl; }
        }

        public string TwitterAccessTokenUrl {
            get { return _data.TwitterAccessTokenUrl; }
        }

        public string TwitterAuthorizeUrl {
            get { return _data.TwitterAuthorizeUrl; }
        }

        public bool FacebookEnabled {
            get { return _data.FacebookEnabled; }
        }

        public string FacebookAppId {
            get { return _data.FacebookAppId; }
        }
    }
}
