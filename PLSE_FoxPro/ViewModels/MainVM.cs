using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.Messaging;
using Microsoft.Toolkit.Mvvm.Messaging.Messages;
using PLSE_FoxPro.Models;

namespace PLSE_FoxPro.ViewModels
{
    public class MainVM : ObservableObject
    {

        #region Fields
        private DateTime _curent_date;
        private string _status_messge;
        private Employee _logged_employee;
        private Laboratory _laboratory;
        private Visibility _frame_visibility = Visibility.Hidden;
        private DispatcherTimer timer;
        private Progress<Event> informer;
        private RelayCommand _openprofile;
        private RelayCommand _openspeciality;
        private RelayCommand _openemployees;
        private RelayCommand _openabout;
        private RelayCommand _openaddresol;
        private RelayCommand _openexpertise;
        private RelayCommand _opensettings;
        private RelayCommand<Event> _eventclose;
        private RelayCommand _home;
        private Stack<Page> _page_stack = new Stack<Page>(5);
        private Page _current_page;
        private MessageStackQuery _messages = new MessageStackQuery();      
        #endregion

        #region Properties
        public DateTime Date
        {
            get => _curent_date;
            set => SetProperty(ref _curent_date, value);
        }
        public Laboratory Laboratory 
        {
            get => _laboratory;
            set => SetProperty(ref _laboratory, value); 
        }
        public Employee LoginEmployee
        {
            get => _logged_employee;
            set => SetProperty(ref _logged_employee, value);
        }
        public Page CurrentPage
        {
            get => _current_page;
            private set
            {
                SetProperty(ref _current_page, value);
                if (value != null) FrameVisibility = Visibility.Visible;
                else FrameVisibility = Visibility.Hidden;
            }
        }
        public string StatusMessage
        {
            get => _status_messge;
            set => SetProperty(ref _status_messge, value);
        }
        public Visibility FrameVisibility
        {
            get => _frame_visibility;
            set => SetProperty(ref _frame_visibility, value);
        }
        public ObservableCollection<Event> EventsList { get; } = new ObservableCollection<Event>();
        public string Aphorism
        {
            get
            {
                var list = GetAphorizmList();
                Random rnd = new Random();
                return list[rnd.Next(list.Length)];
            }
        }
        public MessageStackQuery StackMessages => _messages;
        #endregion Properties

        #region Commands
        public RelayCommand<Window> Exit => new RelayCommand<Window>(w => w.Close());
        public RelayCommand OpenSpeciality => new RelayCommand(() => MessageBox.Show("Invoke OpenSpeciality")); 
        public RelayCommand OpenResolutionAdd => new RelayCommand(() => MessageBox.Show("Invoke OpenResolutionAdd"));
        public RelayCommand OpenEmployees => new RelayCommand(() => MessageBox.Show("Invoke OpenEmployee"));
        public RelayCommand OpenProfile => new RelayCommand(() => LoginEmployee = App.Storage.EmployeeAccessService.GetItemByID(4));
        public RelayCommand OpenExpertises => new RelayCommand(() => MessageBox.Show("Invoke OpenExpertise"));
        public RelayCommand WindowLoaded { get; }
        public RelayCommand OpenAboutPLSE => new RelayCommand(() => MessageBox.Show("Invoke OpenAbout"));
        public RelayCommand OpenSettings => new RelayCommand(() => AddPage(new Pages.Settings()));
        public RelayCommand Home => new RelayCommand(() => MessageBox.Show("Invoke Home"));
        public RelayCommand<Event> EventClose
        {
            get
            {
                return _eventclose != null ? _eventclose : _eventclose = new RelayCommand<Event>(n =>
                {
                    var ev = n as Event;
                    EventsList.Remove(ev);
                });
            }
        }
        public RelayCommand ExpertiseSummaryCmd => new RelayCommand(() => MessageBox.Show("Invoke ExpertiseSummaryCmd"));
        #endregion Commands

