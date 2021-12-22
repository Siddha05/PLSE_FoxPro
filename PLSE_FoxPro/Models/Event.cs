using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using LiveCharts;
using LiveCharts.Wpf;

namespace PLSE_FoxPro.Models
{
    public enum MessageType
    {
        Ordinary = 0,
        Success,
        Warning,
        Error,
        Congratulation
    }
    public abstract class Event
    {
        public DateTime CreateTime { get; }
        public string Source { get; }
        public string ImagePath { get; }

        public Event(string source = null, string image_path = null)
        {
            CreateTime = DateTime.UtcNow;
            Source = source ?? App.Me.AppName;
            ImagePath = image_path ?? App.Me.AppImagePath;
        }
    }

    public class Message : Event
    {
        #region Fields
        string _content;
        MessageType _type;
        TimeSpan _lifetime;
        #endregion

        #region Properties
        public string Content => _content;
        public MessageType Type => _type;
        /// <summary>
        /// Время жизни сообщения
        /// </summary>
        public TimeSpan LifeTime => _lifetime;
        #endregion

        #region Functions
        public override string ToString() => $"{_content}({CreateTime:T})";
        /// <summary>
        /// Истекло ли время жизни сообщения
        /// </summary>
        /// <returns></returns>
        public bool IsExpired() => (DateTime.UtcNow - CreateTime) > _lifetime;

        public static implicit operator Message (string content) => new Message(content);
        #endregion

        /// <summary>
        /// Создает новый экземпляр Message
        /// </summary>
        /// <param name="content">Содержимое сообщения</param>
        /// <param name="type">Тип сообщения</param>
        /// <param name="lifetime">Время жинзи в секундах</param>
        /// <param name="source">Источник ссобщения</param>
        /// <param name="imagepath">Путь с файлу с изображением</param>
        public Message(string content, MessageType type, int lifetime, string source, string imagepath) : base(source, imagepath)
        {
            _content = content;
            _type = type;
            _lifetime = TimeSpan.FromSeconds(lifetime);
        }
        public Message(string content) : this(content, MessageType.Ordinary, 5, null, null) { }
        public Message(string content, MessageType type) : this(content, type, 5, null, null) { }
        public Message(string content, MessageType type, int lifetime) : this(content, type, lifetime, null, null) { }
    }

    public class ExpertisesInWorkOverview : Event
    {
        #region Fields
        int _max_left_days_factor;
        Func<ChartPoint, string> _label = cpoint => $"{cpoint.Y}";
        #endregion

