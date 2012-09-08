using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ServiceStack.Text;
using System.IO;

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
                    new ConfigFile(this){ Source="public.keystore", Destination=@"Mobile\Android\public.keystore" },
                    new ConfigFile(this){ Source="splash.png", Destination=@"Mobile\Android\Resources\Drawable\splash.png" },
                    new ConfigFile(this){ Source="splash.png", Destination=@"Mobile\Android\Resources\drawable-hdpi\splash.png" },
                    new ConfigFile(this){ Source="splash.png", Destination=@"Mobile\Android\Resources\drawable-mdpi\splash.png" },
                    new ConfigFile(this){ Source="Icon.png", Destination=@"Mobile\Android\Resources\Drawable\Icon.png" },            
                    new ConfigFile(this){ Source="header.png", Destination=@"Mobile\Android\Resources\Drawable\header.png" },            
                    new ConfigFile(this){ Source="backgroundblue.png", Destination=@"Mobile\Android\Resources\Drawable\backgroundblue.png" },                        
                    new ConfigXML(this){  Destination=@"Mobile\Android\Properties\AndroidManifest.xml", NodeSelector=@"//manifest", Attribute="package" , SetterAtt = ( app, att )=> att.Value = app.Package  },
                    new ConfigXML(this){  Destination=@"Mobile\Android\Properties\AndroidManifest.xml", NodeSelector=@"//manifest/application", Attribute="android:label" , SetterAtt = ( app, att )=> att.Value = app.AppName  },
                    new ConfigXML(this){  Destination=@"Mobile\Android\Resources\Values-fr\Strings.xml", NodeSelector=@"//resources/string[@name=""ApplicationName""]", SetterEle = ( app, ele )=> ele.InnerText = app.AppName  },
                    new ConfigXML(this){  Destination=@"Mobile\Android\Resources\Values\String.xml", NodeSelector=@"//resources/string[@name=""ApplicationName""]" , SetterEle= ( app, ele )=> ele.InnerText = app.AppName  },               

                    new ConfigXML(this){  Destination=@"Mobile\Android\Resources\Values-fr\Strings.xml", NodeSelector=@"//resources/string[@name=""GoogleMapKey""]", SetterEle = ( app, ele )=> ele.InnerText = app.GoogleMapKey  },
                    new ConfigXML(this){  Destination=@"Mobile\Android\Resources\Values\String.xml", NodeSelector=@"//resources/string[@name=""GoogleMapKey""]" , SetterEle= ( app, ele )=> ele.InnerText = app.GoogleMapKey  },               
                    
                    
                    


                    new ConfigXML(this){  Destination=@"Mobile\Android\MK.Booking.Mobile.Client.Android.csproj", NodeSelector=@"//a:Project/a:PropertyGroup[contains(@Condition, ""'Debug|AnyCPU'"")]/a:AndroidSigningKeyAlias" , SetterEle= ( app, ele )=> ele.InnerText = app.AndroidSigningKeyAlias },               
                    new ConfigXML(this){  Destination=@"Mobile\Android\MK.Booking.Mobile.Client.Android.csproj", NodeSelector=@"//a:Project/a:PropertyGroup[contains(@Condition, ""'Release|AnyCPU'"")]/a:AndroidSigningKeyAlias" , SetterEle= ( app, ele )=> ele.InnerText = app.AndroidSigningKeyAlias },               

                    new ConfigXML(this){  Destination=@"Mobile\Android\MK.Booking.Mobile.Client.Android.csproj", NodeSelector=@"//a:Project/a:PropertyGroup[contains(@Condition, ""'Debug|AnyCPU'"")]/a:AndroidSigningKeyPass" , SetterEle= ( app, ele )=> ele.InnerText = app.AndroidSigningKeyPassStorePass},               
                    new ConfigXML(this){  Destination=@"Mobile\Android\MK.Booking.Mobile.Client.Android.csproj", NodeSelector=@"//a:Project/a:PropertyGroup[contains(@Condition, ""'Release|AnyCPU'"")]/a:AndroidSigningKeyPass" , SetterEle= ( app, ele )=> ele.InnerText = app.AndroidSigningKeyPassStorePass },               

                    
                    new ConfigXML(this){  Destination=@"Mobile\Android\MK.Booking.Mobile.Client.Android.csproj", NodeSelector=@"//a:Project/a:PropertyGroup[contains(@Condition, ""'Debug|AnyCPU'"")]/a:AndroidSigningStorePass" , SetterEle= ( app, ele )=> ele.InnerText = app.AndroidSigningKeyPassStorePass},               
                    new ConfigXML(this){  Destination=@"Mobile\Android\MK.Booking.Mobile.Client.Android.csproj", NodeSelector=@"//a:Project/a:PropertyGroup[contains(@Condition, ""'Release|AnyCPU'"")]/a:AndroidSigningStorePass" , SetterEle= ( app, ele )=> ele.InnerText = app.AndroidSigningKeyPassStorePass },               
                  
  

                    

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
                        _app = JsonSerializer.DeserializeFromStream<AppInfo>(file);
                    }
                }
                return _app;

            }
        }

        public void UnloadApp()
        {
            _app = null;
        }

        public string Name { get; private set; }
        public string ConfigDirectoryPath { get; private set; }
        public string SrcDirectoryPath { get; private set; }

        public void Apply()
        {

            foreach (var config in _configs)
            {
                config.Apply();
            }


        }

    }
}
