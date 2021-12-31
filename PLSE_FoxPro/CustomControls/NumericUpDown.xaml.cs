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

namespace PLSE_FoxPro.CustomControls
{
    /// <summary>
    /// Interaction logic for NumericUpDown.xaml
    /// </summary>
    public partial class NumericUpDown : UserControl
    {

        #region Fields
        public static readonly RoutedEvent ValueChangedEvent = EventManager.RegisterRoutedEvent("ValueChanged", RoutingStrategy.Bubble,
                                                                                                typeof(RoutedPropertyChangedEventHandler<int>), typeof(NumericUpDown));
        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register("Value", typeof(int), typeof(NumericUpDown), 
                                                                                                new PropertyMetadata(0, new PropertyChangedCallback(OnValueChanged),                                                                                       new CoerceValueCallback(CoerceValue)));

        #endregion

        #region Properties
        public int MaxValue { get; set; } = 100;
        public int MinValue { get; set; } = 0;
        public int Value
        {
            get { return (int)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }       
        #endregion

        #region Functions
        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            NumericUpDown cnt = d as NumericUpDown;
            RoutedPropertyChangedEventArgs<int> arg = new RoutedPropertyChangedEventArgs<int>((int)e.OldValue, (int)e.NewValue, ValueChangedEvent);
            cnt.OnValueChanged(arg);
        }
        protected virtual void OnValueChanged(RoutedPropertyChangedEventArgs<int> args) => RaiseEvent(args);
        private static object CoerceValue(DependencyObject element, object val)
        {
            int newval = (int)val;
            NumericUpDown cnt = element as NumericUpDown;
            if (newval < cnt.MinValue) return cnt.MinValue;
            if (newval > cnt.MaxValue) return cnt.MaxValue;
            else return newval;
        }
        private void upButton_Click(object sender, EventArgs e) => Value++;
        private void downButton_Click(object sender, EventArgs e) => Value--;
        #endregion
       
        public event RoutedPropertyChangedEventHandler<int> ValueChanged
        {
            add { AddHandler(ValueChangedEvent, value); }
            remove { RemoveHandler(ValueChangedEvent, value); }
        }
           
        public NumericUpDown()
        {
            InitializeComponent();
        }
    }
}
