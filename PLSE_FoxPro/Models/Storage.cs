using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;

namespace PLSE_FoxPro.Models
{
    public enum InitializationStatus
    {
        Unperform = 0,
        Perfoming,
        Faulted,
        Succsess
    }

    public interface ILocalStorage
    {
        /// <summary>
        /// Отображает статус инициализации хранилища
        /// </summary>
        InitializationStatus InitializationStatus { get;}
        /// <summary>
        /// Строка соединения к базе данных
        /// </summary>
        SqlConnection DBConnection { get; }
        IReadOnlyList<Expertise> ExpertisesInWork { get; }
        LaboratoryDataAccess LaboratoryAccessService { get; }
        SpecialityDataAccess SpecialityAccessService { get; }
        OrganizationsDataAccess OrganizationAccessService { get; }
        SettlementsDataAccess SettlementAccessService { get; }
        EmployeeDataAccess EmployeeAccessService { get; }
        ExpertDataAccess ExpertAccessService { get; }
        EquipmentDataAccess EquipmentAccessService { get; }
        EquipmentUsageDataAccess EquipmentUsageAccessService { get; }
        DepartamentDataAccess DepartamentsAccessService { get; }
        CustomerDataAccess CustomerAccessService { get; }
        ResolutionDataAccess ResolutionAccessService { get; }
        ExpertiseDataAccess ExpertiseAccessService { get; }
        BillDataAccess BillAccessService { get; }
        MovementDataAccess ExpertiseMovementAccessService { get; }
        IReadOnlyList<string> InnerOffices { get; }
        IReadOnlyList<string> EmployeeStatus { get; }
        IReadOnlyList<CaseType> CaseTypes { get; }
        IReadOnlyList<string> OuterOffices { get; }
        IReadOnlyList<string> Ranks { get; }
        IReadOnlyList<string> StreetTypes { get; }
        IReadOnlyList<string> SettlementTypes { get; }
        IReadOnlyList<string> SettlementSignificances { get; }
        IReadOnlyList<string> ResolutionStatuses { get; }
        IReadOnlyList<string> Genders { get; }
        IReadOnlyList<string> SpecialityKinds { get; }
        IReadOnlyList<MovementInfo> ExpertiseMovements { get; }
        IReadOnlyCollection<string> ExpertiseTypes { get; }
        IReadOnlyCollection<string> ExpertiseResults { get; }
        IReadOnlyCollection<string> ResolutionTypes { get; }
        //Employee[] GetAdministration();
        /// <summary>
        /// Инициализирует хранилище и инициирует событие StatusChanged
        /// </summary>
        void Inicialize();
        /// <summary>
        /// Событие изменения статуса инициализации хранилища
        /// </summary>
        event Action<object,InitializationStatus> StatusChanged;
    }

    public class Storage_Cached : ILocalStorage
    {
        #region Fields
        InitializationStatus _status;
        IReadOnlyList<string> _streettypes;
        IReadOnlyList<string> _inneroffices;
        IReadOnlyList<string> _settlementprefixs;
        IReadOnlyList<string> _settlementsigns;
        IReadOnlyList<string> _employeestatus;
        IReadOnlyList<CaseType> _casetypes;
        IReadOnlyList<string> _resolutionstatus;
        IReadOnlyList<string> _ranks;
        IReadOnlyList<string> _outeroffices;
        IReadOnlyList<string> _speciality_kinds;
        IReadOnlyList<string> _genders;
        IReadOnlyList<MovementInfo> _movements;
        IReadOnlyCollection<string> _expertise_types;
        IReadOnlyCollection<string> _expertise_result;
        IReadOnlyCollection<string> _resolution_types;
        LaboratoryDataAccess _lab_da;
        SpecialityDataAccess _speciality_da;
        OrganizationsDataAccess _organization_da;
        SettlementsDataAccess _settlement_da;
        EmployeeDataAccess _employee_da;
        ExpertDataAccess _expert_da;
        EquipmentDataAccess _equipment_da;
        EquipmentUsageDataAccess _quipmentusage_da;
        DepartamentDataAccess _departament;
        CustomerDataAccess _customer_da;
        ResolutionDataAccess _resolution_da;
        ExpertiseDataAccess _expertise_da;
        BillDataAccess _bill_da;
        MovementDataAccess _movement_da;
        #endregion

