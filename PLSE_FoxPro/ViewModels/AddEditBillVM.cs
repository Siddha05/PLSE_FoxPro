using Microsoft.Extensions.DependencyInjection;
using Microsoft.Toolkit.Mvvm.Messaging;
using PLSE_FoxPro.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace PLSE_FoxPro.ViewModels
{
    public class AddEditBillVM
    {
        #region Fields
        private Bill _bill;
        #endregion

        #region Properties
        public Bill Bill => _bill;
        public IReadOnlyList<String> CommonPayers => new List<string> { "истец", "ответчик", "истец и ответчик" };
        #endregion

        #region Commands
        public ICommand CancelCmd => new RelayCommand(() => App.Services.GetService<IPagesService>().RemovePage());
        public ICommand SaveCmd
        {
            get
            {
                return new RelayCommand<Window>(n =>
                {
                    if(App.HasValidState(n))
                    {
                        WeakReferenceMessenger.Default.Send(_bill, 1);
                        App.Services.GetService<IPagesService>().RemovePage();
                    }
                });
            }
        }
        #endregion
        public AddEditBillVM(Bill bill = null)
        {
            _bill = bill ?? Bill.New;
            _bill.Validate();
        }
    }
}
