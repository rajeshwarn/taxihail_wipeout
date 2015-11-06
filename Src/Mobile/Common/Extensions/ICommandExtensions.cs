using System.Runtime.CompilerServices;
using apcurium.MK.Booking.Mobile;
using apcurium.MK.Common.Diagnostic;
using Cirrious.CrossCore;
using Cirrious.MvvmCross.ViewModels;

namespace System.Windows.Input
{
	public static class ICommandExtensions
    {

		public static void ExecuteIfPossible(this ICommand command, object parameter = null)
		{
			if (command != null && command.CanExecute(parameter))
			{
				command.Execute(parameter);
			}
		}

		/// <summary>
		/// This method will attempt to raise CanExecuteChanged on the command if the command type supports it.
		/// </summary>
		/// <param name="command">The command to attempt CanRaiseExecuteChanged</param>
		/// <param name="memberName">Calling method</param>
		public static void RaiseCanExecuteChangedIfPossible(this ICommand command, [CallerMemberName] string memberName = "UnknownCaller")
		{
			var asyncCommand = command as AsyncCommandBase;
			if (asyncCommand != null)
			{
				asyncCommand.RaiseCanExecuteChanged();
				return;
			}

			var mvxCommand = command as MvxCommandBase;
			if (mvxCommand != null)
			{
				mvxCommand.RaiseCanExecuteChanged();

				return;
			}

			Mvx.Resolve<ILogger>().LogMessage("Unable to execute RaiseCanExecuteChanged in {0}", memberName);
		}

    }
}

