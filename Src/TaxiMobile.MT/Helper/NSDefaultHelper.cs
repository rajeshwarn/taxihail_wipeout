using MonoTouch.Foundation;
using TaxiMobile.Lib.Framework.Extensions;

namespace TaxiMobile.Helper
{
	public static class NSDefaultHelper
	{

		public static T GetSerializedObject<T> (this NSUserDefaults instance, string defaultName)
		{
			return SerializerHelper.DeserializeObject<T> (instance.StringForKey (defaultName));
		}

		public static void SetSerializedObject<T> (this NSUserDefaults instance, T data, string defaultName)
		{
			if (data != null) {
				instance.SetString (data.Serialize (), defaultName);
			} else {
				instance.RemoveObject (defaultName);
			}
			instance.Synchronize ();
		}

		public static void SetStringOrClear (this NSUserDefaults instance, string data, string defaultName)
		{
			if (data.HasValue ()) {
				instance.SetString (data, defaultName);
			} else {
				instance.RemoveObject (defaultName);
			}
			instance.Synchronize ();
		}
		
	}
}

