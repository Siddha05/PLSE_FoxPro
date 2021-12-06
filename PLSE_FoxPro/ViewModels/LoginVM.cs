using System;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using System.Reflection;
using PLSE_FoxPro.Models;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;

namespace PLSE_FoxPro.ViewModels
{
    class LoginVM : ObservableObject
    {
        RelayCommand _passchanged;
        RelayCommand _logchanged;
        string _error_msg;
        private string _lang = "Ru";
        
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
        public string Version
        {
            get => Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }
        #endregion
        #region Commands
        public RelayCommand EnterCommand
        {
            get => new RelayCommand(() => MessageBox.Show("Invoke EnterCommand"));
        }
        public RelayCommand ExitCommand
        {
            get => new RelayCommand(() => App.Me.Shutdown());
        }
        public RelayCommand PassChangedCommand => _passchanged;
        public RelayCommand LogChangedCommand => _logchanged;
        public RelayCommand WindowLoadedCommand
        {
            get
            {
                return new RelayCommand(() => MessageBox.Show("Invoke WindowLoadedCommand"));
            }
        }

        //private Task<InitializationStatus> LoadPLSEContexAsync()
        //{
        //    return Task<InitializationStatus>.Run(() =>
        //    {
        //        return App.PLSE_Storage.InitializationStatus;
        //    });
        //}
        #endregion
     
        public LoginVM()
        {
            InputLanguage = InputLanguageManager.Current.CurrentInputLanguage.DisplayName;
            InputLanguageManager.Current.InputLanguageChanged += Current_InputLanguageChanged;
            _passchanged = new RelayCommand(() =>
            {
                MessageBox.Show("Invode PassChangedCommand");
            });
            _logchanged = new RelayCommand(() =>
            {
                MessageBox.Show("Invoke LogChangedCommand");
            });
            //if (Properties.Settings.Default.IsLastLoginSave)
            //{
            //    Login = Properties.Settings.Default.LastLogin;
            //}
        }
        private void Current_InputLanguageChanged(object sender, InputLanguageEventArgs e)
        {
            InputLanguage = InputLanguageManager.Current.CurrentInputLanguage.DisplayName;
        }
    }
}
