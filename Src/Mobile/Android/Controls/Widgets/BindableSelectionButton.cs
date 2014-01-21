using System;
using Android.Content;
using Android.Runtime;
using Android.Util;
using Android.Widget;
using apcurium.MK.Booking.Mobile.Style;
using System.Windows.Input;

namespace apcurium.MK.Booking.Mobile.Client.Controls
{
    /// <summary>
    /// A button which update the selected property when clicked
    /// </summary>
    public class BindableSelectionButton : Button
    {
        protected BindableSelectionButton(IntPtr javaReference, JniHandleOwnership transfer)
            : base(javaReference, transfer)
        {
            Initialize();
        }

        public BindableSelectionButton(Context context) : base(context)
        {
            Initialize();
        }

        public BindableSelectionButton(Context context, IAttributeSet attrs) : base(context, attrs)
        {
            Initialize();
        }

        public BindableSelectionButton(Context context, IAttributeSet attrs, int defStyle)
            : base(context, attrs, defStyle)
        {
            Initialize();
        }

// ReSharper disable once MemberCanBePrivate.Global
// ReSharper disable once UnusedAutoPropertyAccessor.Global
        public ICommand SelectedChangedCommand { get; set; }

        private void Initialize()
        {
            Click += delegate
            {
                Selected = !Selected;
                if (Selected
                    && SelectedChangedCommand != null
                    && SelectedChangedCommand.CanExecute())
                {
                    SelectedChangedCommand.Execute(Tag);
                }
            };

            if (StyleManager.Current.ButtonFontSize.HasValue)
            {
                TextSize = StyleManager.Current.ButtonFontSize.Value;
            }
        }
    }
}