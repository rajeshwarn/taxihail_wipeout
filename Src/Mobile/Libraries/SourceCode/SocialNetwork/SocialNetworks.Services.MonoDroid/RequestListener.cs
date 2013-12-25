using Android.Util;
using Java.IO;
using Java.Lang;
using Java.Net;
using System;

namespace com.facebook.droid
{
    public class RequestListener : Java.Lang.Object, AsyncFacebookRunner.IRequestListener
    {
		
		Action<string, Java.Lang.Object> _onCompleteHandler;
		
		public RequestListener (Action<string, Java.Lang.Object> onCompleteHandler)
		{
			_onCompleteHandler = onCompleteHandler;
		}

        public void OnFacebookError (FacebookError e, Java.Lang.Object state)
        {
            Log.Error ("Facebook", e.Message);
            e.PrintStackTrace ();
        }

        public void OnFileNotFoundException (FileNotFoundException e,
                                             Java.Lang.Object state)
        {
            Log.Error ("Facebook", e.Message);
            e.PrintStackTrace ();
        }

        public void OnIOException (Java.IO.IOException e, Java.Lang.Object state)
        {
            Log.Error ("Facebook", e.Message);
            e.PrintStackTrace ();
        }

        public void OnMalformedURLException (MalformedURLException e,
                                             Java.Lang.Object state)
        {
            Log.Error ("Facebook", e.Message);
            e.PrintStackTrace ();
        }    

        public void OnComplete (string response, Java.Lang.Object state)
		{
			_onCompleteHandler(response, state);	
		}
    }
}