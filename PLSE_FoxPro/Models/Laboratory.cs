using System;
using System.Collections.Generic;
using System.Text;

namespace PLSE_FoxPro.Models
{
    public class Laboratory : Organization
    {
        #region Fields
        private decimal _hourprice;
        private int _hourprice_orderid;
        private string _oktmo;
        private string _kbk;
        private string _kpp;
        private string _inn;
        private string _bic;
        private string _bank_correspondent_account;
        private string _bank_recipient;
        private string _personal_account;
        private string _payment_account;
        private double _paidout_percent;
        private DateTime? _regdate;
        #endregion
        #region Properties
        /// <summary>
        /// Номер расчетного счета
        /// </summary>
        public string PaymentAccount
        {
            get => _payment_account;
            set => SetProperty(ref _payment_account, value);        }
        /// <summary>
        /// Номер личного счета
        /// </summary>
        public string PersonalAccount
        {
            get => _personal_account;
            set => SetProperty(ref _personal_account, value); 
        }
        /// <summary>
        /// Банк получатель
        /// </summary>
        public string BankRecipient
        {
            get => _bank_recipient;
            set => SetProperty(ref _bank_recipient, value);
        }
        /// <summary>
        /// Корреспондентский счет банка получателя
        /// </summary>
        public string BankCorrecpondentAccount
        {
            get => _bank_correspondent_account;
            set => SetProperty(ref _bank_correspondent_account, value);
        }
        /// <summary>
        /// Банковский идентификационный код (БИК)
        /// </summary>
        public string BIC
        {
            get => _bic;
            set => SetProperty(ref _bic, value);
        }
        /// <summary>
        /// Идентификационный номер налогоплательщика (ИНН)
        /// </summary>
        public string INN
        {
            get => _inn;
            set => SetProperty(ref _inn, value);
        }
        /// <summary>
        /// Код причины постановки на учет (КПП)
        /// </summary>
        public string KPP
        {
            get => _kpp;
            set => SetProperty(ref _kpp, value);
        }
        /// <summary>
        /// Код бюджетной классификации (КБК)
        /// </summary>
        public string KBK
        {
            get => _kbk; 
            set => SetProperty(ref _kbk, value);
        }
        /// <summary>
        /// Общероссийский классификатор территорий муниципальных образований (ОКТМО)
        /// </summary>
        public string OKTMO
        {
            get => _oktmo;
            set => SetProperty(ref _oktmo, value);
        }
        /// <summary>
        /// Дата регистрации в качестве юридического лица
        /// </summary>
        public DateTime? RegistrationDate
        {
            get => _regdate;
            set => SetProperty(ref _regdate, value);
        }
        public int HourPriceOrder
        {
            get => _hourprice_orderid;
            set => SetProperty(ref _hourprice_orderid, value);
        }
        public decimal HourPrice
        {
            get => _hourprice;
            set => SetProperty(ref _hourprice, value);
        }
        /// <summary>
        /// Процент, выплачиваемый эксперту от платных экспертиз
        /// </summary>
        public double PaidOutPersent
        {
            get => _paidout_percent;
            set => SetProperty(ref _paidout_percent, value);
        }
        public static new Laboratory New => new Laboratory() { IsValid = true, Version = Version.New };
        #endregion

        private Laboratory() { }
        public Laboratory(int id, string name, string shortname, string postcode, Adress adress, string telephone, string telephone2, string fax,
                        string email, string website, bool status, Version vr, DateTime updatedate,
                        string payment_acc, string personal_acc, string bank_recipient, string bank_corresp, string bic, string inn, string kpp, string kbk, string oktmo,
                        int price_order_id, decimal hourprice, DateTime? registrationdate, double paidoutpercent)
            : base(id, name, shortname, postcode, adress, telephone, telephone2, fax, email, website, status, vr, updatedate)
        {
            _payment_account = payment_acc;
            _personal_account = personal_acc;
            _bank_recipient = bank_recipient;
            _bank_correspondent_account = bank_corresp;
            _bic = bic;
            _inn = inn;
            _kpp = kpp;
            _kbk = kbk;
            _oktmo = oktmo;
            _hourprice_orderid = price_order_id;
            _hourprice = hourprice;
            _paidout_percent = paidoutpercent;
            _regdate = registrationdate;
        }
        #region Functions

        #endregion
    }
}
