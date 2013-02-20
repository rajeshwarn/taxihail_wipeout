using System;
using Android.Content;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using Cirrious.MvvmCross.Interfaces.Commands;
using apcurium.MK.Booking.Mobile.Style;
using apcurium.MK.Booking.Mobile.Client.Helpers;

namespace apcurium.MK.Booking.Mobile.Client.Controls
{
    /// <summary>
    /// A button which update the selected property when clicked
    /// </summary>
    public class BindableSelectionButton : Button
    {
        protected BindableSelectionButton(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {
            Initialize();
        }

        private void Initialize()
        {
            this.Click += delegate
                              {
                                  Selected = !Selected;
                                  if (Selected
                                      && SelectedChangedCommand != null
                                      && SelectedChangedCommand.CanExecute())
                                  {
                                      SelectedChangedCommand.Execute(this.Tag);
                                  }
                              };

            if ( StyleManager.Current.ButtonFontSize.HasValue )
            {
                TextSize =  StyleManager.Current.ButtonFontSize.Value;
            }

        }

        public BindableSelectionButton(Context context) : base(context)
        {
            Initialize();
        }

        public BindableSelectionButton(Context context, IAttributeSet attrs) : base(context, attrs)
        {
            Initialize();
        }

        public BindableSelectionButton(Context context, IAttributeSet attrs, int defStyle) : base(context, attrs, defStyle)
        {
            Initialize();
        }

        public IMvxCommand SelectedChangedCommand { get; set; }
    }
}