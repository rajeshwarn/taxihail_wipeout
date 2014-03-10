using System;
using System.Reactive.Disposables;
using System.Runtime.CompilerServices;
using Cirrious.CrossCore;
using Cirrious.MvvmCross.ViewModels;
using MK.Common.Configuration;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Diagnostic;
using TinyIoC;
using apcurium.MK.Booking.Mobile.Extensions;

namespace apcurium.MK.Booking.Mobile.ViewModels
{
	public class ChildViewModel: BaseViewModel, IDisposable
    {
		public void Dispose()
		{
			Dispose(true);
		}

	
    }
}

