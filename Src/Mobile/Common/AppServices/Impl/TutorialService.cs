using System;
using System.Linq;
using apcurium.MK.Booking.Mobile.Data;
using System.IO;
using ServiceStack.Text;

namespace apcurium.MK.Booking.Mobile.AppServices.Impl
{
    public class TutorialService : ITutorialService
    {

        public TutorialService ()
        {
        }

        private TutorialContent _content;

        public TutorialContent Content {
            get {
                if (_content == null) {
                    _content = LoadTutorialContent ();
                }
                return _content;
            }

        }

        public TutorialItem[] GetTutorialItems ()
        {
            return Content.Items.OrderBy ( i=>i.Position ).ToArray ();
        }

        private static TutorialContent LoadTutorialContent ()
        {
            TutorialContent result = null;
            string resourceName = "";
            
            foreach (string name in typeof(TutorialService).Assembly.GetManifestResourceNames()) { 
                if (name.ToLower ().EndsWith (".tutorial.json")) {
                    resourceName = name;
                    break;
                }
            }
            
            
            using (var stream = typeof(TutorialContent).Assembly.GetManifestResourceStream( resourceName)) {
                using (var reader = new StreamReader(stream)) {
                    
                    string serializedData = reader.ReadToEnd ();
                    result = JsonSerializer.DeserializeFromString<TutorialContent> (serializedData);
                }
            }
            
            return result;
            
        }


    }
}

