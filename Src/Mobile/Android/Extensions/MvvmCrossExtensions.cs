using Cirrious.MvvmCross.Binding.BindingContext;

namespace apcurium.MK.Booking.Mobile.Client.Extensions
{
    public static class MvvmCrossExtensions
    {
        public static MvxFluentBindingDescription<TChildTarget, TSource> BindSafe<TChildTarget, TSource, TOwningTarget>(this MvxFluentBindingDescriptionSet<TOwningTarget, TSource> bindingSet, TChildTarget childTarget) where TChildTarget : class where TOwningTarget : class, IMvxBindingContextOwner
        {
            if (childTarget != null) {
                return bindingSet.Bind (childTarget);
            }
            return new MvxFluentBindingDescription<TChildTarget, TSource>(null, childTarget);
        }
    }
}

