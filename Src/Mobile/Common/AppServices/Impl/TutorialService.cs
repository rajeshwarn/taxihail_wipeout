using System;
using System.Linq;
using apcurium.MK.Booking.Mobile.Data;
using System.IO;
using ServiceStack.Text;
using apcurium.MK.Booking.Mobile.Infrastructure;
using TinyIoC;
using apcurium.MK.Common.Configuration;

namespace apcurium.MK.Booking.Mobile.AppServices.Impl
{
    public class TutorialService : ITutorialService
    {
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

            var disabledSlidesString = TinyIoCContainer.Current.Resolve<IConfigurationManager>().GetSetting("Client.DisabledTutorialSlides");
			if (!string.IsNullOrWhiteSpace(disabledSlidesString))
			{
				try
				{
					var disabledSlides = disabledSlidesString
						.Split(new char[]{','}, StringSplitOptions.RemoveEmptyEntries)
						.Select(x => int.Parse(x));

					if (disabledSlides.Count() > 0)
					{
						var listOfSlides = result.Items.ToList();
						foreach (var disabledSlide in disabledSlides)
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
				catch
				{
				}
			}

            return result;
        }
    }
}

