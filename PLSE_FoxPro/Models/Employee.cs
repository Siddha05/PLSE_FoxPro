using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace PLSE_FoxPro.Models
{
    class Employee : Person, ICloneable
    {
        #region Fields
        private string _inneroffice;
        private Departament _departament;
        private string _employeeStaus;
        private PermissionProfile _profile;
        private string _password;
        public Byte[] _foto;
        private UploadResult _upload;
        private Lazy<Employee_SlightPart> _slightpart;
        #endregion
        #region Properties
        public string Inneroffice
        {
            get => _inneroffice;
            set
            {
                if (_inneroffice == value) return;
                _inneroffice = value;
                OnPropertyChanged();
            }
        }
        public UploadResult UploadStatus
        {
            get => _upload;
            set
            {
                _upload = value;
                OnPropertyChanged(suppressverchanging: true);
            }
        }
        public Departament Departament
        {
            get => _departament;
            set
            {
                if (_departament == value) return;
                _departament = value;
                OnPropertyChanged("Departament");
            }
        }
        public string EmployeeStatus
        {
            get => _employeeStaus;
            set
            {
                if (_employeeStaus == value) return;
                _employeeStaus = value;
                OnPropertyChanged();
            }
        }
        public PermissionProfile Profile
        {
            get => _profile;
            set
            {
                if (value != _profile)
                {
                    _profile = value;
                    OnPropertyChanged();
                }
            }
        }
        public string Password
        {
            get => _password;
            set
            {
                if (value != _password)
                {
                    _password = value;
                    OnPropertyChanged(suppressverchanging: true);
                }
            }
        }
        public Byte[] Foto
        {
            get => _foto;
            set
            {
                _foto = value;
                OnPropertyChanged();
            }
        }
        public Employee_SlightPart Employee_SlightPart
        {
            get => _slightpart.Value;
            set
            {
                if (value != null)
                {
                    _slightpart = new Lazy<Employee_SlightPart>(() => value);
                    _slightpart.Value.PropertyChanged += _slightpart_PropertyChanged;
                    OnPropertyChanged();
                }
            }
        }
        //public BitmapImage Image
        //{
        //    get
        //    {
        //        BitmapImage image = new BitmapImage();
        //        if (_foto != null)
        //        {
        //            image.BeginInit();
        //            image.StreamSource = new MemoryStream(_foto);
        //            image.EndInit();
        //        }
        //        else
        //        {
        //            image.BeginInit();
        //            image.UriSource = new Uri(@"pack://application:,,,/Resources/Unknown.jpg");
        //            image.EndInit();
        //        }
        //        return image;
        //    }
        //}
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
            return new Employee(id: _id, departament: _departament, office: _inneroffice, firstname: _fname, middlename: _mname, secondname: _sname, gender: _gender,
                                declinated: _declinated, emplstatus: _employeeStaus, profile: _profile, password: _password, uploadstatus: _upload,
                                vr: Version, updatedate: ModificationDate, slightPart: _slightpart.Value.Clone(), foto: _foto);
        }
        object ICloneable.Clone() => Clone();
        #endregion

        private Employee() : base() { _slightpart = new Lazy<Employee_SlightPart>(() => FetchSlight()); }
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
            _slightpart = new Lazy<Employee_SlightPart>(() => FetchSlight());
        }
        private void _slightpart_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            OnPropertyChanged(e.PropertyName);
        }
        private Employee_SlightPart FetchSlight()
        {
            Employee_SlightPart res = null;
            try
            {
                res = App.PLSE_Storage.EmployeeAccessService.DownloadEmployeeSligth(this);
            }
            catch (Exception)
            {
            }
            return res;
        }
        private Employee(int id, Departament departament, string office, string firstname, string middlename, string secondname, bool? gender, bool declinated,
                         string emplstatus, PermissionProfile profile, string password, byte[] foto, Employee_SlightPart slightPart, UploadResult uploadstatus, Version vr, DateTime updatedate)
            : this(id: id, firstname: firstname, middlename: middlename, secondname: secondname, gender: gender, declinated: declinated, vr: vr, updatedate: updatedate,
                  departament: departament, office: office, emplstatus: emplstatus, profile: profile, password: password, foto: foto)
        {
            _slightpart = new Lazy<Employee_SlightPart>(() => slightPart);
            _upload = uploadstatus;
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
        private Adress _adress;
        private bool _is_hide_personal;
        #endregion Fields
        #region Properties
        public Adress Adress
        {
            get => _adress;
            set => SetProperty(ref _adress, value);
        }
        [MaxLength(250, ErrorMessage ="превышел лимит символов")]
        public string Education1
        {
            get => _education1;
            set => SetProperty(ref _education1, value, true);
        }
        [MaxLength(250, ErrorMessage = "превышел лимит символов")]
        public string Education2
        {
            get => _education2;
            set => SetProperty(ref _education2, value,true);
        }
        [MaxLength(250, ErrorMessage = "превышел лимит символов")]
        public string Education3
        {
            get => _education3;
            set => SetProperty(ref _education3, value, true);
        }
        [MaxLength(250, ErrorMessage = "превышел лимит символов")]
        public string Sciencedegree
        {
            get => _sciencedegree;
            set => SetProperty(ref _sciencedegree, value, true);
        }
        [RegularExpression(@"^[1-9]\d{9}$", ErrorMessage = "неверный формат")]
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
        [RegularExpression(@"^[1-9]\d{3,6}$", ErrorMessage = "неверный формат")]
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
            if (adress != null)
            {
                _adress = adress;
                _adress.PropertyChanged += _adress_PropertyChanged;
            }
            else _adress = new Adress();
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
                        adress: _adress?.Clone(),
                        mobilephone: _mobilephone,
                        workphone: _workphone,
                        email: _email,
                        hiredate: _hiredate,
                        birthdate: _birthdate,
                        hidepersonal: _is_hide_personal);
        }
        protected override void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
        }
    }
}
