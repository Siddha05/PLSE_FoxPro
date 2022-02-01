using PLSE_FoxPro.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace PLSE_FoxPro.ViewModels
{
    internal class TestVM
    {

        public List<Expertise> Expertises { get; set; } = new List<Expertise> { DesignData.TestInstance.Expertise1 };
        
        public ICommand TestCmd
        {
            get
            {
                return new RelayCommand<Button>(n =>
                {

                    MessageBox.Show("Invoke command");
                });
            }
        }
        public TestVM()
        {
            
            
            
        }
       
    }
}
