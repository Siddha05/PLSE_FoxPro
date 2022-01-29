using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;

namespace PLSE_FoxPro.Models
{
    public enum ResolutionTypes : byte
    {
        Unknown,
        [Description("постановление")]
        Resolution = 1,
        [Description("определение")]
        Definition = 2,
        [Description("отношение")]
        Relationship = 4,
        [Description("договор")]
        Contract = 8,
    }
    public struct CaseType : IEquatable<CaseType>
    {
        private readonly string _code;
        private readonly string _type;
        public string Code => _code;
        public string Type => _type;

        public CaseType(string code, string type)
        {
            _code = code;
            _type = type;
        }
        public override string ToString() => _type;
        //public override bool Equals(object obj) => Equals((CaseType)obj);
        public override int GetHashCode() => _code.GetHashCode();
        
        public bool Equals([AllowNull] CaseType other) => Type == other.Type && Code == other.Code;
        
    }
    
    public sealed class Resolution : VersionBase
    {
        #region Fields
        private DateTime _regdate;
        private DateTime? _resdate;
        private string _restype;
        private Customer _customer;
        private readonly NumberingObservableCollection _objects = new NumberingObservableCollection();
        private string _prescribetype;
        private readonly NumberingObservableCollection _quest = new NumberingObservableCollection();
        private bool _nativenumeration;
        private string _status;
        private readonly ObservableCollection<Expertise> _expertisies = new ObservableCollection<Expertise>();
        private string _casenumber;
        private string _respondent;
        private string _plaintiff;
        private CaseType _typecase;
        private string _annotate;
        private string _comment;
        private string _uid;
        #endregion

        #region Property
        public string ResolutionStatus
        {
            get => _status;
            private set => SetProperty(ref _status, value, true);
        }
        public NumberingObservableCollection Questions => _quest;
        /// <summary>
        /// Нумерация вопросов согласно постановления
        /// </summary>
        public bool NativeQuestionNumeration
        {
            get => _nativenumeration;
            set => SetProperty(ref _nativenumeration, value);
        }
        /// <summary>
        /// Список предоставленных объектов
        /// </summary>
        public NumberingObservableCollection Objects => _objects;
        /// <summary>
        /// Вид экспертизы, назначенной в постановлении
        /// </summary>
        [MaxLength(200, ErrorMessage = "превышен лимит символов")]
        public string PrescribeType
        {
            get => _prescribetype;
            set => SetProperty(ref _prescribetype, value, true);
        }
        [Required(ErrorMessage = "обязательное поле")]
        public Customer Customer
        {
            get => _customer;
            set => SetProperty(ref _customer, value, true);
        }
        [Required(ErrorMessage = "обязательное поле")]
        public string ResolutionType
        {
            get => _restype;
            set => SetProperty(ref _restype, value, true);
        }
        /// <summary>
        /// Дата вынесения постановления
        /// </summary>
        public DateTime? ResolutionDate
        {
            get => _resdate;
            set => SetProperty(ref _resdate, value, true);
        }
        /// <summary>
        /// Дата регистрации постановления
        /// </summary>
        [GraterOrEqualDate(PropertyName = nameof(ResolutionDate))]
        public DateTime RegistrationDate
        {
            get => _regdate;
            set => SetProperty(ref _regdate, value, true);
        }
        public ObservableCollection<Expertise> Expertisies => _expertisies;
        /// <summary>
        /// Номера всех экспертиз, перечисленных через запятую
        /// </summary>
        /// /// <example>213, 214</example>
        public string OverallExpertisesNumbers
        {
            get
            {
                StringBuilder sb = new StringBuilder(21);
                foreach (var item in _expertisies)
                {
                    sb.Append($", {item.Number}");
                }
                return sb.Length > 2 ? sb.Remove(0, 2).ToString() : null;
            }
        }
        [MaxLength(500, ErrorMessage = "превышен лимит символов")]
        public string Comment
        {
            get => _comment;
            set => SetProperty(ref _comment, value, true);
        }
        public string CaseAnnotate
        {
            get => _annotate;
            set => SetProperty(ref _annotate, value);
        }
        public CaseType TypeCase
        {
            get => _typecase;
            set => SetProperty(ref _typecase, value);
        }
        [CustomValidation(typeof(Resolution), nameof(ValidateSidePresence))]
        public string Plaintiff
        {
            get => _plaintiff;
            set => SetProperty(ref _plaintiff, value);
        }
        [CustomValidation(typeof(Resolution), nameof(ValidateSidePresence))]
        public string Respondent
        {
            get => _respondent;
            set => SetProperty(ref _respondent, value, true);
        }
        public string CaseNumber
        {
            get => _casenumber;
            set => SetProperty(ref _casenumber, value, true);
        }
        public string UidCase
        {
            get => _uid;
            set => SetProperty(ref _uid, value, true);
        }
        public string Essense => AnnotateBuilder();
      
        #endregion