        #region Properties
        /// <summary>
        /// Общее количество экспертиз
        /// </summary>
        public int ExpertiseCount { get; }
        public int OtherExpertiseCount => ExpertiseCount - OverdueCount - XDaysLeftCount - SuspendCount;
        /// <summary>
        /// Параметр количества дней до окончания экспертизы, ниже которого срабатывает предупреждение
        /// <para>Установлен в параметре WarnLeftDaysExpTreshold настроек</para>
        /// </summary>
        public int MaxLeftDaysFactor => _max_left_days_factor;
        /// <summary>
        /// Количество просроченных экспертиз
        /// </summary>
        public int OverdueCount { get; }
        /// <summary>
        /// Количество экспертиз, до окончания которых менее X дней
        /// <para>X = MaxLeftDaysFactor</para>
        /// </summary>
        public int XDaysLeftCount { get; }
        /// <summary>
        /// Количество экспертиз на ходатайстве
        /// </summary>
        public int SuspendCount { get; }
        /// <summary>
        /// Общая сумма счетов
        /// </summary>
        public decimal TotalMoney { get; }
        /// <summary>
        /// Выплачиваемая сумма процентов от общей суммы счетов
        /// </summary>
        public decimal TotalMoneyPaidOutConv => TotalMoney * (decimal)PaidOutFactor;
        /// <summary>
        /// Общая выплаченная сумма счетов
        /// </summary>
        public decimal PaidMoney { get; }
        /// <summary>
        /// Выплаченный процент
        /// </summary>
        public double PaidMoneyPercent { get; }
        /// <summary>
        /// Количество полностью оплаченных счетов
        /// </summary>
        public int PaidBillsCount { get; }
        /// <summary>
        /// Количество частично оплаченных счетов
        /// </summary>
        public int PartialPaidBillsCount { get; }
        public double PaidBillPercent { get; }
        /// <summary>
        /// Сумма процентов от оплаченных счетов
        /// </summary>
        public decimal PaidMoneyPaidOutConv => PaidMoney * (decimal)PaidOutFactor;
        /// <summary>
        /// Количество невыставленных счетов по платным экспертизам
        /// </summary>
        public int NonPerformBillCount { get; }
        /// <summary>
        /// Общее количество счетов
        /// </summary>
        public int BillsCount { get; }
        public SeriesCollection ExpertiseSeries
        {
            get
            {
                SeriesCollection series = new SeriesCollection();
                if (OtherExpertiseCount > 0)
                {
                    series.Add(new PieSeries
                    {
                        Values = new ChartValues<int>() { OtherExpertiseCount },
                        Title = "иные",
                        DataLabels = true,
                        Fill = Brushes.Blue,
                        FontSize = 14,
                        LabelPoint = _label

                    });
                }
                if (OverdueCount > 0)
                {
                    series.Add(new PieSeries
                    {
                        Values = new ChartValues<int>() { OverdueCount },
                        Title = "проср.",
                        DataLabels = true,
                        Fill = Brushes.Red,
                        FontSize = 14,
                        LabelPoint = _label
                    });
                }
                if (XDaysLeftCount > 0)
                {
                    series.Add(new PieSeries
                    {
                        Values = new ChartValues<int>() { XDaysLeftCount },
                        Title = $"<{_max_left_days_factor} д.",
                        DataLabels = true,
                        Fill = Brushes.Goldenrod,
                        FontSize = 14,
                        LabelPoint = _label
                    });
                }
                if (SuspendCount > 0)
                {
                    series.Add(new PieSeries
                    {
                        Values = new ChartValues<int>() { SuspendCount },
                        Title = "приостан.",
                        DataLabels = true,
                        Fill = Brushes.ForestGreen,
                        FontSize = 14,
                        LabelPoint = _label
                    });
                }
                return series;
            }
        }
        /// <summary>
        /// Выплачиваемый процент от платных экспертиз
        /// </summary>
        public double PaidOutFactor { get; }
        public string StringProp => $"Количество счетов: {ExpertiseCount}\n\tна сумму {TotalMoney:c}\nОплачено счетов:{PaidBillsCount} ({PaidBillPercent:p})\n\t" +
                                    $"на сумму{PaidMoney:c} ({PaidMoneyPercent:p})\n\n{PaidOutFactor:p} конверсия:\n\tвсе счета: {TotalMoneyPaidOutConv:c}".ToString(System.Globalization.CultureInfo.GetCultureInfo("ru-RU"));
        #endregion
        public ExpertisesInWorkOverview(IEnumerable<Expertise> expertises)
        {
            PaidOutFactor = App.Me.Laboratory.PaidOutPersent;
            _max_left_days_factor = Properties.Settings.Default.WarnLeftDaysExpTreshold;
            int bill_cnt = 0, nonperform_bill_cnt = 0, paid_bill_cnt = 0, partial_paid_bill_cnt = 0;
            int overdue_cnt = 0, suspend_cnt = 0, xdaysleft_cnt = 0;
            decimal total_money = 0m, paid_money = 0m;
            foreach (var item in expertises.SelectMany(n => n.Bills))
            {
                bill_cnt++;
                total_money += item.Price;
                paid_money += item.Paid;
                if (item.Balance >= 0) paid_bill_cnt++;
                else if (item.Balance > -item.Price) partial_paid_bill_cnt++;
            }
            foreach (var item in expertises)
            {
                if (ResolutionHelper.IsPayableResolution(item?.FromResolution) && item.Bills.Count == 0) nonperform_bill_cnt++;
                if (item.Remain2 < 0) overdue_cnt++;
                if (item.IsOnRequest()) suspend_cnt++;
                if (item.Remain2 >= 0 && item.Remain2 <= MaxLeftDaysFactor) xdaysleft_cnt++;
            }
            ExpertiseCount = expertises.Count();
            OverdueCount = overdue_cnt;
            XDaysLeftCount = xdaysleft_cnt;
            SuspendCount = suspend_cnt;
            TotalMoney = total_money;
            PaidMoney = paid_money;
            PaidMoneyPercent = total_money == 0m ? 1 : (double)(paid_money / total_money);
            PaidBillsCount = paid_bill_cnt;
            PartialPaidBillsCount = partial_paid_bill_cnt;
            PaidBillPercent = bill_cnt == 0 ? 1 : (double)paid_bill_cnt / bill_cnt;
            NonPerformBillCount = nonperform_bill_cnt;
            BillsCount = bill_cnt;
        }
    }

    public class AnnualDatesOverview : Event
    {
        //TODO: implement
    }
}
