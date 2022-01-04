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
        static Message _error_on_page = new Message("Ошибки на форме. Исправьте перед сохранением", MessageType.Error);
        static Message _error_on_save = new Message("Ошибка при сохранении в базу данных", MessageType.Error);
        static Message _error_on_delete = new Message("Ошибка при удалении из базы данных", MessageType.Error);
        static Message _success_save = new Message("Успешно сохранено в базу данны", MessageType.Success);
        static Message _success_delete = new Message("Успешно удалено из базы данных", MessageType.Success);
        #endregion

        #region Properties
        public static App Me => (App)Application.Current;
        public static Message ErrorOnPage => _error_on_page;
        public static Message ErrorOnSave => _error_on_save;
        public static Message ErrorOnDelete => _error_on_delete;
        public static Message SuccessSave => _success_save;
        public static Message SuccessDelete => _success_delete;
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
        /// <summary>
        /// Возвращает последовательность все предков элемента
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="parent"></param>
        /// <returns></returns>
        public static IEnumerable<T> GetDecendants<T>(DependencyObject parent)
           where T : DependencyObject
        {
            foreach (var child in LogicalTreeHelper.GetChildren(parent))
            {
                if (child is T)
                    yield return (T)child;

                if (child is DependencyObject)
                {
                    foreach (var grandChild in GetDecendants<T>((DependencyObject)child))
                        yield return grandChild;
                }
            }
        }
        #endregion

        static App()
        {
            Storage = new Storage_Cached();
            ErrorLogger = new ConsoleErrorLogger();
        }
    }
}
