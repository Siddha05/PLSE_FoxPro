using Microsoft.Extensions.DependencyInjection;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Messaging;
using PLSE_FoxPro.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace PLSE_FoxPro.ViewModels
{
    public class AddEditCustomerVM : ObservableObject
    {
        #region Fields
        RelayCommand<string> _orgsearch;
        RelayCommand<Organization> _open_org;
        bool _IsOrganizationPopupOpen;
        bool _is_individual_person;
        bool _is_enable_office = true;
        Visibility _org_visible;
        #endregion

        #region Properties
        public Customer Customer { get; set; }
        public bool IsOrganizationPopupOpen
        {
            get => _IsOrganizationPopupOpen;
            set => SetProperty(ref _IsOrganizationPopupOpen, value);
        }
        public ListCollectionView Organizations { get; } = new ListCollectionView(App.Services.GetService<ILocalStorage>().OrganizationAccessService.Items().ToList());
        public IReadOnlyCollection<string> Ranks { get; } = App.Services.GetService<ILocalStorage>().Ranks;
        public IReadOnlyCollection<string> Genders => App.Services.GetService<ILocalStorage>().Genders;
        public bool IsIndividualPerson
        {
            get => _is_individual_person;
            set
            {
                _is_individual_person = value;
                if (_is_individual_person)
                {
                    Customer.Office = "гражданин";
                    Customer.Rank = null;
                    IsEnableOffice = false;
                    OrganizationVisible = Visibility.Collapsed;
                }
                else
                {
                    Customer.Office = null;
                    IsEnableOffice = true;
                    OrganizationVisible = Visibility.Visible;
                }
                OnPropertyChanged();
            }
        }
        public bool IsEnableOffice
        {
            get => _is_enable_office;
            set
            {
                _is_enable_office = value;
                OnPropertyChanged();
            }
        }
        public Visibility OrganizationVisible
        {
            get => _org_visible;
            set => SetProperty(ref _org_visible, value);
        }
        #endregion


        #region Commands
        public ICommand CancelCmd => new RelayCommand(() => App.Services.GetService<IPagesService>().RemovePage());
        public ICommand SaveCmd
        {
            get
            {
                return new RelayCommand<Page>(n =>
                {
                    if (App.HasValidState(n))
                    {
                        WeakReferenceMessenger.Default.Send(Customer);
                        App.Services.GetService<IPagesService>().RemovePage();
                    }
                });
            }
        }
        public ICommand OrganizationSearchCmd
        {
            get
            {
                return _orgsearch ??= new RelayCommand<string>(n =>
                {
                    if (n.Length > 2)
                    {
                        IsOrganizationPopupOpen = true;
                        Organizations.Filter = x => (x as Organization).Name.ContainWithComparison(n, StringComparison.OrdinalIgnoreCase);
                    }
                    else
                    {
                        IsOrganizationPopupOpen = false;
                        Organizations.Filter = null;
                    }
                });
            }
        }
        public ICommand OrganizationFocusLostCmd
        {
            get
            {
                return new RelayCommand<TextBox>(n =>
                {
                    n.GetBindingExpression(TextBox.TextProperty).UpdateTarget();
                });
            }
        }
        public ICommand OrganizationSelectCmd
        {
            get
            {
                return new RelayCommand<Organization>(n =>
                {
                    Customer.Organization = n;
                    IsOrganizationPopupOpen = false;
                });
            }
        }
        public ICommand OrganizationOpenCmd
        {
            get
            {
                return _open_org ??= new RelayCommand<Organization>(n =>
                {
                    if (!WeakReferenceMessenger.Default.IsRegistered<Organization>(this))
                        WeakReferenceMessenger.Default.Register<Organization>(this, (s, m) => SetAndUnregister(s, m));
                    var p = new Pages.AddEditOrganization();
                    p.DataContext = new AddEditOrganizationVM(n);
                    App.Services.GetService<IPagesService>().AddPage(p);
                });
            }
        }
        #endregion

        public AddEditCustomerVM(Customer customer = null)
        {
            Customer = customer?.Clone() ?? Customer.New;
            Customer.Validate();
            (Customer as Person).Validate();
        }
        private void SetAndUnregister(object sender, Organization message)
        {
            Customer.Organization = message;
            WeakReferenceMessenger.Default.Unregister<Organization>(this);
        }
    }
}
