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
        IErrorLogger ErrorReporter { get; }
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
        void Inicialize();
    }

    public class Storage_Cached : ILocalStorage
    {
        #region Fields
        InitializationStatus _status;
        IErrorLogger _logger = new ConsoleErrorLogger();
        #endregion

        #region Properties
        public InitializationStatus InitializationStatus => _status;
        public IErrorLogger ErrorReporter => _logger;
        public SqlConnection DBConnection => new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["PLSE"].ConnectionString);
        public IReadOnlyList<Expertise> ExpertisesInWork => throw new NotImplementedException();
        public LaboratoryDataAccess LaboratoryAccessService => throw new NotImplementedException();
        public SpecialityDataAccess SpecialityAccessService => throw new NotImplementedException();
        public OrganizationsDataAccess OrganizationAccessService => throw new NotImplementedException();
        public SettlementsDataAccess SettlementAccessService => throw new NotImplementedException();
        public EmployeeDataAccess EmployeeAccessService => throw new NotImplementedException();
        public ExpertDataAccess ExpertAccessService => throw new NotImplementedException();
        public EquipmentDataAccess EquipmentAccessService => throw new NotImplementedException();
        public EquipmentUsageDataAccess EquipmentUsageAccessService => throw new NotImplementedException();
        public DepartamentDataAccess DepartamentsAccessService => throw new NotImplementedException();
        public CustomerDataAccess CustomerAccessService => throw new NotImplementedException();
        public ResolutionDataAccess ResolutionAccessService => throw new NotImplementedException();
        public ExpertiseDataAccess ExpertiseAccessService => throw new NotImplementedException();
        public BillDataAccess BillAccessService => throw new NotImplementedException();
        public MovementDataAccess ExpertiseMovementAccessService => throw new NotImplementedException();
        public IReadOnlyList<string> InnerOffices => throw new NotImplementedException();
        public IReadOnlyList<string> EmployeeStatus => throw new NotImplementedException();
        public IReadOnlyList<CaseType> CaseTypes => throw new NotImplementedException();
        public IReadOnlyList<string> OuterOffices => throw new NotImplementedException();
        public IReadOnlyList<string> Ranks => throw new NotImplementedException();
        public IReadOnlyList<string> StreetTypes => throw new NotImplementedException();
        public IReadOnlyList<string> SettlementTypes => throw new NotImplementedException();
        public IReadOnlyList<string> SettlementSignificances => throw new NotImplementedException();
        public IReadOnlyList<string> ResolutionStatuses => throw new NotImplementedException();
        public IReadOnlyList<string> Genders => throw new NotImplementedException();
        public IReadOnlyList<string> SpecialityKinds => throw new NotImplementedException();
        public IReadOnlyList<MovementInfo> ExpertiseMovements => throw new NotImplementedException();
        #endregion

        #region Functions
        public void Inicialize()
        {
            _status = InitializationStatus.Perfoming;

            _status = InitializationStatus.Succsess;
        }
        #endregion

       
        public Storage_Cached() => Inicialize();
    }
}
