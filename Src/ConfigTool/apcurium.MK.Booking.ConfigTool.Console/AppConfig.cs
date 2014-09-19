﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO.Compression;
using System.Linq;
using System.IO;
using System.Text.RegularExpressions;
using apcurium.MK.Booking.ConfigTool.ServiceClient;
using CustomerPortal.Web.Entities;
using Newtonsoft.Json;
using Internals;

namespace apcurium.MK.Booking.ConfigTool
{
    public class AppConfig
    {
		private List<Config> _configs;
        private Regex pattern;
        
		public AppConfig(string name, Company company, string srcDirectoryPath, string configDirectoryPath)
        {
            Name = name;
            Company = company;
            SrcDirectoryPath = srcDirectoryPath;
			ConfigDirectoryPath = configDirectoryPath;
        }

        private void Init()
        {

            var androidPackage = string.IsNullOrWhiteSpace(Config.PackageAndroid) ? Config.Package : Config.PackageAndroid;

			var c = new Config[]
           {
					/**CallBox **/
//                    new ConfigFile(this){ Source=@"CallBox\background_empty.png", Destination=@"Mobile\MK.Callbox.Mobile.Client.Android\Resources\Drawable\background_empty.png" },
//                    new ConfigFile(this){ Source=@"CallBox\background_logo.png", Destination=@"Mobile\MK.Callbox.Mobile.Client.Android\Resources\Drawable\background_logo.png" },
//					
//                    new ConfigFile(this){ Source=@"Logo.png", Destination=@"Mobile\MK.Callbox.Mobile.Client.Android\Resources\Drawable\Logo.png" },
//                    new ConfigFile(this){ Source=@"Logo@2x.png", Destination=@"Mobile\MK.Callbox.Mobile.Client.Android\Resources\Drawable-xhdpi\Logo.png" },
//					
//                    new ConfigFile(this){ Source=@"Icon.png", Destination=@"Mobile\MK.Callbox.Mobile.Client.Android\Resources\Drawable\icon.png" },					
//				    
//                    new ConfigFile(this){ Source=@"navBar.png", Destination=@"Mobile\MK.Callbox.Mobile.Client.Android\Resources\Drawable\navBar.png" },
//                    new ConfigFile(this){ Source=@"navBar@2x.png", Destination=@"Mobile\MK.Callbox.Mobile.Client.Android\Resources\Drawable-xhdpi\navBar.png" },
//					
//                    new ConfigXML(this){  Destination=@"Mobile\MK.Callbox.Mobile.Client.Android\Properties\AndroidManifest.xml", NodeSelector=@"//manifest", Attribute="package" , SetterAtt = ( app, att )=> att.Value = androidPackage + "CallBox" },
//				    new ConfigXML(this){  Destination=@"Mobile\MK.Callbox.Mobile.Client.Android\Properties\AndroidManifest.xml", NodeSelector=@"//manifest/application", Attribute="android:label" , SetterAtt = ( app, att )=> att.Value = Config.ApplicationName + " CallBox" },
//					
//                    new ConfigXmlNamespace(this){  Destination=@"Mobile\MK.Callbox.Mobile.Client.Android\Resources\Layout\", Namespace = "xmlns:local", Value= androidPackage + "CallBox"},
//					
//                    new ConfigXML(this){  Destination=@"Mobile\MK.Callbox.Mobile.Client.Android\MK.Callbox.Mobile.Client.Android.csproj", NodeSelector=@"//a:Project/a:PropertyGroup[contains(@Condition, ""'Debug|AnyCPU'"")]/a:AndroidSigningKeyAlias" , SetterEle= ( app, ele )=> ele.InnerText = Config.AndroidSigningKeyAlias },               
//                    new ConfigXML(this){  Destination=@"Mobile\MK.Callbox.Mobile.Client.Android\MK.Callbox.Mobile.Client.Android.csproj", NodeSelector=@"//a:Project/a:PropertyGroup[contains(@Condition, ""'Release|AnyCPU'"")]/a:AndroidSigningKeyAlias" , SetterEle= ( app, ele )=> ele.InnerText = Config.AndroidSigningKeyAlias },               
//					
//                    new ConfigXML(this){  Destination=@"Mobile\MK.Callbox.Mobile.Client.Android\MK.Callbox.Mobile.Client.Android.csproj", NodeSelector=@"//a:Project/a:PropertyGroup[contains(@Condition, ""'Debug|AnyCPU'"")]/a:AndroidSigningKeyPass" , SetterEle= ( app, ele )=> ele.InnerText = Config.AndroidSigningKeyPassStorePass},               
//                    new ConfigXML(this){  Destination=@"Mobile\MK.Callbox.Mobile.Client.Android\MK.Callbox.Mobile.Client.Android.csproj", NodeSelector=@"//a:Project/a:PropertyGroup[contains(@Condition, ""'Release|AnyCPU'"")]/a:AndroidSigningKeyPass" , SetterEle= ( app, ele )=> ele.InnerText = Config.AndroidSigningKeyPassStorePass },               
//					
//					
//                    new ConfigXML(this){  Destination=@"Mobile\MK.Callbox.Mobile.Client.Android\MK.Callbox.Mobile.Client.Android.csproj", NodeSelector=@"//a:Project/a:PropertyGroup[contains(@Condition, ""'Debug|AnyCPU'"")]/a:AndroidSigningStorePass" , SetterEle= ( app, ele )=> ele.InnerText = Config.AndroidSigningKeyPassStorePass},               
//                    new ConfigXML(this){  Destination=@"Mobile\MK.Callbox.Mobile.Client.Android\MK.Callbox.Mobile.Client.Android.csproj", NodeSelector=@"//a:Project/a:PropertyGroup[contains(@Condition, ""'Release|AnyCPU'"")]/a:AndroidSigningStorePass" , SetterEle= ( app, ele )=> ele.InnerText = Config.AndroidSigningKeyPassStorePass },               
//					
//				    
//
//                    new ConfigFile(this){ Source="public.keystore", Destination=@"Mobile\MK.Callbox.Mobile.Client.Android\public.keystore" },


                    /**TaxiHail **/
                    new ConfigFile(this){ Source="Settings.json", Destination=@"Mobile\Common\Settings\Settings.json" },

                    new ConfigFile(this){ Source="public.keystore", Destination=@"Mobile\Android\public.keystore" },
				    new ConfigSplash(this,"splash.png",@"Mobile\Android\Resources\","splash.png"),
                    new ConfigFile(this){ Source="app@2x.png", Destination=@"Mobile\Android\Resources\Drawable\Icon.png" },

	                new ConfigXML(this){  Destination=@"Mobile\Android\Properties\AndroidManifest.xml", NodeSelector=@"//manifest", Attribute="package" , SetterAtt = ( app, att )=> att.Value = androidPackage  },
				    new ConfigXML(this){  Destination=@"Mobile\Android\Properties\AndroidManifest.xml", NodeSelector=@"//manifest/application", Attribute="android:label" , SetterAtt = ( app, att )=> att.Value = Config.ApplicationName  },
	                	
					/* open app from browser settings */
				    new ConfigSource(this) { Source = @"Mobile\Android\Activities\SplashActivity.cs", ToReplace = "TaxiHailDemo", ReplaceWith = Config.ApplicationName},

					/* notification */
	                new ConfigXML(this){  Destination=@"Mobile\Android\Properties\AndroidManifest.xml", NodeSelector=@"//manifest/permission[contains(@android:name,""permission.C2D_MESSAGE"")]", Attribute="android:name" , SetterAtt = ( app, att )=> att.Value = Config.Package + ".permission.C2D_MESSAGE" },
				    new ConfigXML(this){  Destination=@"Mobile\Android\Properties\AndroidManifest.xml", NodeSelector=@"//manifest/uses-permission[contains(@android:name,""permission.C2D_MESSAGE"")]", Attribute="android:name", SetterAtt = ( app, att )=> 
					{
                        att.Value = androidPackage + ".permission.C2D_MESSAGE";
					}},
					new ConfigMultiXML(this){  Destination=@"Mobile\Android\Properties\AndroidManifest.xml", NodeSelector=@"//manifest/application/receiver/intent-filter/category", Attribute="android:name" , SetterAtt = ( app, att )=> att.Value = androidPackage  },
	                

                    /** Google Maps */
                     new ConfigXML(this){  Destination=@"Mobile\Android\Properties\AndroidManifest.xml", NodeSelector=@"//manifest/permission[contains(@android:name,""permission.MAPS_RECEIVE"")]", Attribute="android:name" , SetterAtt = ( app, att )=> att.Value = Config.Package + ".permission.MAPS_RECEIVE" },
	                new ConfigXML(this){  Destination=@"Mobile\Android\Properties\AndroidManifest.xml", NodeSelector=@"//manifest/uses-permission[contains(@android:name,""permission.MAPS_RECEIVE"")]", Attribute="android:name", SetterAtt = ( app, att )=> 
					{
                        att.Value = androidPackage + ".permission.MAPS_RECEIVE";
					}},

				    new ConfigXML(this){  Destination=@"Mobile\Common\Localization\Master.resx", NodeSelector=@"//root/data[@name=""ApplicationName""]" , SetterEle= ( app, ele )=> ele.InnerText = Config.ApplicationName  },               
                    new ConfigXML(this){  Destination=@"Mobile\Common\Localization\Master.ar.resx", NodeSelector=@"//root/data[@name=""ApplicationName""]" , SetterEle= ( app, ele )=> ele.InnerText = Config.ApplicationName  },   
                    new ConfigXML(this){  Destination=@"Mobile\Common\Localization\Master.fr.resx", NodeSelector=@"//root/data[@name=""ApplicationName""]" , SetterEle= ( app, ele )=> ele.InnerText = Config.ApplicationName  },   

					new ConfigXML(this){  Destination=@"Mobile\Android\TaxiHail.csproj", NodeSelector=@"//a:Project/a:PropertyGroup[contains(@Condition, ""'Debug|AnyCPU'"")]/a:AndroidSigningKeyAlias" , SetterEle= ( app, ele )=> ele.InnerText = Config.AndroidSigningKeyAlias },               
					new ConfigXML(this){  Destination=@"Mobile\Android\TaxiHail.csproj", NodeSelector=@"//a:Project/a:PropertyGroup[contains(@Condition, ""'Release|AnyCPU'"")]/a:AndroidSigningKeyAlias" , SetterEle= ( app, ele )=> ele.InnerText = Config.AndroidSigningKeyAlias },               

					new ConfigXML(this){  Destination=@"Mobile\Android\TaxiHail.csproj", NodeSelector=@"//a:Project/a:PropertyGroup[contains(@Condition, ""'Debug|AnyCPU'"")]/a:AndroidSigningKeyPass" , SetterEle= ( app, ele )=> ele.InnerText = Config.AndroidSigningKeyPassStorePass},               
					new ConfigXML(this){  Destination=@"Mobile\Android\TaxiHail.csproj", NodeSelector=@"//a:Project/a:PropertyGroup[contains(@Condition, ""'Release|AnyCPU'"")]/a:AndroidSigningKeyPass" , SetterEle= ( app, ele )=> ele.InnerText = Config.AndroidSigningKeyPassStorePass },               

		                
					new ConfigXML(this){  Destination=@"Mobile\Android\TaxiHail.csproj", NodeSelector=@"//a:Project/a:PropertyGroup[contains(@Condition, ""'Debug|AnyCPU'"")]/a:AndroidSigningStorePass" , SetterEle= ( app, ele )=> ele.InnerText = Config.AndroidSigningKeyPassStorePass},               
					new ConfigXML(this){  Destination=@"Mobile\Android\TaxiHail.csproj", NodeSelector=@"//a:Project/a:PropertyGroup[contains(@Condition, ""'Release|AnyCPU'"")]/a:AndroidSigningStorePass" , SetterEle= ( app, ele )=> ele.InnerText = Config.AndroidSigningKeyPassStorePass },               


                    new ConfigFile(this){ Source="Default.png", Destination=@"Mobile\iOS\Default.png" },
                    new ConfigFile(this){ Source="Default@2x.png", Destination=@"Mobile\iOS\Default@2x.png" },
                    new ConfigFile(this){ Source="Default-568h@2x.png", Destination=@"Mobile\iOS\Default-568h@2x.png" },                  

                    new ConfigFile(this){ Source="app.png", Destination=@"Mobile\iOS\app.png" },
                    new ConfigFile(this){ Source="app@2x.png", Destination=@"Mobile\iOS\app@2x.png" },
					new ConfigFile(this){ Source="120px.png", Destination=@"Mobile\iOS\Resources\Icon-60@2x.png" },
                   
				    new ConfigPList(this){ Destination=@"Mobile\iOS\Info.plist", Key = "CFBundleDisplayName",  SetterEle = ( ele )=> ele.InnerText = Config.ApplicationName },
                    new ConfigPList(this){ Destination=@"Mobile\iOS\Info.plist", Key = "CFBundleIdentifier",  SetterEle = ( ele )=> ele.InnerText = Config.Package },
                    new ConfigPList(this){ Destination=@"Mobile\iOS\Info.plist", Key = "CFBundleURLSchemes",  SetterEle = ( ele )=> 
	                    {
                            //Sets space and the comma as pattern to ignore
                            pattern = new Regex("[ ']");
	                        if ( string.IsNullOrEmpty( Config.FacebookAppId ) )
	                        {
                                ele.InnerXml = string.Format("<string>mk{1}</string>", Config.FacebookAppId, pattern.Replace(Config.ApplicationName, string.Empty));
	                        }
	                        else
	                        {
                                ele.InnerXml = string.Format("<string>fb{0}{1}</string><string>mk{1}</string>", Config.FacebookAppId, pattern.Replace(Config.ApplicationName, string.Empty));
	                        }
						}
					},



					/** Version 1.5 */
				new ConfigXML(this)
				{  
					Destination=@"Mobile\Android\Resources\Values\Themes.xml", 
					NodeSelector=@"//resources/color[@name=""company_color""]", 
					SetterEle = (app,ele) => ele.InnerText = GetHexaColorCode(Company.Style.CompanyColor) 
				},
				new ConfigXML(this)
				{  
					Destination=@"Mobile\Android\Resources\Values\Themes.xml", 
					NodeSelector=@"//resources/color[@name=""login_color""]", 
					SetterEle = (app,ele) => ele.InnerText = GetHexaColorCode(Company.Style.LoginColor) 
				},
				new ConfigXML(this)
				{  
					Destination=@"Mobile\Android\Resources\Values\Themes.xml", 
					NodeSelector=@"//resources/color[@name=""top_bar_color""]", 
					SetterEle = (app,ele) => ele.InnerText = GetHexaColorCode(Company.Style.CompanyColor) 
				},
				new ConfigXML(this)
				{  
					Destination=@"Mobile\Android\Resources\Values\Themes.xml", 
					NodeSelector=@"//resources/color[@name=""button_text_color""]", 
					SetterEle = (app,ele) => ele.InnerText = GetHexaColorCode(Company.Style.TitleColor) 
				},
				new ConfigXML(this)
				{  
					Destination=@"Mobile\Android\Resources\Values\Themes.xml", 
					NodeSelector=@"//resources/color[@name=""label_text_color""]", 
					SetterEle = (app,ele) => ele.InnerText = GetHexaColorCode(Company.Style.TitleColor) 
				},
				new ConfigXML(this)
				{  
					Destination=@"Mobile\Android\Resources\Values\Themes.xml", 
					NodeSelector=@"//resources/color[@name=""top_bar_title_color""]", 
					SetterEle = (app,ele) => ele.InnerText = GetHexaColorCode(Company.Style.TitleColor) 
                },
                new ConfigXML(this)
                {  
                    Destination=@"Mobile\Android\Resources\Values\Themes.xml", 
                    NodeSelector=@"//resources/color[@name=""setting_menu_color""]", 
                    SetterEle = (app,ele) => ele.InnerText = GetHexaColorCode(Company.Style.MenuColor) 
                },

//                new ConfigXML(this)
//                {  
//                    Destination=@"Mobile\Android\Resources\Values\Themes.xml", 
//                    NodeSelector=@"//resources/color[@name=""setting_menu_text_color""]", 
//                    SetterEle = (app,ele) => ele.InnerText = GetColorFromBackground(Company.Style.MenuColor, "#ffffff", "#4f4c47") 
//                },
//                new ConfigXML(this)
//                {  
//                    Destination=@"Mobile\Android\Resources\Values\Themes.xml", 
//                    NodeSelector=@"//resources/color[@name=""setting_menu_separator_color""]", 
//                    SetterEle = (app,ele) => ele.InnerText = GetColorFromBackground(Company.Style.MenuColor, "#454545", "#a09f9c") 
//                },
//                new ConfigXML(this)
//                {  
//                    Destination=@"Mobile\Android\Resources\Values\Themes.xml", 
//                    NodeSelector=@"//resources/color[@name=""setting_item_pressed_background_color""]", 
//                    SetterEle = (app,ele) => ele.InnerText = GetColorFromBackground(Company.Style.MenuColor, "#282828", "#ffffff") 
//                },
//                new ConfigXML(this)
//                {  
//                    Destination=@"Mobile\Android\Resources\Values\Themes.xml", 
//                    NodeSelector=@"//resources/color[@name=""button_border_color""]", 
//                    SetterEle = (app,ele) => ele.InnerText = GetColorFromBackground(Company.Style.LoginColor, "#ffffff", "#031b31") 
//                },
//                new ConfigXML(this)
//                {  
//                    Destination=@"Mobile\Android\Resources\Values\Themes.xml", 
//                    NodeSelector=@"//resources/color[@name=""button_pressed_background_color""]", 
//                    SetterEle = (app,ele) => ele.InnerText = GetColorFromBackground(Company.Style.LoginColor, "#282828", "#70004785") 
//                },
                
				new ConfigFile(this){ Source="logo_1_5@2x.png", Destination=@"Mobile\Android\Resources\drawable-xhdpi\th_logo.png" },
				new ConfigFile(this){ Source="logo_1_5.png", Destination=@"Mobile\iOS\Resources\th_logo.png" },
				new ConfigFile(this){ Source="logo_1_5@2x.png", Destination=@"Mobile\iOS\Resources\th_logo@2x.png" },

				new ConfigXML(this)
				{  
					Destination=@"Mobile\iOS\Style\Theme.xml", 
					NodeSelector=@"//ThemeValues/CompanyColor", 
					SetterEle = (app,ele) => ele.InnerText = GetHexaColorCode(Company.Style.CompanyColor) 
				},
				new ConfigXML(this)
				{  
					Destination=@"Mobile\iOS\Style\Theme.xml", 
					NodeSelector=@"//ThemeValues/LoginColor", 
					SetterEle = (app,ele) => ele.InnerText = GetHexaColorCode(Company.Style.LoginColor) 
				},
				new ConfigXML(this)
				{  
					Destination=@"Mobile\iOS\Style\Theme.xml", 
					NodeSelector=@"//ThemeValues/ButtonTextColor", 
					SetterEle = (app,ele) => ele.InnerText = GetHexaColorCode(Company.Style.TitleColor) 
				},
				new ConfigXML(this)
				{  
					Destination=@"Mobile\iOS\Style\Theme.xml", 
					NodeSelector=@"//ThemeValues/LabelTextColor", 
					SetterEle = (app,ele) => ele.InnerText = GetHexaColorCode(Company.Style.TitleColor) 
                },
                new ConfigXML(this)
                {  
                    Destination=@"Mobile\iOS\Style\Theme.xml", 
                    NodeSelector=@"//ThemeValues/MenuColor", 
                    SetterEle = (app,ele) => ele.InnerText = GetHexaColorCode(Company.Style.MenuColor) 
                }
           };

			_configs = new List<apcurium.MK.Booking.ConfigTool.Config> ();
			_configs.AddRange (c);

			/***Optional files ****/

            var allResources = GetFilesFromAssetsDirectory("png");

            foreach (var g in allResources) 
            {
				_configs.Add (new ConfigFile (this) {
					Source = g+"@2x.png",
					Destination = @"Mobile\Android\Resources\drawable-xhdpi\"+g+".png"
				});

				_configs.Add (new ConfigFile (this) {
					Source = g+".png",
					Destination = @"Mobile\Android\Resources\Drawable\"+g+".png"
				});
				_configs.Add (new ConfigFile (this) {
					Source = g+"@2x.png",
					Destination = @"Mobile\iOS\Resources\"+g+"@2x.png"
				});

				_configs.Add (new ConfigFile (this){ 
					Source = g+".png", 
					Destination = @"Mobile\iOS\Resources\"+g+".png" 
				});	
			}

            /*** Custom themes for Buttons ****/
            /* iOS */
            _configs.Add (new ConfigFile (this) {
                Source = "FlatButtonStyle.xml",
                Destination = @"Mobile\iOS\Style\FlatButtonStyle.xml"
            });

            /* Android */
            // warning: only those colors are supported, when a company asks for more customization, we need to change this
            var customThemesForButtons = new string[] { "green", "red", "gray", "label" };
            foreach (var styleName in customThemesForButtons)
            {
                _configs.Add (new ConfigFile (this) {
                    Source = string.Format("button_action_{0}_selector.xml", styleName),
                    Destination = @"Mobile\Android\Resources\Drawable\" + string.Format("button_action_{0}_selector.xml", styleName)
                });
                _configs.Add (new ConfigFile (this) {
                    Source = string.Format("button_action_{0}_text_selector.xml", styleName),
                    Destination = @"Mobile\Android\Resources\Drawable\" + string.Format("button_action_{0}_text_selector.xml", styleName)
                });
            }

            /*** Tutorial ****/
            var tutorialContent = GetFilesFromAssetsDirectory("json");
            foreach (var file in tutorialContent)
            {
                _configs.Add(new ConfigFile(this)
                {
                    Source = file + ".json",
                    Destination = @"Mobile\Common\TutorialContent\"+ file + ".json" 
                });
            }
        }

        private string[] GetFilesFromAssetsDirectory(string extension)
        {
           var assetsDirectory = new DirectoryInfo(ConfigDirectoryPath);
           var listofFiles = assetsDirectory.EnumerateFiles("*." + extension, SearchOption.TopDirectoryOnly);
           return listofFiles.Select(x => x.Name.Replace(x.Extension, string.Empty)).ToArray();
        }

        private AppConfigFile _config;

        public AppConfigFile Config
        {
            get
            {
                if (_config == null)
                {

					var json = File.ReadAllText(Path.Combine(ConfigDirectoryPath, "AllSettings.json"));
					_config = JsonConvert.DeserializeObject<AppConfigFile> (json);
                    
                }
                return _config;

            }
        }

        public void UnloadApp()
        {
           
        }

        string GetColorFromBackground(string color, string lightColor, string darkColor)
        {
            return ShouldHaveLightContent(color) ? lightColor : darkColor;
        }

        bool ShouldHaveLightContent(string colorCode)
        {
            var color = ColorTranslator.FromHtml(GetHexaColorCode(colorCode));

            // Counting the perceptive luminance - human eye favors green color... 
            var a = 1 - (0.299 * color.R + 0.587 * color.G + 0.114 * color.B) / 255;

            return !(a < 0.5);
        }


		string GetHexaColorCode (string color)
		{
			if(!string.IsNullOrWhiteSpace(color)
				&& color[0] != '#')
			{
				color = "#" + color;
			}
			return color;
		}

        public string Name { get; private set; }

        public Company Company { get; private set; }

        public string SrcDirectoryPath { get; private set; }

        public string CommonDirectoryPath { get; private set; }

        public string ConfigDirectoryPath { get; set; }

		string _serviceUrl;

        public void Apply (string serviceUrl)
        {
			_serviceUrl = serviceUrl;
            GetFiles();

            Init();

			var errorsList = new List<string> ();
			foreach (var config in _configs) {
				try {
					Console.WriteLine("Applying : " + config.ToString ());
					config.Apply ();

				} catch (Exception e) {
					errorsList.Add (string.Format ("Error for {0} : {1} {2}", config.ToString (), e.Message, Environment.NewLine));
				}
			}

			if (errorsList.Any ()) {

				throw new Exception(errorsList.Aggregate((x, y) => x + " " + y));
			}
        }

		void CreateConfigFile (string fileName, Func<CompanySetting,bool> predicate)
		{
			var newFile = File.CreateText (fileName);
			var dict = Company.CompanySettings.Where(predicate).ToDictionary (s => s.Key, s => s.Value ?? "");

			dict["ServiceUrl"] = _serviceUrl;

			newFile.Write (JsonConvert.SerializeObject (dict));

			newFile.Close ();

			newFile.Dispose ();
		}

        private string GetFiles()
        {
			var tempPath = ConfigDirectoryPath;
            if ( Directory.Exists(tempPath))
            {
                Directory.Delete( tempPath, true  );
            }

            Directory.CreateDirectory(tempPath);

            var service = new CompanyServiceClient();
            var stream = service.GetCompanyFiles(Company.Id , "assets");
            var zipFile = Path.Combine(tempPath, "out.zip");
            using (Stream file = File.OpenWrite(zipFile))
            {
                CopyStream(stream, file);
            }

			using (var unzip = new Unzip(zipFile))
			{
				unzip.ExtractToDirectory(tempPath);
			}

            File.Delete( zipFile );


			CreateConfigFile (Path.Combine (tempPath, "settings.json"), s => s.IsClientSetting);
			CreateConfigFile (Path.Combine (tempPath, "allSettings.json"), s => true);

            return tempPath;
        }        
            
        public static void CopyStream(Stream input, Stream output)
        {
            byte[] buffer = new byte[8 * 1024];
            int len;
            while ((len = input.Read(buffer, 0, buffer.Length)) > 0)
            {
                output.Write(buffer, 0, len);
            }
        }        
    }
}
