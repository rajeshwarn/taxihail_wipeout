using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.Security;
using apcurium.MK.Common;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Web.Attributes;
using Infrastructure.Messaging;
using ServiceStack.CacheAccess;
using ServiceStack.Configuration;
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
            return View(_serverSettings.ServerData);
        }

        // POST: AdminTH/CompanySettings/Update
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Update(FormCollection form)
        {
            var appSettings = form.ToDictionary();
            appSettings.Remove("__RequestVerificationToken");

            if (appSettings.Any())
            {
                var command = new AddOrUpdateAppSettings
                {
                    AppSettings = appSettings,
                    CompanyId = AppConstants.CompanyId
                };
                _commandBus.Send(command);
            }

            TempData["Info"] = "Settings updated";
            TempData["CompanySetting"] = form;

            return RedirectToAction("Index");
        }
    }
}