        #region Properties
        public InitializationStatus InitializationStatus => _status;
        public SqlConnection DBConnection => new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["PLSE"].ConnectionString);
        public IReadOnlyList<Expertise> ExpertisesInWork => throw new NotImplementedException();
        public LaboratoryDataAccess LaboratoryAccessService => _lab_da;
        public SpecialityDataAccess SpecialityAccessService => _speciality_da ??= new SpecialityDataAccessCached(this);
        public OrganizationsDataAccess OrganizationAccessService => _organization_da ??= new OrganizationsDataAccess(this);
        public SettlementsDataAccess SettlementAccessService => _settlement_da;
        public EmployeeDataAccess EmployeeAccessService => _employee_da;
        public ExpertDataAccess ExpertAccessService => _expert_da ??= new ExpertDataAccess(this);
        public EquipmentDataAccess EquipmentAccessService => throw new NotImplementedException();
        public EquipmentUsageDataAccess EquipmentUsageAccessService => throw new NotImplementedException();
        public DepartamentDataAccess DepartamentsAccessService => _departament;
        public CustomerDataAccess CustomerAccessService => _customer_da ??= new CustomerDataAccess(this);
        public ResolutionDataAccess ResolutionAccessService => _resolution_da ??= new ResolutionDataAccess(this);
        public ExpertiseDataAccess ExpertiseAccessService => _expertise_da ??= new ExpertiseDataAccess(this);
        public BillDataAccess BillAccessService => _bill_da ??= new BillDataAccess(this);
        public MovementDataAccess ExpertiseMovementAccessService => throw new NotImplementedException();
        public IReadOnlyList<string> InnerOffices => _inneroffices;
        public IReadOnlyList<string> EmployeeStatus => _employeestatus;
        public IReadOnlyList<CaseType> CaseTypes => _casetypes;
        public IReadOnlyList<string> OuterOffices => _outeroffices;
        public IReadOnlyList<string> Ranks => _ranks;
        public IReadOnlyList<string> StreetTypes => _streettypes;
        public IReadOnlyList<string> SettlementTypes => _settlementprefixs;
        public IReadOnlyList<string> SettlementSignificances => _settlementsigns;
        public IReadOnlyList<string> ResolutionStatuses => _resolutionstatus;
        public IReadOnlyList<string> Genders => _genders;
        public IReadOnlyList<string> SpecialityKinds => _speciality_kinds;
        public IReadOnlyList<MovementInfo> ExpertiseMovements => _movements;
        public IReadOnlyCollection<string> ExpertiseTypes => _expertise_types;
        public IReadOnlyCollection<string> ExpertiseResults => _expertise_result;
        public IReadOnlyCollection<string> ResolutionTypes => _resolution_types;
        #endregion

