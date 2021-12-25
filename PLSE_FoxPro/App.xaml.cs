using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using PLSE_FoxPro.Models;

namespace PLSE_FoxPro
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {

        #region Fields
        string _app_name;
        Employee _log_employee;
        #endregion

        #region Properties
        public static App Me => (App)Application.Current;
        public ViewModels.MainVM MainViewModel => Me.MainWindow.DataContext as ViewModels.MainVM;
        public ILocalStorage Storage { get; }
        public Laboratory Laboratory { get; }
        public Employee LoggedEmployee => _log_employee;
        public string AppName => _app_name == null ? _app_name = Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyProductAttribute>().Product : _app_name;
        public string AppImagePath => "pack://application:,,,/Resources/FoxLogo.png";
        #endregion

        #region Functions
        public void RemovePage() => MainViewModel.RemovePage();
        
        #endregion
        public App()
        {
            Storage = new Storage_Cached();
            Laboratory = Storage.LaboratoryAccessService.GetItemByID(1);
        }
    }
}