        private Resolution() : base()
        {
            //_quest.CollectionChanged += _quest_CollectionChanged;
            //_objects.CollectionChanged += _quest_CollectionChanged;
            _expertisies.CollectionChanged += ExpertiseListChanged;
        }
        public static Resolution New => new Resolution() { RegistrationDate = DateTime.Now, ResolutionStatus = "рассмотрение" };
        public Resolution(int id, DateTime registrationdate, DateTime? resolutiondate, string resolutiontype, Customer customer, string obj, string prescribe,
                            string quest, bool nativenumeration, string status, string casenumber, string respondent, string plaintiff, string uidcase,
                            CaseType typecase, string annotate, string comment, Version vr) : base(id, vr)
        {
            _regdate = registrationdate;
            _resdate = resolutiondate;
            _restype = resolutiontype;
            _customer = customer;
            ResolutionHelper.UnwrapDBStringToCollection(obj, _objects);
            ResolutionHelper.UnwrapDBStringToCollection(quest, _quest);
            _prescribetype = prescribe;
            _nativenumeration = nativenumeration;
            _status = status;
            _uid = uidcase;
            _casenumber = casenumber;
            _respondent = respondent;
            _plaintiff = plaintiff;
            _typecase = typecase;
            _annotate = annotate;
            _comment = comment;
            _expertisies.CollectionChanged += ExpertiseListChanged;
            //_quest.CollectionChanged += _quest_CollectionChanged;
            //_objects.CollectionChanged += _quest_CollectionChanged;
        }
        #region Methods
        //private void _quest_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        //{
        //    switch (e.Action)
        //    {
        //        case NotifyCollectionChangedAction.Add:
        //        case NotifyCollectionChangedAction.Remove:
        //        case NotifyCollectionChangedAction.Replace:
        //        case NotifyCollectionChangedAction.Move:
        //            var c = sender as ObservableCollection<NumerableContentWrapper>;
        //            for (int i = 0; i < c.Count; i++)
        //            {
        //                c[i].Number = i + 1;
        //            }
        //            break;
        //        default:
        //            break;
        //    }
        //}
        private void ExpertiseStatusChanged(object o, PropertyChangedEventArgs e)
        {
            var ex = o as Expertise;
            if (e.PropertyName == "ExpertiseStatus")
            {
                if (ex.EndDate == null) ResolutionStatus = "в работе";
                return;
            }
            ResolutionStatus = "выполнено";
        }
        private void ExpertiseListChanged(object o, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Replace:
                case NotifyCollectionChangedAction.Add:
                    foreach (Expertise item in e.NewItems)
                    {
                        item.FromResolution = this;
                        if (item.EndDate == null) ResolutionStatus = "в работе";
                        item.PropertyChanged += ExpertiseStatusChanged;
                    }
                    break;
                case NotifyCollectionChangedAction.Reset:
                case NotifyCollectionChangedAction.Remove:
                    foreach (Expertise item in e.OldItems)
                    {
                        item.PropertyChanged -= ExpertiseStatusChanged;
                    }
                    if (_expertisies.Count > 0)
                    {
                        foreach (var item in _expertisies)
                        {
                            if (item.EndDate == null) ResolutionStatus = "в работе";
                            return;
                        }
                    }
                    else ResolutionStatus = "рассмотрение";
                    break;
            }
        }
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder("<Resolution> ID is ");
            sb.AppendLine(ID.ToString());
            sb.AppendLine($"Status: {ResolutionStatus}");
            sb.Append("Type: ");
            sb.AppendLine(ResolutionType.ToString());
            sb.Append("PrescribeType: ");
            sb.AppendLine(PrescribeType);
            sb.Append("RegDate: ");
            sb.AppendLine(RegistrationDate.ToString("d"));
            sb.Append("ResDate: ");
            sb.AppendLine(ResolutionDate?.ToString("d"));
            sb.Append("Customer: ");
            sb.AppendLine(Customer?.ToString());
            sb.AppendLine("----------------------------");
            sb.AppendLine("Questions: ");
            foreach (var item in Questions)
            {
                sb.Append("\t");
                sb.AppendLine(item.Content);
            }
            sb.AppendLine("---------------------------");
            sb.AppendLine("Expertisies: ");
            sb.Append(Expertisies.Count);
            return sb.ToString();
        }
        public string AnnotateBuilder()
        {
            StringBuilder builder = new StringBuilder(500);
            builder.Append("по материалам ");
            builder.Append(_typecase.Type);//TODO: add declination
            if (!string.IsNullOrEmpty(_casenumber))
            {
                builder.Append(" дела №  ");
                builder.Append(_casenumber);
            }
            if (!string.IsNullOrEmpty(_respondent) && !string.IsNullOrEmpty(_plaintiff))
            {
                builder.Append(" по иску ");
                builder.Append(_plaintiff);//TODO: add declination
                builder.Append(" к ");
                builder.Append(_respondent);
            }
            return builder.ToString();

        }
        /// <summary>
        /// Номера всех экспертиз с кодом отдела и кодом дела, перечисленных через запятую
        /// </summary>
        /// <example>213_2-3, 214_2-2</example>
        public string FullOverallExpertisesNumbers(string placeholder = @"\")
        {
            string s = "";
            foreach (var item in _expertisies)
            {
                s += $", {item.Number}{placeholder}{item.Expert.Employee.Departament.DigitalCode}-{TypeCase.Code}";
            }
            return s.Length > 2 ? s.Remove(0, 2).ToString() : null;
        }
       
        /// <summary>
        /// Необходимы ли стороны по делу
        /// </summary>
        /// <param name="side"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public static ValidationResult ValidateSidePresence(string side, ValidationContext context)//TODO: implement logic
        {
            return ValidationResult.Success;
        }
        #endregion
    }
}
