using System;
using System.ComponentModel;
using System.Windows.Input;

namespace TodoListApp
{
    public class BaseViewModel : INotifyPropertyChanged
    {
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }

    public class AppCommand : ICommand
    {
        private Func<object, bool> _canExecute;
        private Action<object> _execute;

        public AppCommand(Func<object, bool> canExecute, Action<object> execute)
        {
            _canExecute = canExecute;
            _execute = execute;
        }

        public bool CanExecute(object parameter)
        {
            return _canExecute == null? true : _canExecute(parameter);
        }

        public event EventHandler CanExecuteChanged;

        public void Execute(object parameter)
        {
            _execute?.Invoke(parameter);
        }
    }
}
