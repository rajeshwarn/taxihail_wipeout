using System;
using System.Linq;
using CustomerPortal.Web.Entities;
using System.Collections.Generic;

namespace MK.DeploymentService.Mobile
{
	public static class SettingsExtensions
	{
		public static bool ContainsKey( this List<CompanySetting> list, string key )
		{
			return list.Any (l => l.Key == key);
		}

		public static string GetValue( this List<CompanySetting> list, string key )
		{
			if (list.Any (l => l.Key == key)) {
				return list.First (l => l.Key == key).Value;
			} else {
				return null;
			}
		}

	}
}

