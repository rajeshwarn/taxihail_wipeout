using apcurium.MK.Booking.Mobile.Framework.Extensions;
using MonoTouch.Foundation;

namespace apcurium.MK.Booking.Mobile.Client.Helper
{
// ReSharper disable once InconsistentNaming
	public static class NSDefaultHelper
	{

		public static T GetSerializedObject<T> (this NSUserDefaults instance, string defaultName)
		{
			return SerializerHelper.DeserializeObject<T> (instance.StringForKey (defaultName));
		}

		public static void SetSerializedObject<T> (this NSUserDefaults instance, T data, string defaultName) where T : class
		{
			if (data != null) {
				instance.SetString (data.Serialize(), defaultName);
			} else {
				instance.RemoveObject (defaultName);
			}
			instance.Synchronize ();
		}

		public static void SetStringOrClear (this NSUserDefaults instance, string data, string defaultName)
		{
			if (data.HasValue()) {
				instance.SetString (data, defaultName);
			} else {
				instance.RemoveObject (defaultName);
			}
			instance.Synchronize ();
		}
		
	}
}

