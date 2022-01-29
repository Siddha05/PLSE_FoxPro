using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace PLSE_FoxPro.Models
{
    public class Departament : VersionBase
    {
        #region Fields
        private string _title;
        private string _digitalcode;
        private string _acronym;
        private bool _isvalid;
        #endregion
       
        #region Properties
        public bool IsValid
        {
            get { return _isvalid; }
            set => SetProperty(ref _isvalid, value);
        }
        /// <summary>
        /// Цифровой код отдела
        /// </summary>
        [Required(ErrorMessage = "обязательное поле")][MaxLength(10, ErrorMessage = "превышен лимит символов")]
        public string DigitalCode
        {
            get => _digitalcode;
            set => SetProperty(ref _digitalcode, value, true);
        }
        [Required(ErrorMessage = "обязательное поле")][StringLength(10, ErrorMessage = "превышен лимит символов")]
        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value, true);
        }
        /// <summary>
        /// Сокращенное название отдела
        /// </summary>
        [Required(ErrorMessage = "обязательное поле")][MaxLength(10, ErrorMessage = "превышен лимит символов")]
        public string Acronym
        {
            get => _acronym;
            set => SetProperty(ref _acronym, value, true);
        }
        #endregion

        #region Functions
        public override string ToString() => Acronym;
        #endregion

        public Departament(int id, string title, string acronym, string code, bool isvalid, Version vr)
            : base(id, vr)
        {
            _title = title;
            _acronym = acronym;
            _digitalcode = code;
            _isvalid = isvalid;
        }
       
    }
}
