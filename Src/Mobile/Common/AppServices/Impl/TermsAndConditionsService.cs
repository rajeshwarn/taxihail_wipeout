using System.Text;
using apcurium.MK.Booking.Mobile.Data;
using System.IO;
using ServiceStack.Text;

namespace apcurium.MK.Booking.Mobile.AppServices.Impl
{
    public class TermsAndConditionsService : ITermsAndConditionsService
    {
        private TermsAndConditionsContent _content;

		public TermsAndConditionsContent Content {
            get { return _content ?? (_content = LoadTermsAndConditionsContent()); }
		}

		public string GetText()
		{
			return Content.Text;
		}

        private static TermsAndConditionsContent LoadTermsAndConditionsContent ()
        {
            TermsAndConditionsContent result = null;
            string resourceName = string.Empty;
            
            foreach (string name in typeof(TermsAndConditionsService).Assembly.GetManifestResourceNames()) { 
                if (name.ToLower ().EndsWith (".termsandconditions.json")) {
                    resourceName = name;
                    break;
                }
            }

            
			using (var stream = typeof(TermsAndConditionsContent).Assembly.GetManifestResourceStream( resourceName)) {
			    if (stream != null)
			        using (var reader = new StreamReader(stream, Encoding.UTF8)) {
                    
			            string serializedData = reader.ReadToEnd ();
			            result = JsonSerializer.DeserializeFromString<TermsAndConditionsContent> (serializedData);
			        }
			}
            
            return result;
            
        }


    }
}

