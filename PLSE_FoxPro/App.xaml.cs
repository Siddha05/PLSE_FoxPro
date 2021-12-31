using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using PLSE_FoxPro.Models;

namespace PLSE_FoxPro
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {

        #region Fields
        static string _app_name;
        #endregion

        #region Properties
        public static App Me => (App)Application.Current;
        public static ViewModels.MainVM MainViewModel => App.Current.MainWindow.DataContext as ViewModels.MainVM;
        public static ILocalStorage Storage { get; }
        public static IErrorLogger ErrorLogger { get; }
        public static string AppName => _app_name ??= Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyProductAttribute>().Product;
        public static string AppImagePath => "pack://application:,,,/Resources/FoxLogo.png";
        #endregion

        #region Functions
        public static void RemovePage() => MainViewModel.RemovePage();
        public static void AddPage(Page p) => MainViewModel.AddPage(p);
        public static void SendMessage(Message message) => MainViewModel.AddStackMessage(message);
        #endregion

        static App()
        {
            Storage = new Storage_Cached();
            ErrorLogger = new ConsoleErrorLogger();
        }
    }
}
