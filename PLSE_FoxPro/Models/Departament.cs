﻿using System;
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
        [Required][MaxLength(10)]
        public string DigitalCode
        {
            get => _digitalcode;
            set => SetProperty(ref _digitalcode, value, true);
        }
        [Required][StringLength(10)]
        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value, true);
        }
        /// <summary>
        /// Сокращенное название отдела
        /// </summary>
        [Required][MaxLength(10)]
        public string Acronym
        {
            get => _acronym;
            set => SetProperty(ref _acronym, value, true);
        }
        #endregion

        #region Functions
        public override string ToString() => Acronym;
        #endregion

        public Departament(int id, string title, string acronym, string code, bool isvalid)
        {
            ID = id;
            _title = title;
            _acronym = acronym;
            _digitalcode = code;
            _isvalid = isvalid;
        }
       
    }
}