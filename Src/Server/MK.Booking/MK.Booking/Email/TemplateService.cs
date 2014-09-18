#region

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using apcurium.MK.Booking.Resources;
using apcurium.MK.Booking.Services.Impl;
using apcurium.MK.Common.Configuration;
using Nustache.i18n;

#endregion

namespace apcurium.MK.Booking.Email
{
    public class TemplateService : ITemplateService
    {
        private readonly Resources.Resources _resources;
        private const string DefaultLanguageCode = "en";
        private readonly Dictionary<string, string> _templatesDictionary=new Dictionary<string, string>();

        public TemplateService(IConfigurationManager configurationManager, IAppSettings appSettings)
        {
            _resources = new Resources.Resources(configurationManager.GetSetting("TaxiHail.ApplicationKey"), appSettings);
        }
        
        public static string AssemblyDirectory
        {
            get
            {
                var codeBase = Assembly.GetExecutingAssembly().CodeBase;
                var uri = new UriBuilder(codeBase);
                var path = Uri.UnescapeDataString(uri.Path);
                return Path.GetDirectoryName(path);
            }
        }

        public string Find(string templateName, string languageCode = DefaultLanguageCode)
        {
            if (string.IsNullOrWhiteSpace(languageCode))
            {
                languageCode = DefaultLanguageCode;
            }

            var path = GetTemplatePath(templateName, languageCode);
            if (File.Exists(path))
            {
                
                var templateBody = File.ReadAllText(path);
                var translatedTemplateBody = Localizer.Translate(templateBody, _resources.GetLocalizedDictionary(languageCode), "!!MISSING!!");

                if (!_templatesDictionary.ContainsKey(templateName))
                {
                    var result =
                        PreMailer.Net.PreMailer.MoveCssInline(translatedTemplateBody, true, ignoreElements: "#ignore")
                            .Html;
                    _templatesDictionary.Add(templateName, result);
                    return result;
                }
                else
                {
                    return _templatesDictionary[templateName];
                }


            }

            return null;
        }

        public string Render(string template, object data)
        {
            if (template == null) throw new ArgumentNullException("template");
            return Nustache.Core.Render.StringToString(template, data);
        }

        public string ImagePath(string imageName)
        {
            return Path.Combine(AssemblyDirectory, "Email\\Images", imageName);
        }

        private string GetTemplatePath(string templateName, string languageCode)
        {
            // first check for language specific template
            var path = Path.Combine(AssemblyDirectory, "Email\\Templates", languageCode, templateName + ".html");
            if (File.Exists(path))
            {
                return path;
            }

            // if not found, return the default template
            return Path.Combine(AssemblyDirectory, "Email\\Templates", templateName + ".html");
        }
    }
}