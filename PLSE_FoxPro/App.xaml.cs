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
using Microsoft.Toolkit.Mvvm.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace PLSE_FoxPro
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {

        #region Fields
        static string _app_name;
        static Message _error_on_page;
        static Message _error_on_save;
        static Message _error_on_delete;
        static Message _success_save;
        static Message _success_delete ;
        #endregion

        #region Properties
        public static App Me => (App)Application.Current;
        public static Message ErrorOnPage => _error_on_page ??= new Message("Ошибки на форме. Исправьте перед сохранением", MessageType.Error);
        public static Message ErrorOnSave => _error_on_save ??= new Message("Ошибка при сохранении в базу данных", MessageType.Error);
        public static Message ErrorOnDelete => _error_on_delete ??= new Message("Ошибка при удалении из базы данных", MessageType.Error);
        public static Message SuccessSave => _success_save ??= new Message("Успешно сохранено в базу данны", MessageType.Success);
        public static Message SuccessDelete => _success_delete ??= new Message("Успешно удалено из базы данных", MessageType.Success);
        public static ViewModels.MainVM MainViewModel => App.Current.MainWindow.DataContext as ViewModels.MainVM;
        public static string AppName => _app_name ??= Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyProductAttribute>().Product;
        public static string AppImagePath => "pack://application:,,,/Resources/FoxLogo.png";
        public static IServiceProvider Services { get; }
        #endregion

        #region Functions
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
        /// <summary>
        /// Определяет имеет ли переданный элемент управления ошибки и выводит предупреждение (в случае наличия) через делетат <paramref name="reporter"/>
        /// <para>Если переданный делегат null, используется реализация по умолчанию MessageBox.Show(App.ErrorOnPage.Content)</para>
        /// </summary>
        /// <param name="obj">Проверяемый элемент управления</param>
        /// <param name="message">Сообщение при наличии ошибки</param>
        /// <param name="reporter">Делегат выводящий сообщение при наличии ошибки</param>
        /// <returns>True если имеет, иначе false</returns>
        public static bool HasValidState(DependencyObject obj, Action<Message> reporter = null, Message message = null)
        {
            var el = App.GetDecendants<DependencyObject>(obj).FirstOrDefault(e => Validation.GetHasError(e));
            if (el == null)
            {
                return true;
            }
            else
            {
                if (reporter != null) reporter(message ?? App.ErrorOnPage);
                else MessageBox.Show(message?.Content ?? App.ErrorOnPage.Content);
                return false;
            }
        }
        public static void ShowErrorWindow(Message message) => MessageBox.Show(message.Content, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);

        private static IServiceProvider ConfigureServices()
        {
            var services = new ServiceCollection();
            services.AddSingleton<ILocalStorage, Storage_Cached>();
            services.AddSingleton<IErrorLogger, ConsoleErrorLogger>();
            services.AddSingleton<IPagesService, PagesService>();
            
            return services.BuildServiceProvider();
        }
        #endregion

        static App()
        {
            Services = ConfigureServices();
        }
    }
}
