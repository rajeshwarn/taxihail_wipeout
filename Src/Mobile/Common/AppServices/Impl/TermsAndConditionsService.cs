using System;
using System.Linq;
using apcurium.MK.Booking.Mobile.Data;
using System.IO;
using ServiceStack.Text;

namespace apcurium.MK.Booking.Mobile.AppServices.Impl
{
    public class TermsAndConditionsService : ITermsAndConditionsService
    {

        public TermsAndConditionsService ()
        {
        }

        private TermsAndConditionsContent _content;

		public TermsAndConditionsContent Content {
            get {
				if (_content == null) {
					_content = LoadTermsAndConditionsContent ();
                }
				return _content;
            }

        }

		public string GetText()
		{
			return Content.Text;
		}

        private static TermsAndConditionsContent LoadTermsAndConditionsContent ()
        {
            TermsAndConditionsContent result = null;
            string resourceName = "";
            
            foreach (string name in typeof(TermsAndConditionsService).Assembly.GetManifestResourceNames()) { 
                if (name.ToLower ().EndsWith (".termsandconditions.json")) {
                    resourceName = name;
                    break;
                }
            }

            
			using (var stream = typeof(TermsAndConditionsContent).Assembly.GetManifestResourceStream( resourceName)) {
                using (var reader = new StreamReader(stream)) {
                    
                    string serializedData = reader.ReadToEnd ();
					result = JsonSerializer.DeserializeFromString<TermsAndConditionsContent> (serializedData);
                }
            }
            
            return result;
            
        }


    }
}

