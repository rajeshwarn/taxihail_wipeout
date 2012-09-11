using System;
using System.Collections.Generic;
using System.Linq;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using TinyIoC;
using apcurium.Framework.Extensions;
using apcurium.MK.Booking.Mobile.Practices;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Mobile.Infrastructure;
using ServiceStack.Text;
using SocialNetworks.Services.MonoTouch;
using SocialNetworks.Services;

namespace apcurium.MK.Booking.Mobile.Client
{
    public class Application
    {
        static void Main(string[] args)
        {
            try
            {
                UIApplication.Main(args);
                
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
            }
        }
        
    }

    // The name AppDelegate is referenced in the MainWindow.xib file.
    public partial class AppDelegate : UIApplicationDelegate
    {
        private RootTabController _tabBarController;
        private bool _callbackFromFB = false;
        // This method is invoked when the application has loaded its UI and its ready to run
        public override bool FinishedLaunching(UIApplication app, NSDictionary options)
        {


            Background.Load(window, "Assets/background_full_nologo.png", false, 0, 0);          
            
            AppContext.Initialize(window);
            
            Logger.LogMessage("Context initialized");

            _tabBarController = new RootTabController();

            window.RootViewController = _tabBarController;

            Logger.LogMessage("MainTabController initialized");
            
            AppContext.Current.Controller = _tabBarController;

            new Bootstrapper(new IModule[] { new AppModule() }).Run();
            
            ThreadHelper.ExecuteInThread(() =>
            {
                try
                {
                    InvokeOnMainThread(() =>
                    {


                        SetUIDefaults();

                        _tabBarController.Load();

                        window.AddSubview(_tabBarController.View);

                        if (AppContext.Current.LoggedUser == null)
                        {
                            _tabBarController.ViewControllers[0].PresentModalViewController(new LoginView(), true);
                        }

                        window.MakeKeyAndVisible();
                        
                    }
                    );

                }
                catch (Exception ex)
                {
                    Logger.LogError(ex);
                }
            }
            );
            
            ThreadHelper.ExecuteInThread(() => TinyIoCContainer.Current.Resolve<IAccountService>().EnsureListLoaded());

            return true;
        }

        private void SetUIDefaults()
        {
            var buttonAtt = new UITextAttributes{ TextColor = AppStyle.LightCorporateColor, TextShadowColor = UIColor.Clear };
            UIBarButtonItem.Appearance.SetTitleTextAttributes(buttonAtt, UIControlState.Normal);

        }
        // This method is required in iPhoneOS 3.0
        public override void OnActivated(UIApplication application)
        {
            Logger.LogMessage("OnActivated");

            JsConfig.RegisterTypeForAot<OrderStatus>();
            JsConfig.RegisterTypeForAot<OrderStatusDetail>();
         
            if (!_callbackFromFB)
            {

                if (AppContext.Current.LoggedUser != null)
                {
                    ThreadHelper.ExecuteInThread(() =>
                    {
                        try
                        {

                            AppContext.Current.Controller.View.InvokeOnMainThread(() => {
                                if ((AppContext.Current.Controller.TopViewController != null) && (AppContext.Current.Controller.TopViewController is BookView))
                                {
                                    AppContext.Current.Controller.SelectedRefreshableViewController.RefreshData();
                                }                       
                                if ((AppContext.Current.Controller.SelectedRefreshableViewController != null) && (!(AppContext.Current.Controller.TopViewController  is BookView)))
                                {
                                    AppContext.Current.Controller.SelectedRefreshableViewController.RefreshData(); 
                                }

                            });

                        }
                        catch (Exception ex)
                        {
                            Logger.LogError(ex);
                        }
                        finally
                        {
                        
                        }
                    }
                    );
                
                }
                else
                {
                    if (_tabBarController.TopViewController != null)
                    {   
                        InvokeOnMainThread(() => _tabBarController.TopViewController.PresentModalViewController(new LoginView(), true));
                    }
                }
            }
            else
            {
                _callbackFromFB = false;
            }           
        }

        public override void ReceiveMemoryWarning(UIApplication application)
        {
            AppContext.Current.ReceiveMemoryWarning = true;
            Logger.LogMessage("ReceiveMemoryWarning");
        }
        
        public override bool HandleOpenURL(UIApplication application, NSUrl url)
        {
            Console.WriteLine(url.ToString());
            if (url.AbsoluteString.StartsWith("fb134284363380764"))
            {
                _callbackFromFB = true;
                return (TinyIoCContainer.Current.Resolve<IFacebookService>() as FacebookServiceMT).HandleOpenURL(application, url);
            }
            return false;               
        }
        
        public override bool OpenUrl(UIApplication application, NSUrl url, string sourceApplication, NSObject annotation)
        {
            return HandleOpenURL(application, url);
        }       


    }
    
}

