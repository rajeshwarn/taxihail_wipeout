using System;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Widget;
using SocialNetworks.Services;
using SocialNetworks.Services.Entities;
using SocialNetworks.Services.MonoDroid;
using apcurium.Framework.Extensions;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Booking.Mobile.Client.Helpers;
using apcurium.MK.Booking.Mobile.Client.Validation;
using Android.Graphics;
using Android.Views;
using TinyIoC;
using apcurium.MK.Booking.Mobile.Infrastructure;


namespace apcurium.MK.Booking.Mobile.Client.Activities.Account
{
    [Activity(Label = "Login", Theme = "@android:style/Theme.NoTitleBar", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class LoginActivity : Activity
    {
        private ProgressDialog _progressDialog;
        /// <summary>
        /// use for SSO when FB app is isntalled
        /// </summary>
        /// <param name='requestCode'>
        /// Request code.
        /// </param>
        /// <param name='resultCode'>
        /// Result code.
        /// </param>
        /// <param name='data'>
        /// Data.
        /// </param>
        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            var facebook = TinyIoC.TinyIoCContainer.Current.Resolve<IFacebookService>();
            facebook.SetCurrentContext(this);
            (facebook as FacebookServicesMD).AuthorizeCallback(requestCode, (int)resultCode, data);
        }

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            SplashActivity.TopActivity = this;

            SetContentView(Resource.Layout.Login);

            var facebook = TinyIoC.TinyIoCContainer.Current.Resolve<IFacebookService>();
            facebook.SetCurrentContext(this);

            var twitterService = TinyIoC.TinyIoCContainer.Current.Resolve<ITwitterService>();
            twitterService.SetLoginContext(this);

            facebook.ConnectionStatusChanged -= HandleFacebookConnection;
            facebook.ConnectionStatusChanged += HandleFacebookConnection;

            twitterService.ConnectionStatusChanged -= HandleTwitterConnection;
            twitterService.ConnectionStatusChanged += HandleTwitterConnection;

            _progressDialog = new ProgressDialog(this);

            if (AppContext.Current.LastEmail.HasValue())
            {
                FindViewById<EditText>(Resource.Id.Username).Text = AppContext.Current.LastEmail;
            }


            FindViewById<Button>(Resource.Id.SignUpButton).Click += new EventHandler(SignUpButton_Click);

            FindViewById<Button>(Resource.Id.SignInButton).Click += new EventHandler(btnSignIn_Click);

            FindViewById<Button>(Resource.Id.ForgotPasswordButton).Click += new EventHandler(ForgotPassword_Click);

            if (TinyIoCContainer.Current.Resolve<IAppSettings>().FacebookEnabled)
            {

                FindViewById<Button>(Resource.Id.FacebookButton).Click += delegate
                                                                              {
                                                                                  ShowProgressDialog();
                                                                                  if (facebook.IsConnected)
                                                                                  {
                                                                                      facebook.GetUserInfos(CheckIfFacebookAccountExist, () =>
                                                                                      {
                                                                                          facebook.Disconnect();
                                                                                          HideProgressDialog();
                                                                                      });
                                                                                  }
                                                                                  else
                                                                                  {
                                                                                      facebook.Connect("email, publish_stream, publish_actions");
                                                                                  }

                                                                              };
            }
            else
            {
                FindViewById<Button>(Resource.Id.FacebookButton).Visibility = ViewStates.Gone;
            }



            if (TinyIoCContainer.Current.Resolve<IAppSettings>().CanChangeServiceUrl)
            {
                FindViewById<Button>(Resource.Id.ServerButton).Click += delegate
                {
                    PromptServer();
                };
            }
            else
            {
                FindViewById<Button>(Resource.Id.ServerButton).Visibility = ViewStates.Gone;
            }

            if (TinyIoCContainer.Current.Resolve<IAppSettings>().TwitterEnabled)
            {

                FindViewById<Button>(Resource.Id.TwitterButton).Click += delegate
                    {
                        if (twitterService.IsConnected)
                        {
                            twitterService.GetUserInfos(CheckIfTwitterAccountExist);
                        }
                        else
                        {
                            twitterService.Connect();
                        }
                    };
            }
            else
            {
                FindViewById<Button>(Resource.Id.TwitterButton).Visibility = ViewStates.Gone;
            }

            facebook.ConnectionStatusChanged -= HandleFacebookConnection;
            facebook.ConnectionStatusChanged += HandleFacebookConnection;

            twitterService.ConnectionStatusChanged += (s, e) =>
            {
                if (e.IsConnected)
                {

                    twitterService.GetUserInfos(CheckIfTwitterAccountExist);
                }
            };

        }

        private void HideProgressDialog()
        {
            RunOnUiThread(() =>
            {
                if (_progressDialog != null)
                {
                    _progressDialog.Dismiss();
                    _progressDialog = null;
                }
            });
        }

        private void HandleFacebookConnection(object sender, FacebookStatus e)
        {
            if (e.IsConnected)
            {
                var facebook = TinyIoC.TinyIoCContainer.Current.Resolve<IFacebookService>();
                facebook.ConnectionStatusChanged -= HandleFacebookConnection;
                facebook.ConnectionStatusChanged += HandleFacebookConnection;
                facebook.GetUserInfos(CheckIfFacebookAccountExist, () =>
                {
                    facebook.Disconnect();
                    HideProgressDialog();
                });
            }
            else
            {
                HideProgressDialog();
            }
        }

        private void HandleTwitterConnection(object sender, TwitterStatus e)
        {
            if (e.IsConnected)
            {
                var twitter = TinyIoC.TinyIoCContainer.Current.Resolve<ITwitterService>();
                twitter.ConnectionStatusChanged -= HandleTwitterConnection;
                twitter.GetUserInfos(CheckIfTwitterAccountExist);
            }
        }

        private void ShowProgressDialog()
        {
            RunOnUiThread(() =>
            {

                _progressDialog = ProgressDialog.Show(this, "", this.GetString(Resource.String.LoadingMessage), true, false);
                _progressDialog.Show();

            });
        }

        private void PromptServer()
        {
            AlertDialog.Builder alert = new AlertDialog.Builder(this);
            alert.SetTitle("Server Configuration");
            alert.SetMessage("Enter Server Url");

            var input = new EditText(this);
            
            input.Text = TinyIoCContainer.Current.Resolve<IAppSettings>().ServiceUrl;
            alert.SetView(input);



            alert.SetPositiveButton("Ok", (s, e) =>
                {
                    var serverUrl = input.Text;
                    
                    TinyIoCContainer.Current.Resolve<IAppSettings>().ServiceUrl = serverUrl;
                });

            alert.SetNegativeButton("Cancel", (s, e) =>
            {
                
            });

            alert.Show();
        }

        private void CancelAction(object sender, EventArgs e)
        {
            _progressDialog.CancelEvent -= CancelAction;
            _progressDialog.Cancel();
        }


        private void CheckIfFacebookAccountExist(UserInfos infos)
        {
            //ShowProgressDialog();

            string err = "";
            Api.Contract.Resources.Account account = null;

            account =
                TinyIoC.TinyIoCContainer.Current.Resolve<IAccountService>().GetFacebookAccount(
                    infos.Id,
                    out err);
            if (account != null)
            {
                AppContext.Current.UpdateLoggedInUser(account, false);
                AppContext.Current.LastEmail = account.Email;
                RunOnUiThread(() =>
                    {
                        _progressDialog.Dismiss();
                        this.Finish();
                        this.StartActivity(typeof(MainActivity));
                    });
                return;
            }
            else
            {
                RunOnUiThread(() => _progressDialog.Dismiss());
                DoSignUpWithParameter(infos.Firstname, infos.Lastname, infos.Email, "",
                                      FacebookId: infos.Id);
            }
        }

        private void CheckIfTwitterAccountExist(UserInfos infos)
        {
            ShowProgressDialog();
            string err = "";
            Api.Contract.Resources.Account account = null;
            try
            {
                account = TinyIoC.TinyIoCContainer.Current.Resolve<IAccountService>().GetTwitterAccount(infos.Id, out err);
            }
            catch (Exception)
            {

                account = null;
            }
            if (account != null)
            {

                AppContext.Current.UpdateLoggedInUser(account, false);
                AppContext.Current.LastEmail = account.Email;

                RunOnUiThread(() =>
                {
                    _progressDialog.Dismiss();
                    this.Finish();
                    this.StartActivity(typeof(MainActivity));
                });
                return;
            }
            else
            {
                RunOnUiThread(() => _progressDialog.Dismiss());
                DoSignUpWithParameter(infos.Firstname, infos.Lastname, infos.Email, "", TwitterId: infos.Id);
            }
            //}, true));
        }

        protected override void OnResume()
        {
            base.OnResume();

            if (AppContext.Current.LastEmail.HasValue())
            {
                FindViewById<EditText>(Resource.Id.Username).Text = AppContext.Current.LastEmail;
            }
        }


        protected override void OnDestroy()
        {
            base.OnDestroy();
            var facebook = TinyIoC.TinyIoCContainer.Current.Resolve<IFacebookService>();
            facebook.ConnectionStatusChanged -= (HandleFacebookConnection);
            var twitter = TinyIoC.TinyIoCContainer.Current.Resolve<ITwitterService>();
            twitter.ConnectionStatusChanged -= (HandleTwitterConnection);


            UnbindDrawables(this.FindViewById(Resource.Id.RootView));
            GC.Collect();
        }

        private void UnbindDrawables(View view)
        {
            if (view.Background != null)
            {
                view.Background.SetCallback(null);
            }
            if (view is ViewGroup)
            {
                for (int i = 0; i < ((ViewGroup)view).ChildCount; i++)
                {
                    UnbindDrawables(((ViewGroup)view).GetChildAt(i));
                }
                ((ViewGroup)view).RemoveAllViews();
            }
        }

        void SignUpButton_Click(object sender, EventArgs e)
        {
            DoSignUp();
        }
        void ForgotPassword_Click(object sender, EventArgs e)
        {
            PasswordRecovery();
        }

        private void PasswordRecovery()
        {
            StartActivity(typeof(PasswordRecoveryActivity));
        }
        void btnSignIn_Click(object sender, EventArgs e)
        {
            DoSignIn();
        }

        private void DoSignIn()
        {
            ThreadHelper.ExecuteInThread(this, () =>
            {

                EditText txtUserName = FindViewById<EditText>(Resource.Id.Username);
                EditText txtPassword = FindViewById<EditText>(Resource.Id.Password);

                bool isValid = EmailValidation.IsValid(txtUserName.Text);

                if (isValid)
                {
                    string err = "";
                    var account = TinyIoC.TinyIoCContainer.Current.Resolve<IAccountService>().GetAccount(txtUserName.Text, txtPassword.Text, out err);
                    if (account != null)
                    {
                        AppContext.Current.UpdateLoggedInUser(account, false);
						AppContext.Current.ServerName = TinyIoCContainer.Current.Resolve<IApplicationInfoService>().GetServerName();
						AppContext.Current.ServerVersion = TinyIoCContainer.Current.Resolve<IApplicationInfoService>().GetServerVersion();
                        AppContext.Current.LastEmail = account.Email;
                        RunOnUiThread(() =>
                        {
                            Finish();
                            StartActivity(typeof(MainActivity));
                        });
                        return;
                    }
                }

                RunOnUiThread(() => this.ShowAlert(Resource.String.InvalidLoginMessageTitle, Resource.String.InvalidLoginMessage));


            }, true);
        }
        private void DoSignUp()
        {
            StartActivity(typeof(SignUpActivity));
        }

        private void DoSignUpWithParameter(string FirstName, string LastName, string Email, string Phone, string TwitterId = "", string FacebookId = "")
        {
            Intent intent = new Intent(this, typeof(SignUpActivity));
            Bundle b = new Bundle();
            b.PutString("firstName", FirstName);
            b.PutString("lastName", LastName);
            b.PutString("email", Email);
            b.PutString("phone", Phone);
            b.PutString("twitterId", TwitterId);
            b.PutString("facebookId", FacebookId);
            intent.PutExtras(b);
            if (TwitterId != string.Empty
                || FacebookId != string.Empty)
            {
                Finish();
            }
            StartActivity(intent);
        }
    }
}