        #region Functions
        public void Inicialize()
        {
            SetStatusAndRaiseEvent(this, InitializationStatus.Perfoming);
            try
            {
                LoadDomainTables();
                SetStatusAndRaiseEvent(this, InitializationStatus.Succsess);
            }
            catch (Exception ex)
            {
                SetStatusAndRaiseEvent(this, InitializationStatus.Faulted);
                App.Services.GetService<IErrorLogger>().LogError(ex.Message);
            }
        }
        private void SetStatusAndRaiseEvent(object sender, InitializationStatus status)
        {
            _status = status;
            StatusChanged?.Invoke(this, status);
        }
        private void LoadDomainTables()
        {
            SqlCommand cmd = DBConnection.CreateCommand();
            cmd.CommandType = System.Data.CommandType.StoredProcedure;
            cmd.CommandText = "InnResources.prLoadDomainTables";
            try
            {
                cmd.Connection.Open();
                var rd = cmd.ExecuteReader();
                //StreetType
                if (rd.HasRows)
                {
                    List<string> streettype = new List<string>();
                    while (rd.Read())
                    {
                        streettype.Add(rd.GetString(0));
                    }
                    _streettypes = streettype;
                }
                //InnerOffice
                if (rd.NextResult())
                {
                    List<string> lInnerOffice = new List<string>();
                    while (rd.Read())
                    {
                        lInnerOffice.Add(rd.GetString(0));
                    }
                    _inneroffices = lInnerOffice;
                }
                //EmployeeStatus
                if (rd.NextResult())
                {
                    List<string> lEmployeeStatus = new List<string>();
                    while (rd.Read())
                    {
                        lEmployeeStatus.Add(rd.GetString(0));
                    }
                    _employeestatus = lEmployeeStatus;
                }
                //SettlementPrefix
                if (rd.NextResult())
                {
                    List<string> lSettlementPrefixes = new List<string>();
                    while (rd.Read())
                    {
                        lSettlementPrefixes.Add(rd.GetString(0));
                    }
                    _settlementprefixs = lSettlementPrefixes;
                }
                //SettlementSign
                if (rd.NextResult())
                {
                    List<string> lSettlementSignificances = new List<string>();
                    while (rd.Read())
                    {
                        lSettlementSignificances.Add(rd.GetString(0));
                    }
                    _settlementsigns = lSettlementSignificances;
                }            
                //TypeCase
                if (rd.NextResult())
                {
                    List<CaseType> lTypeCase = new List<CaseType>();
                    while (rd.Read())
                    {
                        lTypeCase.Add(new CaseType(rd.GetString(1), rd.GetString(0)));
                    }
                    _casetypes = lTypeCase;
                }
                //ResolutionStatus
                if (rd.NextResult())
                {
                    List<string> lResolutionStatus = new List<string>();
                    while (rd.Read())
                    {
                        lResolutionStatus.Add(rd.GetString(0));
                    }
                    _resolutionstatus = lResolutionStatus;
                }
                //Ranks
                if (rd.NextResult())
                {
                    var ranks = new List<string>(80);
                    while (rd.Read())
                    {
                        ranks.Add(rd.GetString(0));
                    }
                    _ranks = ranks;
                }
                //Speciality kinds
                if (rd.NextResult())
                {
                    var kinds = new List<string>(30);
                    while (rd.Read())
                    {
                        kinds.Add(rd.GetString(0));
                    }
                    _speciality_kinds = kinds;
                }
                if (rd.NextResult())
                {
                    var expt = new List<string>(3);
                    while (rd.Read())
                    {
                        expt.Add(rd.GetString(0));
                    }
                    _expertise_types = expt;
                }
                if (rd.NextResult())
                {
                    var rest = new List<string>(4);
                    while (rd.Read())
                    {
                        rest.Add(rd.GetString(0));
                    }
                    _resolution_types = rest;
                }
                if(rd.NextResult())
                {
                    var expr = new List<string>(2);
                    while (rd.Read())
                    {
                        expr.Add(rd.GetString(0));
                    }
                    _expertise_result = expr;
                }
                rd.Close();
            }
            catch (Exception ex)
            {
                App.Services.GetService<IErrorLogger>().LogError(ex.Message);
                throw;
            }
            finally
            {
                cmd.Connection.Close();
            }
        }
        #endregion

        public event Action<object,InitializationStatus> StatusChanged;

        public Storage_Cached()
        {
            _genders = new List<string> { "мужской", "женский", "неизвестный" };
            _lab_da = new LaboratoryDataAccess(this);
            _settlement_da = new SettlementsDataAccessCached(this);
            _employee_da = new EmployeeDataAccessCached(this);
            _departament = new DepartamentDataAccessCashed(this);
        }
    }
}
