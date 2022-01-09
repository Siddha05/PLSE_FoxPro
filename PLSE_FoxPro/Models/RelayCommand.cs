using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Input;

namespace PLSE_FoxPro.Models
{
    public class RelayCommand : ICommand
    {
        Action _execute;
        Func<bool> _canexec;
        
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object parameter) => _canexec == null ? true : _canexec();

        public void Execute(object parameter)
        {
            if (CanExecute(null)) _execute?.Invoke();
        }

        public RelayCommand(Action action, Func<bool> canexec = null)
        {
            _canexec = canexec; _execute = action;
        }
    }

    public class RelayCommand<T> : ICommand
    {
        Action<T> _execute;
        Func<T,bool> _canexec;

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool CanExecute(T param) => _canexec == null || _canexec(param);
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Execute(T param)
        {
            if(CanExecute(param)) _execute(param); 
        }

        bool ICommand.CanExecute(object parameter) => CanExecute((T)parameter);

        void ICommand.Execute(object parameter) => Execute((T)parameter);
        
        public RelayCommand(Action<T> action, Func<T,bool> canexec = null)
        {
            _canexec = canexec; _execute = action;
        }
    }
}
