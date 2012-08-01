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

namespace apcurium.MK.Booking.Mobile.Client.Activities.Account
{
    [Activity(Label = "Sign Up", Theme = "@android:style/Theme.NoTitleBar", ScreenOrientation=Android.Content.PM.ScreenOrientation.Portrait)]
    public class SignUpActivity : Activity
    {
        private bool IsCreatedFromSocial;
        private Bundle b;
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.SignUp);
            IsCreatedFromSocial = false;
            b = Intent.Extras;
            //Si on vient de facebook ou twitter on prerempli les champs
            if(b!=null)
            {
                FindViewById<EditText>(Resource.Id.SignUpEditEmail).Text = b.GetString("email");
                FindViewById<EditText>(Resource.Id.SignUpName).Text = b.GetString("firstName") + " " + b.GetString("lastName");
                FindViewById<EditText>(Resource.Id.SignUpPhone).Text = b.GetString("phone");
                FindViewById<EditText>(Resource.Id.SignUpPassword).Visibility = ViewStates.Invisible;
                FindViewById<EditText>(Resource.Id.SignUpConfirmPassword).Visibility = ViewStates.Invisible;
                IsCreatedFromSocial = true;
            }

            var btnCreate = FindViewById<Button>(Resource.Id.SignUpCreateBtn);
            
            btnCreate.Click += new EventHandler(Create_Click);
        }
        

        void Create_Click(object sender, EventArgs e)
        {
            if (!EmailValidation.IsValid (FindViewById<EditText>(Resource.Id.SignUpEditEmail).Text))
            {
                this.ShowAlert(Resource.String.CreateAccountInvalidDataTitle, Resource.String.ResetPasswordInvalidDataMessage);
            }
            else if (!ValidatePassword() && !IsCreatedFromSocial)
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
                        var data = GetRegisterAccount();
                        string error = "";
                        service.Register(data, out error);
                        if (error.HasValue())
                        {
                            this.RunOnUiThread(() => this.ShowAlert(this.GetString(Resource.String.CreateAccountErrorMessage), error));
                            return;
                        }
                        AppContext.Current.LastEmail = data.Email;
                        this.RunOnUiThread(() =>
                                               {
                                                   Finish();
                                                   if(IsCreatedFromSocial)
                                                   {
                                                       StartActivity(typeof(MainActivity));
                                                   }
                                                   
                                               });
                    }, true);
            }

        }
        private RegisterAccount GetRegisterAccount()
        {
            var data = new RegisterAccount();
            data.Password = FindViewById<EditText>(Resource.Id.SignUpPassword).Text;            
            data.Email = FindViewById<EditText>(Resource.Id.SignUpEditEmail).Text;
            data.Name= FindViewById<EditText>(Resource.Id.SignUpName).Text;            
            data.Phone = FindViewById<EditText>(Resource.Id.SignUpPhone).Text;
            if (IsCreatedFromSocial)
            {
                data.FacebookId = string.IsNullOrEmpty(b.GetString("facebookId")) ? null : b.GetString("facebookId");
                data.TwitterId = string.IsNullOrEmpty(b.GetString("twitterId")) ? null : b.GetString("twitterId");
            }
            
            return data;

        }
        private bool ValidateOtherFields()
        {
            var password = FindViewById<EditText>(Resource.Id.SignUpPassword).Text;
            var confirmPassword = FindViewById<EditText>(Resource.Id.SignUpConfirmPassword).Text;
            var email = FindViewById<EditText>(Resource.Id.SignUpEditEmail).Text;
            var name = FindViewById<EditText>(Resource.Id.SignUpName).Text;            
            var phone = FindViewById<EditText>(Resource.Id.SignUpPhone).Text;
            if (!IsCreatedFromSocial)
            {
                if ((string.IsNullOrEmpty(password)) || (string.IsNullOrEmpty(confirmPassword))  ||
                (string.IsNullOrEmpty(email)) || (string.IsNullOrEmpty(name)) ||
                (string.IsNullOrEmpty(phone)))
                {
                    return false;
                }
            }
            
            if (IsCreatedFromSocial)
            {
                if ((string.IsNullOrEmpty(email)) || (string.IsNullOrEmpty(name)) ||
                (string.IsNullOrEmpty(phone)))
                {
                    return false;
                }
            }
            
            return true;

        }

        private bool ValidatePassword()
        {
            var password = FindViewById<EditText>(Resource.Id.SignUpPassword).Text;
            var confirmPassword = FindViewById<EditText>(Resource.Id.SignUpConfirmPassword).Text;
            if (password.Length>=6 && password.Equals(confirmPassword))
            {
                return true;
            }
            else
            {
                return false;
            }

        }
        

    }
}
