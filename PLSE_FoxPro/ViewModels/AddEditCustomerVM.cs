using Microsoft.Extensions.DependencyInjection;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using PLSE_FoxPro.Models;
using System;
using System.Collections.Generic;
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
        bool _IsOrganizationPopupOpen;
        bool _is_individual_person;
        bool _is_enable_office = true;
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
                }
                else
                {
                    Customer.Office = null;
                    IsEnableOffice = true;
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
        public ICommand OpenNewOrganizationCmd
        {
            get
            {
                return new RelayCommand(() =>
                {

                    MessageBox.Show("Invoke OpenNewOrganizationCmd");
                });
            }
        }
        public ICommand EditOrganizationCmd
        {
            get
            {
                return new RelayCommand<Organization>(n =>
                {

                    MessageBox.Show("Invoke EditOrganizationCmd");
                });
            }
        }
        #endregion
        public AddEditCustomerVM(Customer customer = null)
        {
            Customer = customer ?? Customer.New;
            Customer.Validate();
        }
        //public AddEditCustomer()
        //{
        //    Customer = Customer.New;
        //}
    }
}
