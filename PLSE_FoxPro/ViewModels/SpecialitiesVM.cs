using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using PLSE_FoxPro.Models;
using System.Windows.Data;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using System.Windows.Input;
using Microsoft.Toolkit.Mvvm.Messaging;

namespace PLSE_FoxPro.ViewModels
{
    internal class SpecialitiesVM
    {
        #region Fields
        ObservableCollection<Speciality> _list;
        #endregion

        #region Properties
        //public ListCollectionView Specialities { get; private set;
        public ObservableCollection<Speciality> Specialities => _list;
        #endregion

        #region Commands
        public ICommand NewSpecialityCmd
        {
            get
            {
                return new RelayCommand<Speciality>(n =>
                {
                    var p = new Pages.AddEditSpeciality();
                    p.DataContext = new AddEditSpecialityVM();
                    App.Services.GetService<IPagesService>().AddPage(p);
                });
            }
        }
        public ICommand EditSpecialityCmd
        {
            get
            {
                return new RelayCommand<Speciality> (n =>
                {
                    var p = new Pages.AddEditSpeciality();
                    p.DataContext = new AddEditSpecialityVM(n);
                    App.Services.GetService<IPagesService>().AddPage(p);
                },
                    CanExecuteOperation
                );
            }
        }
        public ICommand DeleteSpecialityCmd
        {
            get
            {
                return new RelayCommand<Speciality>(s =>
                {
                    if (MessageBox.Show("Удалить выбранную специальность?", "Удаление", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    {
                        try
                        {
                            App.Services.GetService<ILocalStorage>().SpecialityAccessService.Delete(s);
                            _list.Remove(s);
                            App.SendMessage(App.SuccessSave);
                        }
                        catch (Exception ex)
                        {
                            App.Services.GetService<IErrorLogger>().LogError(ex);
                            App.ShowErrorWindow(App.ErrorOnSave);
                        }
                    }
                },
                    CanExecuteOperation
                );
            }
        }
        public ICommand CancelCmd => new RelayCommand(() => App.Services.GetService<IPagesService>().RemovePage());
        #endregion

        private bool CanExecuteOperation(object o) => o == null ? false : true;
        public SpecialitiesVM()
        {
            _list = new ObservableCollection<Speciality>(App.Services.GetService<ILocalStorage>().SpecialityAccessService.Items());
            WeakReferenceMessenger.Default.Register<Speciality>(this, (r, m) => _list.Add(m));
        }
    }
}
