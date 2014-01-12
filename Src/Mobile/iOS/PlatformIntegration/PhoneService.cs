using System;
using System.IO;
using Cirrious.MvvmCross.Touch.Views.Presenters;
using MonoTouch.EventKit;
using MonoTouch.Foundation;
using MonoTouch.MessageUI;
using MonoTouch.ObjCRuntime;
using MonoTouch.UIKit;
using TinyIoC;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Common.Diagnostic;
using apcurium.MK.Booking.Mobile.Client.Localization;
using Cirrious.MvvmCross.ViewModels;

namespace apcurium.MK.Booking.Mobile.Client.PlatformIntegration
{
    public class PhoneService : IPhoneService
    {

        private EKEventStore _eventStore;

        public EKEventStore EventStore 
        {
            get {
                if (_eventStore == null) 
                {
                    _eventStore = new EKEventStore ();
                }
                return _eventStore;
            }
        }
        #region IPhoneService implementation

        public void Call (string phoneNumber)
        {
            var url = new NSUrl ("tel://" + phoneNumber);
            
            if (!UIApplication.SharedApplication.OpenUrl (url)) 
            {
                var av = new UIAlertView("Not supported", "Calls are not supported on this device", null, Localize.GetValue("Close"), null);
                av.Show ();
            }
        }

        public void SendFeedbackErrorLog(string errorLogPath, string supportEmail, string subject)
        {
            if (!MFMailComposeViewController.CanSendMail)
            {
                return;
            }
            
            var mailComposer = new MFMailComposeViewController ();
            
            if (File.Exists (errorLogPath))
            {
                mailComposer.AddAttachmentData (NSData.FromFile (errorLogPath), "text", "errorlog.txt");
            }
            var presenter = TinyIoCContainer.Current.Resolve<IMvxTouchViewPresenter>();
            mailComposer.SetToRecipients (new [] { supportEmail  });
            mailComposer.SetMessageBody ("", false);
            mailComposer.SetSubject (subject);
            mailComposer.Finished += delegate
            {
				//TODO: Does this work with null ?
				presenter.ChangePresentation(new MvxClosePresentationHint(null)); 
                if (File.Exists (errorLogPath))
                {
                    File.Delete (errorLogPath);
                }
            };

            presenter.PresentModalViewController(mailComposer, true);
        }


        #region IPhoneService implementation


        public void AddEventToCalendarAndReminder (string title, string addInfo, string place, DateTime startDate, DateTime alertDate)
        {
            if(EventStore.RespondsToSelector(new Selector("requestAccessToEntityType:completion:")))
            {
                //iOS6 code
                EventStore.RequestAccess (EKEntityType.Event,(granted, e) => {
                        if (granted)
                        {
                            AddEvent (title, addInfo, startDate, alertDate);
                        }
                        else
                        {
                            TinyIoCContainer.Current.Resolve<ILogger>().LogMessage("Cant save reminder. User Denied Access to Calendar Data");
                        }
                } );
            }else{
                //iOS 5 code
                AddEvent (title, addInfo, startDate, alertDate);
            }
        }

        void AddEvent (string title, string addInfo, DateTime startDate, DateTime alertDate)
        {
            var newEvent = EKEvent.FromStore (EventStore);
            newEvent.AddAlarm (EKAlarm.FromDate (alertDate));
            newEvent.StartDate = startDate;
            newEvent.EndDate = startDate.AddHours (1);
            newEvent.Title = title;
            newEvent.Notes = addInfo;
            newEvent.Calendar = EventStore.DefaultCalendarForNewEvents;
            NSError err;
            EventStore.SaveEvent (newEvent, EKSpan.ThisEvent, out err);
            if (err != null) {
                TinyIoCContainer.Current.Resolve<ILogger> ().LogMessage ("Err Saving Event : " + err);
            }
            else {
                TinyIoCContainer.Current.Resolve<ILogger> ().LogMessage ("Event Saved,  ID: " + newEvent.EventIdentifier);
            }
        }


        #region IPhoneService implementation

        public bool CanUseCalendarAPI ()
        {
            return true;
        }

        #endregion
        #endregion
        #endregion
    }
}

