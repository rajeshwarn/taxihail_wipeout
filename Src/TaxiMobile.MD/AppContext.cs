using System;
using Android.Content;
using Android.Locations;
using Android.OS;
using TaxiMobile.Diagnostic;
using TaxiMobile.Helpers;
using TaxiMobile.Lib.Data;
using TaxiMobile.Lib.Framework.Extensions;
using TaxiMobile.Lib.Infrastructure;
using TaxiMobile.Lib.Practices;
using TaxiMobile.Lib.Services;

namespace TaxiMobile
{
    public class AppContext : IAppContext, ILocationListener
    {
        private const string _sharedPreferences = "TaxiMobile.SharedPreferences";
		private AccountData _loggedUser;

        
        public static AppContext Current
        {
            get
            {
                return (AppContext)ServiceLocator.Current.GetInstance<IAppContext>();
            }
        }

        public AppContext(TaxiMobileApplication app)
        {
            App = app;
          
        }
         

        public TaxiMobileApplication App { get; set; }
        public Context Context { get; set; }

        public AccountData LoggedUser
        {

            get
            {
                if( _loggedUser == null )
				{
					var pref = App.GetSharedPreferences(_sharedPreferences, FileCreationMode.Private);
	                var serializedUser = pref.GetString("LoggedUser", "");
	                if (serializedUser.HasValue())
	                {
	                    _loggedUser = SerializerHelper.DeserializeObject<AccountData>(serializedUser);
	                }
//	                else
//	                {
//	                    return null;
//	                }
				}
				return _loggedUser;
            }
            private set
            {
				_loggedUser = value;
                if (value != null)
                {
                    string serializedUser = SerializerHelper.Serialize(value);
                    var pref = App.GetSharedPreferences(_sharedPreferences, FileCreationMode.Private);
                    pref.Edit().PutString("LoggedUser", serializedUser).Commit();

                    Current.LoggedInEmail = value.Email;
                    Current.LoggedInPassword = value.Password;
                }
                else
                {
                    var pref = App.GetSharedPreferences(_sharedPreferences, FileCreationMode.Private);
                    pref.Edit().PutString("LoggedUser", "").Commit();
                    Current.LoggedInEmail = "";
                    Current.LoggedInPassword = "";
                }
            }
        }


        public void UpdateLoggedInUser(AccountData data, bool syncWithServer)
        {

            Logger.LogMessage("UpdateLoggedInUser");
            if (data != null)
            {
                Logger.LogMessage("UpdateLoggedInUser != null");
                LoggedUser = data;                
                if (syncWithServer)
                {
                    ServiceLocator.Current.GetInstance<IAccountService>().UpdateUser(data);
                }
            }
            else
            {
                Logger.LogMessage("UpdateLoggedInUser == null");
            }
        }
        public string LastEmail
        {

            get
            {
                var pref = App.GetSharedPreferences(_sharedPreferences, FileCreationMode.Private);
                return pref.GetString( "LastEmail" , "" );
            }
            set
            {
                var pref = App.GetSharedPreferences(_sharedPreferences, FileCreationMode.Private);
                pref.Edit().PutString( "LastEmail", value ).Commit();
            }
        }

        
        public string LoggedInEmail
        {

            get
            {
                var pref = App.GetSharedPreferences(_sharedPreferences, FileCreationMode.Private);
                return pref.GetString( "LoggedInEmail" , "" );
            }
            private set
            {
                var pref = App.GetSharedPreferences(_sharedPreferences, FileCreationMode.Private);
                pref.Edit().PutString( "LoggedInEmail", value ).Commit();
            }
        }

        public int? LastOrder
        {

            get
            {
                var pref = App.GetSharedPreferences(_sharedPreferences, FileCreationMode.Private);
                var o = pref.GetInt("LastOrder", 0);
                if ( o == 0 )
                {
                    return null;
                }
                else
                {
                    return o;
                }

            }
            set
            {
                if (value.HasValue)
                {
                    var pref = App.GetSharedPreferences(_sharedPreferences, FileCreationMode.Private);
                    pref.Edit().PutInt("LastOrder", value.Value).Commit();
                }
                else
                {
                    var pref = App.GetSharedPreferences(_sharedPreferences, FileCreationMode.Private);
                    pref.Edit().Remove("LastOrder").Commit();
                }
            }
        }

        

        public string LoggedInPassword
        {

            get
            {
                var pref = App.GetSharedPreferences(_sharedPreferences, FileCreationMode.Private);
                return pref.GetString( "LoggedInPassword" , "" );
            }
            private set
            {
                var pref = App.GetSharedPreferences(_sharedPreferences, FileCreationMode.Private);
                pref.Edit().PutString( "LoggedInPassword", value ).Commit();
            }
        }
        
        public Location CurrentLocation { get; set; }

        public void SignOut()
        {
            LoggedUser = null;
        }

        public void OnLocationChanged(Location location)
        {
            if ( ( CurrentLocation == null ) || ( CurrentLocation.Accuracy > location.Accuracy ) )
            {
                CurrentLocation = location;
            }
        }

        public void OnProviderDisabled(string provider)
        {
        
        }

        public void OnProviderEnabled(string provider)
        {
        
        }

        public void OnStatusChanged(string provider, Availability status, Bundle extras)
        {
        
        }
		
        //public int? TripIdToRebook {
        //    get;
        //    set;
        //}

        
        public IntPtr Handle
        {
            get { return App.Handle; }
        }
    }
}