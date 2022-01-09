using System;
using System.Collections.Generic;
using System.Text;
using PLSE_FoxPro.Models;
using System.Windows.Input;
using System.Windows.Controls;
using System.Linq;
using System.Windows;
using Microsoft.Toolkit.Mvvm.Messaging;
using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;

namespace PLSE_FoxPro.ViewModels
{
    internal class AddEditSettlementVM
    {

        #region Fields

        #endregion

        #region Properties
        public Settlement Settlement { get; private set; }
        public IReadOnlyCollection<string> SettlementType { get; } = App.Services.GetService<ILocalStorage>().SettlementTypes;
        public IReadOnlyCollection<string> SettlementSignificance { get; } = App.Services.GetService<ILocalStorage>().SettlementSignificances;
        #endregion

        #region Commands
        public ICommand CancelCmd => new RelayCommand(() => App.Services.GetService<IPagesService>().RemovePage());
        public ICommand SaveCmd => new RelayCommand<Page>(n =>
                                                    {
                                                        if (App.HasValidState(n))
                                                        {
                                                            WeakReferenceMessenger.Default.Send(this.Settlement);
                                                            App.Services.GetService<IPagesService>().RemovePage();
                                                        }
                                                    });
        #endregion

        public AddEditSettlementVM(Settlement settlement = null)
        {
            Settlement = settlement?.Clone() ?? Settlement.New;
            Settlement.Validate();
        }
    }
}
