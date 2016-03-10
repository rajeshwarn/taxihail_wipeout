using UIKit;
using System.Linq;
using Cirrious.MvvmCross.Binding.Touch.Target;
using Cirrious.MvvmCross.Binding.ExtensionMethods;
using Cirrious.MvvmCross.Binding.Bindings.Target.Construction;

namespace apcurium.MK.Booking.Mobile.Client.Binding
{
    public class MvxUIViewHiddenExTargetBinding : MvxBaseUIViewVisibleTargetBinding
    {
        public static void Register(IMvxTargetBindingFactoryRegistry registry)
        {           
            registry.RegisterFactory(new MvxCustomBindingFactory<UIView>("HiddenEx", obj => new MvxUIViewHiddenExTargetBinding(obj)));
        }

        public MvxUIViewHiddenExTargetBinding(UIView target)
            : base(target)
        {
        }

        protected override void SetValueImpl(object target, object value)
        {
            var view = View;
            if (view == null)
                return;

            var hidden = value.ConvertToBoolean();
            HideAndDealWithConstraints(view, hidden);
        }

        private NSLayoutConstraint[] _detachedConstraints { get; set; }

        private void HideAndDealWithConstraints(UIView view, bool hidden)
        {
            view.Hidden = hidden;

            if (hidden)
            {
                _detachedConstraints = view.Superview.Constraints != null 
                    ? view.Superview.Constraints.Where(x => x.FirstItem == view || x.SecondItem == view).ToArray()
                    : null;

                view.Superview.RemoveConstraints(_detachedConstraints);
            }

            if (!hidden && _detachedConstraints != null)
            {
                view.Superview.AddConstraints(_detachedConstraints);
                _detachedConstraints = null;
            }
        }
    }
}

