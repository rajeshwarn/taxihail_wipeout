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
using Cirrious.MvvmCross.Interfaces.Views;
using Cirrious.MvvmCross.Views;
using apcurium.MK.Booking.Mobile.ViewModels;
using Cirrious.MvvmCross.Interfaces.ViewModels;
using SocialNetworks.Services;
using Android.App;

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


    public class AppContext :  IAppContext
    {
        private const string _sharedPreferences = "TaxiMobile.SharedPreferences";
		

        
        public static AppContext Current
        {
            get
            {
                return (AppContext)TinyIoCContainer.Current.Resolve<IAppContext>();
            }
        }

        private Account _loggedUser;
        private Context _appContext;
        public AppContext(Context appContext)
        {
            _appContext = appContext;
        }

        

        //public Account LoggedUser
        //{

        //    get
        //    {
        //        if( _loggedUser == null )
        //        {

        //            _loggedUser = TinyIoCContainer.Current.Resolve<ICacheService>().Get<Account>("LoggedUser");

        //            var pref = Application.Context.GetSharedPreferences(_sharedPreferences, FileCreationMode.Private);

        //            var serializedUser = pref.GetString("LoggedUser", "");
        //            if (serializedUser.HasValue())
        //            {
        //                _loggedUser = SerializerHelper.DeserializeObject<Account>(serializedUser);
        //            }
        //            if (_loggedUser != null)
        //            {
        //                if (_loggedUser.Settings == null)
        //                {
        //                    _loggedUser.Settings = new BookingSettings();
        //                }                        
        //            }
        //        }
        //        return _loggedUser;
        //    }
        //    private set
        //    {
        //        _loggedUser = value;
        //        if (value != null)
        //        {
        //            string serializedUser = SerializerHelper.Serialize(value);
        //            var pref = Application.Context.GetSharedPreferences(_sharedPreferences, FileCreationMode.Private);
        //            pref.Edit().PutString("LoggedUser", serializedUser).Commit();

        //            AppContext.Current.LoggedInEmail = value.Email;
                    
        //        }
        //        else
        //        {
        //            var pref = Application.Context.GetSharedPreferences(_sharedPreferences, FileCreationMode.Private);
        //            pref.Edit().PutString("LoggedUser", "").Commit();
        //            pref.Edit().PutString("ServerName", "" ).Commit();
        //            pref.Edit().PutString("ServerVersion", "" ).Commit();
        //            AppContext.Current.LoggedInEmail = "";
        //            AppContext.Current.LoggedInPassword = "";
        //        }
        //    }
        //}

		

        //public void UpdateLoggedInUser(Account data)
        //{

        //    Logger.LogMessage("UpdateLoggedInUser");
        //    if (data != null)
        //    {
        //        Logger.LogMessage("UpdateLoggedInUser != null");
        //        LoggedUser = data;                
        //    }
        //    else
        //    {
        //        Logger.LogMessage("UpdateLoggedInUser == null");
        //    }
        //}
        public string LastEmail
        {

            get
            {
                var pref = Application.Context.GetSharedPreferences(_sharedPreferences, FileCreationMode.Private);
                return pref.GetString( "LastEmail" , "" );
            }
            set
            {
                var pref = Application.Context.GetSharedPreferences(_sharedPreferences, FileCreationMode.Private);
                pref.Edit().PutString( "LastEmail", value ).Commit();
            }
        }

        
        public string LoggedInEmail
        {

            get
            {
                var pref = Application.Context.GetSharedPreferences(_sharedPreferences, FileCreationMode.Private);
                return pref.GetString( "LoggedInEmail" , "" );
            }
            set
            {
                var pref = Application.Context.GetSharedPreferences(_sharedPreferences, FileCreationMode.Private);
                pref.Edit().PutString( "LoggedInEmail", value ).Commit();
            }
        }

        public Guid? LastOrder
        {

            get
            {
                var pref = Application.Context.GetSharedPreferences(_sharedPreferences, FileCreationMode.Private);
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
                    var pref = Application.Context.GetSharedPreferences(_sharedPreferences, FileCreationMode.Private);
                    pref.Edit().PutString("LastOrder", value.Value.ToString()).Commit();
                }
                else
                {
                    var pref = Application.Context.GetSharedPreferences(_sharedPreferences, FileCreationMode.Private);
                    pref.Edit().Remove("LastOrder").Commit();
                }
            }
        }

        

        public string LoggedInPassword
        {

            get
            {
                var pref = Application.Context.GetSharedPreferences(_sharedPreferences, FileCreationMode.Private);
                return pref.GetString( "LoggedInPassword" , "" );
            }
            set
            {
                var pref = Application.Context.GetSharedPreferences(_sharedPreferences, FileCreationMode.Private);
                pref.Edit().PutString( "LoggedInPassword", value ).Commit();
            }
        }
        
		public Location CurrentLocation { get; set; }

   
   
		
		public void Dispose()
		{}



        public IntPtr Handle
        {
            get { return Application.Context.Handle; }
        }
    }
}