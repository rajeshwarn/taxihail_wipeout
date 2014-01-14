using System;

namespace System.Windows.Input
{
	public static class ICommandExtensions
    {
		public static void Execute(this ICommand command)
		{
			command.Execute(null);
		}

		public static bool CanExecute(this ICommand command)
		{
			return command.CanExecute(null);
		}
    }
}

