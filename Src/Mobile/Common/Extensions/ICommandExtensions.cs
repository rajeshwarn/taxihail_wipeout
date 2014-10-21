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

		public static void ExecuteIfPossible(this ICommand command, object parameter = null)
		{
			if (command != null && command.CanExecute(parameter))
			{
				command.Execute(parameter);
			}
		}

    }
}

