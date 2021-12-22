using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows.Threading;

namespace PLSE_FoxPro.Models
{
    public sealed class MessageStackQuery
    {
        #region Fields
        private int _maxstack;
        private DispatcherTimer _timer;
        private int _interval = 1;
        private Queue<Message> _inner_queue;
        #endregion

        #region Properties
        /// <summary>
        /// Максимальное количество сообщений выводимых за раз
        /// </summary>
        public int MaxStack => _maxstack;
        /// <summary>
        /// Видимаю очередь сообщений
        /// </summary>
        public ObservableCollection<Message> StackQuery { get; }
        #endregion

        #region Functions
        public void Add(Message message)
        {

            _timer.Dispatcher.Invoke(() =>
            {

                if (StackQuery.Count < _maxstack)
                {
                    StackQuery.Add(message);
                }
                else _inner_queue.Enqueue(message);
            });
        }
        /// <summary>
        /// Очищает всю очередь сообщений, видимую и внутреннюю
        /// </summary>
        public void Clear()
        {
            _timer.Dispatcher.Invoke(() => { _inner_queue.Clear(); StackQuery.Clear(); });
        }
        private void _timer_Tick(object sender, EventArgs e)
        {
            for (int i = 0; ; )
            {
                if (i == StackQuery.Count) break;
                if (StackQuery[i].IsExpired())
                {
                    StackQuery.RemoveAt(i);
                }
                else
                {
                    i++;
                }
            }
            if (_inner_queue.Count > 0 && StackQuery.Count < _maxstack) StackQuery.Add(_inner_queue.Dequeue());
        }
        #endregion

        public MessageStackQuery(int maxstack = 3, int capacity = 5)
        {
            _maxstack = maxstack;
            _inner_queue = new Queue<Message>(capacity);
            StackQuery = new ObservableCollection<Message>();
            _timer = new DispatcherTimer(DispatcherPriority.Normal)
            {
                Interval = TimeSpan.FromSeconds(_interval)
            };
            _timer.Tick += _timer_Tick;
            _timer.Start();
        }
    }
}
