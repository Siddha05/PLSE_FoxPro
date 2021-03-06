using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Controls;
using System.Windows.Forms;
using PLSE_FoxPro.Models;
using Microsoft.Toolkit.Mvvm;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using System.Windows;
using System.Windows.Data;
using System.Linq;
using MessageBox = System.Windows.MessageBox;
using System.Diagnostics;
using System.Windows.Input;
using Microsoft.Toolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;

namespace PLSE_FoxPro.ViewModels
{
    internal class ProfileVM : ObservableObject
    {
        #region Fields
        RelayCommand<string> _settlementsearch;
        RelayCommand<Settlement> _settlementselect;
        RelayCommand<System.Windows.Controls.TextBox> _settlostfocus;
        RelayCommand<PasswordBox> _newpasschanged;
        RelayCommand<PasswordBox> _newpassrepeatchanged;
        Visibility _expert_add_vis = Visibility.Collapsed;
        System.Collections.ObjectModel.ObservableCollection<Expert> _expert_specialities = new System.Collections.ObjectModel.ObservableCollection<Expert>();
        ListCollectionView _specialities = new ListCollectionView(App.Services.GetService<ILocalStorage>().SpecialityAccessService.Items().ToList());
        ListCollectionView _settlements = new ListCollectionView(App.Services.GetService<ILocalStorage>().SettlementAccessService.Items().ToList());
        Expert _new_expert;
        PasswordValidator _pass_validator = new PasswordValidator();
        bool _settlements_open;
        #endregion

        #region Properties
        public Employee Employee { get; set; }
        public IReadOnlyCollection<string> EmployeeStatuses { get; } = App.Services.GetService<ILocalStorage>().EmployeeStatus;
        public IReadOnlyCollection<string> InnerOffices { get; } = App.Services.GetService<ILocalStorage>().InnerOffices;
        public ICollection<Departament> Departaments { get; } = App.Services.GetService<ILocalStorage>().DepartamentsAccessService.Items();
        public IReadOnlyCollection<string> StreetTypes { get; } = App.Services.GetService<ILocalStorage>().StreetTypes;
        public ListCollectionView Specialities => _specialities;
        public ListCollectionView Settlements => _settlements;
        public Expert NewExpert
        {
            get => _new_expert;
            set => SetProperty(ref _new_expert, value);
        }     
        public PasswordValidator PasswordValidator
        {
            get => _pass_validator;
            set => SetProperty(ref _pass_validator, value );
        }
        public bool PopupSettlementOpen
        {
            get => _settlements_open;
            set => SetProperty(ref _settlements_open, value);
        }
        public System.Collections.ObjectModel.ObservableCollection<Expert> ExpertSpecilities => _expert_specialities;
        public Visibility ExpertAddVisibility
        {
            get => _expert_add_vis;
            set => SetProperty(ref _expert_add_vis, value );
        }
        #endregion

