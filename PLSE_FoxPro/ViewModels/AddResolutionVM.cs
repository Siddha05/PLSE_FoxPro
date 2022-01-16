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
        RelayCommand _newcustomeropen;
        RelayCommand _editcustomer;
        RelayCommand _addexpertise;
        RelayCommand<Expertise> _deleteexpertise;
        RelayCommand _resoltype_changed;
        Predicate<object> _empty = n => false;
        Predicate<object> _onlycontract = n => (n as string) == "исследование";
        Predicate<object> _exeptcontract = n => (n as string) != "исследование";
        #endregion

        #region Properties
        public Resolution Resolution { get; set; }
        public bool IsCustomerPopupOpen
        {
            get => _iscustomerpopupopen;
            set => SetProperty(ref _iscustomerpopupopen, value);
        }

        //public IReadOnlyDictionary<ResolutionTypes, string> ResolutionTypes => App.PLSE_Storage.ResolutionTypesMap;
        public IReadOnlyCollection<string> ResolutionStatus => App.Services.GetService<ILocalStorage>().ResolutionStatuses;
        public IReadOnlyCollection<CaseType> CaseTypes => App.Services.GetService<ILocalStorage>().CaseTypes;
        public ListCollectionView Customers { get; } //= new ListCollectionView(App.Services.GetService<ILocalStorage>().CustomerAccessService.Items().ToList());
        
        public List<Expertise> FakeExpertise { get; } //= new List<Expertise> { DesignData.TestInstance.Expertise1, DesignData.TestInstance.Expertise2 };
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
                    string text = n as string;
                    if (text.Length > 1)
                    {
                        //Customers.Filter = e => (e as Customer).Sname.StartsWith(text, StringComparison.OrdinalIgnoreCase);
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
        public ICommand OpenNewCustomerCmd
        {
            get
            {
                return _newcustomeropen ??= new RelayCommand(() =>
                {
                    //var w = new Pages.AddEditCustomer();
                    //var cnx = new ViewModels.AddEditCustomer(target: Resolution);
                    //w.DataContext = cnx;
                    //App.AddPage(w);

                    MessageBox.Show("Invoke OpenCustomerCmd");
                });
            }
        }
        public ICommand EditCustomerCmd
        {
            get
            {
                return _editcustomer ??= new RelayCommand(() =>
                {
                    //var w = new Pages.AddEditCustomer();
                    //var cnx = new ViewModels.AddEditCustomer(n as Customer, Resolution);
                    //w.DataContext = cnx;
                    //App.AddPage(w);

                    MessageBox.Show("Invoke EditCustomerCmd");
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
                        try
                        {
                            App.Services.GetService<ILocalStorage>().ResolutionAccessService.SaveChanges(this.Resolution);
                            App.SendMessage(App.SuccessSave);
                            App.Services.GetService<IPagesService>().RemovePage();
                        }
                        catch (Exception ex)
                        {
                            App.Services.GetService<IErrorLogger>().LogError(ex);
                            App.SendMessage(App.ErrorOnSave);
                        }
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
        public RelayCommand ResolutionTypeChangedCmd
        {
            get
            {
                return _resoltype_changed;
                //    != null ? _resoltype_changed : _resoltype_changed = new RelayCommand(n =>
                //{
                //    var sel = (KeyValuePair<ResolutionTypes, string>)n;
                //    switch (sel.Key)
                //    {
                //        case Model.ResolutionTypes.Resolution:
                //        case Model.ResolutionTypes.Definition:
                //        case Model.ResolutionTypes.Relationship:
                //            CaseTypes.Filter = _exeptcontract;
                //            break;
                //        case Model.ResolutionTypes.Contract:
                //            CaseTypes.Filter = _onlycontract;
                //            break;
                //        default:
                //            CaseTypes.Filter = _empty;
                //            break;
                //    }
                //});
            }
        }
        #endregion

        public AddResolutionVM()
        {
            Resolution = Resolution.New;
            Resolution.Validate();
        }
    }
}
