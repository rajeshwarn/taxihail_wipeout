using System;
using MonoTouch.UIKit;

namespace TaxiMobileApp
{
	public class Binding<T>
	{
		private UITextField _txt;
		private T _data;
		//private Func< T, string> _getValue;
		private Action<T, string> _setValue;
		
		public Binding ( UITextField txt, T data, Func< T, string> getValue, Action<T, string> setValue )
		{
			_txt = txt;
			_data = data;
		 //	_getValue = getValue;
			_setValue = setValue; 
			
			txt.EditingChanged += HandleTxtEditingChanged;
			txt.EditingDidEnd  += TxtValueChanged;
			txt.Text = getValue( data );
		}

		void HandleTxtEditingChanged (object sender, EventArgs e)
		{
			_setValue( _data, _txt.Text );
		}

		void TxtValueChanged (object sender, EventArgs e)
		{
			_setValue( _data, _txt.Text );
		}
		
		
	}
}

