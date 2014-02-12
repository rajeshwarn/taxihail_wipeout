using System;
using Cirrious.MvvmCross.ViewModels;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Input;

namespace apcurium.MK.Booking.Mobile.Extensions
{
	public static class MvxNavigatingObjectExtensions
	{
		public static ICommand GetCommand(this MvxNavigatingObject viewModel, Action execute, Func<bool> canExecute = null, bool async = true)
		{
			return async 
				? (ICommand)new AsyncCommand(WrapInTask(execute), canExecute)
					: (ICommand)new MvxCommand(execute, canExecute);
		}

		public static ICommand GetCommand<T>(this MvxNavigatingObject viewModel, Action<T> execute, Func<T, bool> canExecute = null, bool async = true)
		{   
			return async 
				? (ICommand)new AsyncCommand<T>(WrapInTask(execute), canExecute)
				: (ICommand)new MvxCommand<T>(execute, canExecute);
		}

		public static ICommand GetCommand(this MvxNavigatingObject viewModel, Func<Task> execute, Func<bool> canExecute = null)
		{
			return new AsyncCommand(execute, canExecute);
		}



		public static ICommand GetCommand<T>(this MvxNavigatingObject viewModel, Func<T, Task> execute, Func<T, bool> canExecute = null)
		{
			return new AsyncCommand<T>(execute, canExecute);
		}

		private static Func<T, Task> WrapInTask<T>(Action<T> execute)
		{
			return p =>
			{
				return Task.Factory.StartNew(() => execute(p),
					default(CancellationToken),
					TaskCreationOptions.None,
					GetTaskScheduler());
			};
		}

		private static Func<Task> WrapInTask(Action execute)
		{
			return () =>
			{
				return Task.Factory.StartNew(execute,
					default(CancellationToken),
					TaskCreationOptions.None,
					GetTaskScheduler());
			};
		}

		private static TaskScheduler GetTaskScheduler()
		{
			try
			{
				return TaskScheduler.FromCurrentSynchronizationContext();
			}
			catch (Exception)
			{
				return TaskScheduler.Default;
			}
		}
	}
}

