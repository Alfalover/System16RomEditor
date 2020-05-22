using System;
using System.Windows.Input;

namespace S16Editor.ViewModel
{
    public class DelegateCommand : ICommand
    {
        private readonly Action _action;
        private readonly Func<bool> canExecute;

        public DelegateCommand(Action action, Func<bool> canExecute)
        {
            _action = action;
            this.canExecute = canExecute;
        }

        public void Execute(object parameter)
        {
            _action();
        }

        public bool CanExecute(object parameter)
        {
            return canExecute();
        }

        public void Check()
        {
            CanExecuteChanged?.Invoke(this, null);
        }
#pragma warning disable 67
        public event EventHandler CanExecuteChanged;
#pragma warning restore 67
    }
}
