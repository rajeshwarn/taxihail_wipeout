using System;

namespace apcurium.MK.Booking.Mobile.Infrastructure
{
    public interface IPhoneService
    {
        void Call(string phoneNumber);
        void SendFeedbackErrorLog(string supportEmail, string subject);
        void AddEventToCalendarAndReminder(string title, string addInfo, string place, DateTime startDate, DateTime alertDate);
		bool CanUseCardIO();
    }
}