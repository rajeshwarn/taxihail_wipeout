using System;
using CrossUI.Droid.Dialog.Elements;
using Android.Views;
using Android.Widget;
using Android.Text;

namespace apcurium.MK.Booking.Mobile.Client.Dialog
{
	 

	public class EntryElementWithInputType : EntryElement
	{
		private InputTypes _inputTypes;

		public EntryElementWithInputType( string caption, string  hint, string  value, string layoutName , InputTypes inputTypes):base( caption, hint, value, layoutName )
		{
			_inputTypes = inputTypes;
		}
		protected override void UpdateCaptionDisplay (View cell)
		{
			base.UpdateCaptionDisplay (cell);
			SetCellInputView (cell);

		}

		void SetCellInputView( View cell )
		{
			if (cell != null) {
				var editText = cell.FindViewById<EditText> (Resource.Id.dialog_ValueField);
				if (editText != null) {
					editText.InputType = _inputTypes;
				}
			}
		}

		protected override void UpdateCellDisplay (View cell)
		{
			base.UpdateCellDisplay (cell);
			SetCellInputView (cell);

		}

		protected override void UpdateDetailDisplay (View cell)
		{
			base.UpdateDetailDisplay (cell);
			SetCellInputView (cell);
		}
	}
}

