using System;
using System.IO;
using EventKit;
using Foundation;
using MessageUI;
using ObjCRuntime;
using UIKit;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Common.Diagnostic;
using apcurium.MK.Booking.Mobile.Client.Localization;
using Cirrious.CrossCore.Touch.Views;
using apcurium.MK.Booking.Mobile.Client.Helper;

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
            get 
            {
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

            var canCall = UIApplication.SharedApplication.CanOpenUrl(new Foundation.NSUrl("tel:15146543024"));
            if (!canCall)
            {
                var av = new UIAlertView ("Not supported", "Calls are not supported on this device", null, Localize.GetValue ("Close"), null);
                av.Show ();
            }
            else
            {
                UIApplication.SharedApplication.OpenUrl (url);
            }
        }

        public void SendFeedbackErrorLog(string supportEmail, string subject)
        {
            if (!MFMailComposeViewController.CanSendMail)
            {
                var av = new UIAlertView (Localize.GetValue("PanelMenuViewReportProblemText"), Localize.GetValue("ServiceError_EmailClientAbsent"), null, Localize.GetValue ("OkButtonText"), null);
                av.Show ();
                return;
            }
            
            var mailComposer = new MFMailComposeViewController ();
			var logFile = _logger.MergeLogFiles();

			if (logFile != null)
			{
				mailComposer.AddAttachmentData(NSData.FromFile(logFile), "text", Path.GetFileName(logFile));
			}

            mailComposer.SetToRecipients (new [] { supportEmail });
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
            };
            _modalHost.PresentModalViewController(mailComposer, true);
        }

        public void AddEventToCalendarAndReminder (string title, string addInfo, string place, DateTime startDate, DateTime alertDate)
        {
            if(EventStore.RespondsToSelector(new Selector("requestAccessToEntityType:completion:")))
            {
                //iOS6 code
                EventStore.RequestAccess (EKEntityType.Event,(granted, e) => 
                {
                    if (granted)
                    {
                        AddEvent (title, addInfo, startDate, alertDate);
                    }
                    else
                    {
                        _logger.LogMessage("Cant save reminder. User Denied Access to Calendar Data");
                    }
                });
            }
            else
            {
                //iOS 5 code
                AddEvent (title, addInfo, startDate, alertDate);
            }
        }

        private void AddEvent (string title, string addInfo, DateTime startDate, DateTime alertDate)
        {
            var newEvent = EKEvent.FromStore (EventStore);
            newEvent.AddAlarm (EKAlarm.FromDate (alertDate.LocalDateTimeToNSDate()));
            newEvent.StartDate = startDate.LocalDateTimeToNSDate();
            newEvent.EndDate = startDate.AddHours (1).LocalDateTimeToNSDate();
            newEvent.Title = title;
            newEvent.Notes = addInfo;
            newEvent.Calendar = EventStore.DefaultCalendarForNewEvents;
            NSError err;
            EventStore.SaveEvent (newEvent, EKSpan.ThisEvent, out err);
            if (err != null) 
            {
                _logger.LogMessage ("Err Saving Event : " + err);
            }
            else 
            {
                _logger.LogMessage ("Event Saved,  ID: " + newEvent.EventIdentifier);
            }
        }

        public bool CanUseCalendarAPI ()
        {
            return true;
        }
    }
}