        public MainVM()
        {
            informer = new Progress<Event>(n => EventsList.Add(n));
            timer = new DispatcherTimer()
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            WindowLoaded = new RelayCommand(() =>
            {
                Greeting();
                UploadEmployee();
                if (Properties.Settings.Default.IsExpertiseScan)
                {
                    ScanExpertises();
                }
                if (Properties.Settings.Default.IsExpertSpecialityScan)
                {
                    ScanExpiredSpeciality();
                }
                if (Properties.Settings.Default.IsShowNotification)
                {
                    ScanAnnualDate();
                }
            });
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        #region Functions
        public void AddPage(Page page)
        {
            _page_stack.Push(page);
            CurrentPage = page;
        }
        public void RemovePage()
        {
            _page_stack.Pop();
            if (_page_stack.Count > 0) CurrentPage = _page_stack.Peek();
            else CurrentPage = null;
        }
        public void AddStackMessage(Message message) => StackMessages.Add(message);
        private void Timer_Tick(object sender, EventArgs e) => Date = DateTime.UtcNow;
        public void Greeting()
        {
            string s = DateTime.UtcNow switch
            {
                { Hour: var h } when h >= 4 && h < 11 => "Доброе утро",
                { Hour: var h } when h >= 11 && h < 16 => "День добрый",
                { Hour: var h } when h >= 16 && h < 21 => "Добрый вечер",
                _ => "Доброй ночи"
            };
            EventsList.Add(new Message($"{s}, {LoginEmployee.Fname} {LoginEmployee.Mname}"));
        }
        private string[] GetAphorizmList()
        {
            return new string[]
            {
                "Начинающий видит много возможностей, эксперт — лишь несколько.",
                "Авторитетность экспертного заключения обратно пропорциональна числу утверждений, понятных широкой публике.",
                "Эксперт излагает объективную точку зрения. А именно свою собственную.",
                "Эксперт – это человек, который совершил все возможные ошибки в очень узкой специальности.",
                "Эксперт даст все нужные вам ответы, если получит нужные ему вопросы.",
                "Сделайте три верные догадки подряд – и репутация эксперта вам обеспечена.",
                "Если нужно выбрать среди экспертов одного настоящего, выбирай того, кто обещает наибольший срок завершения проекта и его наибольшую стоимость.",
                "Если консультироваться с достаточно большим числом экспертов, можно подтвердить любую теорию.",
                "Начальник не всегда прав, но он всегда начальник.",
                "Не спеши выполнять приказ — его могут отменить.",
                "От трудной работы еще никто не умирал. Но зачем испытывать судьбу?",
                "Чем больше знаешь, тем больше знаешь лишнего.",
                "Вывод — то место в тексте, где вы устали думать.",
                "Если хотите остаться при своем мнении, держите его при себе.",
                "Узы брака тяжелы. Поэтому нести их приходится вдвоем. А иногда даже втроем.",
                "Если Вас уже третий рабочий день подряд клонит в сон, значит сегодня среда.",
                "Ни одно доброе дело не должно оставаться безнаказанным.",
                "Недостаточно иметь мозги, надо их иметь достаточно, чтобы воздержаться от того, чтобы иметь их слишком много.",
                "Наличие мозгов — дополнительная нагрузка на позвоночник.",
                "Работа в команде очень важна. Она позволяет свалить вину на другого.",
                "Незаменимые бывают только аминокислоты.",
                "Брак - единственная война, во время которой вы спите с врагом.",
                "Любовь — это болезнь, требующая постельного режима.",
                "Эксперт - это человек, который сделал больше ошибок, чем вы.",
                "Ничто так не деморализует, как скромный, но постоянный доход.",
                "Брак — основная причина разводов.",
                "За одним зайцем погонишься - двух не поймаешь",
                "Настоящий специалист — это человек, который не допускает мелких ошибок, а выдает грандиозные ляпы.",
                "Все, что хорошо начинается, заканчивается плохо. Все, что плохо начинается, заканчивается еще хуже.",
                "Бисексуальность удваивает ваши шансы найти себе пару в субботу вечером.",
                "Теория — это когда все известно, но ничего не работает. Практика — это когда все работает, но никто не знает почему. Мы же объединяем теорию и практику: ничего не работает... и никто не знает почему!"
            };
        }
        #endregion



        public async void ScanAnnualDate()
        {
            var l = await App.Storage.EmployeeAccessService.LoadAnnualDatesAsync(DateTime.UtcNow);
            foreach (var item in l)
            {
                EventsList.Add(new Message(item + " празднует день рождения !!!", MessageType.Congratulation));//TODO: make FIO
            }
        }
        public async void ScanExpiredSpeciality()
        {
            try
            {
                var l = await GetExpiredExperts(LoginEmployee);
                if (l.Count > 0)
                {
                    foreach (var item in l)
                    {
                        EventsList.Add(new Message($"Аттестация по специальности {item.Speciality.FullTitle} просрочена!", MessageType.Warning));
                    }
                }
            }
            catch (Exception)
            {
                EventsList.Add(new Message("Ошибка при загрузке экспертных специальностей"));
            }

        }
        public async void ScanExpertises()
        {
            try
            {
                var res = await App.Storage.ExpertiseAccessService.LoadExpertiseInWorkAsync(LoginEmployee);
                EventsList.Add(new ExpertisesInWorkOverview(res.Item1));
            }
            catch (Exception)
            {
                EventsList.Add(new Message("Ошибка при загрузке экспертиз"));
            }
        }
        private Task<List<Expert>> GetExpiredExperts(Employee employee)
        {
            return Task.Run(async () =>
            {
                List<Expert> expired = new List<Expert>();
                var l = await App.Storage.ExpertAccessService.LoadExpertSpecialitiesAsync(LoginEmployee);
                foreach (var item in l)
                {
                    if (!item.IsValidAttestation)
                    {
                        expired.Add(item);
                    }
                }
                return expired;
            });
        }    
        public async void UploadEmployee()
        {
            if (LoginEmployee.UploadStatus == UploadResult.UnPerform || LoginEmployee.UploadStatus == UploadResult.Error)
            {
                LoginEmployee.Employee_SlightPart = await App.Storage.EmployeeAccessService.LoadSlightPartAsync(LoginEmployee);
            }
        }
        
    }
}
