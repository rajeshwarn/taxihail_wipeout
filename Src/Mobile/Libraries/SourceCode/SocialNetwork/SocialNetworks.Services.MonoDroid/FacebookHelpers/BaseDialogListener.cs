using Android.OS;
using Java.Lang;

namespace com.facebook.droid
{
	public abstract class BaseDialogListener : Object, Facebook.IDialogListener
    {
        public abstract void OnComplete (Bundle bundle);
		
        public void OnFacebookError (FacebookError e)
        {
            e.PrintStackTrace ();
        }

        public void OnError (DialogError e)
        {
            e.PrintStackTrace ();        
        }

        public void OnCancel ()
        {        
        }
    }
}