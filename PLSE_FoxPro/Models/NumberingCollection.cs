using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace PLSE_FoxPro.Models
{
    /// <summary>
    /// Класс обертка для нумерованных строк
    /// </summary>
    public class NumerableContentWrapper : INotifyPropertyChanged //TODO: rework in struct?
    {
        #region Fields
        private string _content;
        private int _num;
        #endregion

        #region Properties
        public string Content
        {
            get => _content;
            set
            {
                _content = value;
                OnPropertyChanged();
            }
        }
        public int Number
        {
            get => _num;
            set
            {
                _num = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region Functions
        public override string ToString() => _content;
        void OnPropertyChanged([CallerMemberName] string prop = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        public static implicit operator NumerableContentWrapper(string s) => new NumerableContentWrapper(s);
        #endregion

        public NumerableContentWrapper(string text)
        {
            _content = text;
        }
        
        public event PropertyChangedEventHandler PropertyChanged;
       
    }
    [Serializable]
    public class NumberingObservableCollection : Collection<NumerableContentWrapper>, INotifyCollectionChanged, INotifyPropertyChanged
    {
        protected override void InsertItem(int index, NumerableContentWrapper item)
        {
            base.InsertItem(index, item);
            SetNumeration(index);
            OnPropertyChanged(new PropertyChangedEventArgs("Count"));
            OnCollecttionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, new List<NumerableContentWrapper> { item }));
        }
        protected override void ClearItems()
        {
            base.ClearItems();
            OnPropertyChanged(new PropertyChangedEventArgs("Count"));
            OnCollecttionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }
        protected override void RemoveItem(int index)
        {
            var old = this[index];
            base.RemoveItem(index);
            SetNumeration(index);
            OnPropertyChanged(new PropertyChangedEventArgs("Count"));
            OnCollecttionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, new List<NumerableContentWrapper> { old }));
        }
        protected override void SetItem(int index, NumerableContentWrapper item)
        {
            var old = this[index];
            base.SetItem(index, item);
            item.Number = old.Number;
            OnCollecttionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, new List<NumerableContentWrapper> { item },
                                                                                                            new List<NumerableContentWrapper> { old }));
        }
        public event NotifyCollectionChangedEventHandler CollectionChanged;
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnCollecttionChanged(NotifyCollectionChangedEventArgs e) => CollectionChanged?.Invoke(this, e);
        private void OnPropertyChanged(PropertyChangedEventArgs e) => PropertyChanged?.Invoke(this, e);
        private void SetNumeration(int start_index)
        {
            for (int i = start_index; i < this.Count;)
            {
                this[i++].Number = i;
            }
        }
    }
}
