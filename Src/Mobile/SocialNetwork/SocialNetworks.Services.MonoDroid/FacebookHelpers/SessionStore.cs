using Android.Content;

namespace Com.Facebook.Android
{
    public class SessionStore
    {    
        const string TOKEN = "access_token";
        const string EXPIRES = "expires_in";
        const string KEY = "facebook-session";
    
        public static bool Save (Facebook session, Context context)
        {
            var editor = context.GetSharedPreferences (KEY,FileCreationMode.Private).Edit ();
            editor.PutString (TOKEN, session.AccessToken);
            editor.PutLong (EXPIRES, session.AccessExpires);
            return editor.Commit ();
        }

        public static bool Restore (Facebook session, Context context)
        {
            var savedSession = context.GetSharedPreferences (KEY, FileCreationMode.Private);
            session.AccessToken = savedSession.GetString (TOKEN, null);
            session.AccessExpires = savedSession.GetLong (EXPIRES, 0);
            return session.IsSessionValid;
        }

        public static void Clear (Context context)
        {
            var editor = context.GetSharedPreferences (KEY, FileCreationMode.Private).Edit ();
            editor.Clear ();
            editor.Commit ();
        }    
    }
}