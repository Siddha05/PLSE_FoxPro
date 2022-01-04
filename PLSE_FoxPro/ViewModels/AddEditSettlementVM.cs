using System;
using System.Collections.Generic;
using System.Text;
using PLSE_FoxPro.Models;
using Microsoft.Toolkit.Mvvm.Input;
using System.Windows.Input;
using System.Windows.Controls;
using System.Linq;
using System.Windows;
using Microsoft.Toolkit.Mvvm.Messaging;

namespace PLSE_FoxPro.ViewModels
{
    internal class AddEditSettlementVM
    {

        #region Fields

        #endregion

        #region Properties
        public Settlement Settlement { get; private set; }
        public IReadOnlyCollection<string> SettlementType { get; } = App.Storage.SettlementTypes;
        public IReadOnlyCollection<string> SettlementSignificance { get; } = App.Storage.SettlementSignificances;
        #endregion

        #region Commands
        ICommand CancelCmd => new RelayCommand(() => App.RemovePage());
        ICommand SaveCmd => new RelayCommand<Page>(n =>
                                                    {
                                                        var el = App.GetDecendants<FrameworkElement>(n).FirstOrDefault(e => Validation.GetHasError(e));
                                                        if (el == null)
                                                        {
                                                            WeakReferenceMessenger.Default.Send(this.Settlement);
                                                            App.RemovePage();
                                                        }
                                                        else
                                                        {
                                                            MessageBox.Show(App.ErrorOnPage.Content);
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
