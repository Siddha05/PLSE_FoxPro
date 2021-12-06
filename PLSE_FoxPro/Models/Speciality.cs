using System;
using System.Collections.Generic;
using System.Text;
using PLSE_FoxPro.Models;
using System.ComponentModel.DataAnnotations;

namespace PLSE_FoxPro.Models
{
    class Speciality : VersionBase, ICloneable
    {

        #region Fields
        private string _code;
        private string _species;
        private Byte? _category_1;
        private Byte? _category_2;
        private Byte? _category_3;
        private string _acronym;
        private bool _isvalid;
        private string _title;
        private DateTime _last_modify_date;
        #endregion

        #region Properties
        [Required(ErrorMessage = "обязательное поле")][MaxLength(10)]
        public string Code
        {
            get => _code;
            set => SetProperty(ref _code, value, true);
        }
        [Required(ErrorMessage ="обязательное поле")][MaxLength(75)]
        public string Species
        {
            get => _species;
            set => SetProperty(ref _species, value, true);
        }
        public Byte? Category_1
        {
            get => _category_1;
            set => SetProperty(ref _category_1, value, true);
        }
        public Byte? Category_2
        {
            get => _category_2;
            set => SetProperty(ref _category_2, value, true);
        }
        public Byte? Category_3
        {
            get => _category_3;
            set => SetProperty(ref _category_3, value, true);
        }
        public string Acronym
        {
            get => _acronym;
            set => SetProperty(ref _acronym, value);
        }
        public string Categories
        {
            get
            {
                StringBuilder r = new StringBuilder(_category_1.HasValue ? _category_1.ToString() : "?", 14);
                r.Append("/");
                r.Append(_category_2.HasValue ? _category_2.ToString() : "?");
                r.Append("/");
                r.Append(_category_3.HasValue ? _category_3.ToString() : "?");
                return r.ToString();
            }
        }
        public bool IsValid
        {
            get => _isvalid;
            set => SetProperty(ref _isvalid, value);
        }
        [Required(ErrorMessage = "обязательное поле")][MaxLength(250)]
        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value, true);
        }
        public string FullTitle => this.ToString();
        public DateTime DBModifyDate => _last_modify_date;
        public static Speciality New => new Speciality() { IsValid = true };

        #endregion

        #region Functions
        public override string ToString() => $"{Code} \"{Title}\"";
        public Speciality Clone()
        {
            return new Speciality(id: ID,
                                    code: _code,
                                    title: _title,
                                    species: _species,
                                    cat_1: _category_1,
                                    cat_2: _category_2,
                                    cat_3: _category_3,
                                    acr: _acronym,
                                    isvalid: _isvalid,
                                    vr: this.Version,
                                    updatedate: this.DBModifyDate);
        }
        object ICloneable.Clone() => Clone();
        #endregion
        private Speciality() { }
        public Speciality(int id, string code, string title, string species, Byte? cat_1, Byte? cat_2, Byte? cat_3, string acr, bool isvalid, DateTime updatedate, Version vr)
            : base(id, vr)
        {
            _code = code; _title = title; _species = species; _category_1 = cat_1; _category_2 = cat_2; _category_3 = cat_3;
            _acronym = acr; _isvalid = isvalid; _last_modify_date = updatedate;
        }
    }
}
