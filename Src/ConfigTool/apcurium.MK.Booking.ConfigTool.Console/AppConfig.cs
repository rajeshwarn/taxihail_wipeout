using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using ServiceStack.ServiceClient.Web;

namespace apcurium.MK.Booking.ConfigTool
{
    public class AppConfig
    {

        private Config[] _configs;
        
        public AppConfig(string name, string configDirectoryPath, string srcDirectoryPath, string commonDirectoryPath)
        {
            Name = name;
            ConfigDirectoryPath = configDirectoryPath;
            SrcDirectoryPath = srcDirectoryPath;
            CommonDirectoryPath = commonDirectoryPath;
            Init();
        }

        private void Init()
        {
            _configs = new Config[]
           {
               
               new ConfigFile(this){ Source="AppFont_Bold.otf", Destination=@"Mobile\Android\Assets\AppFont_Bold.otf" },    
               new ConfigFile(this){ Source="AppFont_Italic.otf", Destination=@"Mobile\Android\Assets\AppFont_Italic.otf" },    
               new ConfigFile(this){ Source="AppFont_Medium.otf", Destination=@"Mobile\Android\Assets\AppFont_Medium.otf" },    
               new ConfigFile(this){ Source="AppFont_Regular.otf", Destination=@"Mobile\Android\Assets\AppFont_Regular.otf" },    
               new ConfigFile(this){ Source="SubView_BookButtons.axml", Destination=@"Mobile\Android\Resources\Layout\SubView_BookButtons.axml" },    
               

                new ConfigFile(this){ Source="Styles.xml", Destination=@"Mobile\Android\Resources\Values\Styles.xml" },    
             
                new ConfigFile(this){ Source="Settings.json", Destination=@"Mobile\Common\Settings\Settings.json" },
                new ConfigFile(this){ Source="Style.json", Destination=@"Mobile\Common\Style\Style.json" },

                new ConfigFile(this){ Source="public.keystore", Destination=@"Mobile\Android\public.keystore" },
                new ConfigFile(this){ Source="splash.png", Destination=@"Mobile\Android\Resources\Drawable\splash.png" },
                new ConfigFile(this){ Source="splash.png", Destination=@"Mobile\Android\Resources\drawable-hdpi\splash.png" },
                new ConfigFile(this){ Source="splash.png", Destination=@"Mobile\Android\Resources\drawable-mdpi\splash.png" },
                new ConfigFile(this){ Source="Icon.png", Destination=@"Mobile\Android\Resources\Drawable\Icon.png" },            
               
                new ConfigFile(this){ Source="navBar.png", Destination=@"Mobile\Android\Resources\Drawable\navBar.png" },            
                new ConfigFile(this){ Source="navBar@2x.png", Destination=@"Mobile\Android\Resources\Drawable-hdpi\navBar.png" },                                

                new ConfigFile(this){ Source="Logo.png", Destination=@"Mobile\Android\Resources\Drawable\Logo.png" },            
                new ConfigFile(this){ Source="Logo@2x.png", Destination=@"Mobile\Android\Resources\Drawable-hdpi\Logo.png" },

				new ConfigFile(this) { Source="pin_cab.png", Destination=@"Mobile\Android\Resources\Drawable\pin_cab.png" },
				new ConfigFile(this) { Source="pin_cab@2x.png", Destination=@"Mobile\Android\Resources\Drawable-hdpi\pin_cab.png" },
				new ConfigFile(this) { Source="pin_hail.png", Destination=@"Mobile\Android\Resources\Drawable\pin_hail.png" },
				new ConfigFile(this) { Source="pin_hail@2x.png", Destination=@"Mobile\Android\Resources\Drawable-hdpi\pin_hail.png" },
				new ConfigFile(this) { Source="pin_destination.png", Destination=@"Mobile\Android\Resources\Drawable\pin_destination.png" },
				new ConfigFile(this) { Source="pin_destination@2x.png", Destination=@"Mobile\Android\Resources\Drawable-hdpi\pin_destination.png" },


                new ConfigFile(this){ Source="backgroundblue.png", Destination=@"Mobile\Android\Resources\Drawable\backgroundblue.png" },                        
                new ConfigXML(this){  Destination=@"Mobile\Android\Properties\AndroidManifest.xml", NodeSelector=@"//manifest", Attribute="package" , SetterAtt = ( app, att )=> att.Value = Config.Package  },
                new ConfigXML(this){  Destination=@"Mobile\Android\Properties\AndroidManifest.xml", NodeSelector=@"//manifest/application", Attribute="android:label" , SetterAtt = ( app, att )=> att.Value = Config.AppName  },
                new ConfigXML(this){  Destination=@"Mobile\Android\Resources\Values-fr\Strings.xml", NodeSelector=@"//resources/string[@name=""ApplicationName""]", SetterEle = ( app, ele )=> ele.InnerText = Config.AppName  },
                new ConfigXML(this){  Destination=@"Mobile\Android\Resources\Values\String.xml", NodeSelector=@"//resources/string[@name=""ApplicationName""]" , SetterEle= ( app, ele )=> ele.InnerText = Config.AppName  },               
                new ConfigXML(this){  Destination=@"Mobile\Android\Resources\Values-fr\Strings.xml", NodeSelector=@"//resources/string[@name=""GoogleMapKey""]", SetterEle = ( app, ele )=> ele.InnerText = Config.GoogleMapKey  },
                new ConfigXML(this){  Destination=@"Mobile\Android\Resources\Values\String.xml", NodeSelector=@"//resources/string[@name=""GoogleMapKey""]" , SetterEle= ( app, ele )=> ele.InnerText = Config.GoogleMapKey  },                                                                          


                new ConfigXmlNamespace(this){  Destination=@"Mobile\Android\Resources\Layout\View_BookingStatus.axml", Namespace = "xmlns:local", Value= Config.Package },

                new ConfigXmlNamespace(this){  Destination=@"Mobile\Android\Resources\Layout\SubView_BookButtons.axml", Namespace = "xmlns:local", Value= Config.Package },


                new ConfigXmlNamespace(this){  Destination=@"Mobile\Android\Resources\Layout\View_Book.axml", Namespace = "xmlns:local", Value= Config.Package },
                new ConfigXmlNamespace(this){  Destination=@"Mobile\Android\Resources\Layout\SimpleListItem.axml", Namespace = "xmlns:local", Value= Config.Package },
                new ConfigXmlNamespace(this){  Destination=@"Mobile\Android\Resources\Layout\View_SearchAddress.axml", Namespace = "xmlns:local", Value= Config.Package },
				new ConfigXmlNamespace(this){  Destination=@"Mobile\Android\Resources\Layout\SubView_MainMenu.axml", Namespace = "xmlns:local", Value= Config.Package },
				new ConfigXmlNamespace(this){  Destination=@"Mobile\Android\Resources\Layout\View_Login.axml", Namespace = "xmlns:local", Value= Config.Package },
				new ConfigXmlNamespace(this){  Destination=@"Mobile\Android\Resources\Layout\View_RefineAddress.axml", Namespace = "xmlns:local", Value= Config.Package },

                new ConfigXmlNamespace(this){  Destination=@"Mobile\Android\Resources\Layout\RatingListItem.axml", Namespace = "xmlns:local", Value= Config.Package },
                new ConfigXmlNamespace(this){  Destination=@"Mobile\Android\Resources\Layout\SimpleOrderListItem.axml", Namespace = "xmlns:local", Value= Config.Package },
                new ConfigXmlNamespace(this){  Destination=@"Mobile\Android\Resources\Layout\View_BookingDetail.axml", Namespace = "xmlns:local", Value= Config.Package },
                new ConfigXmlNamespace(this){  Destination=@"Mobile\Android\Resources\Layout\View_BookingRating.axml", Namespace = "xmlns:local", Value= Config.Package },
                new ConfigXmlNamespace(this){  Destination=@"Mobile\Android\Resources\Layout\View_HistoryDetail.axml", Namespace = "xmlns:local", Value= Config.Package },
                new ConfigXmlNamespace(this){  Destination=@"Mobile\Android\Resources\Layout\View_HistoryList.axml", Namespace = "xmlns:local", Value= Config.Package },

                new ConfigXmlNamespace(this){  Destination=@"Mobile\Android\Resources\Layout\View_LocationDetail.axml", Namespace = "xmlns:local", Value= Config.Package },
                new ConfigXmlNamespace(this){  Destination=@"Mobile\Android\Resources\Layout\View_LocationList.axml", Namespace = "xmlns:local", Value= Config.Package },
                new ConfigXmlNamespace(this){  Destination=@"Mobile\Android\Resources\Layout\View_PasswordRecovery.axml", Namespace = "xmlns:local", Value= Config.Package },
                new ConfigXmlNamespace(this){  Destination=@"Mobile\Android\Resources\Layout\View_RideSettings.axml", Namespace = "xmlns:local", Value= Config.Package },
                new ConfigXmlNamespace(this){  Destination=@"Mobile\Android\Resources\Layout\View_SignUp.axml", Namespace = "xmlns:local", Value= Config.Package },



                new ConfigXML(this){  Destination=@"Mobile\Android\MK.Booking.Mobile.Client.Android.csproj", NodeSelector=@"//a:Project/a:PropertyGroup[contains(@Condition, ""'Debug|AnyCPU'"")]/a:AndroidSigningKeyAlias" , SetterEle= ( app, ele )=> ele.InnerText = Config.AndroidSigningKeyAlias },               
                new ConfigXML(this){  Destination=@"Mobile\Android\MK.Booking.Mobile.Client.Android.csproj", NodeSelector=@"//a:Project/a:PropertyGroup[contains(@Condition, ""'Release|AnyCPU'"")]/a:AndroidSigningKeyAlias" , SetterEle= ( app, ele )=> ele.InnerText = Config.AndroidSigningKeyAlias },               

                new ConfigXML(this){  Destination=@"Mobile\Android\MK.Booking.Mobile.Client.Android.csproj", NodeSelector=@"//a:Project/a:PropertyGroup[contains(@Condition, ""'Debug|AnyCPU'"")]/a:AndroidSigningKeyPass" , SetterEle= ( app, ele )=> ele.InnerText = Config.AndroidSigningKeyPassStorePass},               
                new ConfigXML(this){  Destination=@"Mobile\Android\MK.Booking.Mobile.Client.Android.csproj", NodeSelector=@"//a:Project/a:PropertyGroup[contains(@Condition, ""'Release|AnyCPU'"")]/a:AndroidSigningKeyPass" , SetterEle= ( app, ele )=> ele.InnerText = Config.AndroidSigningKeyPassStorePass },               

                
                new ConfigXML(this){  Destination=@"Mobile\Android\MK.Booking.Mobile.Client.Android.csproj", NodeSelector=@"//a:Project/a:PropertyGroup[contains(@Condition, ""'Debug|AnyCPU'"")]/a:AndroidSigningStorePass" , SetterEle= ( app, ele )=> ele.InnerText = Config.AndroidSigningKeyPassStorePass},               
                new ConfigXML(this){  Destination=@"Mobile\Android\MK.Booking.Mobile.Client.Android.csproj", NodeSelector=@"//a:Project/a:PropertyGroup[contains(@Condition, ""'Release|AnyCPU'"")]/a:AndroidSigningStorePass" , SetterEle= ( app, ele )=> ele.InnerText = Config.AndroidSigningKeyPassStorePass },               


                new ConfigFile(this){ Source="Default.png", Destination=@"Mobile\iOS\Default.png" },
                new ConfigFile(this){ Source="Default@2x.png", Destination=@"Mobile\iOS\Default@2x.png" },
                new ConfigFile(this){ Source="Default-568h@2x.png", Destination=@"Mobile\iOS\Default-568h@2x.png" },

                new ConfigFile(this){ Source="Default.png", Destination=@"Mobile\iOS\Assets\background_full_nologo.png" },
                new ConfigFile(this){ Source="Default@2x.png", Destination=@"Mobile\iOS\Assets\background_full_nologo@2x.png" },

                new ConfigFile(this){ Source="background_full.png", Destination=@"Mobile\iOS\Assets\background_full.png" },
                new ConfigFile(this){ Source="background_full@2x.png", Destination=@"Mobile\iOS\Assets\background_full@2x.png" },

                new ConfigFile(this){ Source="Logo.png", Destination=@"Mobile\iOS\Assets\Logo.png" },
                new ConfigFile(this){ Source="Logo@2x.png", Destination=@"Mobile\iOS\Assets\Logo@2x.png" },

                new ConfigFile(this){ Source="navBar.png", Destination=@"Mobile\iOS\Assets\navBar.png" },
                new ConfigFile(this){ Source="navBar@2x.png", Destination=@"Mobile\iOS\Assets\navBar@2x.png" },

                new ConfigFile(this){ Source="app.png", Destination=@"Mobile\iOS\app.png" },
                new ConfigFile(this){ Source="app@2x.png", Destination=@"Mobile\iOS\app@2x.png" },

				new ConfigFile(this) { Source="pin_cab.png", Destination=@"Mobile\iOS\Assets\pin_cab.png" },
				new ConfigFile(this) { Source="pin_cab@2x.png", Destination=@"Mobile\iOS\Assets\pin_cab@2x.png" },
				new ConfigFile(this) { Source="pin_hail.png", Destination=@"Mobile\iOS\Assets\pin_hail.png" },
				new ConfigFile(this) { Source="pin_hail@2x.png", Destination=@"Mobile\iOS\Assets\pin_hail@2x.png" },
				new ConfigFile(this) { Source="pin_destination.png", Destination=@"Mobile\iOS\Assets\pin_destination.png" },
				new ConfigFile(this) { Source="pin_destination@2x.png", Destination=@"Mobile\iOS\Assets\pin_destination@2x.png" },

                new ConfigPList(this){ Destination=@"Mobile\iOS\Info.plist", Key = "CFBundleDisplayName",  SetterEle = ( ele )=> ele.InnerText = Config.AppName },
                new ConfigPList(this){ Destination=@"Mobile\iOS\Info.plist", Key = "CFBundleIdentifier",  SetterEle = ( ele )=> ele.InnerText = Config.Package },
                new ConfigPList(this){ Destination=@"Mobile\iOS\Info.plist", Key = "CFBundleURLSchemes",  SetterEle = ( ele )=> 
                {
						if ( string.IsNullOrEmpty( Config.FacebookAppId ) )
						{
							ele.InnerXml = string.Format( "<string>taxihail</string>", Config.FacebookAppId);
						}
						else
						{
							ele.InnerXml = string.Format( "<string>fb{0}</string><string>taxihail</string>", Config.FacebookAppId);
						}
                }
               },             

           };
        }

        private AppConfigFile _config;

        public AppConfigFile Config
        {
            get
            {
                if (_config == null)
                {
                    using (var file = File.Open(Path.Combine(ConfigDirectoryPath, "Settings.json"), FileMode.Open))
                    {
                        _config = JsonSerializer.DeserializeFromStream(typeof(AppConfigFile), file) as AppConfigFile;
                    }
                }
                return _config;

            }
        }

        public void UnloadApp()
        {
           
        }

        public string Name { get; private set; }

        public string ConfigDirectoryPath { get; private set; }

        public string SrcDirectoryPath { get; private set; }

        public string CommonDirectoryPath { get; private set; }

        public void Apply ()
		{
			var errorsList = new List<string> ();
			foreach (var config in _configs) {
				try {
					config.Apply ();

				} catch (Exception e) {
					errorsList.Add (string.Format ("Error for {0} : {1} {2}", config.ToString (), e.Message, Environment.NewLine));
				}
			}

			if (errorsList.Any ()) {

				throw new Exception(errorsList.Aggregate((x, y) => x + " " + y));
			}
        }

    }
}
