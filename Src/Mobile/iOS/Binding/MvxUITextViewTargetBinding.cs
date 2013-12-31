using System.Reflection;
using Cirrious.MvvmCross.Binding.Bindings.Target;
using Cirrious.MvvmCross.Binding.Interfaces;
using Cirrious.MvvmCross.Interfaces.Platform.Diagnostics;
using MonoTouch.UIKit;
using Cirrious.MvvmCross.Binding;
using System;

namespace apcurium.MK.Booking.Mobile.Client.Binding
{
    public class  TextViewDelegate :UITextViewDelegate
    {
        private readonly Action _onEndEdit;
        public TextViewDelegate( Action onEndEdit )
        {
            _onEndEdit = onEndEdit;
        }
        public override void Changed (UITextView textView)
        {
            _onEndEdit();
        }

    }

    public class MvxUITextViewTargetBinding: MvxPropertyInfoTargetBinding<UITextView>
    {
      

        public MvxUITextViewTargetBinding(object target, PropertyInfo targetPropertyInfo)
            : base(target, targetPropertyInfo)
        {
            var editText = View;
            if (editText == null)
            {
                MvxBindingTrace.Trace(MvxTraceLevel.Error,"Error - UITextView is null in MvxUITextViewTargetBinding");
            }
            else
            {

                editText.Delegate = new TextViewDelegate( HandleEditTextValueChanged);
            }
        }


        void HandleEditTextValueChanged ()
        {
            FireValueChanged(View.Text);
        }
        
        public override MvxBindingMode DefaultMode
        {
            get
            {
                return MvxBindingMode.TwoWay;
            }
        }
        
        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            if (isDisposing)
            {
                var editText = View;
                if (editText != null)
                {
                    editText.Delegate = new UITextViewDelegate();
                }
            }
        }
    }
}
