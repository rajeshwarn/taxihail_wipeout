using System;
using System.Collections.Generic;

namespace Com.Facebook.Android
{
    public class SessionEvents
    {

        private static LinkedList<IAuthListener> mAuthListeners = 
            new LinkedList<IAuthListener> ();
        private static LinkedList<ILogoutListener> mLogoutListeners = 
            new LinkedList<ILogoutListener> ();

        /**
     * Associate the given listener with this Facebook object. The listener's
     * callback interface will be invoked when authentication events occur.
     * 
     * @param listener
     *            The callback object for notifying the application when auth
     *            events happen.
     */
        public static void AddAuthListener (IAuthListener listener)
        {
            mAuthListeners.AddLast (listener);
        }

        /**
     * Remove the given listener from the list of those that will be notified
     * when authentication events occur.
     * 
     * @param listener
     *            The callback object for notifying the application when auth
     *            events happen.
     */
        public static void RemoveAuthListener (IAuthListener listener)
        {
            mAuthListeners.Remove (listener);
        }

        /**
     * Associate the given listener with this Facebook object. The listener's
     * callback interface will be invoked when logout occurs.
     * 
     * @param listener
     *            The callback object for notifying the application when log out
     *            starts and finishes.
     */
        public static void AddLogoutListener (ILogoutListener listener)
        {
            mLogoutListeners.AddLast (listener);
        }

        /**
     * Remove the given listener from the list of those that will be notified
     * when logout occurs.
     * 
     * @param listener
     *            The callback object for notifying the application when log out
     *            starts and finishes.
     */
        public static void RemoveLogoutListener (ILogoutListener listener)
        {
            mLogoutListeners.Remove (listener);
        }
    
        public static void OnLoginSuccess ()
        {
            foreach (var listener in mAuthListeners) {
                listener.OnAuthSucceed ();
            }
        }
    
        public static void OnLoginError (String error)
        {
            foreach (var listener in mAuthListeners) {
                listener.OnAuthFail (error);
            }
        }
    
        public static void OnLogoutBegin ()
        {
            foreach (var l in mLogoutListeners) {
                l.OnLogoutBegin ();
            }
        }
    
        public static void OnLogoutFinish ()
        {
            foreach (var l in mLogoutListeners) {
                l.OnLogoutFinish ();
            }   
        }
    
        /**
     * Callback interface for authorization events.
     *
     */
        public interface IAuthListener
        {

            /**
         * Called when a auth flow completes successfully and a valid OAuth 
         * Token was received.
         * 
         * Executed by the thread that initiated the authentication.
         * 
         * API requests can now be made.
         */
            void OnAuthSucceed ();

            /**
         * Called when a login completes unsuccessfully with an error. 
         *  
         * Executed by the thread that initiated the authentication.
         */
            void OnAuthFail (String error);
        }
    
        /**
     * Callback interface for logout events.
     *
     */ 
        public interface ILogoutListener
        {
            /**
         * Called when logout begins, before session is invalidated.  
         * Last chance to make an API call.  
         * 
         * Executed by the thread that initiated the logout.
         */
            void OnLogoutBegin ();

            /**
         * Called when the session information has been cleared.
         * UI should be updated to reflect logged-out state.
         * 
         * Executed by the thread that initiated the logout.
         */
            void OnLogoutFinish ();
        }
    
    }
}