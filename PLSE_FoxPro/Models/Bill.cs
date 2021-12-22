using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace PLSE_FoxPro.Models
{
    public sealed class Bill : VersionBase
    {
        #region Fields
        private string _number;
        private DateTime _billdate;
        private DateTime? _paiddate;
        private string _payer;
        private byte _hours;
        private decimal _hourprice;
        private decimal _paid;
        private Expertise _from;
        #endregion

        #region Properties
        /// <summary>
        /// Номер счета
        /// </summary>
        [Required][Number(ValidationNumberType.Bill)]
        public string Number
        {
            get => _number;
            set => SetProperty(ref _number, value, true);
        }
        /// <summary>
        /// Дата счета
        /// </summary>
        public DateTime BillDate
        {
            get => _billdate;
            set => SetProperty(ref _billdate, value, true);
        }
        /// <summary>
        /// Дата оплаты счета. Null если оплаты не было
        /// </summary>
        public DateTime? PaidDate
        {
            get => _paiddate;
            set => SetProperty(ref _paiddate, value);
        }
        [MaxLength(100, ErrorMessage = "превышен лимит символов")]
        public string Payer
        {
            get => _payer;
            set => SetProperty(ref _payer, value, true);
        }
        public byte Hours
        {
            get => _hours;
            set => SetProperty(ref _hours, value);
        }
        public decimal HourPrice
        {
            get => _hourprice;
            set => SetProperty(ref _hourprice, value);
        }
        public decimal Paid
        {
            get => _paid;
            set => SetProperty(ref _paid, value);
        }
        public Expertise FromExpertise => _from;
        public decimal Price => _hours * _hourprice;
        public decimal Balance => _paid - _hours * _hourprice;
        public static Bill New => new Bill() { Version = Version.New, BillDate = DateTime.Now, HourPrice = App.Me.Laboratory.HourPrice };
        #endregion

        private Bill() : base() { }
        public Bill(int id, Expertise from, string number, DateTime billdate, DateTime? paiddate, string payer, byte hours, decimal hourprice, decimal paid, Version vr)
                        : base(id, vr)
        {
            _number = number; _from = from;
            _billdate = billdate; _paiddate = paiddate; _payer = payer;
            _hours = hours; _hourprice = hourprice; _paid = paid;
        }
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder("Счет #", 200);
            sb.Append(_number); sb.Append(" от "); sb.AppendLine(_billdate.ToString("d"));
            sb.Append("На сумму: "); sb.Append(_hours * _hourprice); sb.Append("/t"); sb.Append("Оплачено: "); sb.Append(_paid);
            return sb.ToString();
        }
    }
}
