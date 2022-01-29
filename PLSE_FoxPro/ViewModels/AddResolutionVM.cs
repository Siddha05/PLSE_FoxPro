using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using PLSE_FoxPro.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using System.Windows.Data;
using System.Windows;
using System.Windows.Controls;
using System.Linq;
using System.Diagnostics;
using Microsoft.Toolkit.Mvvm.Messaging;

namespace PLSE_FoxPro.ViewModels
{
    public class AddResolutionVM : ObservableObject
    {
        #region Fields
        bool _iscustomerpopupopen;
        bool _suppresstextchangedevent;
        RelayCommand<string> _cussearch;
        RelayCommand<Customer> _cusselect;
        RelayCommand _addexpertise;
        RelayCommand<Expertise> _deleteexpertise;
        RelayCommand<string> _resoltype_changed;
        RelayCommand<CaseType> _casetype_changed;
        RelayCommand<NumerableContentWrapper> _del_content;
        RelayCommand<Customer> _open_customer;
        private Visibility _case_num_visible;
        private Visibility _case_plantiff_visible;

        Predicate<object> _empty = n => false;
        Predicate<object> _onlycontract = n => ((CaseType)n).Code.Equals("6", StringComparison.Ordinal);
        Predicate<object> _exeptcontract = n => !((CaseType)n).Code.Equals("6", StringComparison.Ordinal);
        #endregion

        #region Properties
        public Resolution Resolution { get; set; }
        public bool IsCustomerPopupOpen
        {
            get => _iscustomerpopupopen;
            set => SetProperty(ref _iscustomerpopupopen, value);
        }
        public Visibility CaseVisibility
        {
            get => _case_num_visible;
            set => SetProperty(ref _case_num_visible, value);
        }
        public Visibility PlantiffVisiblity
        {
            get => _case_plantiff_visible;
            set => SetProperty(ref _case_plantiff_visible, value);
        }
        public ListCollectionView SuitableCaseTypes { get; } = new ListCollectionView(App.Services.GetService<ILocalStorage>().CaseTypes.ToList());
        public IReadOnlyCollection<string> ResolutionTypes => App.Services.GetService<ILocalStorage>().ResolutionTypes;
        public IReadOnlyCollection<string> ResolutionStatus => App.Services.GetService<ILocalStorage>().ResolutionStatuses;
        public ListCollectionView Customers { get; } = new ListCollectionView(App.Services.GetService<ILocalStorage>().CustomerAccessService.Items().ToList());
        
        #endregion

