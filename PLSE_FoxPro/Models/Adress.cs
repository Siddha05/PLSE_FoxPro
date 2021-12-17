using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace PLSE_FoxPro.Models
{
    public class Adress : ObservableValidator, ICloneable //TODO: rework to struct
    {
        #region Fields
        private Settlement _settlement;
        private string _street;
        private string _streetprefix;
        private string _housing;
        private string _flat;
        private string _corpus;
        private string _structure;
        #endregion Fields       
        #region Properties
        public string Structure
        {
            get => _structure;
            set => SetProperty(ref _structure, value);
        }
        [Required]
        public string StreetPrefix
        {
            get => _streetprefix;
            set => SetProperty(ref _streetprefix, value, true);
        }
        [Required]
        public string Street
        {
            get => _street;
            set => SetProperty(ref _street, value, true);
        }
        public string Flat
        {
            get => _flat;
            set => SetProperty(ref _flat, value);
        }
        public string Corpus
        {
            get => _corpus;
            set => SetProperty(ref _corpus, value);
        }
        public string Housing
        {
            get => _housing;
            set => SetProperty(ref _housing, value);
        }
        [Required]
        public Settlement Settlement
        {
            get => _settlement;
            set => SetProperty(ref _settlement, value, true);
        }
        #endregion Properties

        public Adress() { }
        public Adress(Settlement settlement, string streetprefix, string street, string housing, string flat, string corpus, string structure)
        {
            _settlement = settlement;
            _street = street;
            _streetprefix = streetprefix;
            _housing = housing;
            _flat = flat;
            _corpus = corpus;
            _structure = structure;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(300);
            sb.Append(StreetPrefix);
            sb.Append(" ");
            sb.Append(Street);
            if (Housing != null)
            {
                sb.Append(", д. ");
                sb.Append(Housing);
            }
            if (Corpus != null)
            {
                sb.Append(", корп. ");
                sb.Append(Corpus);
            }
            if (Structure != null)
            {
                sb.Append(", стр. ");
                sb.Append(Structure);
            }
            if (Flat != null)
            {
                sb.Append(", кв.(оф.) ");
                sb.Append(Flat);
            }
            if (Settlement != null)
            {
                sb.Append(", ");
                sb.Append(Settlement.ToString());
            }
            return sb.ToString();
        }
        
        object ICloneable.Clone()
        {
            return Clone();
        }
        public Adress Clone() => new Adress(_settlement?.Clone(), _streetprefix, _street, _housing, _flat, _corpus, _structure);
        /// <summary>
        /// Копирует состояние объекта в <paramref name="adress"/>
        /// </summary>
        /// <param name="adress"></param>
        public void Copy(Adress adress)
        {
            if (adress == null) return;
            adress.Corpus = _corpus;
            adress.Flat = _flat;
            adress.Housing = _housing;
            adress.Street = _street;
            adress.StreetPrefix = _streetprefix;
            adress.Structure = _structure;
            adress.Settlement = _settlement;
        }
    }
}
