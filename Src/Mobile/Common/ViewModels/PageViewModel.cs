using System;
using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using Cirrious.CrossCore;
using Cirrious.MvvmCross.Platform;
using Cirrious.MvvmCross.ViewModels;
using MK.Common.Configuration;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Diagnostic;
using TinyIoC;
using TinyMessenger;
using apcurium.MK.Booking.Mobile.Extensions;
using apcurium.MK.Booking.Mobile.Messages;

namespace apcurium.MK.Booking.Mobile.ViewModels
{
    public abstract class PageViewModel : BaseViewModel 
    {
        public virtual void OnViewLoaded()
        {
        }

        public virtual void OnViewStarted(bool firstTime)
        {
        }

        public virtual void OnViewStopped()
        {
        }

        public virtual void OnViewUnloaded()
        {
            Subscriptions.Clear();
        }

		protected virtual void Dispose(bool disposing)
		{
			if (disposing) 
			{
				Subscriptions.Dispose();
			}
		}

		
		
		public ICommand CloseCommand
		{
			get
			{
				return this.GetCommand(() => Close(this));
			}
		}

    }

}

