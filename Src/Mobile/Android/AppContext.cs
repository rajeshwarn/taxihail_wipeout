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

        public AppContext(Context appContext)
        {
        }

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