using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ServiceStack.Text;
using System.IO;
using ServiceStack.ServiceClient.Web;

namespace apcurium.MK.Booking.ConfigTool
{
    public class AppConfig
    {

        private Config[] _configs;
        
        public AppConfig(string name, string configDirectoryPath, string srcDirectoryPath)
        {
            Name = name;
            ConfigDirectoryPath = configDirectoryPath;
            SrcDirectoryPath = srcDirectoryPath;
            Init();
        }

        private void Init()
        {
            _configs = new Config[]
           {
             
                new ConfigKeyStore(this){ },        

                new ConfigFile(this){ Source="Settings.json", Destination=@"Mobile\Common\Settings\Settings.json" },
                new ConfigFile(this){ Source="Style.json", Destination=@"Mobile\Common\Style\Style.json" },

                new ConfigFile(this){ Source="public.keystore", Destination=@"Mobile\Android\public.keystore" },
                new ConfigFile(this){ Source="splash.png", Destination=@"Mobile\Android\Resources\Drawable\splash.png" },
                new ConfigFile(this){ Source="splash.png", Destination=@"Mobile\Android\Resources\drawable-hdpi\splash.png" },
                new ConfigFile(this){ Source="splash.png", Destination=@"Mobile\Android\Resources\drawable-mdpi\splash.png" },
                new ConfigFile(this){ Source="Icon.png", Destination=@"Mobile\Android\Resources\Drawable\Icon.png" },            
                new ConfigFile(this){ Source="header.png", Destination=@"Mobile\Android\Resources\Drawable\header.png" },            

                new ConfigFile(this){ Source="navBar.png", Destination=@"Mobile\Android\Resources\Drawable\navBar.png" },            
                new ConfigFile(this){ Source="navBar@2x.png", Destination=@"Mobile\Android\Resources\Drawable-hdpi\navBar.png" },                                

                new ConfigFile(this){ Source="Logo.png", Destination=@"Mobile\Android\Resources\Drawable\Logo.png" },            
                new ConfigFile(this){ Source="Logo@2x.png", Destination=@"Mobile\Android\Resources\Drawable-hdpi\Logo.png" },    


                new ConfigFile(this){ Source="backgroundblue.png", Destination=@"Mobile\Android\Resources\Drawable\backgroundblue.png" },                        
                new ConfigXML(this){  Destination=@"Mobile\Android\Properties\AndroidManifest.xml", NodeSelector=@"//manifest", Attribute="package" , SetterAtt = ( app, att )=> att.Value = app.Package  },
                new ConfigXML(this){  Destination=@"Mobile\Android\Properties\AndroidManifest.xml", NodeSelector=@"//manifest/application", Attribute="android:label" , SetterAtt = ( app, att )=> att.Value = app.AppName  },
                new ConfigXML(this){  Destination=@"Mobile\Android\Resources\Values-fr\Strings.xml", NodeSelector=@"//resources/string[@name=""ApplicationName""]", SetterEle = ( app, ele )=> ele.InnerText = app.AppName  },
                new ConfigXML(this){  Destination=@"Mobile\Android\Resources\Values\String.xml", NodeSelector=@"//resources/string[@name=""ApplicationName""]" , SetterEle= ( app, ele )=> ele.InnerText = app.AppName  },               
                new ConfigXML(this){  Destination=@"Mobile\Android\Resources\Values-fr\Strings.xml", NodeSelector=@"//resources/string[@name=""GoogleMapKey""]", SetterEle = ( app, ele )=> ele.InnerText = app.GoogleMapKey  },
                new ConfigXML(this){  Destination=@"Mobile\Android\Resources\Values\String.xml", NodeSelector=@"//resources/string[@name=""GoogleMapKey""]" , SetterEle= ( app, ele )=> ele.InnerText = app.GoogleMapKey  },                                                                          


                new ConfigXmlNamespace(this){  Destination=@"Mobile\Android\Resources\Layout\View_Book.axml", Namespace = "xmlns:local", Value= App.Package },
                new ConfigXmlNamespace(this){  Destination=@"Mobile\Android\Resources\Layout\SimpleListItem.axml", Namespace = "xmlns:local", Value= App.Package },
                new ConfigXmlNamespace(this){  Destination=@"Mobile\Android\Resources\Layout\View_SearchAddress.axml", Namespace = "xmlns:local", Value= App.Package },

                new ConfigXML(this){  Destination=@"Mobile\Android\MK.Booking.Mobile.Client.Android.csproj", NodeSelector=@"//a:Project/a:PropertyGroup[contains(@Condition, ""'Debug|AnyCPU'"")]/a:AndroidSigningKeyAlias" , SetterEle= ( app, ele )=> ele.InnerText = app.AndroidSigningKeyAlias },               
                new ConfigXML(this){  Destination=@"Mobile\Android\MK.Booking.Mobile.Client.Android.csproj", NodeSelector=@"//a:Project/a:PropertyGroup[contains(@Condition, ""'Release|AnyCPU'"")]/a:AndroidSigningKeyAlias" , SetterEle= ( app, ele )=> ele.InnerText = app.AndroidSigningKeyAlias },               

                new ConfigXML(this){  Destination=@"Mobile\Android\MK.Booking.Mobile.Client.Android.csproj", NodeSelector=@"//a:Project/a:PropertyGroup[contains(@Condition, ""'Debug|AnyCPU'"")]/a:AndroidSigningKeyPass" , SetterEle= ( app, ele )=> ele.InnerText = app.AndroidSigningKeyPassStorePass},               
                new ConfigXML(this){  Destination=@"Mobile\Android\MK.Booking.Mobile.Client.Android.csproj", NodeSelector=@"//a:Project/a:PropertyGroup[contains(@Condition, ""'Release|AnyCPU'"")]/a:AndroidSigningKeyPass" , SetterEle= ( app, ele )=> ele.InnerText = app.AndroidSigningKeyPassStorePass },               

                
                new ConfigXML(this){  Destination=@"Mobile\Android\MK.Booking.Mobile.Client.Android.csproj", NodeSelector=@"//a:Project/a:PropertyGroup[contains(@Condition, ""'Debug|AnyCPU'"")]/a:AndroidSigningStorePass" , SetterEle= ( app, ele )=> ele.InnerText = app.AndroidSigningKeyPassStorePass},               
                new ConfigXML(this){  Destination=@"Mobile\Android\MK.Booking.Mobile.Client.Android.csproj", NodeSelector=@"//a:Project/a:PropertyGroup[contains(@Condition, ""'Release|AnyCPU'"")]/a:AndroidSigningStorePass" , SetterEle= ( app, ele )=> ele.InnerText = app.AndroidSigningKeyPassStorePass },               


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


                new ConfigFile(this){ Source="Default@2x.png", Destination=@"Mobile\iOS\Default@2x.png" },

                new ConfigFile(this){ Source="app.png", Destination=@"Mobile\iOS\app.png" },
                new ConfigFile(this){ Source="app@2x.png", Destination=@"Mobile\iOS\app@2x.png" },

                new ConfigPList(this){ Destination=@"Mobile\iOS\Info.plist", Key = "CFBundleDisplayName",  SetterEle = ( ele )=> ele.InnerText = App.AppName },
                new ConfigPList(this){ Destination=@"Mobile\iOS\Info.plist", Key = "CFBundleIdentifier",  SetterEle = ( ele )=> ele.InnerText = App.Package },
                new ConfigPList(this){ Destination=@"Mobile\iOS\Info.plist", Key = "CFBundleURLSchemes",  SetterEle = ( ele )=> 
                {
                    ele.InnerXml = string.Format( "<string>fb{0}</string><string>taxihail</string>", Config.FacebookAppId);
                }
               },             

           };
        }

        private AppInfo _app;

        public AppInfo App
        {
            get
            {
                if (_app == null)
                {
                    using (var file = File.Open(Path.Combine(ConfigDirectoryPath, "AppInfo.json"), FileMode.Open))
                    {
                        _app = JsonSerializer.DeserializeFromStream(typeof(AppInfo), file) as AppInfo;
                    }
                }
                return _app;
            }
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
            _app = null;
        }

        public string Name { get; private set; }

        public string ConfigDirectoryPath { get; private set; }

        public string SrcDirectoryPath { get; private set; }

        public void Apply ()
		{
			List<string> errorsList = new List<string> ();
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
