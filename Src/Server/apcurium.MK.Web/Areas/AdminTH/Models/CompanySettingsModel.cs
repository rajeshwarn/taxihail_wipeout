using System.Collections.Generic;
using System.Reflection;
using apcurium.MK.Common.Configuration;
using MK.Common.Configuration;

namespace apcurium.MK.Web.Areas.AdminTH.Models
{
	public class CompanySettingsModel
	{
		public CompanySettingsModel(IServerSettings serverSettings, bool isSuperAdmin)
		{
			Settings = serverSettings.ServerData;
			IsSuperAdmin = isSuperAdmin;
			SuperAdminSettings = new Dictionary<string, PropertyInfo>();
			AdminSettings = new Dictionary<string, PropertyInfo>();
		}

		public ServerTaxiHailSetting Settings { get; set; }

		public bool IsSuperAdmin { get; set; }

		public Dictionary<string, PropertyInfo> SuperAdminSettings { get; set; }

		public Dictionary<string, PropertyInfo> AdminSettings { get; set; }
	}
}
