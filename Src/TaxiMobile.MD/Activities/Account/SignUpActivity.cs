using System;
using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Widget;
using TaxiMobile.Helpers;
using TaxiMobile.Lib.Data;
using TaxiMobile.Lib.Framework.Extensions;
using TaxiMobile.Lib.Practices;
using TaxiMobile.Lib.Services;
using TaxiMobile.Validation;

namespace TaxiMobile.Activities.Account
{
    [Activity(Label = "Sign Up", Theme = "@android:style/Theme.NoTitleBar", ScreenOrientation=ScreenOrientation.Portrait)]
    public class SignUpActivity : Activity
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.SignUp);


            Spinner spinnerTitle = (Spinner)FindViewById(Resource.Id.SignUpTitle);
            ArrayAdapter adapter = (ArrayAdapter)ArrayAdapter.CreateFromResource(this, Resource.Array.CreateAccountTitleArray, Android.Resource.Layout.SimpleSpinnerItem);
            adapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
            spinnerTitle.Adapter = adapter;
            spinnerTitle.Focusable = true;
            spinnerTitle.FocusableInTouchMode = true;
            
                    
            spinnerTitle.ItemSelected += (sender, args) =>
                {
                    Console.WriteLine("item selected");
                };

            var btncancel = FindViewById<Button>(Resource.Id.SignUpCancelBtn);
            var btnCreate = FindViewById<Button>(Resource.Id.SignUpCreateBtn);
            btncancel.Click += new EventHandler(Cancel_Click);
            btnCreate.Click += new EventHandler(Create_Click);


        }
        void Cancel_Click(object sender, EventArgs e)
        {
            Finish();
        }
        void Create_Click(object sender, EventArgs e)
        {





            if (!EmailValidation.IsValid (FindViewById<EditText>(Resource.Id.SignUpEditEmail).Text))
            {
                this.ShowAlert(Resource.String.ResetPasswordInvalidDataTitle, Resource.String.ResetPasswordInvalidDataMessage);
            }
            else if (!ValidatePassword())
            {
                this.ShowAlert(Resource.String.ResetPasswordInvalidDataTitle);             
            }
            else if (!ValidateOtherFields())
            {
                this.ShowAlert(Resource.String.CreateAccountEmptyField);                             
            }
            else
            {
                ThreadHelper.ExecuteInThread(this, () =>
                    {
                        var service = ServiceLocator.Current.GetInstance<IAccountService>();
                        var data = GetAccountData();
                        string error = "";
                        service.CreateAccount(data, out error);
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
        private CreateAccountData GetAccountData()
        {
            var data = new CreateAccountData();
            data.Password = FindViewById<EditText>(Resource.Id.SignUpPassword).Text;
            data.Confirm = FindViewById<EditText>(Resource.Id.SignUpConfirmPassword).Text;
            data.Email = FindViewById<EditText>(Resource.Id.SignUpEditEmail).Text;
            data.FirstName = FindViewById<EditText>(Resource.Id.SignUpFirstName).Text;
            data.LastName = FindViewById<EditText>(Resource.Id.SignUpLastName).Text;
            data.Mobile = FindViewById<EditText>(Resource.Id.SignUpMobile).Text;
            data.Phone = FindViewById<EditText>(Resource.Id.SignUpPhone).Text;
            data.Title = FindViewById<Spinner>(Resource.Id.SignUpTitle).SelectedItem.ToSafeString();
            return data;

        }
        private bool ValidateOtherFields()
        {
            var password = FindViewById<EditText>(Resource.Id.SignUpPassword).Text;
            var confirmPassword = FindViewById<EditText>(Resource.Id.SignUpConfirmPassword).Text;
            var email = FindViewById<EditText>(Resource.Id.SignUpEditEmail).Text;
            var firstName = FindViewById<EditText>(Resource.Id.SignUpFirstName).Text;
            var lastName = FindViewById<EditText>(Resource.Id.SignUpLastName).Text;
            var mobile = FindViewById<EditText>(Resource.Id.SignUpMobile).Text;
            var phone = FindViewById<EditText>(Resource.Id.SignUpPhone).Text;
            if ((string.IsNullOrEmpty(password)) || (string.IsNullOrEmpty(confirmPassword)) ||
                (string.IsNullOrEmpty(email)) || (string.IsNullOrEmpty(firstName)) ||
                (string.IsNullOrEmpty(lastName)) || (string.IsNullOrEmpty(mobile)) ||
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