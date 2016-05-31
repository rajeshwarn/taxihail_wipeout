﻿using Microsoft.Build.Evaluation;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace SetupDev
{
    class Program
    {
        public const string SourceFolderKey = "SourceFolder";
        public const string CustomerPortalProjectFileKey = "CustomerPortalWebProject";
        public const string CustomerPortalWebConfigKey = "CustomerPortalWebConfig";
        public const string MongoDBConfigSettingKey = "MongoDBConfigSetting";
        public const string JsonSettingsKey = "JsonSettings";
        public const string ServerMiddleWaraWebConfigKey = "ServerMiddleWaraWebConfig";
        public const string ServerMiddleWareDBConnectionStringKey = "ServerMiddleWareDBConnectionString";

        public const string JsonServiceURL = "ServiceUrl";
        public const string JsonTaxiHailApplicationKey = "TaxiHail.ApplicationKey";
        public const string JsonTaxiHailApplicationName = "TaxiHail.ApplicationName";

        public static string _rootFolder;
        public static NameValueCollection _appSettings;
        static void Main(string[] args)
        {
            Console.WriteLine("Reading app settings from config file...");
            _appSettings = ConfigurationManager.AppSettings;

            Console.WriteLine("Reading root folder");
            _rootFolder = _appSettings[SourceFolderKey];
            Console.WriteLine(String.Format("Root folder: {0}", _rootFolder));


            Console.WriteLine("Updating customer json settings...");
            UpdateJSonCode();


            Console.WriteLine("Updating customer portal settings...");
            UpdateCustomerPortalCode();

            Console.WriteLine("Updating customer middle ware settings...");
            UpdateMiddleWareCode();

        }

        #region middleware
        private static void UpdateMiddleWareCode()
        {
            UpdateMiddleWareWebConfig();
        }

        private static void UpdateMiddleWareWebConfig()
        {
            try
            {
                // load the web.config file
                string middleWareWebConfig = _rootFolder + _appSettings[ServerMiddleWaraWebConfigKey];

                XmlDocument doc = new XmlDocument();
                doc.Load(middleWareWebConfig);

                XmlNodeList nodeList;
                nodeList = doc.SelectNodes("/configuration/connectionStrings/add");

                XmlAttribute dbConnectionString = (XmlAttribute)doc.SelectSingleNode("/configuration/connectionStrings/add/@connectionString");
                string oldValue = dbConnectionString.Value;
                dbConnectionString.Value = _appSettings[ServerMiddleWareDBConnectionStringKey];
                Console.WriteLine(String.Format("Updating connection string from " + Environment.NewLine + "{0} " + Environment.NewLine + "to " + Environment.NewLine + "{1}", oldValue, dbConnectionString.Value));

                using (XmlTextWriter xtw = new XmlTextWriter(middleWareWebConfig, Encoding.UTF8))
                {
                    xtw.Formatting = System.Xml.Formatting.Indented; // optional, if you want it to look nice
                    doc.WriteContentTo(xtw);
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(String.Format("Unexpected error in UpdateMiddleWareWebConfig: {0}", e.Message + Environment.NewLine + e.InnerException + Environment.NewLine + e.StackTrace.ToString()));
            }

        }
        #endregion

        #region Customer Portal
        /// <summary>
        /// update the UseIIS setting to False in the CustomerPortal.Web.csproj file
        /// </summary>
        private static void UpdateCustomerPortalCode()
        {
            UpdateCustomerPortalWebConfigFile();

            UpdateCustomerPortalCSProjFile();

        }
        private static void UpdateCustomerPortalWebConfigFile()
        {
            try
            {
                // load the web.config file
                string customerPortalWebConfigFile = _rootFolder + _appSettings[CustomerPortalWebConfigKey];

                XmlDocument doc = new XmlDocument();
                doc.Load(customerPortalWebConfigFile);

                XmlNodeList nodeList;
                nodeList = doc.SelectNodes("/configuration/connectionStrings/add");

                XmlAttribute mongoDBConnectionString = (XmlAttribute)doc.SelectSingleNode("/configuration/connectionStrings/add/@connectionString");
                string oldValue = mongoDBConnectionString.Value;
                mongoDBConnectionString.Value = _appSettings[MongoDBConfigSettingKey];
                Console.WriteLine(String.Format("Updating connection string from " + Environment.NewLine + "{0} " + Environment.NewLine + "to " + Environment.NewLine + "{1} ", oldValue, mongoDBConnectionString.Value));

                using (XmlTextWriter xtw = new XmlTextWriter(customerPortalWebConfigFile, Encoding.UTF8))
                {
                    xtw.Formatting = System.Xml.Formatting.Indented; // optional, if you want it to look nice
                    doc.WriteContentTo(xtw);
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(String.Format("Unexpected error in UpdateCustomerPortalWebConfigFile: {0}", e.Message + Environment.NewLine + e.InnerException + Environment.NewLine + e.StackTrace.ToString()));
            }

        }
        private static void UpdateCustomerPortalCSProjFile()
        {
            try
            {
                // load the csproj file
                string customerPortalProjectPath = _rootFolder + _appSettings[CustomerPortalProjectFileKey];

                XmlDocument doc = new XmlDocument();
                doc.Load(customerPortalProjectPath);
                // Make changes to the document.
                //XmlNodeList list = doc.SelectNodes("/project/configuration[name='Debug']/settings[name='ILINK']/data/option[name='IlinkConfigDefines']/state");

                XmlNodeList nodeList;
                if (doc.DocumentElement.Attributes["xmlns"] != null)
                {
                    string xmlns = doc.DocumentElement.Attributes["xmlns"].Value;
                    XmlNamespaceManager nsmgr = new XmlNamespaceManager(doc.NameTable);

                    nsmgr.AddNamespace("MsBuild", xmlns);

                    //nodeList = doc.SelectNodes("/MsBuild:Project/MsBuild:ProjectExtensions/MsBuild:VisualStudio/MsBuild:FlavorProperties/MsBuild:WebProjectProperties/*", nsmgr);
                    nodeList = doc.SelectNodes("/MsBuild:Project/MsBuild:ProjectExtensions/MsBuild:VisualStudio/MsBuild:FlavorProperties/MsBuild:WebProjectProperties/MsBuild:UseIIS", nsmgr);
                }
                else
                {
                    nodeList = doc.SelectNodes("/Project/ProjectExtensions/VisualStudio/FlavorProperties/WebProjectProperties/UseIIS");
                }

                Console.WriteLine("Updating UseIIS element to false");
                foreach (var node in nodeList)
                {
                    ((XmlNode)node).InnerText = "False";
                }

                using (XmlTextWriter xtw = new XmlTextWriter(customerPortalProjectPath, Encoding.UTF8))
                {
                    xtw.Formatting = System.Xml.Formatting.Indented; // optional, if you want it to look nice
                    doc.WriteContentTo(xtw);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(String.Format("Unexpected error in UpdateCustomerPortalCSProjFile: {0}", e.Message + Environment.NewLine + e.InnerException + Environment.NewLine + e.StackTrace.ToString()));
            }
        }

        #endregion

        #region json
        private static void UpdateJSonCode()
        {
            UpdateJsonSettings();
        }

        private static void UpdateJsonSettings()
        {
            try
            {

                string jsonFileName = _rootFolder + _appSettings[JsonSettingsKey];
                string nextTokenName = String.Empty;
                var fs = File.Open(jsonFileName, FileMode.OpenOrCreate, FileAccess.ReadWrite);
                var fsnew = File.Open(jsonFileName+".tmp", FileMode.OpenOrCreate, FileAccess.ReadWrite);
                //var sw = new StreamWriter(fs);
                //var sr = new StreamReader(fs);

                JsonTextReader reader = new JsonTextReader(new StreamReader(fs));
                StreamWriter streamWriter = new StreamWriter(fsnew);
                //StringWriter sw = new StringWriter(sb);
                using (JsonWriter writer = new JsonTextWriter(streamWriter))
                {
                    writer.Formatting = Newtonsoft.Json.Formatting.Indented;
                    writer.WriteStartObject();

                    while (reader.Read())
                    {
                        if (reader.Value != null)
                        {
                            Console.WriteLine("Token: {0}, Value: {1}", reader.TokenType, reader.Value);
                            if (reader.TokenType == JsonToken.PropertyName)
                            {
                                nextTokenName = String.Empty;
                                if (reader.Value.ToString() == JsonServiceURL)
                                {
                                    nextTokenName = JsonServiceURL;
                                }
                                else if (reader.Value.ToString() == JsonTaxiHailApplicationKey)
                                {
                                    nextTokenName = JsonTaxiHailApplicationKey;
                                }
                                else if (reader.Value.ToString() == JsonTaxiHailApplicationName)
                                {
                                    nextTokenName = JsonTaxiHailApplicationName;
                                }
                                writer.WritePropertyName(reader.Value.ToString());
                            }
                            else
                            {
                                if (nextTokenName == JsonServiceURL)
                                {
                                    writer.WriteValue(_appSettings[JsonServiceURL]);
                                }
                                else if (nextTokenName == JsonTaxiHailApplicationKey)
                                {
                                    writer.WriteValue(_appSettings[JsonTaxiHailApplicationKey]);
                                }
                                else if (nextTokenName == JsonTaxiHailApplicationName)
                                {
                                    writer.WriteValue(_appSettings[JsonTaxiHailApplicationName]);
                                }
                                else
                                {
                                    writer.WriteValue(reader.Value);
                                }
                            }
                        }
                        else
                        {
                            Console.WriteLine("Token: {0}", reader.TokenType);
                            if (reader.TokenType == JsonToken.PropertyName)
                                writer.WritePropertyName(reader.Value.ToString());
                            //else
                                //writer.WriteValue();
                        }
                    }

                    //writer.WriteEnd();
                    writer.WriteEndObject();

                    writer.Flush();
                    writer.Close();
                }

                reader.Close();


                fs.Close();
                fsnew.Close();
                System.IO.File.Delete(jsonFileName);
                System.IO.File.Move(jsonFileName+".tmp", jsonFileName);
            }
            catch (Exception e)
            {
                Console.WriteLine(String.Format("Unexpected error in UpdateJsonSettings: {0}", e.Message + Environment.NewLine + e.InnerException + Environment.NewLine + e.StackTrace.ToString()));
            }
        }
        
        #endregion



    }
}
