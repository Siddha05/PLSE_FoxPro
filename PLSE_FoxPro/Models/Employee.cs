using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
public enum UploadResult
{
    UnPerform = 0,
    Performing,
    Sucsess,
    Error
}
namespace PLSE_FoxPro.Models
{
    public class Employee : Person, ICloneable
    {
        #region Fields
        private string _inneroffice;
        private Departament _departament;
        private string _employeeStaus;
        private PermissionProfile _profile;
        private string _password;
        public Byte[] _foto;
        private UploadResult _upload;
        private Employee_SlightPart _slightpart;
        #endregion

        #region Properties
        [Required(ErrorMessage = "обязательное поле")]
        public string Inneroffice
        {
            get => _inneroffice;
            set => SetProperty(ref _inneroffice, value, true);
        }
        public UploadResult UploadStatus
        {
            get => _upload;
            set => SetProperty(ref _upload, value);
        }
        [Required(ErrorMessage = "обязательное поле")]
        public Departament Departament
        {
            get => _departament;
            set => SetProperty(ref _departament, value, true);
        }
        [Required(ErrorMessage = "обязательное поле")]
        public string EmployeeStatus
        {
            get => _employeeStaus;
            set => SetProperty(ref _employeeStaus, value, true);
        }
        [Required(ErrorMessage = "обязательное поле")]
        public PermissionProfile Profile
        {
            get => _profile;
            set => SetProperty(ref _profile, value, true);
        }
        public string Password
        {
            get => _password;
            set => SetProperty(ref _password, value);
        }
        public Byte[] Foto
        {
            get => _foto;
            set => SetProperty(ref _foto, value);
        }
        public Employee_SlightPart Employee_SlightPart
        {
            get => System.Threading.LazyInitializer.EnsureInitialized(ref _slightpart, () => FetchSlight() );
            set
            {
                SetProperty(ref _slightpart, value);
                _slightpart.PropertyChanged += _slightpart_PropertyChanged;
            }
        }
        public static Employee New => new Employee() { Version = Version.New };
        #endregion

        #region Metods
        public bool IsOperate()
        {
            return Inneroffice == "начальник" || Inneroffice == "заместитель начальника" ||
                Inneroffice == "государственный судебный эксперт" || Inneroffice == "старший государственный судебный эксперт"
                || Inneroffice == "ведущий государственный судебный эксперт" || Inneroffice == "начальник отдела";
        }
        public int? FullAge()
        {
            if (Employee_SlightPart?.Birthdate == null) return null;
            return (int)Age();
        }
        public bool? IsBirthDate()
        {
            if (Employee_SlightPart?.Birthdate == null) return null;
            return DateTime.Today.Day == Employee_SlightPart.Birthdate.Value.Day && DateTime.Today.Month == Employee_SlightPart.Birthdate.Value.Month;
        }
        public double? Age()
        {
            if (Employee_SlightPart?.Birthdate == null) return null;
            return (DateTime.Today - Employee_SlightPart.Birthdate.Value.Date).Days / 365.25;
        }
        public Employee Clone()
        {
            return new Employee(id: ID, departament: _departament, office: _inneroffice, firstname: _fname, middlename: _mname, secondname: _sname, gender: _gender,
                                declinated: _declinated, emplstatus: _employeeStaus, profile: _profile, password: _password, uploadstatus: _upload,
                                vr: Version, updatedate: DBModifyDate, slightPart: _slightpart.Clone(), foto: _foto);
        }
        object ICloneable.Clone() => Clone();
        #endregion

