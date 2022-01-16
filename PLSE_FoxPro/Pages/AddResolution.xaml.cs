using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PLSE_FoxPro.Pages
{
    /// <summary>
    /// Interaction logic for AddResolution.xaml
    /// </summary>
    public partial class AddResolution : Page
    {
        public AddResolution()
        {
            InitializeComponent();
        }

        private void tbEnterQuestion_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                e.Handled = true;
                var tb = sender as TextBox;
                if (tb.Text.Length > 0)
                {
                    (this.DataContext as ViewModels.AddResolutionVM).Resolution.Questions.Add(new Models.NumerableContentWrapper(tb.Text));
                    tb.Text = "";
                }
            }
        }
        private void tbEnterObject_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                e.Handled = true;
                var tb = sender as TextBox;
                if (tb.Text.Length > 0)
                {
                    (this.DataContext as ViewModels.AddResolutionVM).Resolution.Objects.Add(new Models.NumerableContentWrapper(tb.Text));
                    tb.Text = "";
                }
            }
        }

        private void brd_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Keyboard.Focus(tbNative);
        }
    }
}
