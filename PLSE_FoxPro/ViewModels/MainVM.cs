using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.Messaging;
using Microsoft.Toolkit.Mvvm.Messaging.Messages;
using PLSE_FoxPro.Models;

namespace PLSE_FoxPro.ViewModels
{
    public class MainVM : ObservableObject
    {

        #region Fields
        RelayCommand _speciality;
        Page _content;
        #endregion

        #region Properties
        public Page FrameContent
        {
            get => _content;
            set => SetProperty(ref _content, value);
        }
        public RelayCommand ShowSpecialityCommand
        {
            get
            {
                return _speciality != null ? _speciality : _speciality = new RelayCommand(() =>
                {
                    WeakReferenceMessenger.Default.Register<Speciality>(this, (o, m) => MessageBox.Show(m.Acronym));
                    var p = new Pages.AddEditSpeciality();
                    var cnx = new ViewModels.AddEditSpeciality();
                    p.DataContext = cnx;
                    cnx.Speciality.Validate();
                    FrameContent = p;
                });
            }
        }
        public RelayCommand MainCommand => new RelayCommand(() =>
        {

            MessageBox.Show(App.Me.MainViewModel.GetType().Name);

        });
        #endregion

        #region Functions
        #endregion
    }
}
