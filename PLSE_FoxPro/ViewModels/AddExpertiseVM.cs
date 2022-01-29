using Microsoft.Extensions.DependencyInjection;
using Microsoft.Toolkit.Mvvm.Messaging;
using PLSE_FoxPro.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace PLSE_FoxPro.ViewModels
{
    public class AddExpertiseVM
    {
        #region Fields
        RelayCommand<Expert> _expertchanged;
        RelayCommand _addbill;
        RelayCommand<Bill> _deletebill;
        ICollection<Expert> _experts;
        #endregion

        #region Properties
        public Expertise Expertise { get; }
        public IReadOnlyCollection<string> ExpertiseTypes { get; } = App.Services.GetService<ILocalStorage>().ExpertiseTypes;
        public IEnumerable<Expert> Experts { get; }
        public ObservableCollection<Expert> ExpertSpecialities { get; } = new ObservableCollection<Expert>();
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
                        WeakReferenceMessenger.Default.Send(Expertise, 1);
                        App.Services.GetService<IPagesService>().RemovePage();
                    }
                });
            }
        }
        public ICommand ExpertChangedCmd
        {
            get
            {
                return _expertchanged ??= new RelayCommand<Expert>(n =>
                {
                    FillExpertSpecilities(n); 
                });
            }
        }
        public ICommand AttachExpertiseCmd
        {
            get
            {
                return new RelayCommand(() => MessageBox.Show("AttachExpertise invoke"));
            }
        }
        public ICommand DeleteBillCmd
        {
            get
            {
                return _deletebill ??= new RelayCommand<Bill>(n =>
                {
                    Expertise.Bills.Remove(n);
                },
                    _ => Expertise.Bills.Count > 0
                );
            }
        }
        public ICommand AttachBillCmd
        {
            get
            {
                return _addbill ??= new RelayCommand(() =>
                {
                    if (!WeakReferenceMessenger.Default.IsRegistered<Bill, int>(this, 1))
                    {
                        WeakReferenceMessenger.Default.Register<Bill, int>(this, 1, (r, m) => Expertise.Bills.Add(m));
                    }
                    var p = new Pages.AddEditBill();
                    var cnt = new AddEditBillVM();
                    p.DataContext = cnt;
                    App.Services.GetService<IPagesService>().AddPage(p);
                });
            }
        }
        #endregion

        public AddExpertiseVM(Resolution resolution)
        {
            Expertise = Expertise.New;
            Expertise.FromResolution = resolution;
            _experts = App.Services.GetService<ILocalStorage>().ExpertAccessService.Items();
            Experts = _experts.Distinct(new Comparers.ExpertEqualityByEmployeeIDComparer());
            Expertise.Validate();
        }
        private void FillExpertSpecilities(Expert expert)
        {
            ExpertSpecialities.Clear();
            foreach (var item in _experts.Where(n => n.Employee.ID == expert.Employee.ID))
            {
                ExpertSpecialities.Add(item);
            }
        }
    }
}
