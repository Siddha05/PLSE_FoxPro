using System;
using System.Collections.Generic;
using System.Text;
using PLSE_FoxPro.Models;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.Messaging;
using Microsoft.Toolkit.Mvvm.Messaging.Messages;

namespace PLSE_FoxPro.ViewModels
{
    class AddEditSpeciality 
    {
        public Speciality Speciality { get; set; }
        public RelayCommand SendMessage => new RelayCommand(() =>
        {
            WeakReferenceMessenger.Default.Send(Speciality);
            App.RemovePage();
        });
        public AddEditSpeciality() 
        {
            Speciality = Speciality.New;
        }
    }
}
