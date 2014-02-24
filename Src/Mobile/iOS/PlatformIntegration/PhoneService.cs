using System;
using System.IO;
using apcurium.MK.Common.Configuration;
using Cirrious.MvvmCross.Touch.Views.Presenters;
using MonoTouch.EventKit;
using MonoTouch.Foundation;
using MonoTouch.MessageUI;
using MonoTouch.ObjCRuntime;
using MonoTouch.UIKit;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Common.Diagnostic;
using apcurium.MK.Booking.Mobile.Client.Localization;
using Cirrious.MvvmCross.ViewModels;
using Cirrious.CrossCore.Touch.Views;

namespace apcurium.MK.Booking.Mobile.Client.PlatformIntegration
{
    public class PhoneService : IPhoneService
    {
        readonly ILogger _logger;
        readonly IMvxTouchModalHost _modalHost;

        public PhoneService(ILogger logger, IMvxTouchModalHost modalHost)
        {
            _modalHost = modalHost;
            _logger = logger;

            
        }

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

        public void Call (string phoneNumber)
        {
            var url = new NSUrl ("tel://" + phoneNumber);
            
            if (!UIApplication.SharedApplication.OpenUrl (url)) 
            {
                var av = new UIAlertView("Not supported", "Calls are not supported on this device", null, Localize.GetValue("Close"), null);
                av.Show ();
            }
        }

        public void SendFeedbackErrorLog(string supportEmail, string subject)
        {
            if (!MFMailComposeViewController.CanSendMail)
            {
                return;
            }
            
            var mailComposer = new MFMailComposeViewController ();

            var appSettings = TinyIoC.TinyIoCContainer.Current.Resolve<IAppSettings>();
            var errorLogPath = Path.Combine (Environment.GetFolderPath (Environment.SpecialFolder.MyDocuments), "..", "Library", appSettings.Data.ErrorLogFile);
 
            if (File.Exists (errorLogPath))
            {
                mailComposer.AddAttachmentData (NSData.FromFile (errorLogPath), "text", "errorlog.txt");
            }

            mailComposer.SetToRecipients (new [] { supportEmail  });
            mailComposer.SetMessageBody ("", false);
            mailComposer.SetSubject (subject);
            mailComposer.Finished += (sender, args) =>
            {
                var uiViewController = sender as UIViewController;
                if (uiViewController == null)
                {
                    throw new ArgumentException("sender");
                }

                uiViewController.DismissViewController(true, () => { });
                _modalHost.NativeModalViewControllerDisappearedOnItsOwn();

                if (File.Exists (errorLogPath))
                {
                    File.Delete (errorLogPath);
                }
            };
            _modalHost.PresentModalViewController(mailComposer, true);
        }



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
                            _logger.LogMessage("Cant save reminder. User Denied Access to Calendar Data");
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
                _logger.LogMessage ("Err Saving Event : " + err);
            }
            else {
                _logger.LogMessage ("Event Saved,  ID: " + newEvent.EventIdentifier);
            }
        }


        public bool CanUseCalendarAPI ()
        {
            return true;
        }

    }
}

