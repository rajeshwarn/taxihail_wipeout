using System.Linq;
using apcurium.MK.Booking.Mobile.Extensions;
using apcurium.MK.Booking.Mobile.Models;
using apcurium.MK.Booking.Mobile.AppServices;

namespace apcurium.MK.Booking.Mobile.ViewModels
{
	public class TutorialViewModel : BaseViewModel
	{
		private readonly ITutorialService _tutorialService;

		public TutorialViewModel(ITutorialService tutorialService)
		{
			_tutorialService = tutorialService;
		}

		private TutorialItemModel[] _tutorialItemsList;
		public TutorialItemModel[] TutorialItemsList
		{
			get { return _tutorialItemsList; }
			set
			{ 
				_tutorialItemsList = value;
				RaisePropertyChanged();
			}
		}

		public override void Start()
		{
			TutorialItemsList = _tutorialService.GetTutorialItems()
				.Select(item => new TutorialItemModel
					{ 
						Text = item.Text,
						Title = item.Title,
						ImageUri = item.ImageUri
					}).ToArray(); 
		}
	}
}