        private Employee() : base() { }
        public Employee(int id, Departament departament, string office, string firstname, string middlename, string secondname, bool? gender, string emplstatus,
                         PermissionProfile profile, string password, bool declinated, byte[] foto, Version vr, DateTime updatedate)
            : base(id: id, firstname: firstname, middlename: middlename, secondname: secondname, gender: gender, declinated: declinated, vr: vr, updatedate: updatedate)
        {
            _profile = profile;
            _employeeStaus = emplstatus;
            _password = password;
            _departament = departament;
            _inneroffice = office;
            _foto = foto;
            _last_modify_date = updatedate;
            
        }
        private Employee(int id, Departament departament, string office, string firstname, string middlename, string secondname, bool? gender, bool declinated,
                         string emplstatus, PermissionProfile profile, string password, byte[] foto, Employee_SlightPart slightPart, UploadResult uploadstatus,
                         Version vr, DateTime updatedate)
                    : this(id: id, firstname: firstname, middlename: middlename, secondname: secondname, gender: gender, declinated: declinated, vr: vr, updatedate: updatedate,
                            departament: departament, office: office, emplstatus: emplstatus, profile: profile, password: password, foto: foto)
        {

            _slightpart = slightPart;
            _upload = uploadstatus;
        }
        private void _slightpart_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            OnPropertyChanged(e.PropertyName);
        }
        private Employee_SlightPart FetchSlight()
        {
            UploadStatus = UploadResult.Performing;
            Employee_SlightPart res = null;
            try
            {
                //res = App.PLSE_Storage.EmployeeAccessService.DownloadEmployeeSligth(this);
                res = new Employee_SlightPart(null, null, null, null, null, null, null, null, null, null, false);
                UploadStatus = UploadResult.Sucsess;
            }
            catch (Exception)
            {
                UploadStatus = UploadResult.Error;
            }
            return res;
        }
        
    }
    public class Employee_SlightPart : VersionBase, ICloneable
    {
        #region Fields
        private DateTime? _birthdate;
        private DateTime? _hiredate;
        private string _education1;
        private string _education2;
        private string _education3;
        private string _sciencedegree;
        private string _mobilephone;
        private string _workphone;
        private string _email;
        private Adress _adress = new Adress();
        private bool _is_hide_personal;
        #endregion Fields

        #region Properties
        public Adress Adress => _adress;
        [MaxLength(250, ErrorMessage = "превышен лимит символов")]
        public string Education1
        {
            get => _education1;
            set => SetProperty(ref _education1, value, true);
        }
        [MaxLength(250, ErrorMessage = "превышен лимит символов")]
        public string Education2
        {
            get => _education2;
            set => SetProperty(ref _education2, value, true);
        }
        [MaxLength(250, ErrorMessage = "превышен лимит символов")]
        public string Education3
        {
            get => _education3;
            set => SetProperty(ref _education3, value, true);
        }
        [MaxLength(250, ErrorMessage = "превышен лимит символов")]
        public string Sciencedegree
        {
            get => _sciencedegree;
            set => SetProperty(ref _sciencedegree, value, true);
        }
        [Number(ValidationNumberType.MobilePhone, AllowEmpty = false)]
        public string Mobilephone
        {
            get => _mobilephone;
            set => SetProperty(ref _mobilephone, value, true);
        }
        [RegularExpression(@"\A[^@]+@([^@\.]+\.)+[^@\.]+\z", ErrorMessage = "неверный формат")]
        public string Email
        {
            get => _email;
            set => SetProperty(ref _email, value, true);
        }
        [Number(ValidationNumberType.WorkPhone)]
        public string Workphone
        {
            get => _workphone;
            set => SetProperty(ref _workphone, value, true);
        }
        public DateTime? Birthdate
        {
            get => _birthdate;
            set => SetProperty(ref _birthdate, value);
        }
        public DateTime? Hiredate
        {
            get => _hiredate;
            set => SetProperty(ref _hiredate, value);
        }
        public bool IsHidePersonal
        {
            get => _is_hide_personal;
            set => SetProperty(ref _is_hide_personal, value);
        }
        #endregion

        public Employee_SlightPart(DateTime? hiredate, DateTime? birthdate, string mobilephone, string workphone, string email, Adress adress,
                                  string education1, string education2, string education3, string sciencedegree, bool hidepersonal)
        {
            _hiredate = hiredate;
            _birthdate = birthdate;
            _education1 = education1;
            _education2 = education2;
            _education3 = education3;
            _sciencedegree = sciencedegree;
            _mobilephone = mobilephone;
            _workphone = workphone;
            _email = email;
            _is_hide_personal = hidepersonal;
            adress?.Copy(_adress);
            _adress.PropertyChanged += _adress_PropertyChanged;
        }
        private void _adress_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            OnPropertyChanged(e.PropertyName);
        }
        object ICloneable.Clone() => Clone();
        public Employee_SlightPart Clone()
        {
            return new Employee_SlightPart(
                        education1: _education1,
                        education2: _education2,
                        education3: _education3,
                        sciencedegree: _sciencedegree,
                        adress: _adress,
                        mobilephone: _mobilephone,
                        workphone: _workphone,
                        email: _email,
                        hiredate: _hiredate,
                        birthdate: _birthdate,
                        hidepersonal: _is_hide_personal);
        }
    }
}
