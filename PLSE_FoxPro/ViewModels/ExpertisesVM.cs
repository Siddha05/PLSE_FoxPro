using Microsoft.Extensions.DependencyInjection;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using PLSE_FoxPro.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace PLSE_FoxPro.ViewModels
{
    public class ExpertiseFindSettings : ObservableObject
    {
        #region Fields
        string _number;
        DateTime? _exec_date_min;
        DateTime? _exec_date_max;
        int? _evaluation;
        string _exp_result;
        string _exp_status;
        Employee _employee;
        Expert _expert;
        #endregion

        #region Properties
        public string Number
        {
            get => _number;
            set => SetProperty(ref _number, value);
        }
        public DateTime? ExecuteDateMin
        {
            get => _exec_date_min;
            set => SetProperty(ref _exec_date_min, value);
        }
        public DateTime? ExecuteDateMax
        {
            get => _exec_date_max;
            set => SetProperty(ref _exec_date_max, value);
        }
        public int? Evaluation
        {
            get => _evaluation;
            set => SetProperty(ref _evaluation, value);
        }
        public string ExpertiseResult
        {
            get => _exp_result;
            set => SetProperty(ref _exp_result, value);
        }
        public Employee Employee
        {
            get => _employee;
            set => SetProperty(ref _employee, value);
        }
        public string ExpertiseStatus
        {
            get => _exp_status;
            set => SetProperty(ref _exp_status, value);
        }
        public Expert Expert
        {
            get => _expert;
            set => SetProperty(ref _expert, value);
        }
        #endregion

        #region Functions
        /// <summary>
        /// Устанавливает ExecuteDateMin и ExecuteDateMax в зависимости от переданного квртала  <paramref name="quarter"/>
        /// и года <paramref name="year"/>
        /// <para>Если <paramref name="year"/> меньше 0 используется текущий год</para>
        /// </summary>
        /// <param name="quarter"></param>
        /// <param name="year"></param>
        public void SetQuarter(int quarter, int year = -1)
        {
            int cur_year = year < 0 ? DateTime.UtcNow.Year : year;
            switch (quarter)
            {
                case 1:
                    ExecuteDateMin = new DateTime(cur_year, 1,1);
                    ExecuteDateMax = new DateTime(cur_year, 3,31);
                    break;
                case 2:
                    ExecuteDateMin = new DateTime(cur_year, 4, 1);
                    ExecuteDateMax = new DateTime(cur_year, 6, 30);
                    break;
                case 3:
                    ExecuteDateMin = new DateTime(cur_year, 7, 1);
                    ExecuteDateMax = new DateTime(cur_year, 9, 30);
                    break;
                case 4:
                    ExecuteDateMin = new DateTime(cur_year, 10, 1);
                    ExecuteDateMax = new DateTime(cur_year, 12, 31);
                    break;
                default:
                    break;
            }
        }
        /// <summary>
        /// Сброс всех настроек к дефолтным
        /// </summary>
        public void Reset()
        {
            foreach (var item in TypeHelper.GetPublicPropertiesWithSetter(typeof(ExpertiseFindSettings)))
            {
                item.SetValue(this, default);
            }
        }
        #endregion
    }

    public class ExpertisesVM : ObservableObject
    {
        #region Fields
        RelayCommand _find;
        RelayCommand<string> _change_status;
        RelayCommand<int> _change_quarter;
        RelayCommand _reset_settings;
        Visibility _exec_visible;
        double _progress;
        ExpertiseFindSettings _settings = new ExpertiseFindSettings();
        #endregion

        #region Properties
        public IEnumerable<string> ExpertiseResults { get; } = new List<String>() { "заключение", "сондз" }; //= App.Services.GetService<ILocalStorage>().ExpertiseResults;
        public IEnumerable<string> ExpertiseStatuses { get; } = new List<String>() { "в работе", "выполнена" };
        public Visibility ExecuteVisibility 
        { 
            get => _exec_visible;
            set => SetProperty(ref _exec_visible, value); 
        }
        public ExpertiseFindSettings Settings => _settings;
        public double Progress
        {
            get => _progress;
            set => SetProperty(ref _progress,value);
        }
           
        #endregion

        #region Commands
        public RelayCommand FindCmd
        {
            get
            {
                return _find ??= new RelayCommand(async () =>
                {
                    for (int i = 0; i < 101; i++)
                    {
                        
                        Progress = i;
                        await Task.Delay(50);
                    }
                    Progress = 0;
                });
            }
        }
        public ICommand ChangeStatusCmd
        {
            get
            {
                return _change_status ??= new RelayCommand<string>(n =>
                {
                    switch (n)
                    {
                        case "выполнена":
                            ExecuteVisibility = Visibility.Visible;
                            break;
                        default:
                            ExecuteVisibility = Visibility.Collapsed;
                            break;
                    }
                });
            }
        }
        public ICommand QuarterChangedCmd => _change_quarter ??= new RelayCommand<int>(n => _settings.SetQuarter(n + 1));
        public ICommand CancelCmd => new RelayCommand(() => App.Services.GetService<IPagesService>().RemovePage());
        public ICommand ResetSettingsCmd
        {
            get
            {
                return _reset_settings ??= new RelayCommand(() => Settings.Reset());
            }
        }
        #endregion
    }
}
