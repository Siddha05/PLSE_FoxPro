using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using PLSE_FoxPro;

namespace PLSE_FoxPro
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {

        #region Fields

        #endregion

        #region Properties
        public static App Me => (App)Application.Current;
        public ViewModels.MainVM MainViewModel => Me.MainWindow.DataContext as ViewModels.MainVM;
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
