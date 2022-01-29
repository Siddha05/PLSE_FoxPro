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
        
        public Person Person {get;}
        private string _prop;
        
        public ICommand TestCmd
        {
            get
            {
                return new RelayCommand<Button>(n =>
                {

                    MessageBox.Show(_prop);
                });
            }
        }
        public TestVM()
        {
            Person = Customer.New;

            var e = GetValidatedProperties(typeof(string));

            MessageBox.Show($"{e.Count()}");
            
            
        }
        protected IEnumerable<PropertyInfo> GetProperties(Type t)
        {
            foreach (var item in t.GetProperties(BindingFlags.Instance | BindingFlags.Public))
            {
                yield return item;
            }
        }
        protected IEnumerable<PropertyInfo> GetValidatedProperties(Type t)
        {
            foreach (var item in t.GetProperties(BindingFlags.Instance | BindingFlags.Public))
            {
                var att = item.GetCustomAttributes(typeof(ValidationAttribute), false);
                if (att.Length > 0) yield return item;
            }
        }
    }
}