        #region Commands
        public RelayCommand OpenFotoCmd
        {
            get
            {
                return new RelayCommand(() =>
                {
                    OpenFileDialog openFile = new OpenFileDialog()
                    {
                        DefaultExt = ".jpg",
                        Filter = @"image file|*.jpg",
                        Title = "Выберите изображение"
                    };
                    if (openFile.ShowDialog() == DialogResult.OK)
                    {
                        FileInfo fileInfo = new FileInfo(openFile.FileName);
                        FileStream fs = new FileStream(openFile.FileName, FileMode.Open, FileAccess.Read);
                        BinaryReader br = new BinaryReader(fs);
                        Employee.Foto = br.ReadBytes((int)fileInfo.Length);
                    }
                });
            }
        }
        public RelayCommand<FrameworkElement> SaveCmd
        {
            get
            {
                return new RelayCommand<FrameworkElement>(n =>
                {
                    if (App.HasValidState(n))
                    {
                        try
                        {
                            foreach (var expert in ExpertSpecilities)
                            {
                                App.Services.GetService<ILocalStorage>().ExpertAccessService.SaveChanges(expert);
                            }
                        }
                        catch (Exception ex)
                        {
                            App.SendMessage(App.ErrorOnSave);
                            App.Services.GetService<IErrorLogger>().LogError(ex);
                            return;
                        }
                        App.SendMessage(App.SuccessSave);
                        App.Services.GetService<IPagesService>().RemovePage();
                    }
                });
            }
        }
        public RelayCommand CancelCmd => new RelayCommand(() => App.Services.GetService<IPagesService>().RemovePage()); 
        public ICommand AddNewSettlementCmd
        {
            get
            {
                return new RelayCommand(() =>
                {
                    var p = new Pages.AddEditSettlement();
                    var cnx = new AddEditSettlementVM();
                    p.DataContext = cnx;
                    App.Services.GetService<IPagesService>().AddPage(p);
                });
            }
        }
        public RelayCommand<string> SettlementSearchCmd
        {
            get
            {
                return _settlementsearch != null ? _settlementsearch : _settlementsearch = new RelayCommand<string>(n =>
                {
                    
                    if (n.Length > 1)
                    {
                        PopupSettlementOpen = true;
                        Settlements.Filter = x => (x as Settlement).Title.StartsWith(n, StringComparison.CurrentCultureIgnoreCase);
                    }
                    else
                    {
                        PopupSettlementOpen = false;
                        Settlements.Filter = null;
                    }
                });
            }
        }
        public RelayCommand<Settlement> SettlementSelectCmd
        {
            get
            {
                return _settlementselect != null ? _settlementselect : _settlementselect = new RelayCommand<Settlement>(n => SetSettlement(n));
            }
        }
        public ICommand ExpertDeleteCmd
        {
            get
            {
                return new RelayCommand<Expert>(n =>
                {
                    Debug.Print($"selected expert is {n.Speciality.Code}");
                    if (MessageBox.Show($"Удалить выбранную специальность?", "", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    {
                        try
                        {
                            App.Services.GetService<ILocalStorage>().ExpertAccessService.Delete(n);
                            ExpertSpecilities.Remove(n);
                            App.SendMessage("Экспертная специальность удалена");
                        }
                        catch (Exception ex)
                        {
                            App.Services.GetService<IErrorLogger>().LogError(ex);
                            App.SendMessage("Удаление невозможно. В базе данных есть связанные записи");
                        }
                    }
                },
                    n => n != null
                );
            }
        }
        public RelayCommand ExpertAddCmd
        {
            get
            {
                return new RelayCommand(() =>
                {
                    NewExpert = Expert.New;
                    NewExpert.Validate();
                    IEnumerable<int> presentID = ExpertSpecilities.Select(i => i.Speciality.ID);
                    Specialities.Filter = c => !presentID.Contains((c as Speciality).ID);
                    ExpertAddVisibility = Visibility.Visible;
                });
            }
        }
        public RelayCommand ExpertAddSaveCmd
        {
            get
            {
                return new RelayCommand(() =>
                {
                    NewExpert.Employee = Employee;
                    ExpertSpecilities.Add(NewExpert);
                    ExpertAddVisibility = Visibility.Collapsed;
                });
            }
        }
        public RelayCommand ExpertAddCancelCmd
        {
            get
            {
                return new RelayCommand(() =>
                {
                    //var expert = n as Expert;
                    //AddEditExpertVM cnx;
                    //var w = new Windows.AddEditExpert();
                    //if (expert != null) cnx = new AddEditExpertVM(expert);
                    //else cnx = new AddEditExpertVM(App.GetLoginEmployee());
                    //w.DataContext = cnx;
                    //if (w.ShowDialog() ?? false)
                    //{
                    //    var exp = cnx.Expert;
                    //    try
                    //    {
                    //        App.PLSE_Storage.ExpertAccessService.SaveChanges(exp);
                    //        App.SendMessage(App.SuccessDBSaveMessage, MsgType.Normal);
                    //    }
                    //    catch (Exception)
                    //    {
                    //        App.SendMessage(App.ErrorOnDBSaveMessage, MsgType.Error);
                    //    }
                    //}
                    ExpertAddVisibility = Visibility.Collapsed;
                });
            }
        }
        public ICommand SavePasswordCmd
        {
            get
            {
                return new RelayCommand<Grid>(async n =>
                {
                    PasswordBox pbnew = null, pbold = null, pbnewr = null;
                    foreach (var item in App.GetDecendants<System.Windows.Controls.Control>(n))
                    {
                        if (item.Name == "pbOldPass") pbold = (PasswordBox)item;
                        if (item.Name == "pbNewPass") pbnew = (PasswordBox)item;
                        if (item.Name == "pbNewPassRepeat") pbnewr = (PasswordBox)item;
                    }
                    if (Employee.Password.Equals(pbold.Password, StringComparison.CurrentCulture))
                    {
                        try
                        {
                            await App.Services.GetService<ILocalStorage>().EmployeeAccessService.SavePasswordAsync(Employee, PasswordValidator.Password);
                            App.SendMessage("Пароль успешно изменен");
                            pbnew.Password = pbnewr.Password = pbold.Password = "";
                        }
                        catch (Exception)
                        {
                            App.SendMessage(App.ErrorOnSave);
                        }
                    }
                    else App.SendMessage("Некорректный пароль пользователя");
                },
                    _ => PasswordValidator.IsValid()
                );
            }
        }
        public RelayCommand<PasswordBox> NewPasswordChangedCmd
        {
            get
            {
                return _newpasschanged != null ? _newpasschanged : _newpasschanged = new RelayCommand<PasswordBox>(n =>
                {
                    PasswordValidator.Password = n.Password;
                });
            }
        }
        public RelayCommand<PasswordBox> NewPasswordRepeatChangedCmd
        {
            get
            {
                return _newpassrepeatchanged != null ? _newpassrepeatchanged : _newpassrepeatchanged = new RelayCommand<PasswordBox>(n =>
                {
                    PasswordValidator.PasswordRepeat = n.Password;
                });
            }
        }
        public RelayCommand<System.Windows.Controls.TextBox> SettlementLostFocusCmd
        {
            get
            {
                return _settlostfocus != null ? _settlostfocus : _settlostfocus = new RelayCommand<System.Windows.Controls.TextBox>(n =>
                {
                    n.GetBindingExpression(System.Windows.Controls.TextBox.TextProperty).UpdateTarget();
                });
            }
        }
        #endregion

        #region Functions
        private void SetSettlement(Settlement settlement)
        {
            Employee.Employee_SlightPart.Adress.Settlement = settlement;
            PopupSettlementOpen = false;
        }
        #endregion

        public ProfileVM(Employee employee = null)
        {
            if (employee != null)
            {
                Employee = employee.Clone();
                LoadExpertAsync();
            }
            else Employee = Employee.New;
            WeakReferenceMessenger.Default.Register<Settlement>(this, (r, m) => SetSettlement(m));
        }
        private async void LoadExpertAsync()
        {
            var experts = await App.Services.GetService<ILocalStorage>().ExpertAccessService.LoadExpertSpecialitiesAsync(Employee).ConfigureAwait(true);
            foreach (var expert in experts)
            {
                ExpertSpecilities.Add(expert.Clone());
            }
        }
    }
}
