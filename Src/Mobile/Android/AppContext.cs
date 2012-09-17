using System;
using Android.Content;
using Android.Locations;
using Android.OS;
using apcurium.Framework.Extensions;
using apcurium.MK.Booking.Mobile.Data;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Booking.Mobile.Client.Diagnostic;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Booking.Mobile.Client.Helpers;
using apcurium.MK.Booking.Api.Contract.Resources;
using TinyIoC;

namespace apcurium.MK.Booking.Mobile.Client
{

    /// <summary>
    /// THIS CLASS IS DEPRECATED.  DON'T ADD ANYTHING TO IT.
    /// TRY TO AVOID USING IT. REFACTOR ANY CODE THAT USE IT
    /// 
    /// THIS CLASS IS DEPRECATED.  DON'T ADD ANYTHING TO IT.
    /// TRY TO AVOID USING IT. REFACTOR ANY CODE THAT USE IT
    ///
    ///
    /// THIS CLASS IS DEPRECATED.  DON'T ADD ANYTHING TO IT.
    /// TRY TO AVOID USING IT. REFACTOR ANY CODE THAT USE IT
    /// 
    /// THIS CLASS IS DEPRECATED.  DON'T ADD ANYTHING TO IT.
    /// TRY TO AVOID USING IT. REFACTOR ANY CODE THAT USE IT
    /// 
    /// THIS CLASS IS DEPRECATED.  DON'T ADD ANYTHING TO IT.
    /// TRY TO AVOID USING IT. REFACTOR ANY CODE THAT USE IT
    /// 
    /// THIS CLASS IS DEPRECATED.  DON'T ADD ANYTHING TO IT.
    /// TRY TO AVOID USING IT. REFACTOR ANY CODE THAT USE IT
    /// 
    /// THIS CLASS IS DEPRECATED.  DON'T ADD ANYTHING TO IT.
    /// TRY TO AVOID USING IT.
    /// 
    /// THIS CLASS IS DEPRECATED.  DON'T ADD ANYTHING TO IT.
    /// TRY TO AVOID USING IT.
    /// 
    /// THIS CLASS IS DEPRECATED.  DON'T ADD ANYTHING TO IT.
    /// TRY TO AVOID USING IT.
    /// 
    /// THIS CLASS IS DEPRECATED.  DON'T ADD ANYTHING TO IT.
    /// TRY TO AVOID USING IT.
    /// 
    /// THIS CLASS IS DEPRECATED.  DON'T ADD ANYTHING TO IT.
    /// TRY TO AVOID USING IT.
    /// 
    /// THIS CLASS IS DEPRECATED.  DON'T ADD ANYTHING TO IT.
    /// TRY TO AVOID USING IT.
    /// 
    /// THIS CLASS IS DEPRECATED.  DON'T ADD ANYTHING TO IT.
    /// TRY TO AVOID USING IT.
    /// 
    /// THIS CLASS IS DEPRECATED.  DON'T ADD ANYTHING TO IT.
    /// TRY TO AVOID USING IT.
    /// 
    /// THIS CLASS IS DEPRECATED.  DON'T ADD ANYTHING TO IT.
    /// TRY TO AVOID USING IT.
    /// 
    /// THIS CLASS IS DEPRECATED.  DON'T ADD ANYTHING TO IT.
    /// TRY TO AVOID USING IT.
    /// 
    /// THIS CLASS IS DEPRECATED.  DON'T ADD ANYTHING TO IT.
    /// TRY TO AVOID USING IT.
    /// 
    /// THIS CLASS IS DEPRECATED.  DON'T ADD ANYTHING TO IT.
    /// TRY TO AVOID USING IT.
    /// 
    /// THIS CLASS IS DEPRECATED.  DON'T ADD ANYTHING TO IT.
    /// TRY TO AVOID USING IT.
    /// 
    /// THIS CLASS IS DEPRECATED.  DON'T ADD ANYTHING TO IT.
    /// TRY TO AVOID USING IT.
    /// 
    /// THIS CLASS IS DEPRECATED.  DON'T ADD ANYTHING TO IT.
    /// TRY TO AVOID USING IT.
    /// 
    /// 
    /// </summary>


    public class AppContext :  IAppContext, ILocationListener
    {
        private const string _sharedPreferences = "TaxiMobile.SharedPreferences";
		private Account _loggedUser;

        
        public static AppContext Current
        {
            get
            {
                return (AppContext)TinyIoCContainer.Current.Resolve<IAppContext>();
            }
        }

        public AppContext(TaxiMobileApplication app)
        {
            App = app;
          
        }
         

        public TaxiMobileApplication App { get; set; }
        public Context Context { get; set; }

        public Account LoggedUser
        {

            get
            {
                if( _loggedUser == null )
				{
					var pref = App.GetSharedPreferences(_sharedPreferences, FileCreationMode.Private);
	                var serializedUser = pref.GetString("LoggedUser", "");
	                if (serializedUser.HasValue())
	                {
	                    _loggedUser = SerializerHelper.DeserializeObject<Account>(serializedUser);
	                }
                    if (_loggedUser != null)
                    {
                        if (_loggedUser.Settings == null)
                        {
                            _loggedUser.Settings = new BookingSettings();
                        }
                        /*if (_loggedUser.Settings.ProviderId != 12)
                        {
                            _loggedUser.Settings.ProviderId = 12;                            
                        }*/
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
                    
                }
                else
                {
                    var pref = App.GetSharedPreferences(_sharedPreferences, FileCreationMode.Private);
                    pref.Edit().PutString("LoggedUser", "").Commit();
					pref.Edit().PutString("ServerName", "" ).Commit();
					pref.Edit().PutString("ServerVersion", "" ).Commit();
					AppContext.Current.LoggedInEmail = "";
                    AppContext.Current.LoggedInPassword = "";
                }
            }
        }

		

        public void UpdateLoggedInUser(Account data, bool syncWithServer)
        {

            Logger.LogMessage("UpdateLoggedInUser");
            if (data != null)
            {
                Logger.LogMessage("UpdateLoggedInUser != null");
                LoggedUser = data;                
                if (syncWithServer)
                {
                    TinyIoCContainer.Current.Resolve<IAccountService>().UpdateUser(data);
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
            set
            {
                var pref = App.GetSharedPreferences(_sharedPreferences, FileCreationMode.Private);
                pref.Edit().PutString( "LoggedInEmail", value ).Commit();
            }
        }

        public Guid? LastOrder
        {

            get
            {
                 var pref = App.GetSharedPreferences(_sharedPreferences, FileCreationMode.Private);
                 var orderId = pref.GetString("LastOrder", null);
                 Guid r;
                 if (  ( orderId.HasValue() ) && ( Guid.TryParse( orderId, out r ) ))
                 {
                     //r.ToString();
                     
                     //return null;
                     return r;
                 }
                 else
                 {
                    return null;
                 }

            }
            set
            {
                if (value.HasValue)
                {
                    var pref = App.GetSharedPreferences(_sharedPreferences, FileCreationMode.Private);
                    pref.Edit().PutString("LastOrder", value.Value.ToString()).Commit();
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
            set
            {
                var pref = App.GetSharedPreferences(_sharedPreferences, FileCreationMode.Private);
                pref.Edit().PutString( "LoggedInPassword", value ).Commit();
            }
        }
        
		public Location CurrentLocation { get; set; }

        public void SignOut()
        {
            Logger.LogMessage("SignOutUser");      
            LoggedUser = null;			
			TinyIoCContainer.Current.Resolve<IAccountService>().SignOut();
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