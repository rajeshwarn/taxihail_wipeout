﻿using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.IO;
using apcurium.MK.Booking.ConfigTool.ServiceClient;
using CustomerPortal.Web.Entities;
using Newtonsoft.Json;
using Internals;

namespace apcurium.MK.Booking.ConfigTool
{
    public class AppConfig
    {

        private Config[] _configs;
        
		public AppConfig(string name, Company company, string srcDirectoryPath, string configDirectoryPath)
        {
            Name = name;
            Company = company;
            SrcDirectoryPath = srcDirectoryPath;
			ConfigDirectoryPath = configDirectoryPath;
        }

        private void Init()
        {
            _configs = new Config[]
           {
					/**CallBox **/
//                    new ConfigFile(this){ Source=@"CallBox\background_empty.png", Destination=@"Mobile\MK.Callbox.Mobile.Client.Android\Resources\Drawable\background_empty.png" },
//                    new ConfigFile(this){ Source=@"CallBox\background_logo.png", Destination=@"Mobile\MK.Callbox.Mobile.Client.Android\Resources\Drawable\background_logo.png" },
//					
//                    new ConfigFile(this){ Source=@"Logo.png", Destination=@"Mobile\MK.Callbox.Mobile.Client.Android\Resources\Drawable\Logo.png" },
//                    new ConfigFile(this){ Source=@"Logo@2x.png", Destination=@"Mobile\MK.Callbox.Mobile.Client.Android\Resources\Drawable-hdpi\Logo.png" },
//					
//                    new ConfigFile(this){ Source=@"Icon.png", Destination=@"Mobile\MK.Callbox.Mobile.Client.Android\Resources\Drawable\icon.png" },					
//				    
//                    new ConfigFile(this){ Source=@"navBar.png", Destination=@"Mobile\MK.Callbox.Mobile.Client.Android\Resources\Drawable\navBar.png" },
//                    new ConfigFile(this){ Source=@"navBar@2x.png", Destination=@"Mobile\MK.Callbox.Mobile.Client.Android\Resources\Drawable-hdpi\navBar.png" },
//					
//                    new ConfigXML(this){  Destination=@"Mobile\MK.Callbox.Mobile.Client.Android\Properties\AndroidManifest.xml", NodeSelector=@"//manifest", Attribute="package" , SetterAtt = ( app, att )=> att.Value = Config.Package + "CallBox" },
//				    new ConfigXML(this){  Destination=@"Mobile\MK.Callbox.Mobile.Client.Android\Properties\AndroidManifest.xml", NodeSelector=@"//manifest/application", Attribute="android:label" , SetterAtt = ( app, att )=> att.Value = Config.ApplicationName + " CallBox" },
//					
//                    new ConfigXmlNamespace(this){  Destination=@"Mobile\MK.Callbox.Mobile.Client.Android\Resources\Layout\", Namespace = "xmlns:local", Value= Config.Package + "CallBox"},
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
//				    new ConfigXML(this){  Destination=@"Mobile\MK.Callbox.Mobile.Client.Android\Resources\Values\Strings.xml", NodeSelector=@"//resources/string[@name=""ApplicationName""]" , SetterEle= ( app, ele )=> ele.InnerText = Config.ApplicationName + " CallBox" },               
//
//                    new ConfigFile(this){ Source="public.keystore", Destination=@"Mobile\MK.Callbox.Mobile.Client.Android\public.keystore" },


                    /**TaxiHail **/
              
                    new ConfigFile(this){ Source="black_button.xml", Destination=@"Mobile\Android\Resources\Drawable\black_button.xml" },    
                    new ConfigFile(this){ Source="button_no_background.xml", Destination=@"Mobile\Android\Resources\Drawable\button_no_background.xml" },    
                    new ConfigFile(this){ Source="gray_button.xml", Destination=@"Mobile\Android\Resources\Drawable\gray_button.xml" },    
                    new ConfigFile(this){ Source="green_button.xml", Destination=@"Mobile\Android\Resources\Drawable\green_button.xml" },    
                    new ConfigFile(this){ Source="red_button.xml", Destination=@"Mobile\Android\Resources\Drawable\red_button.xml" },    
                    new ConfigFile(this){ Source="address_selector_button.xml", Destination=@"Mobile\Android\Resources\Drawable\address_selector_button.xml" },                     

                    new ConfigFile(this){ Source="backPickupDestination.png", Destination=@"Mobile\iOS\Assets\backPickupDestination.png" },    
                    new ConfigFile(this){ Source="backPickupDestination@2x.png", Destination=@"Mobile\iOS\Assets\backPickupDestination@2x.png" },                    

                      
                    new ConfigFile(this){ Source="Settings.json", Destination=@"Mobile\Common\Settings\Settings.json" },
                    new ConfigFile(this){ Source="Style.json", Destination=@"Mobile\Common\Style\Style.json" },

                    new ConfigFile(this){ Source="public.keystore", Destination=@"Mobile\Android\public.keystore" },
				    new ConfigSplash(this,"splash.png",@"Mobile\Android\Resources\","splash.png"),
                    new ConfigFile(this){ Source="Icon.png", Destination=@"Mobile\Android\Resources\Drawable\Icon.png" },            
	               
                    new ConfigFile(this){ Source="navBar.png", Destination=@"Mobile\Android\Resources\Drawable\navBar.png" },            
                    new ConfigFile(this){ Source="navBar@2x.png", Destination=@"Mobile\Android\Resources\Drawable-hdpi\navBar.png" },

                    new ConfigFile(this) { Source="pin_cab.png", Destination=@"Mobile\Android\Resources\Drawable\pin_cab.png" },			

                    new ConfigFile(this) { Source="pin_hail.png", Destination=@"Mobile\Android\Resources\Drawable\pin_hail.png" },				
                    new ConfigFile(this) { Source="pin_destination.png", Destination=@"Mobile\Android\Resources\Drawable\pin_destination.png" },				

                    new ConfigFile(this){ Source="backPickupDestination@2x.png", Destination=@"Mobile\Android\Resources\Drawable-hdpi\backPickupDestination.png" },    
                    new ConfigFile(this){ Source="backPickupDestination.png", Destination=@"Mobile\Android\Resources\Drawable\backPickupDestination.png" }, 
                    new ConfigFile(this){ Source="backgroundblue.png", Destination=@"Mobile\Android\Resources\Drawable\backgroundblue.png" },                        
	                

	                new ConfigXML(this){  Destination=@"Mobile\Android\Properties\AndroidManifest.xml", NodeSelector=@"//manifest", Attribute="package" , SetterAtt = ( app, att )=> att.Value = Config.Package  },
				    new ConfigXML(this){  Destination=@"Mobile\Android\Properties\AndroidManifest.xml", NodeSelector=@"//manifest/application", Attribute="android:label" , SetterAtt = ( app, att )=> att.Value = Config.ApplicationName  },
	                	
					/* open app from browser settings */
				    new ConfigSource(this) { Source = @"Mobile\Android\Activities\SplashActivity.cs", ToReplace = "TaxiHailDemo", ReplaceWith = Config.ApplicationName},

					/* notification */
	                new ConfigXML(this){  Destination=@"Mobile\Android\Properties\AndroidManifest.xml", NodeSelector=@"//manifest/permission[contains(@android:name,""permission.C2D_MESSAGE"")]", Attribute="android:name" , SetterAtt = ( app, att )=> att.Value = Config.Package + ".permission.C2D_MESSAGE" },
				    new ConfigXML(this){  Destination=@"Mobile\Android\Properties\AndroidManifest.xml", NodeSelector=@"//manifest/uses-permission[contains(@android:name,""permission.C2D_MESSAGE"")]", Attribute="android:name", SetterAtt = ( app, att )=> 
					{
                        att.Value = Config.Package + ".permission.C2D_MESSAGE";
					}},
					new ConfigMultiXML(this){  Destination=@"Mobile\Android\Properties\AndroidManifest.xml", NodeSelector=@"//manifest/application/receiver/intent-filter/category", Attribute="android:name" , SetterAtt = ( app, att )=> att.Value = Config.Package  },
	                

                    /** Google Maps */
                     new ConfigXML(this){  Destination=@"Mobile\Android\Properties\AndroidManifest.xml", NodeSelector=@"//manifest/permission[contains(@android:name,""permission.MAPS_RECEIVE"")]", Attribute="android:name" , SetterAtt = ( app, att )=> att.Value = Config.Package + ".permission.MAPS_RECEIVE" },
	                new ConfigXML(this){  Destination=@"Mobile\Android\Properties\AndroidManifest.xml", NodeSelector=@"//manifest/uses-permission[contains(@android:name,""permission.MAPS_RECEIVE"")]", Attribute="android:name", SetterAtt = ( app, att )=> 
					{
                        att.Value = Config.Package + ".permission.MAPS_RECEIVE";
					}},

				    new ConfigXML(this){  Destination=@"Mobile\Android\Resources\Values\String.xml", NodeSelector=@"//resources/string[@name=""ApplicationName""]" , SetterEle= ( app, ele )=> ele.InnerText = Config.ApplicationName  },               
                    
					new ConfigXML(this){  Destination=@"Mobile\Android\TaxiHail.csproj", NodeSelector=@"//a:Project/a:PropertyGroup[contains(@Condition, ""'Debug|AnyCPU'"")]/a:AndroidSigningKeyAlias" , SetterEle= ( app, ele )=> ele.InnerText = Config.AndroidSigningKeyAlias },               
					new ConfigXML(this){  Destination=@"Mobile\Android\TaxiHail.csproj", NodeSelector=@"//a:Project/a:PropertyGroup[contains(@Condition, ""'Release|AnyCPU'"")]/a:AndroidSigningKeyAlias" , SetterEle= ( app, ele )=> ele.InnerText = Config.AndroidSigningKeyAlias },               

					new ConfigXML(this){  Destination=@"Mobile\Android\TaxiHail.csproj", NodeSelector=@"//a:Project/a:PropertyGroup[contains(@Condition, ""'Debug|AnyCPU'"")]/a:AndroidSigningKeyPass" , SetterEle= ( app, ele )=> ele.InnerText = Config.AndroidSigningKeyPassStorePass},               
					new ConfigXML(this){  Destination=@"Mobile\Android\TaxiHail.csproj", NodeSelector=@"//a:Project/a:PropertyGroup[contains(@Condition, ""'Release|AnyCPU'"")]/a:AndroidSigningKeyPass" , SetterEle= ( app, ele )=> ele.InnerText = Config.AndroidSigningKeyPassStorePass },               

		                
					new ConfigXML(this){  Destination=@"Mobile\Android\TaxiHail.csproj", NodeSelector=@"//a:Project/a:PropertyGroup[contains(@Condition, ""'Debug|AnyCPU'"")]/a:AndroidSigningStorePass" , SetterEle= ( app, ele )=> ele.InnerText = Config.AndroidSigningKeyPassStorePass},               
					new ConfigXML(this){  Destination=@"Mobile\Android\TaxiHail.csproj", NodeSelector=@"//a:Project/a:PropertyGroup[contains(@Condition, ""'Release|AnyCPU'"")]/a:AndroidSigningStorePass" , SetterEle= ( app, ele )=> ele.InnerText = Config.AndroidSigningKeyPassStorePass },               


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

				    new ConfigPList(this){ Destination=@"Mobile\iOS\Info.plist", Key = "CFBundleDisplayName",  SetterEle = ( ele )=> ele.InnerText = Config.ApplicationName },
                    new ConfigPList(this){ Destination=@"Mobile\iOS\Info.plist", Key = "CFBundleIdentifier",  SetterEle = ( ele )=> ele.InnerText = Config.Package },
                    new ConfigPList(this){ Destination=@"Mobile\iOS\Info.plist", Key = "CFBundleURLSchemes",  SetterEle = ( ele )=> 
	                    {
	                            if ( string.IsNullOrEmpty( Config.FacebookAppId ) )
	                            {
								ele.InnerXml = string.Format( "<string>mk{1}</string>", Config.FacebookAppId, Config.ApplicationName.Replace( " " , string.Empty ));
	                            }
	                            else
	                            {
								ele.InnerXml = string.Format( "<string>fb{0}{1}</string><string>mk{1}</string>", Config.FacebookAppId, Config.ApplicationName.Replace( " " , string.Empty ) );
	                            }
						}
					},
					/** Version 1.5 */
				new ConfigXML(this)
				{  
					Destination=@"Mobile\Android\Resources\Values\Themes.xml", 
					NodeSelector=@"//resources/color[@name=""login_background_color""]", 
					SetterEle = (app,ele) => ele.InnerText = GetHexaColorCode(Company.Style.NavigationBarColor) 
				},
				new ConfigXML(this)
				{  
					Destination=@"Mobile\Android\Resources\Values\Themes.xml", 
					NodeSelector=@"//resources/color[@name=""button_main_background_color""]", 
					SetterEle = (app,ele) => ele.InnerText = GetHexaColorCode(Company.Style.NavigationBarColor) + "4b" 
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
				new ConfigFile(this){ Source="logo_1_5@2x.png", Destination=@"Mobile\Android\Resources\drawable-hdpi\th_logo.png" },
				new ConfigFile(this){ Source="logo_1_5.png", Destination=@"Mobile\iOS\Resources\th_logo.png" },
				new ConfigFile(this){ Source="logo_1_5@2x.png", Destination=@"Mobile\iOS\Resources\th_logo@2x.png" },

				new ConfigXML(this)
				{  
					Destination=@"Mobile\iOS\Style\Theme.xml", 
					NodeSelector=@"//ThemeValues/LoginBackgroundColor", 
					SetterEle = (app,ele) => ele.InnerText = GetHexaColorCode(Company.Style.NavigationBarColor) 
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
				}

           };
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
