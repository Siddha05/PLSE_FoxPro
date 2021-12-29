using System;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using System.Reflection;
using PLSE_FoxPro.Models;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using System.Threading.Tasks;
using System.Linq;

namespace PLSE_FoxPro.ViewModels
{
    class LoginVM : ObservableObject
    {
        #region Fields
        RelayCommand<PasswordBox> _passchanged;
        RelayCommand _logchanged;
        string _error_msg;
        string _lang = "Ru";
        InitializationStatus _status;
        #endregion

        #region Properties
        public string Login { get; set; }
        public string Password { get; set; }
        public string ErrorMessage
        {
            get => _error_msg;
            set => SetProperty(ref _error_msg, value);
        }
        public string InputLanguage
        {
            get => _lang;
            set => SetProperty(ref _lang, value);
        }
        public string Version => Assembly.GetExecutingAssembly().GetName().Version.ToString();
        public InitializationStatus StorageInitStatus
        {
            get => _status;
            set => SetProperty(ref _status, value);
        }
        #endregion

        #region Commands
        public RelayCommand<Window> EnterCommand
        {
            get => new RelayCommand<Window>(n =>
                {
                    if (StorageInitStatus != InitializationStatus.Succsess)
                    {
                        ErrorMessage = "отсутствует соединение с БД";
                        return;
                    }
                    else 
                    {
                        var e = App.Storage.EmployeeAccessService.Items().FirstOrDefault(n => n.Password == Password && n.Sname == Login);
                        if (e != null)
                        {
                            var wnd = new MainWindow();
                            App.MainViewModel.LoginEmployee = e;
                            wnd.Show();
                            (n as Window).Close();
                            if (Properties.Settings.Default.IsLastLoginSave)
                            {
                                Properties.Settings.Default.LastLogin = Login;
                                Properties.Settings.Default.Save();
                            }
                        }
                        else
                        {
                            ErrorMessage = "неверные имя пользователя или пароль";
                        }
                    }
                });
        }
        public RelayCommand ExitCommand => new RelayCommand(() => App.Me.Shutdown());
        public RelayCommand<PasswordBox> PassChangedCommand => _passchanged;
        public RelayCommand LogChangedCommand => _logchanged;
        public RelayCommand WindowLoadedCommand => new RelayCommand(async () => await LoadPLSEContexAsync());

      
        #endregion

        public LoginVM()
        {
            InputLanguage = InputLanguageManager.Current.CurrentInputLanguage.DisplayName;
            InputLanguageManager.Current.InputLanguageChanged += Current_InputLanguageChanged;
            _passchanged = new RelayCommand<PasswordBox>(n =>
            {
                Password = n.Password;
                ErrorMessage = "";
            });
            _logchanged = new RelayCommand(() => ErrorMessage = "");
            if (Properties.Settings.Default.IsLastLoginSave)
            {
                Login = Properties.Settings.Default.LastLogin;
            }
        }
        private void Current_InputLanguageChanged(object sender, InputLanguageEventArgs e)
        {
            InputLanguage = InputLanguageManager.Current.CurrentInputLanguage.DisplayName;
        }
        private void StorageInitStatusChanged(object sender, InitializationStatus status)
        {
            StorageInitStatus = status;
        }
        private Task LoadPLSEContexAsync()
        {
            return Task<InitializationStatus>.Run(() =>
            {
                App.Storage.StatusChanged += StorageInitStatusChanged;
                App.Storage.Inicialize();
            });
        }
    }
}
