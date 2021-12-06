using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace PLSE_FoxPro.Models
{
    public sealed class Settlement : VersionBase, ICloneable
    {
        #region Fields
        private string _title;
        private string _settlementtype;
        private string _significance;
        private string _telephonecode;
        private string _postcode;
        private string _federallocation;
        private string _territorylocation;
        private bool _isvalid;
        private DateTime _last_modify_date;
        #endregion Fields
        #region Properties
        public bool IsValid
        {
            get { return _isvalid; }
            set => SetProperty(ref _isvalid, value);
        }
        [Required][MaxLength(40)]
        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value, true);
        }
        /// <summary>
        /// Тип населенного пункта, например город, село, пгт и т.п.
        /// </summary>
        [Required][MaxLength(20)]
        public string SettlementType
        {
            get => _settlementtype;
            set => SetProperty(ref _settlementtype, value, true);
        }
        /// <summary>
        /// Значимость населенного пункта, т.е. тип административной единицы РФ<para>
        /// Например областной, федеральный и т.п.</para>
        /// </summary>
        [Required][MaxLength(15)]
        public string Significance
        {
            get => _significance;
            set => SetProperty(ref _significance, value, true);
        }
        [RegularExpression(@"^[348][0-9]{2,4}$", ErrorMessage ="неверный формат")]
        public string Telephonecode
        {
            get => _telephonecode;
            set => SetProperty(ref _telephonecode, value, true);
        }
        [RegularExpression(@"^[1-6][0-9]{5}$")] //TODO: regular expression for complex postcode like 233432-233501
        public string Postcode
        {
            get => _postcode;
            set => SetProperty(ref _postcode, value, true);
        }
        [MaxLength(50)]
        public string Federallocation
        {
            get => _federallocation;
            set => SetProperty(ref _federallocation, value, true);
        }
        [MaxLength(50)]
        public string Territorylocation
        {
            get => _territorylocation;
            set => SetProperty(ref _territorylocation, value, true);
        }
        /// <summary>
        /// Дата последнего изменения в базе данных
        /// </summary>
        public DateTime DBModifyDate => _last_modify_date;
        public static Settlement New => new Settlement() { Version = Version.New, IsValid = true };
        #endregion Properties

        private Settlement() : base() { }
        public Settlement(int id, string title, string type, string significance, string telephonecode, string postcode, string federallocation,
                            string territoriallocation, bool isvalid, Version vr, DateTime updatedate) : base(id, vr)
        {
            _title = title;
            _settlementtype = type;
            _significance = significance;
            _telephonecode = telephonecode;
            _postcode = postcode;
            _federallocation = federallocation;
            _territorylocation = territoriallocation;
            _isvalid = isvalid;
            _last_modify_date = updatedate;
        }

        #region Methods
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(200);
            if (Significance == "федеральный") sb.Append(Title);
            else
            {
                if (Significance == "нет")
                {
                    sb.Append(SettlementType);
                    sb.Append(" ");
                    sb.Append(Title);
                    if (Territorylocation != null)
                    {
                        sb.Append(", ");
                        sb.Append(Territorylocation.ToString());
                    }
                    if (Federallocation != null)
                    {
                        sb.Append(", ");
                        sb.Append(Federallocation.ToString());
                    }
                }
                else
                {
                    if (Significance == "районный")
                    {
                        sb.Append(SettlementType);
                        sb.Append(" ");
                        sb.Append(Title);
                        if (Federallocation != null)
                        {
                            sb.Append(", ");
                            sb.Append(Federallocation.ToString());
                        }
                    }
                    else
                    {
                        sb.Append(SettlementType);
                        sb.Append(" ");
                        sb.Append(Title);
                    }
                }
            }
            return sb.ToString();
        } 
        object ICloneable.Clone() => Clone();
        public Settlement Clone() => new Settlement(ID, _title, _settlementtype, _significance, _telephonecode, _postcode, _federallocation,
                                                    _territorylocation, _isvalid, this.Version, this._last_modify_date);
        #endregion
    }
}
