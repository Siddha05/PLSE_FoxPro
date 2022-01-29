using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace PLSE_FoxPro.Models
{
    public enum ExpertiseTypes : byte
    {
        Unknown,
        [Description("первичная")]
        Primary = 1,
        [Description("дополнительная")]
        Additional = 2,
        [Description("повторная")]
        Repeated = 4,
    }
    public enum ExpertiseResults : byte
    {
        Unknown,
        [Description("заключение эксперта")]
        Conclusion = 1,
        [Description("сондз")]
        SONDZ = 2,
    }
    public sealed class Expertise : VersionBase, ICloneable
    {
        #region Fields
        private string _number;
        private Expert _expert;
        private Resolution _resolution;
        private string _result;
        private DateTime _startdate;
        private DateTime? _enddate;
        private byte _timelimit;
        private string _type;
        private int? _prevexp;
        private short? _spendhours;
        private short? _nobj;
        private short? _ncat;
        private short? _nver;
        private short? _nalt;
        private short? _nnmet;
        private short? _nnmat;
        private short? _nncom;
        private short? _nnother;
        private string _comment;
        private int? _eval;
        private ObservableCollection<ExpertiseMovement> _movements = new ObservableCollection<ExpertiseMovement>();
        private ObservableCollection<Bill> _bills = new ObservableCollection<Bill>();
        private ObservableCollection<EquipmentUsage> _equipmentusage = new ObservableCollection<EquipmentUsage>();
        private UploadResult _upload;
        #endregion

        #region Properties        
        public short? SpendHours
        {
            get => _spendhours;
            set => SetProperty(ref _spendhours, value);
        }
        public int? PreviousExpertise
        {
            get => _prevexp;
            set => SetProperty(ref _prevexp, value);
        }
        //[Required(ErrorMessage = "обязательное поле")]
        public string ExpertiseType
        {
            get => _type;
            set => SetProperty(ref _type, value);
        }
        [Required(ErrorMessage ="обязательное поле")]
        public Resolution FromResolution
        {
            get => _resolution;
            set => SetProperty(ref _resolution,value,true);
        }
        [Range(1,30,ErrorMessage ="Срок должен быть в пределах 1-30")]
        public byte TimeLimit
        {
            get => _timelimit;
            set => SetProperty(ref _timelimit, value, true);
        }
        public DateTime? EndDate
        {
            get => _enddate;
            set => SetProperty(ref _enddate, value, true);
        }
        public DateTime StartDate
        {
            get => _startdate;
            set => SetProperty(ref _startdate, value, true);
        }
        public string ExpertiseResult
        {
            get => _result;
            set => SetProperty(ref _result, value, true);
        }
        [Required(ErrorMessage = "обязательное поле")]
        public Expert Expert
        {
            get => _expert;
            set => SetProperty(ref _expert, value, true);
        }
        [Required(ErrorMessage = "обязательное поле")][Number(ValidationNumberType.Expertise)]
        public string Number
        {
            get => _number;
            set => SetProperty(ref _number, value, true);
        }
        public short? ObjectsCount
        {
            get => _nobj;
            set => SetProperty(ref _nobj, value, true);
        }
        public short? CategoricalAnswers
        {
            get => _ncat;
            set => SetProperty(ref _ncat, value, true);
        }
        public short? ProbabilityAnswers
        {
            get => _nver;
            set => SetProperty(ref _nver, value, true);
        }
        public short? AlternativeAnswers
        {
            get => _nalt;
            set => SetProperty(ref _nalt, value, true);
        }
        public short? NPV_MetodAnswers
        {
            get => _nnmet;
            set => SetProperty(ref _nnmet, value, true);
        }
        public short? NPV_MaterialAnswers
        {
            get => _nnmat;
            set => SetProperty(ref _nnmat, value, true);
        }
        public short? NPV_CompAnswers
        {
            get => _nncom;
            set => SetProperty(ref _nncom, value, true);
        }
        public short? NPV_OtherAnswers
        {
            get => _nnother;
            set => SetProperty(ref _nnother, value, true);
        }
        [MaxLength(500,ErrorMessage ="превышен лимит символов")]
        public string Comment
        {
            get => _comment;
            set => SetProperty(ref _comment, value, true);
        }
        [Range(1,10, ErrorMessage = "Оценка должна быть в диапазоне 1-10")]
        public int? Evaluation
        {
            get => _eval;
            set => SetProperty(ref _eval, value, true);
        }
        public int TotalAnswers => (CategoricalAnswers ?? 0) + (ProbabilityAnswers ?? 0) + (AlternativeAnswers ?? 0) + (NPV_MetodAnswers ?? 0) +
                                    (NPV_MaterialAnswers ?? 0) + (NPV_CompAnswers ?? 0) + (NPV_OtherAnswers ?? 0);
        public UploadResult UploadStatus
        {
            get => _upload;
            set => SetProperty(ref _upload, value);
        }
        public int? Remain2 => DaysLeft();
        public string Focus
        {
            get
            {
                if (Remain2.HasValue)
                {
                    var sr = Remain2.Value;
                    if (sr >= 5) return "В норме";
                    if (sr > 0) return "Требующие внимания";
                    return "Просроченные";
                }
                else return "Выполненные";
            }
        }
        public string DifficultCategory
        {
            get
            {
                if (SpendHours.HasValue)
                {
                    if (SpendHours.Value > Expert.Speciality.Category_3) return "3+";
                    if (SpendHours.Value <= Expert.Speciality.Category_3 && SpendHours.Value > Expert.Speciality.Category_2) return "3";
                    if (SpendHours.Value <= Expert.Speciality.Category_2 && SpendHours.Value > Expert.Speciality.Category_1) return "2";
                    return "1";
                }
                else return null;
            }
        }
        public int Inwork => EndDate == null ? (DateTime.Now - StartDate).Days : (EndDate.Value - StartDate).Days;
        public int? LinkedExpertisesCount
        {
            get
            {
                if ((FromResolution?.Expertisies.Count ?? 0) > 1)
                {
                    return FromResolution.Expertisies.Count - 1;
                }
                else return null;
            }
        }
        public ObservableCollection<ExpertiseMovement> Movements => _movements;
        public ObservableCollection<Bill> Bills => _bills;
        public ObservableCollection<EquipmentUsage> EquipmentUsages => _equipmentusage;
        public static Expertise New => new Expertise() { _startdate = DateTime.Now, _timelimit = 30};
        #endregion

        #region Function
        public override string ToString() => $"{Number} ({Expert.Speciality.Code} {Expert.Employee})";
        private IEnumerable<T> GetMovements<T>() where T : ExpertiseMovement // TODO: устранить?
        {
            return _movements.Where(n => n.GetType() == typeof(T)).Cast<T>();
        }
        private IEnumerable<Response> GetReferedResponse(Request movement)
        {
            if (movement == null) return Enumerable.Empty<Response>();
            return GetMovements<Response>().Where(n => n.ReferTo == movement.ID);
        }
        public IEnumerable<T> GetOrderedMovements<T>() where T: ExpertiseMovement
        {
            return GetMovements<T>().OrderBy(n => n.RegistrationDate);
        }
        /// <summary>
        /// экспертиза на ходатайстве?
        /// </summary>
        /// <returns></returns>
        public bool IsOnRequest()
        {
            var sm = GetOrderedMovements<Request>().Where(n => n.SuspendExecution).LastOrDefault(n => n.RegistrationDate <= DateTime.Now);
            if (sm != null)
            {
                var usm = GetReferedResponse(sm).FirstOrDefault(n => n.ResumeExecution);
                if (usm != null)
                {
                    if (usm.RegistrationDate > DateTime.Now) return true;
                }
            }
            return false;
        }
        /// <summary>
        /// Количество дней на запросе до даты <paramref name="end"/> включительно
        /// </summary>
        /// <returns>Количесво дней на запросе. -1 если <paramref name="end"/> ранее даты начала експертизы</returns>
        /// <exception cref="ArgumentException"/>
        public int DaysOnRequest(DateTime end)
        {
            if (end < _startdate) return -1;
            int result = 0;
            for (DateTime start = StartDate; ;)
            {
                var sm = GetOrderedMovements<Request>().FirstOrDefault(n => n.RegistrationDate >= start && n.SuspendExecution);
                if (sm != null)
                {
                    var usm = GetReferedResponse(sm).FirstOrDefault(n => n.ResumeExecution);
                    if (usm != null && usm.RegistrationDate < end)
                    {
                        start = usm.RegistrationDate;
                        result += (usm.RegistrationDate - sm.RegistrationDate).Days;
                    }
                    else
                    {
                        result += (end - sm.RegistrationDate).Days;
                        goto end;
                    }
                }
                else
                {
                    goto end;
                }
            }
        end:
            return result;
        }
        /// <summary>
        /// Количество дней в работе, когда экспертиза не была приостановлена до даты <paramref name="end"/> включительно
        /// </summary>
        /// <returns>Количесво дней в работе. -1 если <paramref name="end"/> ранее даты начала експертизы</returns>
        /// <exception cref="ArgumentException"/>
        public int DaysInWork(DateTime end)
        {
            if (end < _startdate) return -1;
            return (end - _startdate).Days - DaysOnRequest(end);
        }
        /// <summary>
        /// Количество дней до истечения срока на текущую дату
        /// </summary>
        /// <returns>Количество дней до окончания срока экспертизы или null если экспертиза сдана</returns>
        public int? DaysLeft()
        {
            if (_enddate.HasValue) return null;
            else
            {
                var report = GetOrderedMovements<Report>().LastOrDefault();
                if (report != null)
                {
                    return (report.DelayDate - DateTime.UtcNow).Days;
                }
                else
                {
                    return _timelimit - DaysInWork(DateTime.UtcNow);
                }
            }
        }
        /// <summary>
        /// Крайняя дата сдачи экспертизы
        /// </summary>
        /// <returns>Крайняя дата или null если экспертиза сдана</returns>
        public DateTime? DeadLine()
        {
            var d = DaysLeft();
            if (d.HasValue) return DateTime.UtcNow.AddDays(d.Value);
            return null;
        }
        object ICloneable.Clone() => Clone();
        public Expertise Clone() //TODO: deep copy of _movements, _bills, _equipmentusage
        {
            return new Expertise(ID, _number, _expert, _resolution, _result, _startdate, _enddate, _timelimit, _type, _prevexp, _spendhours, _nobj,
                                _ncat, _nver, _nalt, _nnmet, _nnmat, _nncom, _nnother, _comment, _eval, _movements, _bills, _equipmentusage, Version);
        }
        /// <summary>
        /// Возвращает дату в промежутке времени пребывания экспертизы в работе
        /// </summary>
        /// <returns></returns>
        public DateTime FitInWorkdays()
        {
            int period;
            if (EndDate.HasValue)
            {
                period = (_enddate.Value - _startdate).Days;
            }
            else period = (DateTime.Now - _startdate).Days;
            Random r = new Random();
            return _startdate.AddDays(r.Next(0, period));
        }
        #endregion

        private Expertise() : base() { }
        public Expertise(int id, string number, Expert expert, string result, DateTime start, DateTime? end, byte timelimit, string type, int? previous,
                        short? spendhours, Version vr) : base(id, vr)
        {
            _number = number;
            _expert = expert;
            _result = result;
            _startdate = start;
            _enddate = end;
            _timelimit = timelimit;
            _type = type;
            _prevexp = previous;
            _spendhours = spendhours;
        }
        private Expertise(int id, string number, Expert expert, Resolution resolution, string result, DateTime start, DateTime? end, byte timelimit, string type, int? previous,
                        short? spendhours, short? nobjects, short? ncatanswers, short? nprobanswers, short? naltanswers, short? nnpvmetodanswers, short? nnpvmeterialanswers, short? nnpvcompanswers,
                        short? nnpvotheranswers, string comment, int? eval, IEnumerable<ExpertiseMovement> movements, IEnumerable<Bill> bills,
                        IEnumerable<EquipmentUsage> equipmentusages, Version vr) : base(id, vr)
        {
            _number = number;
            _expert = expert;
            _resolution = resolution;
            _result = result;
            _startdate = start;
            _enddate = end;
            _timelimit = timelimit;
            _type = type;
            _prevexp = previous;
            _spendhours = spendhours;
            _nobj = nobjects;
            _ncat = ncatanswers;
            _nver = nprobanswers;
            _nalt = naltanswers;
            _nnmet = nnpvmetodanswers;
            _nnmat = nnpvmeterialanswers;
            _nncom = nnpvcompanswers;
            _nnother = nnpvotheranswers;
            _comment = comment;
            _eval = eval;
            foreach (var item in movements)
            {
                _movements.Add(item);
            }
            foreach (var item in equipmentusages)
            {
                _equipmentusage.Add(item);
            }
            foreach (var item in bills)
            {
                _bills.Add(item);
            }
        }

    }
}
