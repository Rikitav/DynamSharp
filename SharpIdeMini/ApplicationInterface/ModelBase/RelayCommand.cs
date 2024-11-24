using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SharpIdeMini.ApplicationInterface.ModelBase
{
    public class RelayCommand : ICommand
    {
        private readonly Action<object?> execute;
        private readonly Func<object?, bool>? canExecute;

        public event EventHandler? CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        public RelayCommand(Action<object?> execute, Func<object?, bool>? canExecute = null)
        {
            this.execute = execute;
            this.canExecute = canExecute;
        }

        public bool CanExecute(object? parameter)
        {
            if (canExecute == null)
                return true;

            return canExecute.Invoke(parameter);
        }

        public void Execute(object? parameter)
        {
            execute(parameter);
        }
    }
}
