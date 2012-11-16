using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System.Text.RegularExpressions;
using TinyIoC;
using apcurium.MK.Booking.Mobile.Data;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Booking.Mobile.Client.Helpers;
using apcurium.MK.Booking.Mobile.Client.Validation;

namespace apcurium.MK.Booking.Mobile.Client.Activities.Account
{
    [Activity(Label = "Password Recovery", Theme = "@android:style/Theme.NoTitleBar", ScreenOrientation=Android.Content.PM.ScreenOrientation.Portrait)]
    public class PasswordRecoveryActivity : BaseActivity
    {
        protected override int ViewTitleResourceId
        {
            get { return Resource.String.View_PasswordRecovery; }
        }

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.View_PasswordRecovery);
            var btnResetRecovery = FindViewById<Button>(Resource.Id.ResetPasswordBtn);            
            btnResetRecovery.Click += new EventHandler(btnResetRecovery_Click);            
        }

        void btnResetRecovery_Click(object sender, EventArgs e)
        {
            var email = FindViewById<EditText>(Resource.Id.EditEmail).Text;
            if (!EmailValidation.IsValid (email))
            {
                this.ShowAlert(Resource.String.ResetPasswordInvalidDataTitle, Resource.String.ResetPasswordInvalidDataMessage);                
            }
            else
            {
                AppContext.Current.LastEmail = email;
                ThreadHelper.ExecuteInThread(this, () => TinyIoCContainer.Current.Resolve<IAccountService>().ResetPassword(email), ()
                                                                                                                                  => RunOnUiThread(() => this.ShowAlert(Resource.String.ResetPasswordSucessTitle, Resource.String.ResetPasswordSucessMessage,this.Finish)), true);                               
            }
        }

       
    }
}