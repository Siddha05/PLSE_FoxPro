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
        #endregion

        #region Properties
        public static App Me => (App)Application.Current;
        public ViewModels.MainVM MainViewModel => Me.MainWindow.DataContext as ViewModels.MainVM;
        public ILocalStorage Storage { get; }
        public Laboratory Laboratory { get; }
        public Employee LoggedEmployee { get; }
        public string AppName => _app_name == null ? Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyProductAttribute>().Product : _app_name;
        public string AppImagePath => "pack://application:,,,/Resources/FoxLogo.png";
        #endregion

        #region Functions
        public void RemovePage()
        {
            MainViewModel.FrameContent = null;
        }
        
        #endregion
        public App()
        {
            
        }
    }
}
