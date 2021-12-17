using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PLSE_FoxPro.Models
{
    public sealed class EquipmentUsage : VersionBase
    {
        #region Fields
        private DateTime? _usagedate;
        private byte _duration = 1;
        private Equipment _equip;
        #endregion
        #region Properties
        [Required(ErrorMessage = "обязательное поле")]
        public Equipment UsedEquipment
        {
            get => _equip;
            set => SetProperty(ref _equip, value, true);
        }
        [Required(ErrorMessage = "обязательное поле")][Range(1,8, ErrorMessage = "Длительность должна быть в интервале 1-8")]
        public byte Duration
        {
            get { return _duration; }
            set => SetProperty(ref _duration, value, true);
        }
        [Required(ErrorMessage = "обязательное поле")]
        public DateTime? UsageDate
        {
            get => _usagedate; 
            set => SetProperty(ref _usagedate, value, true);
        }
        #endregion

        public override string ToString()
        {
            return $"{UsedEquipment?.EquipmentName ?? "<null>"}\t {Duration}\t {UsageDate:d}";
        }
        /// <summary>
        /// Автоматическая установка полей Duration, Usagedate
        /// </summary>
        public void AutoSetParameters(Expertise expertise)
        {
            if (expertise == null) return;
            Random random = new Random();
            Duration = (byte)random.Next(1, 3);
            UsageDate = expertise.FitInWorkdays();
        }
        public static EquipmentUsage New => new EquipmentUsage() { Version = Version.New};
        private EquipmentUsage() { }
        public EquipmentUsage(int id, DateTime usagedate, byte duration, Equipment equipment, Version version) : base(id, version)
        {
            _usagedate = usagedate;
            _duration = duration;
            _equip = equipment;
            Version = version;
        }     
    }
}
