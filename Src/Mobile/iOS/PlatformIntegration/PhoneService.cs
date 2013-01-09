using System;
using apcurium.MK.Booking.Mobile.Infrastructure;
using MonoTouch.UIKit;
using MonoTouch.Foundation;
using MonoTouch.MessageUI;
using System.IO;
using Cirrious.MvvmCross.Touch.Interfaces;
using MonoTouch.EventKit;
using apcurium.MK.Common.Diagnostic;

namespace apcurium.MK.Booking.Mobile.Client
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

        public PhoneService ()
        {
        }

        #region IPhoneService implementation

        public void Call (string phoneNumber)
        {
            var url = new NSUrl ("tel://" + phoneNumber);
            
            if (!UIApplication.SharedApplication.OpenUrl (url)) 
            {             
                var av = new UIAlertView ("Not supported", "Calls are not supported on this device", null, Resources.Close, null);
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
            var presenter = TinyIoC.TinyIoCContainer.Current.Resolve<IMvxTouchViewPresenter>();
            mailComposer.SetToRecipients (new string[] { supportEmail  });
            mailComposer.SetMessageBody ("", false);
            mailComposer.SetSubject (supportEmail);
            mailComposer.Finished += delegate(object mailsender, MFComposeResultEventArgs mfce) 
            {
                presenter.Close ( null ); 
                //presenter.NativeModalViewControllerDisappearedOnItsOwn();
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

            EventStore.RequestAccess (EKEntityType.Event,
                                                  (bool granted, NSError e) => {
                    if (granted)
                    {
                        EKEvent newEvent = EKEvent.FromStore ( EventStore );
                        newEvent.AddAlarm ( EKAlarm.FromDate ( alertDate ) );
                        newEvent.StartDate = startDate;
                        newEvent.EndDate = startDate.AddHours(1);
                        newEvent.Title = title;
                        newEvent.Notes = addInfo ;
                        newEvent.Calendar = EventStore.DefaultCalendarForNewEvents;
                        NSError err = null;
                        EventStore.SaveEvent ( newEvent, EKSpan.ThisEvent, out err );
                        if (err != null) 
                        {
                            TinyIoC.TinyIoCContainer.Current.Resolve<ILogger>().LogMessage("Err Saving Event : " + err.ToString());
                            return;
                        } else 
                        {
                            TinyIoC.TinyIoCContainer.Current.Resolve<ILogger>().LogMessage("Event Saved,  ID: " + newEvent.EventIdentifier);
                        }
                    }
                    else
                    {
                        TinyIoC.TinyIoCContainer.Current.Resolve<ILogger>().LogMessage("Cant save reminder. User Denied Access to Calendar Data");
                    }
            } );
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

