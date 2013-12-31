using Android.App;
using Android.OS;


namespace apcurium.MK.Booking.Mobile.Client.Activities
{
    [Activity(Theme = "@android:style/Theme.Dialog")]
    public class ShowDialogActivity : Activity
    {
        private ProgressDialog _progressDialog;
        private bool _show;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            _progressDialog = new ProgressDialog(this);
            _show = bool.Parse(Intent.GetStringExtra("Show"));
        }

        protected override void OnStart()
        {
            base.OnStart();
            Process();
        }

        private void Process()
        {
            if (_show)
            {
                _progressDialog = ProgressDialog.Show(this, "", GetString(Resource.String.LoadingMessage), true, false);
            }
            else
            {
                if (_progressDialog.IsShowing)
                {
                    _progressDialog.Dismiss();
                    Finish();
                }
            }
        }

        protected override void OnPause()
        {
            base.OnPause();
            if (_progressDialog.IsShowing)
            {
                _progressDialog.Dismiss();
            }
        }
    }
}