using System;
using apcurium.MK.Booking.Mobile.Infrastructure;
using MonoTouch.UIKit;
using MonoTouch.Foundation;
using MonoTouch.MessageUI;
using System.IO;
using Cirrious.MvvmCross.Touch.Interfaces;

namespace apcurium.MK.Booking.Mobile.Client
{
    public class PhoneService : IPhoneService
    {
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
            mailComposer.Finished += delegate(object mailsender, MFComposeResultEventArgs mfce) {
                mailComposer.DismissModalViewControllerAnimated (true);
                presenter.NativeModalViewControllerDisappearedOnItsOwn();
                if (File.Exists (errorLogPath))
                {
                    File.Delete (errorLogPath);
                }
            };

            presenter.PresentModalViewController(mailComposer, true);
        }


        #region IPhoneService implementation

        public void AddEventToCalendarAndReminder (string title, string addInfo, string place, DateTime startDate)
        {
            throw new NotImplementedException ();
        }

        #endregion
        #endregion
    }
}

