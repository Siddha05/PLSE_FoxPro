using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Input;
using PLSE_FoxPro.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Toolkit.Mvvm.Messaging;

namespace PLSE_FoxPro.ViewModels
{
    internal class AddEditSpecialityVM
    {
        #region Fields
        ICommand _categoryupdated;
        #endregion

        public Speciality Speciality { get; set; }
        public IReadOnlyList<string> SpecialityKinds { get; } = App.Services.GetService<ILocalStorage>().SpecialityKinds;

        #region Commands
        public ICommand SaveCmd
        {
            get
            {
                return new RelayCommand<Page>(n =>
                {
                    if (App.HasValidState(n))
                    {
                        try
                        {
                            App.Services.GetService<ILocalStorage>().SpecialityAccessService.SaveChanges(this.Speciality);
                            WeakReferenceMessenger.Default.Send(Speciality);
                            App.SendMessage(App.SuccessSave);
                        }
                        catch (Exception ex)
                        {
                            App.SendMessage(App.ErrorOnSave);
                            App.Services.GetService<IErrorLogger>().LogError(ex.Message);
                        }
                        App.Services.GetService<IPagesService>().RemovePage();
                    }
                    else App.ShowErrorWindow(App.ErrorOnPage);
                });
            }
        }
        public ICommand Category2UpdatedCmd
        {
            get
            {
                return _categoryupdated != null ? _categoryupdated : _categoryupdated = new RelayCommand<TextBox>(n =>
                {
                    n.GetBindingExpression(TextBox.TextProperty).UpdateSource();
                });
            }
        }
        public ICommand Category3UpdatedCmd
        {
            get
            {
                return _categoryupdated != null ? _categoryupdated : _categoryupdated = new RelayCommand<TextBox>(n =>
                {
                    n.GetBindingExpression(TextBox.TextProperty).UpdateSource();
                });
            }
        }
        public ICommand CancelCmd => new RelayCommand(() => App.Services.GetService<IPagesService>().RemovePage());
        #endregion
        public AddEditSpecialityVM(Speciality sp = null)
        {
            Speciality = sp?.Clone() ?? Speciality.New;
            Speciality.Validate();
        }
    }
}
