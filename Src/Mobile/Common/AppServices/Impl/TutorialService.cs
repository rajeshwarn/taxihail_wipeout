using System;
using System.IO;
using System.Linq;
using apcurium.MK.Booking.Mobile.Data;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Booking.Mobile.ViewModels;
using apcurium.MK.Common.Configuration;
using ServiceStack.Text;

namespace apcurium.MK.Booking.Mobile.AppServices.Impl
{
    public class TutorialService : ITutorialService
    {
		private readonly IAppSettings _settings;
		private readonly ICacheService _cacheService;
		private readonly IMessageService _messageService;

		public TutorialService(IAppSettings settings, 
			ICacheService cacheService,
			IMessageService messageService)
		{
			_messageService = messageService;
			_cacheService = cacheService;
			_settings = settings;	
		}

		public void DisplayTutorialToNewUser()
		{
			if(_settings.Data.TutorialEnabled
				&& _cacheService.Get<object>("TutorialDisplayed") == null)
			{
				_messageService.ShowDialog(typeof(TutorialViewModel));
				_cacheService.Set<object>("TutorialDisplayed", new object());
			}
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
            
            foreach (string name in typeof(TutorialService).Assembly.GetManifestResourceNames()) 
			{ 
                if (name.ToLower ().EndsWith (".tutorial.json")) 
				{
                    resourceName = name;
                    break;
                }
            }
            
            using (var stream = typeof(TutorialContent).Assembly.GetManifestResourceStream( resourceName)) 
			{
				if (stream != null)
				{
					using (var reader = new StreamReader(stream))
					{
						string serializedData = reader.ReadToEnd();
						result = JsonSerializer.DeserializeFromString<TutorialContent>(serializedData);
					}
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

