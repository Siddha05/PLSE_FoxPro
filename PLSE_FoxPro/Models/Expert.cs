using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace PLSE_FoxPro.Models
{
    public class Expert : VersionBase, ICloneable
    {
        #region Fields     
        private Speciality _speciality;
        private DateTime? _receiptdate;
        private DateTime? _lastattestationdate;
        private Employee _employee;
        private bool _closed;
        private DateTime _last_modify_date;
        #endregion
        #region Properties
        [Required(ErrorMessage ="обязательное поле")]
        public Employee Employee
        {
            get => _employee;
            set =>SetProperty(ref _employee, value,true);
        }
        [Required(ErrorMessage ="обязательное поле")]
        public Speciality Speciality
        {
            get => _speciality;
            set => SetProperty(ref _speciality, value,true);
        }
        [Required(ErrorMessage = "обязательное поле")]
        public DateTime? ReceiptDate
        {
            get => _receiptdate;
            set => SetProperty(ref _receiptdate, value, true);
        }
        public DateTime? LastAttestationDate
        {
            get => _lastattestationdate;
            set => SetProperty(ref _lastattestationdate, value);
        }
        public DateTime DBModifyDate => _last_modify_date;
        public bool IsClosed
        {
            get { return _closed; }
            set => SetProperty(ref _closed, value);
        }
        public int? Experience => ReceiptDate.HasValue ? DateTime.Now.Year - ReceiptDate.Value.Year : new int?();
        public bool IsValidAttestation
        {
            get
            {
                if (ReceiptDate.HasValue && !IsClosed)
                {
                    return (DateTime.Now - (LastAttestationDate ?? ReceiptDate.Value)).Days / 365.25 < 5.0;
                }
                else return true;
            }
        }
        public static Expert New => new Expert() { IsClosed = false, Version = Version.New};
        #endregion
        private Expert() : base() { }
        public Expert(int id, Speciality speciality, Employee employee, DateTime? receiptdate, DateTime? lastattestationdate, Version vr,
                        DateTime updatedate, bool closed = false) : base(id, vr)
        {
            _employee = employee;
            _last_modify_date = updatedate;
            _closed = closed;
            _speciality = speciality;
            _receiptdate = receiptdate;
            _lastattestationdate = lastattestationdate;
        }
        /// <summary>
        /// Shallow copy
        /// </summary>
        /// <returns></returns>
        public Expert Clone()
        {
            return new Expert(id:ID,
                              speciality: _speciality,
                              employee: _employee,
                              receiptdate: _receiptdate,
                              lastattestationdate: _lastattestationdate,
                              vr: this.Version,
                              updatedate: _last_modify_date,
                              closed: _closed);
        }
        object ICloneable.Clone() => Clone();
    }
}
