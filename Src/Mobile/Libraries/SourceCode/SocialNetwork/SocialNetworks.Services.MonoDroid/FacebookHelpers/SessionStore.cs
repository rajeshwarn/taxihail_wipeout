using Android.Content;

namespace com.facebook.droid
{
    public class SessionStore
    {    
        const string TOKEN = "access_token";
        const string EXPIRES = "expires_in";
        const string KEY = "facebook-session";

		public static string AccessToken { get; set; }
		public static long AccessExpires { get; set; }

		public static bool Save (string accessToken, long accessExpires, Context context)
        {
            var editor = context.GetSharedPreferences (KEY,FileCreationMode.Private).Edit ();
//            editor.PutString (TOKEN, session.AccessToken);
//            editor.PutLong (EXPIRES, session.AccessExpires);
			editor.PutString (TOKEN, accessToken);
			editor.PutLong (EXPIRES, accessExpires);

			return editor.Commit ();
        }

		public static bool Restore ( Context context)
        {
            var savedSession = context.GetSharedPreferences (KEY, FileCreationMode.Private);
//            session.AccessToken = savedSession.GetString (TOKEN, null);
//            session.AccessExpires = savedSession.GetLong (EXPIRES, 0);
			AccessToken = savedSession.GetString (TOKEN, null);
			AccessExpires = savedSession.GetLong (EXPIRES, 0);
			return true;
        }

        public static void Clear (Context context)
        {
            var editor = context.GetSharedPreferences (KEY, FileCreationMode.Private).Edit ();
            editor.Clear ();
            editor.Commit ();
        }    
    }
}