using System;
using System.Linq;
using apcurium.MK.Booking.Mobile.Data;
using System.IO;
using ServiceStack.Text;
using apcurium.MK.Common.Configuration;
using MK.Common.iOS.Configuration;

namespace apcurium.MK.Booking.Mobile.AppServices.Impl
{
    public class TutorialService : ITutorialService
    {
		readonly IAppSettings _settings;

		public TutorialService(IAppSettings settings)
		{
			_settings = settings;	
		}

        private TutorialContent _content;
        public TutorialContent Content 
		{
            get 
			{
                if (_content == null) 
				{
                    _content = LoadTutorialContent ();
                }
                return _content;
            }
        }

        public TutorialItem[] GetTutorialItems ()
        {
            return Content.Items.OrderBy ( i=>i.Position ).ToArray ();
        }

        private TutorialContent LoadTutorialContent ()
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
                if (stream != null)
                    using (var reader = new StreamReader(stream)) {
                    
                        string serializedData = reader.ReadToEnd ();
                        result = JsonSerializer.DeserializeFromString<TutorialContent> (serializedData);
                    }
            }

			var disabledSlidesString = _settings.Data.DisabledTutorialSlides;
			if (!string.IsNullOrWhiteSpace(disabledSlidesString))
			{
				var disabledSlides = disabledSlidesString
					.Split(new []{','}, StringSplitOptions.RemoveEmptyEntries)
					.Select(int.Parse);

				var slides = disabledSlides as int[] ?? disabledSlides.ToArray();
				if (slides.Any())
				{
					if (result != null)
					{
					    var listOfSlides = result.Items.ToList();
					    foreach (var disabledSlide in slides)
					    {
					        var disabledItem = listOfSlides.FirstOrDefault(x => x.Position == disabledSlide);
					        if (disabledItem != null)
					        {
					            listOfSlides.Remove(disabledItem);
					        }
					    }
					    result.Items = listOfSlides.ToArray();
					}
				}
			}

            return result;
        }
    }
}

