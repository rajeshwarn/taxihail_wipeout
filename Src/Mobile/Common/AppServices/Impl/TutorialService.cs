using System;
using System.IO;
using System.Linq;
using apcurium.MK.Booking.Mobile.Data;
using apcurium.MK.Booking.Mobile.Extensions;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Booking.Mobile.ViewModels;
using apcurium.MK.Common.Configuration;
using TinyIoC;
using apcurium.MK.Common.Extensions;

namespace apcurium.MK.Booking.Mobile.AppServices.Impl
{
    public class TutorialService : ITutorialService
    {
		private readonly IAppSettings _settings;
		private readonly ICacheService _cacheService;
		private readonly IMessageService _messageService;

	    private Action _onEndedTutorial;

		public TutorialService(IAppSettings settings, 
			ICacheService cacheService,
			IMessageService messageService)
		{
			_messageService = messageService;
			_cacheService = cacheService;
			_settings = settings;	
		}
		
		public bool DisplayTutorialToNewUser(Action onEndedTutorial)
		{
			if(_settings.Data.TutorialEnabled && _cacheService.Get<object>("TutorialDisplayed") == null)
			{
				_cacheService.Set("TutorialDisplayed", new object());
				_messageService.ShowDialog(typeof(TutorialViewModel));
				
				_onEndedTutorial = onEndedTutorial; //Needed in android to, for instance, correctly zoom on first login.

				return true;
			}

			return false;
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
			return Content.Items
							.OrderBy(i=>i.Position)
							.Where(x => x.ImageUri.HasValue())
							.ToArray ();
        }

	    public void NotifyTutorialEnded()
	    {
		    if (_onEndedTutorial != null)
		    {
			    _onEndedTutorial();

			    _onEndedTutorial = null;
		    }
	    }

	    private TutorialContent LoadTutorialContent ()
        {
            TutorialContent result = null;
            var resourceName = "";
            
            foreach (var name in typeof(TutorialService).Assembly.GetManifestResourceNames())
            {
                if (name.ToLower ().EndsWith (string.Format(".tutorial-{0}.json", TinyIoCContainer.Current.Resolve<ILocalization>().CurrentLanguage)))
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
						var serializedData = reader.ReadToEnd();
					    result = serializedData.FromJson<TutorialContent>();
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

