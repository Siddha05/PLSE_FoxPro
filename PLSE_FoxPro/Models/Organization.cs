using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace PLSE_FoxPro.Models
{
    public class Organization : VersionBase, ICloneable
    {
        #region Fields
        protected string _name;
        protected string _shortname;
        protected string _postcode;
        protected Adress _adress = new Adress { StreetPrefix = "ул." };
        protected string _telephone;
        protected string _telephone2;
        protected string _fax;
        protected string _email;
        protected string _website;
        protected bool _status;
        protected DateTime _last_modify_date;
        #endregion
        #region Properties
        public bool IsValid
        {
            get => _status;
            set =>SetProperty(ref _status, value);
        }
        [MaxLength(50, ErrorMessage ="превышен лимит символов")]
        public string WebSite
        {
            get => _website;
            set => SetProperty(ref _website, value);
        }
        [RegularWithEmpty(@"\A[^@]+@([^@\.]+\.)+[^@\.]+\z", ErrorMessage = "неверный формат", AllowEmpty = true)]
        public string Email
        {
            get => _email;
            set => SetProperty(ref _email, value, true);
        }
        [Number(ValidationNumberType.Fax, AllowEmpty = true)]
        public string Fax
        {
            get => _fax;
            set => SetProperty(ref _fax, value, true);
        }
        [Number(ValidationNumberType.WorkPhone, AllowEmpty = true)]
        public string Telephone2
        {
            get => _telephone2;
            set => SetProperty(ref _telephone2, value, true);
        }
        [Number(ValidationNumberType.WorkPhone, AllowEmpty = true)]
        public string Telephone
        {
            get => _telephone;
            set => SetProperty(ref _telephone, value, true);
        }
        public Adress Adress => _adress;
        [Number(ValidationNumberType.PostCode)]
        public string PostCode
        {
            get => _postcode;
            set => SetProperty(ref _postcode, value, true);
        }
        [MaxLength(150, ErrorMessage = "превышен лимит символов")]
        public string ShortName
        {
            get => _shortname;
            set => SetProperty(ref _shortname,value, true);
        }
        [Required(ErrorMessage ="обязательное поле")][MaxLength(200, ErrorMessage = "превышен лимит символов")]
        public string Name
        {
            get => _name;
            set => SetProperty(ref _name,value, true);
        }
        public DateTime DBModifyDate => _last_modify_date;
        public string Requisite => (ShortName ?? Name) + Environment.NewLine + PostAddress;
        public string PostAddress => Adress.ToString() + ", " + PostCode;
        public double Completeness
        {
            get
            {
                int all = 8 + 4;
                double compleate = 0;
                if (!string.IsNullOrWhiteSpace(_website)) compleate++;
                if (!string.IsNullOrWhiteSpace(_email)) compleate++;
                if (!string.IsNullOrWhiteSpace(_fax)) compleate++;
                if (!string.IsNullOrWhiteSpace(_telephone)) compleate++;
                if (!string.IsNullOrWhiteSpace(_telephone2)) compleate++;
                if (!string.IsNullOrWhiteSpace(_postcode)) compleate++;
                if (!string.IsNullOrWhiteSpace(_shortname)) compleate++;
                if (!string.IsNullOrWhiteSpace(_name)) compleate++;
                if (Adress?.Settlement != null) compleate++;
                if (!string.IsNullOrWhiteSpace(Adress?.Street)) compleate++;
                if (!string.IsNullOrWhiteSpace(Adress?.StreetPrefix)) compleate++;
                if (!string.IsNullOrWhiteSpace(Adress?.Housing)) compleate++;
                return Math.Round(100 * compleate / all);
            }
        }
        public static Organization New => new Organization() { Version = Version.New, IsValid = true };
        #endregion Properties

        protected Organization() : base()
        {
            _adress.PropertyChanged += AdressChanged;
        }
        public Organization(int id, string name, string shortname, string postcode, Adress adress, string telephone, string telephone2, string fax,
                                string email, string website, bool status, Version vr, DateTime updatedate) : base(id, vr)
        {
            _name = name;
            _shortname = shortname;
            _postcode = postcode;
            if (adress != null)
            {
                adress.Copy(_adress);
            }
            _adress.PropertyChanged += AdressChanged;
            _telephone = telephone;
            _telephone2 = telephone2;
            _fax = fax;
            _email = email;
            _website = website;
            _status = status;
            _last_modify_date = updatedate;
        }
        public override string ToString() => ShortName ?? Name;
        private void AdressChanged(object sender, PropertyChangedEventArgs e)
        {
            OnPropertyChanged(e.PropertyName);
        }
       
        public Organization Clone()
        {
            return new Organization(ID, _name, _shortname, _postcode, _adress, _telephone, _telephone2, _fax, _email, _website, _status,
                                    this.Version, _last_modify_date);
        }
        object ICloneable.Clone() => Clone();
    }
}
