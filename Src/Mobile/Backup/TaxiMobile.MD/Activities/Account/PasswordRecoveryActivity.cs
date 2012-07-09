using System;
using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Widget;
using TaxiMobile.Helpers;
using TaxiMobile.Lib.Data;
using TaxiMobile.Lib.Practices;
using TaxiMobile.Lib.Services;
using TaxiMobile.Validation;

namespace TaxiMobile.Activities.Account
{
    [Activity(Label = "Password Recovery", Theme = "@android:style/Theme.NoTitleBar", ScreenOrientation=ScreenOrientation.Portrait)]
    public class PasswordRecoveryActivity : Activity
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.PasswordRecovery);
            var btnResetRecovery = FindViewById<Button>(Resource.Id.ResetPasswordBtn);
            var cancelRecovery = FindViewById<Button>(Resource.Id.CancelBtn);
            btnResetRecovery.Click += new EventHandler(btnResetRecovery_Click);
            cancelRecovery.Click += new EventHandler(cancelRecovery_Click);

        }

        void cancelRecovery_Click(object sender, EventArgs e)
        {
            Finish();
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
                ThreadHelper.ExecuteInThread(this, () => ServiceLocator.Current.GetInstance<IAccountService>().ResetPassword(new ResetPasswordData { Email = email }), () => this.Finish() , true);                               
            }
        }

       
    }
}