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
        //IReadOnlyDictionary<ExpertiseTypes, string> ExpertiseTypesMap { get; }
        //IReadOnlyDictionary<ResolutionTypes, string> ResolutionTypesMap { get; }
        //IReadOnlyDictionary<string, ExpertiseResults> ExpertiseResultsMap { get; }
        //IReadOnlyDictionary<string, ExpertiseTypes> ExpertiseTypesMapInv { get; }
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
        public OrganizationsDataAccess OrganizationAccessService => throw new NotImplementedException();
        public SettlementsDataAccess SettlementAccessService => _settlement_da;
        public EmployeeDataAccess EmployeeAccessService => _employee_da;
        public ExpertDataAccess ExpertAccessService => _expert_da ??= new ExpertDataAccess(this);
        public EquipmentDataAccess EquipmentAccessService => throw new NotImplementedException();
        public EquipmentUsageDataAccess EquipmentUsageAccessService => throw new NotImplementedException();
        public DepartamentDataAccess DepartamentsAccessService => _departament;
        public CustomerDataAccess CustomerAccessService => throw new NotImplementedException();
        public ResolutionDataAccess ResolutionAccessService => throw new NotImplementedException();
        public ExpertiseDataAccess ExpertiseAccessService => throw new NotImplementedException();
        public BillDataAccess BillAccessService => throw new NotImplementedException();
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
                App.ErrorLogger.LogError(ex.Message);
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
                        lTypeCase.Add(new CaseType(rd.GetString(0), rd.GetString(1)));
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
                rd.Close();
            }
            catch (Exception ex)
            {
                App.ErrorLogger.LogError(ex.Message);
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
            _lab_da = new LaboratoryDataAccess(this);
            _settlement_da = new SettlementsDataAccessCached(this);
            _employee_da = new EmployeeDataAccessCached(this);
            _departament = new DepartamentDataAccessCashed(this);
        }
    }
}
