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
        InitializationStatus InitializationStatus { get; }
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
}
