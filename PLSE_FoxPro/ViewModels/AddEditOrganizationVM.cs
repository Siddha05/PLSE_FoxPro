using Microsoft.Extensions.DependencyInjection;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Messaging;
using PLSE_FoxPro.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace PLSE_FoxPro.ViewModels
{
    internal class AddEditOrganizationVM : ObservableObject
    {
        #region Fields
        RelayCommand<TextBox> _settlementsearch;
        RelayCommand<Settlement> _settlementselect;
        RelayCommand<TextBox> _settlostfocus;
        RelayCommand<Settlement> _open_settlement;
        private bool _ispopsettlementopen;
        #endregion

        #region Properties
        public Organization Organization { get; set; }
        public ListCollectionView Settlements { get; } = new ListCollectionView(App.Services.GetService<ILocalStorage>().SettlementAccessService.Items().ToList());
        public IReadOnlyCollection<String> StreetPrefixes => App.Services.GetService<ILocalStorage>().StreetTypes;
        public bool IsPopupSettlementsOpen
        {
            get => _ispopsettlementopen;
            set => SetProperty(ref _ispopsettlementopen, value);
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
                        WeakReferenceMessenger.Default.Send(Organization);
                        App.Services.GetService<IPagesService>().RemovePage();
                    }
                });
            }
        }
        public ICommand SettlementSearchCmd
        {
            get
            {
                return _settlementsearch ??= new RelayCommand<TextBox>(n =>
                {
                    if (!n.IsKeyboardFocused) return;
                    if (n.Text.Length > 1)
                    {
                        IsPopupSettlementsOpen = true;
                        Settlements.Filter = x => (x as Settlement).Title.StartsWith(n.Text, StringComparison.CurrentCultureIgnoreCase);
                    }
                    else
                    {
                        IsPopupSettlementsOpen = false;
                        Settlements.Filter = null;
                    }
                });
            }
        }
        public ICommand SettlementSelectCmd
        {
            get
            {
                return _settlementselect ??= new RelayCommand<Settlement>(n =>
                {
                    Organization.Adress.Settlement = n;
                    IsPopupSettlementsOpen = false;
                });
            }
        }
        public ICommand OpenSettlementCmd
        {
            get
            {
                return _open_settlement ??= new RelayCommand<Settlement>(n =>
                {
                    if (!WeakReferenceMessenger.Default.IsRegistered<Settlement>(this))
                    {
                        WeakReferenceMessenger.Default.Register<Settlement>(this, SetAndUnregister);
                    }
                    var w = new Pages.AddEditSettlement();
                    w.DataContext = new AddEditSettlementVM(n);
                    App.Services.GetService<IPagesService>().AddPage(w);
                });
            }
        }
        //public ICommand AddNewSettlementCmd
        //{
        //    get
        //    {
        //        return new RelayCommand(() =>
        //        {
        //            var w = new Pages.AddEditSettlement();
        //            w.DataContext = new AddEditSettlementVM();
        //            WeakReferenceMessenger.Default.Register<Settlement>(this, SetAndUnregister);
        //            App.Services.GetService<IPagesService>().AddPage(w);
        //        });
        //    }
        //}
        //public ICommand EditSettlementCmd
        //{
        //    get
        //    {
        //        return new RelayCommand<Settlement>(n =>
        //        {
        //            var w = new Pages.AddEditSettlement();
        //            w.DataContext = new AddEditSettlementVM(n);
        //            WeakReferenceMessenger.Default.Register<Settlement>(this, SetAndUnregister);
        //            App.Services.GetService<IPagesService>().AddPage(w);
        //        });
        //    }
        //}
        public ICommand SettlementLostFocusCmd
        {
            get
            {
                return _settlostfocus ??= new RelayCommand<TextBox>(n =>
                {
                    n.GetBindingExpression(TextBox.TextProperty).UpdateTarget();
                });
            }
        }
        #endregion

        public AddEditOrganizationVM(Organization organization = null)
        {
            Organization = organization?.Clone() ?? Organization.New;
            Organization.Validate();
        }
        private void SetAndUnregister(object sender, Settlement message)
        {
            Organization.Adress.Settlement = message;
            WeakReferenceMessenger.Default.Unregister<Settlement>(this);
        }
        //public AddEditOrganizationVM()
        //{
        //    Organization = Organization.New;
        //}
    }
}