        #region Commands
        public ICommand CustomerSearchCmd
        {
            get
            {
                return _cussearch ??= new RelayCommand<string>(n =>
                {
                    if (_suppresstextchangedevent)
                    {
                        _suppresstextchangedevent = false;
                        return;
                    }
                    if (n.Length > 1)
                    {
                        Customers.Filter = e => (e as Customer).Sname.StartsWith(n, StringComparison.OrdinalIgnoreCase);
                        IsCustomerPopupOpen = true;
                    }
                    else
                    {
                        IsCustomerPopupOpen = false;
                    }
                });
            }
        }
        public ICommand CustomerSelectCmd
        {
            get
            {
                return _cusselect ??= new RelayCommand<Customer>(n =>
                {
                    Resolution.Customer = n;
                    IsCustomerPopupOpen = false;
                });
            }
        }
        public ICommand OpenCustomerCmd
        {
            get
            {
                return _open_customer ??= new RelayCommand<Customer>(n =>
               {
                   if (!WeakReferenceMessenger.Default.IsRegistered<Customer>(this))
                            WeakReferenceMessenger.Default.Register<Customer>(this, SetAndUnRegister);
                   var w = new Pages.AddEditCustomer();
                   var cnx = new AddEditCustomerVM(n);
                   w.DataContext = cnx;
                   App.Services.GetService<IPagesService>().AddPage(w);
               });
            }
        }
        public ICommand CustomerLostFocusCmd => new RelayCommand<TextBox>(n => n.GetBindingExpression(TextBox.TextProperty).UpdateTarget());
        public ICommand CustomerGotFocusCmd
        {
            get
            {
                return new RelayCommand<TextBox>(n =>
                {
                    _suppresstextchangedevent = true;
                    n.SetCurrentValue(TextBox.TextProperty, "");
                });
            }
        }
        public ICommand ResolutionDateChangedCmd
        {
            get
            {
                return new RelayCommand<DatePicker>(n =>
                {
                    n.GetBindingExpression(DatePicker.SelectedDateProperty).UpdateSource();
                });
            }
        }
        public ICommand SaveResolutionCmd
        {
            get
            {
                return new RelayCommand<Page>(n =>
                {
                    if (App.HasValidState(n))
                    {
                        //try
                        //{
                            App.Services.GetService<ILocalStorage>().ResolutionAccessService.SaveChanges(this.Resolution);
                            App.SendMessage(App.SuccessSave);
                            App.Services.GetService<IPagesService>().RemovePage();
                        //}
                        //catch (Exception ex)
                        //{
                        //    App.Services.GetService<IErrorLogger>().LogError(ex);
                        //    App.SendMessage(App.ErrorOnSave);
                        //}
                    }
                });
            }
        }
        public ICommand AddExpertiseCmd
        {
            get
            {
                return _addexpertise ??= new RelayCommand(() =>
                {
                    if (!WeakReferenceMessenger.Default.IsRegistered<Expertise,int>(this, 1))
                    {
                        WeakReferenceMessenger.Default.Register<Expertise, int>(this, 1, (r, m) => Resolution.Expertisies.Add(m));
                    }
                    var p = new Pages.AddExpertise();
                    p.DataContext = new AddExpertiseVM(Resolution);
                    App.Services.GetService<IPagesService>().AddPage(p);
                });
            }
        }
        public ICommand DeleteExpertiseCmd
        {
            get
            {
                return _deleteexpertise ??= new RelayCommand<Expertise>(n =>
                {
                    if (MessageBox.Show("Удалить выбранную экспертизу?", "", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    {
                        Resolution.Expertisies.Remove(n);
                    }
                });
            }
        }
        public ICommand CancelCmd => new RelayCommand(() => App.Services.GetService<IPagesService>().RemovePage());
        public ICommand ResolutionTypeChangedCmd
        {
            get
            {
                return _resoltype_changed ??= new RelayCommand<string>(n =>
                {
                    switch(n)
                    {
                        case "договор":
                            SuitableCaseTypes.Filter = _onlycontract;
                            break;
                        case null:
                            SuitableCaseTypes.Filter = _empty;
                            break;
                        default:
                            SuitableCaseTypes.Filter = _exeptcontract;
                            break;
                    }
                });
            }
        }
        public ICommand CaseTypeChangedCmd
        {
            get
            {
                return _casetype_changed ??= new RelayCommand<CaseType>(n =>
                {
                    switch (n.Code)
                    {
                        case "6":
                            CaseVisibility = PlantiffVisiblity = Visibility.Collapsed;
                            break;
                        case "1":
                        case "5":
                        case "4":
                            CaseVisibility = Visibility.Visible;
                            PlantiffVisiblity = Visibility.Collapsed;
                            break;
                        default:
                            CaseVisibility = PlantiffVisiblity = Visibility.Visible;
                            break;
                    }
                });
            }
        }
        public ICommand DeleteContentCmd
        {
            get
            {
                return _del_content ??= new RelayCommand<NumerableContentWrapper>(n =>
                {
                    Resolution.Questions.Remove(n);
                    Resolution.Objects.Remove(n);
                });
            }
        }
        #endregion

        public AddResolutionVM()
        {
            Resolution = Resolution.New;
            Resolution.Validate();
        }

        private void SetAndUnRegister(object s, Customer message)
        {
            Resolution.Customer = message;
            WeakReferenceMessenger.Default.Unregister<Customer>(this);
        }
    }
}
