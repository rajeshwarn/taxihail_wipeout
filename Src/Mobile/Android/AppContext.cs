using System;
using Android.Content;
using Android.Locations;
using Android.OS;
using apcurium.Framework.Extensions;
using apcurium.MK.Booking.Mobile.Data;
using Microsoft.Practices.ServiceLocation;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Booking.Mobile.Client.Diagnostic;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Booking.Mobile.Client.Helpers;


namespace apcurium.MK.Booking.Mobile.Client
{
    public class AppContext :  IAppContext, ILocationListener
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
                    if (_loggedUser != null)
                    {
                        if (_loggedUser.DefaultSettings == null)
                        {
                            _loggedUser.DefaultSettings = new BookingSetting();
                        }
                        if (_loggedUser.DefaultSettings.Company != 12)
                        {
                            _loggedUser.DefaultSettings.Company = 12;
                            _loggedUser.DefaultSettings.CompanyName = "Taxi Diamond";
                        }
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

                    AppContext.Current.LoggedInEmail = value.Email;
                    AppContext.Current.LoggedInPassword = value.Password;
                }
                else
                {
                    var pref = App.GetSharedPreferences(_sharedPreferences, FileCreationMode.Private);
                    pref.Edit().PutString("LoggedUser", "").Commit();
                    AppContext.Current.LoggedInEmail = "";
                    AppContext.Current.LoggedInPassword = "";
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
		
   
		
		public void Dispose()
		{}



        public IntPtr Handle
        {
            get { return App.Handle; }
        }
    }
}