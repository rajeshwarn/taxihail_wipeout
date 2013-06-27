using System;
using Android.Widget;
using Android.Views.InputMethods;
using Android.App;
using Android.Content;

namespace Extensions
{
	public static class EditTextExtensions
	{
		public static void HideKeyboard(this EditText thisControl, Activity parentActivity)
		{			
			((InputMethodManager)parentActivity.GetSystemService(Context.InputMethodService)).HideSoftInputFromWindow(thisControl.WindowToken,0);//Hide keyboard
		}
	}
}

