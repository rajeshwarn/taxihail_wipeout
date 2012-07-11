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

using apcurium.Framework.Extensions;
using apcurium.MK.Booking.Mobile.Data;
using apcurium.MK.Booking.Mobile.Client.Helpers;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Booking.Mobile.Client.Validation;
using apcurium.MK.Booking.Api.Contract.Requests;
using TinyIoC;

namespace apcurium.MK.Booking.Mobile.Client.Activities.Account
{
    [Activity(Label = "Sign Up", Theme = "@android:style/Theme.NoTitleBar", ScreenOrientation=Android.Content.PM.ScreenOrientation.Portrait)]
    public class SignUpActivity : Activity
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.SignUp);

            var btnCreate = FindViewById<Button>(Resource.Id.SignUpCreateBtn);
            
            btnCreate.Click += new EventHandler(Create_Click);
        }
        

        void Create_Click(object sender, EventArgs e)
        {
            if (!EmailValidation.IsValid (FindViewById<EditText>(Resource.Id.SignUpEditEmail).Text))
            {
                this.ShowAlert(Resource.String.CreateAccountInvalidDataTitle, Resource.String.ResetPasswordInvalidDataMessage);
            }
            else if (!ValidatePassword())
            {
                this.ShowAlert(Resource.String.CreateAccountInvalidDataTitle, Resource.String.CreateAccountInvalidPassword);             
            }
            else if (!ValidateOtherFields())
            {
                this.ShowAlert(Resource.String.CreateAccountInvalidDataTitle, Resource.String.CreateAccountEmptyField);                             
            }
            else
            {
                ThreadHelper.ExecuteInThread(this, () =>
                    {
                        var service = TinyIoCContainer.Current.Resolve<IAccountService>();
                        var data = GetRegisterAccountData();
                        string error = "";
                        service.Register(data, out error);
                        if (error.HasValue())
                        {
                            this.RunOnUiThread(() => this.ShowAlert(this.GetString(Resource.String.CreateAccountErrorMessage), error));
                            return;
                        }
                        AppContext.Current.LastEmail = data.Email;
                        this.RunOnUiThread(() => Finish());
                    },true );
            }

        }
        private RegisterAccount GetRegisterAccountData()
        {
            var data = new RegisterAccount();
            data.AccountId = Guid.NewGuid();
            data.Password = FindViewById<EditText>(Resource.Id.SignUpPassword).Text;            
            data.Email = FindViewById<EditText>(Resource.Id.SignUpEditEmail).Text;
            data.Name= FindViewById<EditText>(Resource.Id.SignUpName).Text;            
            data.Phone = FindViewById<EditText>(Resource.Id.SignUpPhone).Text;            
            return data;

        }
        private bool ValidateOtherFields()
        {
            var password = FindViewById<EditText>(Resource.Id.SignUpPassword).Text;
            var confirmPassword = FindViewById<EditText>(Resource.Id.SignUpConfirmPassword).Text;
            var email = FindViewById<EditText>(Resource.Id.SignUpEditEmail).Text;
            var name = FindViewById<EditText>(Resource.Id.SignUpName).Text;            
            var phone = FindViewById<EditText>(Resource.Id.SignUpPhone).Text;
            if ((string.IsNullOrEmpty(password)) || (string.IsNullOrEmpty(confirmPassword)) ||
                (string.IsNullOrEmpty(email)) || (string.IsNullOrEmpty(name)) ||
                (string.IsNullOrEmpty(phone)))
            {
                return false;
            }
            return true;

        }

        private bool ValidatePassword()
        {
            var password = FindViewById<EditText>(Resource.Id.SignUpPassword).Text;
            var confirmPassword = FindViewById<EditText>(Resource.Id.SignUpConfirmPassword).Text;
            return password == confirmPassword;

        }
        

    }
}