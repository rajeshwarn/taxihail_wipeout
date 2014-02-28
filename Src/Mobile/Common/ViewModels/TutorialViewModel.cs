using System.Linq;
using apcurium.MK.Booking.Mobile.Extensions;
using apcurium.MK.Booking.Mobile.Models;

namespace apcurium.MK.Booking.Mobile.ViewModels
{
	public class TutorialViewModel : BaseViewModel
	{
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
			TutorialItemsList = this.Services().Tutorial
				.GetTutorialItems()
				.Select(item => new TutorialItemModel
					{ 
						Text = item.Text,
						Title = item.Title,
						ImageUri = item.ImageUri
					}).ToArray(); 
		}
	}
}