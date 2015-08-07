using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.Security;
using apcurium.MK.Common;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Configuration.Attributes;
using apcurium.MK.Common.Enumeration.TimeZone;
using apcurium.MK.Common.Extensions;
using apcurium.MK.Web.Areas.AdminTH.Models;
using apcurium.MK.Web.Attributes;
using Infrastructure.Messaging;
using ServiceStack.CacheAccess;
using ServiceStack.Common;
using ServiceStack.ServiceModel.Extensions;

namespace apcurium.MK.Web.Areas.AdminTH.Controllers
{
    [AuthorizationRequired(RoleName.Admin)]
    public class CompanySettingsController : ServiceStackController
    {
        private readonly IServerSettings _serverSettings;
        private readonly ICommandBus _commandBus;

        // GET: AdminTH/CompanySettings
        public CompanySettingsController(ICacheClient cache, IServerSettings serverSettings, ICommandBus commandBus)
            : base(cache, serverSettings)
        {
            _serverSettings = serverSettings;
            _commandBus = commandBus;
        }

        public ActionResult Index()
        {
            return View(GetAvailableSettingsForUser());
        }

        // POST: AdminTH/CompanySettings/Update
        [HttpPost]
        [ValidateInput(false)]
        public async Task<ActionResult> Update(FormCollection form)
        {
            var appSettings = form.ToDictionary();
            appSettings.Remove("__RequestVerificationToken");

            if (!ValidateSettingsValue(appSettings))
            {
                return RedirectToAction("Index");
            }

            if (appSettings.Any())
            {
                // Only the superadmin should be able to change the availability option of the settings.
                var isSuperAdmin = Convert.ToBoolean(ViewData["IsSuperAdmin"]);
                if (isSuperAdmin)
                {
                    SetSettingsAvailableToAdmin(appSettings);
                }

                var command = new AddOrUpdateAppSettings
                {
                    AppSettings = appSettings,
                    CompanyId = AppConstants.CompanyId
                };
                _commandBus.Send(command);
            }

            TempData["Info"] = "Settings updated.";

            // Wait for settings to be updated before reloading the page
            await Task.Delay(2000);

            return RedirectToAction("Index");
        }

        private bool ValidateSettingsValue(Dictionary<string, string> appSettings)
        {
            var isValid = true;
            var errorMessageBuilder = new StringBuilder();

            var model = GetAvailableSettingsForUser();

            foreach (var appSetting in appSettings)
            {
                Type settingType;
                string settingName;

                if (model.SuperAdminSettings.ContainsKey(appSetting.Key))
                {
                    settingType = model.SuperAdminSettings[appSetting.Key].PropertyType;
                    settingName = model.SuperAdminSettings[appSetting.Key].GetDisplayName();
                }
                else if (model.AdminSettings.ContainsKey(appSetting.Key))
                {
                    settingType = model.AdminSettings[appSetting.Key].PropertyType;
                    settingName = model.AdminSettings[appSetting.Key].GetDisplayName();
                }
                else
                {
                    continue;
                }

                var typeConverter = TypeDescriptor.GetConverter(settingType);
                    
                if (typeConverter.CanConvertFrom(typeof(string)))
                {
                    try
                    {
                        if (settingType == typeof (bool?))
                        {
                            if (appSetting.Value == "null")
                            {
                                // Null value is valid for type NullableBool
                                continue;
                            }
                        }

                        // Try to convert the value to the setting type
                        typeConverter.ConvertFromString(appSetting.Value);
                    }
                    catch (Exception)
                    {
                        isValid = false;

                        if (errorMessageBuilder.Length > 0)
                        {
                            errorMessageBuilder.Append(", "); 
                        }
                        errorMessageBuilder.Append(settingName);
                    }
                }
            }


			bool disableImmediateBooking = bool.Parse(appSettings.Where(x => x.Key == "DisableImmediateBooking").First().Value);
            bool disableFutureBooking = bool.Parse(appSettings.Where(x => x.Key == "DisableFutureBooking").First().Value);

            if (disableImmediateBooking && disableFutureBooking)
            {
                isValid = false;

                if (errorMessageBuilder.Length > 0)
                {
                    errorMessageBuilder.Append(", ");
                }
                errorMessageBuilder.Append("Disable Immediate Booking and Disable Future Booking can not be 'Yes' simultaneously");
            }

            if (!isValid)
            {
                // Set error message
                TempData["ValidationErrors"] = string.Format("Invalid value for settings: {0}", errorMessageBuilder);
            }

            return isValid;
        }

        private void SetSettingsAvailableToAdmin(Dictionary<string, string> appSettings)
        {
            var checkBoxKeys = appSettings.Keys.Where(k => k.StartsWith("CheckBox_")).ToArray();

            var settingsAvailableToAdmin = new StringBuilder();

            foreach (var checkBoxKey in checkBoxKeys)
            {
                var isSettingAvailableToAdmin = Convert.ToBoolean(appSettings[checkBoxKey]);
                if (isSettingAvailableToAdmin)
                {
                    if (settingsAvailableToAdmin.Length > 0)
                    {
                        settingsAvailableToAdmin.Append(',');
                    }

                    var settingName = checkBoxKey.Replace("CheckBox_", string.Empty);
                    settingsAvailableToAdmin.Append(settingName);
                }
            }

            appSettings.RemoveKeys(checkBoxKeys);
            appSettings["SettingsAvailableToAdmin"] = settingsAvailableToAdmin.ToString();
        }

        private CompanySettings GetAvailableSettingsForUser()
        {
            var settings = _serverSettings.ServerData.GetType().GetAllProperties().OrderBy(s => s.Key);

            var isSuperAdmin = Convert.ToBoolean(ViewData["IsSuperAdmin"]);
            var companySettings = new CompanySettings(_serverSettings, isSuperAdmin);
            
            foreach (var setting in settings)
            {
                var attributes = setting.Value.GetCustomAttributes(false);
                var isSettingHidden = attributes.OfType<HiddenAttribute>().FirstOrDefault();
                if (isSettingHidden != null)
                {
                    // Setting is hidden, do not display to user
                    continue;
                }

                var isSettingAvailableToAdmin =
                    _serverSettings.ServerData.SettingsAvailableToAdmin.HasValue()
                    && _serverSettings.ServerData.SettingsAvailableToAdmin.Contains(setting.Key);

                if (isSuperAdmin)
                {
                    if (isSettingAvailableToAdmin)
                    {
                        companySettings.AdminSettings.Add(setting.Key, setting.Value);
                    }
                    else
                    {
                        companySettings.SuperAdminSettings.Add(setting.Key, setting.Value);
                    }
                }
                else if (isSettingAvailableToAdmin)
                {
                    companySettings.AdminSettings.Add(setting.Key, setting.Value);
                }
            }

            return companySettings;
        }
    }
}