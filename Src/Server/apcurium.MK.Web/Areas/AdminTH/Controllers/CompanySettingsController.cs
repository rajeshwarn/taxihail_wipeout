using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.Security;
using apcurium.MK.Common;
using apcurium.MK.Common.Configuration;
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
            return View(FilterAvailableSettings());
        }

        // POST: AdminTH/CompanySettings/Update
        [HttpPost]
        [ValidateInput(false)]
        public async Task<ActionResult> Update(FormCollection form)
        {
            var appSettings = form.ToDictionary();
            appSettings.Remove("__RequestVerificationToken");

            if (appSettings.Any())
            {
                SetSettingsHiddenToAdmin(appSettings);

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

        private void SetSettingsHiddenToAdmin(Dictionary<string, string> appSettings)
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

        private CompanySettings FilterAvailableSettings()
        {
            var settings = _serverSettings.ServerData.GetType().GetAllProperties().OrderBy(s => s.Key);

            var isSuperAdmin = Convert.ToBoolean(ViewData["IsSuperAdmin"]);
            var companySettings = new CompanySettings(_serverSettings, isSuperAdmin);
            
            foreach (var setting in settings)
            {
                if (setting.Key == "SettingsAvailableToAdmin")
                {
                    // Do not display this property as it is displayed as checkboxes to the user
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