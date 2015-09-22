using System;
using apcurium.MK.Booking.Mobile.Data;

namespace apcurium.MK.Booking.Mobile.AppServices
{
    public interface ITutorialService
    {
        TutorialItem[] GetTutorialItems();
		bool DisplayTutorialToNewUser(Action onEndedTutorial);


	    void NotifyTutorialEnded();
    }
}

