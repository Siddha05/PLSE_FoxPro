using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using PLSE_FoxPro;
using System.ComponentModel.DataAnnotations;

namespace PLSE_FoxPro.Models
{
    public class Person : VersionBase, ICloneable
    {
        #region Fields
        protected string _fname;
        protected bool _declinated;
        protected string _mname;
        protected string _sname;
        protected bool? _gender;
        protected DateTime _last_modify_date;
        #endregion
        #region Properties
        [RegularExpression(@"^\p{IsCyrillic}{2,25}$|^\p{IsCyrillic}\.$", ErrorMessage ="неверный формат имени")]
        public string Fname
        {
            get => _fname;
            set => SetProperty(ref _fname, value.ToProperNoun(), true);
        }
        [RegularExpression(@"^\p{IsCyrillic}{2,25}$|^\p{IsCyrillic}\.$", ErrorMessage = "неверный формат отчества")]
        public string Mname
        {
            get => _mname;
            set => SetProperty(ref _mname, value.ToProperNoun(), true);
        }
        [RegularExpression(@"^\p{IsCyrillic}{2,15}(?:-\p{IsCyrillic}{2,15})?$", ErrorMessage = "неверный формат фамилии")]
        public string Sname
        {
            get => _sname;
            set => SetProperty(ref _sname, value.ToProperNoun(), true);
        }
        /// <summary>
        /// true - мужской, false - женский
        /// </summary>
        public bool? Gender
        {
            get => _gender;
            set => SetProperty(ref _gender, value);
        }
        /// <summary>
        /// Склонять ли фамилию?
        /// </summary>
        public bool IsDeclinated
        {
            get => _declinated;
            set => SetProperty(ref _declinated, value);
        }
        public string Fio => Sname + " " + Fname[0] + "." + Mname[0] + ".";
        public DateTime DBModifyDate => _last_modify_date;
        #endregion

        public Person(int id, string firstname, string middlename, string secondname, bool? gender, bool declinated, Version vr, DateTime updatedate)
            : base(id, vr)
        {
            _fname = firstname;
            _mname = middlename;
            _sname = secondname;
            _gender = gender;
            _declinated = declinated;
            _last_modify_date = updatedate;
        }
        public Person() { }

        #region Methods
        private Person Clone()
        {
            return new Person(ID, _fname, _sname, _mname, _gender, _declinated, this.Version, _last_modify_date);
        }
        object ICloneable.Clone() => Clone();
        #endregion
    }
}
