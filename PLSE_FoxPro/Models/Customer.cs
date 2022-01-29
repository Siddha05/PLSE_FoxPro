using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace PLSE_FoxPro.Models
{
    public class Customer : Person, ICloneable
    {
        #region Fields
        private string _rank;
        private string _office;
        private Organization _organization;
        private bool _status;
        private string _departament;
        private int? _previd;
        private string _mobilephone;
        private string _workphone;
        private string _email;
        private bool _actual;
        #endregion

        #region Properties
        public int? PreviousID
        {
            get => _previd;
            set => SetProperty(ref _previd, value);
        }
        [MaxLength(100, ErrorMessage = "превышен лимит символов")]
        public string Departament
        {
            get => _departament;
            set => SetProperty(ref _departament, value, true);
        }
        public bool IsValid
        {
            get => _status;
            set => SetProperty(ref _status, value);
        }
        public Organization Organization
        {
            get => _organization;
            set => SetProperty(ref _organization, value);
        }
        [Required][MaxLength(150, ErrorMessage = "превышен лимит символов")]
        public string Office
        {
            get => _office;
            set => SetProperty(ref _office, value,true);
        }
        [MaxLength(100, ErrorMessage = "превышен лимит символов")]
        public string Rank
        {
            get => _rank;
            set => SetProperty(ref _rank, value,true);
        }
        [Number(ValidationNumberType.MobilePhone, AllowEmpty =true)]
        public string Mobilephone
        {
            get => _mobilephone;
            set => SetProperty(ref _mobilephone,value, true);
        }
        [RegularWithEmpty(@"\A[^@]+@([^@\.]+\.)+[^@\.]+\z", ErrorMessage = "неверный формат", AllowEmpty = true, MaxLenght = 50)]
        public string Email
        {
            get => _email;
            set => SetProperty(ref _email, value,true);
        }
        [Number(ValidationNumberType.WorkPhone, AllowEmpty = true)]
        public string Workphone
        {
            get => _workphone;
            set => SetProperty(ref _workphone, value,true);
        }
        public bool Actual => _actual;
        public string Requisite => ToString();
        public static Customer New => new Customer() { IsValid = true, Version = Version.New, _actual = true };
        public double Completeness
        {
            get
            {
                double cum = 0; int total = 7;
                if (!string.IsNullOrWhiteSpace(Sname)) cum++;
                if (!string.IsNullOrWhiteSpace(Mname))
                {
                    if (Mname.Length > 2) cum++;
                    else cum += 0.5;
                }
                if (!string.IsNullOrWhiteSpace(Fname))
                {
                    if (Fname.Length > 2) cum++;
                    else cum += 0.5;
                }
                if (Gender != null) cum++;
                if (!string.IsNullOrWhiteSpace(Email)) cum++;
                if (!string.IsNullOrWhiteSpace(Mobilephone)) cum++;
                if (!string.IsNullOrWhiteSpace(Office)) cum++;
                if (Office != "гражданин")
                {
                    total += 2;
                    if (!string.IsNullOrWhiteSpace(Workphone)) cum++;
                    if (Organization != null) cum++;
                }
                return Math.Round(100 * cum / total);
            }
        }
        #endregion

        #region Metods
        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder(25);
            if (Office != null)
            {
                stringBuilder.Append(Office).Append(" ");
            }
            stringBuilder.Append(Fio);
            if (Organization != null)
            {
                stringBuilder.AppendLine();
                if (Departament != null)
                {
                    stringBuilder.Append(Departament).Append(" ");
                }
                stringBuilder.Append(Organization.ToString());
            }
            return stringBuilder.ToString();
        }
        public Customer Clone()
        {
            return new Customer(firstname: Fname,
                                  middlename: Mname,
                                  secondname: Sname,
                                  mobilephone: Mobilephone,
                                  workphone: Workphone,
                                  gender: Gender,
                                  email: Email,
                                  declinated: IsDeclinated,
                                  vr: Version,
                                  updatedate: DBModifyDate,
                                  id: ID,
                                  rank: _rank,
                                  office: _office,
                                  organization: _organization.Clone(),
                                  departament: _departament,
                                  status: _status,
                                  previd: _previd);
        }
        object ICloneable.Clone() => Clone();
        #endregion

        private Customer() : base() { }
        public Customer(string firstname, string middlename, string secondname, string mobilephone, string workphone, bool? gender, string email, bool declinated, Version vr,
                        DateTime updatedate, int id, int? previd, string rank, string office, Organization organization, string departament, bool status)
            : base(id, firstname, middlename, secondname, gender, declinated, vr, updatedate)
        {
            _rank = rank;
            _office = office;
            _organization = organization;
            _departament = departament;
            _status = status;
            _previd = previd;
            _workphone = workphone;
            _mobilephone = mobilephone;
            _email = email;
        }
    }
}
