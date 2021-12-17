using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace PLSE_FoxPro.Models
{
    public class Equipment : VersionBase
    {
        private string _eqname;
        private string _descr;
        private DateTime? _commisiondate;
        private bool _status;
        private DateTime? _checkdate;
        private DateTime _last_modify_date;

        public bool IsTrack
        {
            get => _status;
            set => SetProperty(ref _status, value);
        }
        public DateTime? CommisionDate
        {
            get => _commisiondate;
            set => SetProperty(ref _commisiondate,value);
        }
        /// <summary>
        /// Дата последней поверки
        /// </summary>
        public DateTime? LastCheckDate
        {
            get => _checkdate;
            set => SetProperty(ref _checkdate, value);
        }
        [MaxLength(500, ErrorMessage = "превышен лимит символов")]
        public string Description
        {
            get => _descr;
            set => SetProperty(ref _descr, value, true);
        }
        [Required(ErrorMessage = "обязательное поле")][MaxLength(100, ErrorMessage = "превышен лимит символов")]
        public string EquipmentName
        {
            get => _eqname;
            set => SetProperty(ref _eqname, value, true);
        }
        public DateTime DBModifyDate => _last_modify_date;

        public Equipment() : base() { }
        public Equipment(int id, string name, string description, DateTime? commisiondate, DateTime? check, bool istrack, Version vr, DateTime updatedate)
            : base(id, vr)
        {
            _eqname = name;
            _descr = description;
            _commisiondate = commisiondate;
            _status = istrack;
            _checkdate = check;
            _last_modify_date = updatedate;
        }
        public override string ToString() => _eqname;
    }
}
