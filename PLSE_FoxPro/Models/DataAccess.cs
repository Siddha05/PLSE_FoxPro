using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace PLSE_FoxPro.Models
{
    public abstract class DataAccess<T> where T : VersionBase
    {
        protected ILocalStorage _storage;
        public ILocalStorage Storage => _storage;
        public virtual void SaveChanges(T item, SqlTransaction tran = null)
        {
            if (item == null) return;
            if (tran != null)
            {
                switch (item.Version)
                {
                    case Version.New:
                        Add(item, tran.Connection, tran);
                        break;
                    case Version.Edited:
                        Edit(item, tran.Connection, tran);
                        break;
                    default:
                        break;
                }
            }
            else
            {
                SqlConnection con = _storage.DBConnection;
                try
                {
                    con.Open();
                    switch (item.Version)
                    {
                        case Version.New:
                            Add(item, con);
                            break;
                        case Version.Edited:
                            Edit(item, con);
                            break;
                        default:
                            break;
                    }
                }
                catch (Exception ex)
                {
                    throw;
                }
                finally
                {
                    con.Close();
                }
            }

        }
        protected abstract void Add(T item, SqlConnection connection, SqlTransaction tran = null);
        protected abstract void Edit(T item, SqlConnection connection, SqlTransaction tran = null);
        public abstract T GetItemByID(int id);
        public virtual void Delete(T item) => throw new InvalidOperationException();
        public abstract ICollection<T> Items();
        public DataAccess(ILocalStorage storage)
        {
            if (storage == null) throw new ArgumentException($"Parameter {storage} was null");
            _storage = storage;
        }
        protected object ConvertToDBNull<V>(V obj)
        {
            if (obj == null) return DBNull.Value;
            else return obj;
        }
        protected IEnumerable<FieldInfo> GetAllFields(Type t)
        {
            foreach (var item in t.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public))
            {
                yield return item;
            }
            var btype = t.BaseType;
            if (btype != typeof(object))
            {
                foreach (var item in GetAllFields(btype))
                {
                    yield return item;
                }
            }
        }
        protected IEnumerable<PropertyInfo> GetAllProperties(Type t)
        {
            foreach (var item in t.GetProperties(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public))
            {
                yield return item;
            }
        }
        /// <summary>
        /// Копирует все поля из <paramref name="item2"/> в <paramref name="item1"/> и запускает событие для каждого свойства через делегат PropertyChangedEventHandler 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item1">Донор</param>
        /// <param name="item2">Реципиент</param>
        protected void CopyFields<V>(V item1, V item2) where V : VersionBase
        {
            Type type = typeof(V);
            FieldInfo pc = null;
            foreach (var field in GetAllFields(type))
            {
                if (field.Name == "PropertyChanged")
                {
                    pc = field;
                    continue;
                }
                field.SetValue(item1, field.GetValue(item2));
            }
            var e = pc?.GetValue(item1) as PropertyChangedEventHandler;
            if (e != null)
            {
                foreach (var prop in GetAllProperties(type))
                {
                    e.Invoke(item1, new PropertyChangedEventArgs(prop.Name));
                }
            }
        }
        public void EditByID<V>(ConcurrentDictionary<int, V> collection, V item) where V : VersionBase
        {
            if (collection.TryGetValue(item.ID, out V obj))
            {
                CopyFields(obj, item);
            }
        }
    }

    public class SettlementsDataAccess : DataAccess<Settlement>
    {
        protected override void Add(Settlement item, SqlConnection connection, SqlTransaction tran = null)
        {
            SqlCommand cmd = connection.CreateCommand();
            cmd.Transaction = tran;
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "OutResources.prAddSettlement";
            cmd.Parameters.Add("@Title", SqlDbType.NVarChar, 40).Value = item.Title;
            cmd.Parameters.Add("@SettlementType", SqlDbType.NVarChar, 20).Value = item.SettlementType;
            cmd.Parameters.Add("@Significance", SqlDbType.NVarChar, 15).Value = item.Significance;
            cmd.Parameters.Add("@FederalLocation", SqlDbType.VarChar, 50).Value = ConvertToDBNull(item.Federallocation);
            cmd.Parameters.Add("@TerritorialLocation", SqlDbType.NVarChar, 50).Value = ConvertToDBNull(item.Territorylocation);
            cmd.Parameters.Add("@TelephoneCode", SqlDbType.NVarChar, 5).Value = ConvertToDBNull(item.Telephonecode);
            cmd.Parameters.Add("@PostCode", SqlDbType.NVarChar, 13).Value = ConvertToDBNull(item.Postcode);
            var par = cmd.Parameters.Add("@InsertedID", SqlDbType.Int);
            par.Direction = ParameterDirection.Output;
            try
            {
                cmd.ExecuteNonQuery();
                item.ID = (int)cmd.Parameters["@InsertedID"].Value;
                item.Version = Version.Original;
            }
            catch (Exception)
            {
                throw;
            }
        }
        protected override void Edit(Settlement item, SqlConnection connection, SqlTransaction tran = null)
        {
            SqlCommand cmd = connection.CreateCommand();
            cmd.Transaction = tran;
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "OutResources.prEditSettlement";
            cmd.Parameters.Add("@Title", SqlDbType.NVarChar, 40).Value = item.Title;
            cmd.Parameters.Add("@SettlementType", SqlDbType.NVarChar, 20).Value = item.SettlementType;
            cmd.Parameters.Add("@Significance", SqlDbType.NVarChar, 15).Value = item.Significance;
            cmd.Parameters.Add("@FederalLocation", SqlDbType.VarChar, 50).Value = ConvertToDBNull(item.Federallocation);
            cmd.Parameters.Add("@TerritorialLocation", SqlDbType.NVarChar, 50).Value = ConvertToDBNull(item.Territorylocation);
            cmd.Parameters.Add("@TelephoneCode", SqlDbType.NVarChar, 5).Value = ConvertToDBNull(item.Telephonecode);
            cmd.Parameters.Add("@PostCode", SqlDbType.NVarChar, 13).Value = ConvertToDBNull(item.Postcode);
            cmd.Parameters.Add("@IsValid", SqlDbType.Bit).Value = item.IsValid;
            cmd.Parameters.Add("@SettlementID", SqlDbType.Int).Value = item.ID;
            try
            {
                cmd.ExecuteNonQuery();
                item.Version = Version.Original;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public override void Delete(Settlement item)
        {
            SqlCommand cmd = _storage.DBConnection.CreateCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "OutResources.prDeleteSettlement";
            cmd.Parameters.Add("@SettlementID", SqlDbType.Int).Value = item.ID;
            try
            {
                cmd.Connection.Open();
                cmd.ExecuteNonQuery();
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                cmd.Connection.Close();
            }
        }
        public override ICollection<Settlement> Items()
        {
            SqlCommand cmd = _storage.DBConnection.CreateCommand();
            cmd.CommandText = "OutResources.prGetSettlements";
            cmd.CommandType = CommandType.StoredProcedure;
            List<Settlement> list = new List<Settlement>(700); //TODO: Implement constant in settings
            try
            {
                cmd.Connection.Open();
                var rd = cmd.ExecuteReader();
                if (rd.HasRows)
                {
                    int colSettlementID = rd.GetOrdinal("SettlementID");
                    int colTitle = rd.GetOrdinal("Title");
                    int colSettlementType = rd.GetOrdinal("SettlementType");
                    int colSignificance = rd.GetOrdinal("Significance");
                    int colFederalLocationID = rd.GetOrdinal("FederalLocation");
                    int colTerritorialLocationID = rd.GetOrdinal("TerritorialLocation");
                    int colTelephoneCode = rd.GetOrdinal("TelephoneCode");
                    int colPostCode = rd.GetOrdinal("PostCode");
                    int colUpdateDate = rd.GetOrdinal("UpdateDate");
                    int colIsValid = rd.GetOrdinal("IsValid");
                    while (rd.Read())
                    {
                        Settlement setl = new Settlement(id: rd.GetInt32(colSettlementID),
                                                         title: rd.GetString(colTitle),
                                                         type: rd.GetString(colSettlementType),
                                                         significance: rd.GetString(colSignificance),
                                                         telephonecode: rd[colTelephoneCode] == DBNull.Value ? null : rd.GetString(colTelephoneCode),
                                                         postcode: rd[colPostCode] == DBNull.Value ? null : rd.GetString(colPostCode),
                                                         federallocation: rd[colFederalLocationID] == DBNull.Value ? null : rd.GetString(colFederalLocationID),
                                                         territoriallocation: rd[colTerritorialLocationID] == DBNull.Value ? null : rd.GetString(colTerritorialLocationID),
                                                         isvalid: rd.GetBoolean(colIsValid),
                                                         vr: Version.Original,
                                                         updatedate: rd.GetDateTime(colUpdateDate));
                        list.Add(setl);
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                cmd.Connection.Close();
            }
            return list;
        }
        public override Settlement GetItemByID(int id)
        {
            Settlement setl = null;
            SqlCommand cmd = _storage.DBConnection.CreateCommand();
            cmd.CommandText = "OutResources.prGetSettlementByID";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("@SettlementID", SqlDbType.Int).Value = id;
            try
            {
                cmd.Connection.Open();
                var rd = cmd.ExecuteReader();
                if (rd.HasRows)
                {
                    int colSettlementID = rd.GetOrdinal("SettlementID");
                    int colTitle = rd.GetOrdinal("Title");
                    int colSettlementType = rd.GetOrdinal("SettlementType");
                    int colSignificance = rd.GetOrdinal("Significance");
                    int colFederalLocationID = rd.GetOrdinal("FederalLocation");
                    int colTerritorialLocationID = rd.GetOrdinal("TerritorialLocation");
                    int colTelephoneCode = rd.GetOrdinal("TelephoneCode");
                    int colPostCode = rd.GetOrdinal("PostCode");
                    int colUpdateDate = rd.GetOrdinal("UpdateDate");
                    int colIsValid = rd.GetOrdinal("IsValid");
                    while (rd.Read())
                    {
                        setl = new Settlement(id: rd.GetInt32(colSettlementID),
                                                         title: rd.GetString(colTitle),
                                                         type: rd.GetString(colSettlementType),
                                                         significance: rd.GetString(colSignificance),
                                                         telephonecode: rd[colTelephoneCode] == DBNull.Value ? null : rd.GetString(colTelephoneCode),
                                                         postcode: rd[colPostCode] == DBNull.Value ? null : rd.GetString(colPostCode),
                                                         federallocation: rd[colFederalLocationID] == DBNull.Value ? null : rd.GetString(colFederalLocationID),
                                                         territoriallocation: rd[colTerritorialLocationID] == DBNull.Value ? null : rd.GetString(colTerritorialLocationID),
                                                         isvalid: rd.GetBoolean(colIsValid),
                                                         vr: Version.Original,
                                                         updatedate: rd.GetDateTime(colUpdateDate));
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                cmd.Connection.Close();
            }
            return setl;
        }
        public SettlementsDataAccess(ILocalStorage storage) : base(storage) { }
    }
    public class SettlementsDataAccessCached : SettlementsDataAccess
    {
        ConcurrentDictionary<int, Settlement> _cache = new ConcurrentDictionary<int, Settlement>();
        bool _has_full_access;

        public override Settlement GetItemByID(int id)
        {
            if (_cache.ContainsKey(id)) return _cache[id];
            else
            {
                var d = base.GetItemByID(id);
                _cache.TryAdd(id, d);
                return d;
            }
        }
        public override ICollection<Settlement> Items()
        {
            if (_has_full_access) return _cache.Values;
            else
            {
                SqlCommand cmd = _storage.DBConnection.CreateCommand();
                cmd.CommandText = "OutResources.prGetSettlements";
                cmd.CommandType = CommandType.StoredProcedure;
                try
                {
                    cmd.Connection.Open();
                    var rd = cmd.ExecuteReader();
                    if (rd.HasRows)
                    {
                        int colSettlementID = rd.GetOrdinal("SettlementID");
                        int colTitle = rd.GetOrdinal("Title");
                        int colSettlementType = rd.GetOrdinal("SettlementType");
                        int colSignificance = rd.GetOrdinal("Significance");
                        int colFederalLocationID = rd.GetOrdinal("FederalLocation");
                        int colTerritorialLocationID = rd.GetOrdinal("TerritorialLocation");
                        int colTelephoneCode = rd.GetOrdinal("TelephoneCode");
                        int colPostCode = rd.GetOrdinal("PostCode");
                        int colUpdateDate = rd.GetOrdinal("UpdateDate");
                        int colIsValid = rd.GetOrdinal("IsValid");
                        while (rd.Read())
                        {
                            if (!_cache.ContainsKey(rd.GetInt32(colSettlementID)))
                            {
                                Settlement setl = new Settlement(id: rd.GetInt32(colSettlementID),
                                                             title: rd.GetString(colTitle),
                                                             type: rd.GetString(colSettlementType),
                                                             significance: rd.GetString(colSignificance),
                                                             telephonecode: rd[colTelephoneCode] == DBNull.Value ? null : rd.GetString(colTelephoneCode),
                                                             postcode: rd[colPostCode] == DBNull.Value ? null : rd.GetString(colPostCode),
                                                             federallocation: rd[colFederalLocationID] == DBNull.Value ? null : rd.GetString(colFederalLocationID),
                                                             territoriallocation: rd[colTerritorialLocationID] == DBNull.Value ? null : rd.GetString(colTerritorialLocationID),
                                                             isvalid: rd.GetBoolean(colIsValid),
                                                             vr: Version.Original,
                                                             updatedate: rd.GetDateTime(colUpdateDate));
                                _cache.TryAdd(setl.ID, setl);
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    throw;
                }
                finally
                {
                    cmd.Connection.Close();
                }
                _has_full_access = true;
                return _cache.Values;
            }
        }
        protected override void Add(Settlement item, SqlConnection connection, SqlTransaction tran = null)
        {
            base.Add(item, connection, tran);
            _cache.TryAdd(item.ID, item);
        }
        protected override void Edit(Settlement item, SqlConnection connection, SqlTransaction tran = null)
        {
            base.Edit(item, connection, tran);
            EditByID(_cache,item);
        }
        public override void Delete(Settlement item)
        {
            base.Delete(item);
            _cache.TryRemove(item.ID, out Settlement _);
        }
        public SettlementsDataAccessCached(ILocalStorage storage) : base(storage) { }
    }

    public class SpecialityDataAccess : DataAccess<Speciality>
    {
        public override ICollection<Speciality> Items()
        {
            SqlCommand cmd = _storage.DBConnection.CreateCommand();
            cmd.CommandText = "InnResources.prGetSpecialities";
            cmd.CommandType = CommandType.StoredProcedure;
            List<Speciality> list = new List<Speciality>(60); //TODO: replace constant inicial size of list
            try
            {
                cmd.Connection.Open();
                var rd = cmd.ExecuteReader();
                if (rd.HasRows)
                {
                    int colID = rd.GetOrdinal("SpecialityID");
                    int colCode = rd.GetOrdinal("Code");
                    int colTitle = rd.GetOrdinal("Title");
                    int colCat1 = rd.GetOrdinal("Category1");
                    int colCat2 = rd.GetOrdinal("Category2");
                    int colCat3 = rd.GetOrdinal("Category3");
                    int colSpecies = rd.GetOrdinal("Species");
                    int colAcronym = rd.GetOrdinal("Acronym");
                    int colUpdateDate = rd.GetOrdinal("UpdateDate");
                    int colIsValid = rd.GetOrdinal("IsValid");
                    while (rd.Read())
                    {
                        list.Add(new Speciality(updatedate: rd.GetDateTime(colUpdateDate),
                                                id: rd.GetInt16(colID),
                                                code: rd.GetString(colCode),
                                                title: rd.GetString(colTitle),
                                                species: rd[colSpecies] != DBNull.Value ? rd.GetString(colSpecies) : null,
                                                isvalid: rd.GetBoolean(colIsValid),
                                                acr: rd[colAcronym] != DBNull.Value ? rd.GetString(colAcronym) : null,
                                                cat_1: rd[colCat1] != DBNull.Value ? new Byte?(rd.GetByte(colCat1)) : null,
                                                cat_2: rd[colCat2] != DBNull.Value ? new Byte?(rd.GetByte(colCat2)) : null,
                                                cat_3: rd[colCat3] != DBNull.Value ? new Byte?(rd.GetByte(colCat3)) : null,
                                                vr: Version.Original)
                                 );
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                cmd.Connection.Close();
            }
            return list;
        }
        protected override void Add(Speciality item, SqlConnection connection, SqlTransaction tran = null)
        {
            SqlCommand cmd = connection.CreateCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "InnResources.prAddSpeciality";
            cmd.Transaction = tran;
            cmd.Parameters.Add("@Code", SqlDbType.NVarChar, 10).Value = item.Code;
            cmd.Parameters.Add("@Title", SqlDbType.NVarChar, 250).Value = item.Title;
            cmd.Parameters.Add("@Cat1", SqlDbType.TinyInt).Value = ConvertToDBNull(item.Category_1);
            cmd.Parameters.Add("@Cat2", SqlDbType.TinyInt).Value = ConvertToDBNull(item.Category_2);
            cmd.Parameters.Add("@Cat3", SqlDbType.TinyInt).Value = ConvertToDBNull(item.Category_3);
            cmd.Parameters.Add("@Species", SqlDbType.NVarChar, 75).Value = ConvertToDBNull(item.Species);
            cmd.Parameters.Add("@Acronym", SqlDbType.NVarChar, 10).Value = ConvertToDBNull(item.Acronym);
            var p = cmd.Parameters.Add("@InsertedID", SqlDbType.SmallInt);
            p.Direction = ParameterDirection.Output;
            try
            {
                cmd.ExecuteNonQuery();
                item.ID = (Int16)cmd.Parameters["@InsertedID"].Value;
                item.Version = Version.Original;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public override void Delete(Speciality item)
        {
            SqlCommand cmd = _storage.DBConnection.CreateCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "InnResources.prDeleteSpeciality";
            cmd.Parameters.Add("@SpecialityID", SqlDbType.SmallInt).Value = item.ID;
            try
            {
                cmd.Connection.Open();
                cmd.ExecuteNonQuery();
            }
            catch (Exception)
            {
                throw;
            }
        }
        protected override void Edit(Speciality item, SqlConnection connection, SqlTransaction tran = null)
        {
            SqlCommand cmd = connection.CreateCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "InnResources.prEditSpeciality";
            cmd.Transaction = tran;
            cmd.Parameters.Add("@Code", SqlDbType.NVarChar, 10).Value = item.Code;
            cmd.Parameters.Add("@Title", SqlDbType.NVarChar, 250).Value = item.Title;
            cmd.Parameters.Add("@Cat1", SqlDbType.TinyInt).Value = ConvertToDBNull(item.Category_1);
            cmd.Parameters.Add("@Cat2", SqlDbType.TinyInt).Value = ConvertToDBNull(item.Category_2);
            cmd.Parameters.Add("@Cat3", SqlDbType.TinyInt).Value = ConvertToDBNull(item.Category_3);
            cmd.Parameters.Add("@Species", SqlDbType.NVarChar, 75).Value = ConvertToDBNull(item.Species);
            cmd.Parameters.Add("@Acronym", SqlDbType.NVarChar, 10).Value = ConvertToDBNull(item.Acronym);
            cmd.Parameters.Add("@IsValid", SqlDbType.Bit).Value = item.IsValid;
            cmd.Parameters.Add("@SpecialityID", SqlDbType.SmallInt).Value = item.ID;
            try
            {
                cmd.ExecuteNonQuery();
                item.Version = Version.Original;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public override Speciality GetItemByID(int id)
        {
            SqlCommand cmd = _storage.DBConnection.CreateCommand();
            cmd.CommandText = "InnResources.prGetSpecialityByID";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("@SpecialityID", SqlDbType.SmallInt).Value = id;
            Speciality speciality = null;
            try
            {
                cmd.Connection.Open();
                var rd = cmd.ExecuteReader();
                if (rd.HasRows)
                {
                    int colID = rd.GetOrdinal("SpecialityID");
                    int colCode = rd.GetOrdinal("Code");
                    int colTitle = rd.GetOrdinal("Title");
                    int colCat1 = rd.GetOrdinal("Category1");
                    int colCat2 = rd.GetOrdinal("Category2");
                    int colCat3 = rd.GetOrdinal("Category3");
                    int colSpecies = rd.GetOrdinal("Species");
                    int colAcronym = rd.GetOrdinal("Acronym");
                    int colUpdateDate = rd.GetOrdinal("UpdateDate");
                    int colIsValid = rd.GetOrdinal("IsValid");
                    while (rd.Read())
                    {
                        speciality = new Speciality(updatedate: rd.GetDateTime(colUpdateDate),
                                                id: rd.GetInt16(colID),
                                                code: rd.GetString(colCode),
                                                title: rd.GetString(colTitle),
                                                species: rd[colSpecies] != DBNull.Value ? rd.GetString(colSpecies) : null,
                                                isvalid: rd.GetBoolean(colIsValid),
                                                acr: rd[colAcronym] != DBNull.Value ? rd.GetString(colAcronym) : null,
                                                cat_1: rd[colCat1] != DBNull.Value ? new Byte?(rd.GetByte(colCat1)) : null,
                                                cat_2: rd[colCat2] != DBNull.Value ? new Byte?(rd.GetByte(colCat2)) : null,
                                                cat_3: rd[colCat3] != DBNull.Value ? new Byte?(rd.GetByte(colCat3)) : null,
                                                vr: Version.Original);
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                cmd.Connection.Close();
            }
            return speciality;
        }
        public SpecialityDataAccess(ILocalStorage storage) : base(storage) { }
    }
    public class SpecialityDataAccessCached : SpecialityDataAccess
    {
        ConcurrentDictionary<int, Speciality> _cache = new ConcurrentDictionary<int, Speciality>();
        bool _has_full_access;

        public override Speciality GetItemByID(int id)
        {
            if (_cache.ContainsKey(id)) return _cache[id];
            else
            {
                var d = base.GetItemByID(id);
                _cache.TryAdd(id, d);
                return d;
            }
        }
        public override ICollection<Speciality> Items()
        {
            if (_has_full_access) return _cache.Values;
            else
            {
                SqlCommand cmd = _storage.DBConnection.CreateCommand();
                cmd.CommandText = "InnResources.prGetSpecialities";
                cmd.CommandType = CommandType.StoredProcedure;
                try
                {
                    cmd.Connection.Open();
                    var rd = cmd.ExecuteReader();
                    if (rd.HasRows)
                    {
                        int colID = rd.GetOrdinal("SpecialityID");
                        int colCode = rd.GetOrdinal("Code");
                        int colTitle = rd.GetOrdinal("Title");
                        int colCat1 = rd.GetOrdinal("Category1");
                        int colCat2 = rd.GetOrdinal("Category2");
                        int colCat3 = rd.GetOrdinal("Category3");
                        int colSpecies = rd.GetOrdinal("Species");
                        int colAcronym = rd.GetOrdinal("Acronym");
                        int colUpdateDate = rd.GetOrdinal("UpdateDate");
                        int colIsValid = rd.GetOrdinal("IsValid");
                        while (rd.Read())
                        {
                            if (!_cache.ContainsKey(rd.GetInt16(colID)))
                            {
                                var s = new Speciality(updatedate: rd.GetDateTime(colUpdateDate),
                                                    id: rd.GetInt16(colID),
                                                    code: rd.GetString(colCode),
                                                    title: rd.GetString(colTitle),
                                                    species: rd[colSpecies] != DBNull.Value ? rd.GetString(colSpecies) : null,
                                                    isvalid: rd.GetBoolean(colIsValid),
                                                    acr: rd[colAcronym] != DBNull.Value ? rd.GetString(colAcronym) : null,
                                                    cat_1: rd[colCat1] != DBNull.Value ? new Byte?(rd.GetByte(colCat1)) : null,
                                                    cat_2: rd[colCat2] != DBNull.Value ? new Byte?(rd.GetByte(colCat2)) : null,
                                                    cat_3: rd[colCat3] != DBNull.Value ? new Byte?(rd.GetByte(colCat3)) : null,
                                                    vr: Version.Original);
                                _cache.TryAdd(s.ID, s);
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    throw;
                }
                finally
                {
                    cmd.Connection.Close();
                }
                _has_full_access = true;
                return _cache.Values;
            }
        }
        protected override void Edit(Speciality item, SqlConnection connection, SqlTransaction tran = null)
        {
            base.Edit(item, connection, tran);
            EditByID(_cache,item);
        }
        public override void Delete(Speciality item)
        {
            base.Delete(item);
            _cache.TryRemove(item.ID, out Speciality _);
        }
        protected override void Add(Speciality item, SqlConnection connection, SqlTransaction tran = null)
        {
            base.Add(item, connection, tran);
            _cache.TryAdd(item.ID, item);
        }
        public SpecialityDataAccessCached(ILocalStorage storage) : base(storage) { }
    }

    public class DepartamentDataAccess : DataAccess<Departament>
    {
        public override Departament GetItemByID(int id)
        {
            SqlCommand cmd = _storage.DBConnection.CreateCommand();
            cmd.CommandText = "InnResources.prGetDepartamentByID";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("@DepartamentID", SqlDbType.TinyInt).Value = id;
            Departament departament = null;
            try
            {
                cmd.Connection.Open();
                var rd = cmd.ExecuteReader();
                if (rd.HasRows)
                {
                    int colTitle = rd.GetOrdinal("Title");
                    int colAcronym = rd.GetOrdinal("Acronym");
                    int colID = rd.GetOrdinal("DepartamentID");
                    int colCode = rd.GetOrdinal("DigitalCode");
                    int colIsValid = rd.GetOrdinal("IsValid");
                    while (rd.Read())
                    {
                        departament = new Departament(isvalid: rd.GetBoolean(colIsValid),
                                                      title: rd.GetString(colTitle),
                                                      acronym: rd.GetString(colAcronym),
                                                      id: rd.GetByte(colID),
                                                      code: rd.GetString(colCode));
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                cmd.Connection.Close();
            }
            return departament;
        }
        public override ICollection<Departament> Items()
        {
            SqlCommand cmd = _storage.DBConnection.CreateCommand();
            cmd.CommandText = "InnResources.prGetDepartaments";
            cmd.CommandType = CommandType.StoredProcedure;
            List<Departament> list = new List<Departament>(5);
            try
            {
                cmd.Connection.Open();
                var rd = cmd.ExecuteReader();
                if (rd.HasRows)
                {
                    int colTitle = rd.GetOrdinal("Title");
                    int colAcronym = rd.GetOrdinal("Acronym");
                    int colID = rd.GetOrdinal("DepartamentID");
                    int colCode = rd.GetOrdinal("DigitalCode");
                    int colIsValid = rd.GetOrdinal("IsValid");
                    while (rd.Read())
                    {
                        list.Add(new Departament(isvalid: rd.GetBoolean(colIsValid),
                                                 title: rd.GetString(colTitle),
                                                 acronym: rd.GetString(colAcronym),
                                                 id: rd.GetByte(colID),
                                                 code: rd.GetString(colCode))
                                );
                    }
                }
            }
            catch (Exception)
            {

                throw;
            }
            finally
            {
                cmd.Connection.Close();
            }
            return list;
        }
        protected override void Add(Departament item, SqlConnection connection, SqlTransaction tran = null)
        {
            throw new NotImplementedException();
        }
        protected override void Edit(Departament item, SqlConnection connection, SqlTransaction tran = null)
        {
            throw new NotImplementedException();
        }
        public DepartamentDataAccess(ILocalStorage storage) : base(storage) { }
    }
    public class DepartamentDataAccessCashed : DepartamentDataAccess
    {
        ConcurrentDictionary<int, Departament> _cache = new ConcurrentDictionary<int, Departament>();
        bool _has_full_access;

        public override Departament GetItemByID(int id)
        {
            if (_cache.ContainsKey(id)) return _cache[id];
            else
            {
                var d = base.GetItemByID(id);
                _cache.TryAdd(id, d);
                return d;
            }
        }
        public override ICollection<Departament> Items()
        {
            if (_has_full_access) return _cache.Values;
            else
            {
                SqlCommand cmd = _storage.DBConnection.CreateCommand();
                cmd.CommandText = "InnResources.prGetDepartaments";
                cmd.CommandType = CommandType.StoredProcedure;
                try
                {
                    cmd.Connection.Open();
                    var rd = cmd.ExecuteReader();
                    if (rd.HasRows)
                    {
                        int colTitle = rd.GetOrdinal("Title");
                        int colAcronym = rd.GetOrdinal("Acronym");
                        int colID = rd.GetOrdinal("DepartamentID");
                        int colCode = rd.GetOrdinal("DigitalCode");
                        int colIsValid = rd.GetOrdinal("IsValid");
                        while (rd.Read())
                        {
                            if (!_cache.ContainsKey(rd.GetByte(colID)))
                            {
                                var d = new Departament(isvalid: rd.GetBoolean(colIsValid),
                                                        title: rd.GetString(colTitle),
                                                        acronym: rd.GetString(colAcronym),
                                                        id: rd.GetByte(colID),
                                                        code: rd.GetString(colCode));
                                _cache.TryAdd(d.ID, d);
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    throw;
                }
                finally
                {
                    cmd.Connection.Close();
                }
                _has_full_access = true;
                return _cache.Values;
            }
        }
        protected override void Edit(Departament item, SqlConnection connection, SqlTransaction tran = null)
        {
            base.Edit(item, connection, tran);
            EditByID(_cache, item);
        }
        public override void Delete(Departament item)
        {
            base.Delete(item);
            _cache.TryRemove(item.ID, out Departament _);
        }
        protected override void Add(Departament item, SqlConnection connection, SqlTransaction tran = null)
        {
            base.Add(item, connection, tran);
            _cache.TryAdd(item.ID, item);
        }
        public DepartamentDataAccessCashed(ILocalStorage storage) : base(storage) { }
    }

    public class EmployeeDataAccess : DataAccess<Employee>
    {
        public override ICollection<Employee> Items()
        {
            SqlCommand cmd = _storage.DBConnection.CreateCommand();
            cmd.CommandText = "InnResources.prGetEmployees_short";
            cmd.CommandType = CommandType.StoredProcedure;
            List<Employee> list = new List<Employee>(60); //TODO: replace constant
            try
            {
                cmd.Connection.Open();
                var rd = cmd.ExecuteReader();
                if (rd.HasRows)
                {
                    int colFirstName = rd.GetOrdinal("FirstName");
                    int colMiddleName = rd.GetOrdinal("MiddleName");
                    int colSecondName = rd.GetOrdinal("SecondName");
                    int colDeclinated = rd.GetOrdinal("Declinated");
                    int colDepartament = rd.GetOrdinal("DepartamentID");
                    int colEmployeeID = rd.GetOrdinal("EmployeeID");
                    int colGender = rd.GetOrdinal("Gender");
                    int colInnerOffice = rd.GetOrdinal("InnerOfficeID");
                    int colCondition = rd.GetOrdinal("EmployeeStatusID");
                    int colProfile = rd.GetOrdinal("PermissionProfile");
                    int colPassword = rd.GetOrdinal("UserPassword");
                    int colFoto = rd.GetOrdinal("Foto");
                    int colUpdate = rd.GetOrdinal("UpdateDate");
                    while (rd.Read())
                    {
                        list.Add(new Employee(id: rd.GetInt32(colEmployeeID),
                                            departament: App.Storage.DepartamentsAccessService.GetItemByID(rd.GetByte(colDepartament)),
                                            office: rd.GetString(colInnerOffice),
                                            firstname: rd.GetString(colFirstName),
                                            middlename: rd.GetString(colMiddleName),
                                            secondname: rd.GetString(colSecondName),
                                            gender: rd.GetBoolean(colGender),
                                            declinated: rd.GetBoolean(colDeclinated),
                                            emplstatus: rd.GetString(colCondition),
                                            foto: rd.IsDBNull(colFoto) ? null : (byte[])rd[colFoto],
                                            profile: (PermissionProfile)rd.GetByte(colProfile),
                                            password: rd.IsDBNull(colPassword) ? null : rd.GetString(colPassword),
                                            vr: Version.Original,
                                            updatedate: rd.GetDateTime(colUpdate))
                               );
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                cmd.Connection.Close();
            }
            return list;
        }
        protected override void Add(Employee item, SqlConnection connection, SqlTransaction tran = null)
        {
            SqlCommand cmd = connection.CreateCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "InnResources.prAddEmployee";
            cmd.Transaction = tran;
            cmd.Parameters.Add("@WPhone", SqlDbType.VarChar, 20).Value = ConvertToDBNull(item.Employee_SlightPart.Workphone);
            cmd.Parameters.Add("@Educ_1", SqlDbType.NVarChar, 150).Value = ConvertToDBNull(item.Employee_SlightPart.Education1);
            cmd.Parameters.Add("@Educ_2", SqlDbType.NVarChar, 150).Value = ConvertToDBNull(item.Employee_SlightPart.Education2);
            cmd.Parameters.Add("@Educ_3", SqlDbType.NVarChar, 150).Value = ConvertToDBNull(item.Employee_SlightPart.Education3);
            cmd.Parameters.Add("@Science", SqlDbType.NVarChar, 250).Value = ConvertToDBNull(item.Employee_SlightPart.Sciencedegree);
            cmd.Parameters.Add("@SettlementID", SqlDbType.Int).Value = ConvertToDBNull(item.Employee_SlightPart.Adress.Settlement.ID);
            cmd.Parameters.Add("@StreetPrefix", SqlDbType.NVarChar, 20).Value = ConvertToDBNull(item.Employee_SlightPart.Adress.StreetPrefix);
            cmd.Parameters.Add("@Street", SqlDbType.NVarChar, 40).Value = ConvertToDBNull(item.Employee_SlightPart.Adress.Street);
            cmd.Parameters.Add("@Housing", SqlDbType.NVarChar, 8).Value = ConvertToDBNull(item.Employee_SlightPart.Adress.Housing);
            cmd.Parameters.Add("@Flat", SqlDbType.NVarChar, 8).Value = ConvertToDBNull(item.Employee_SlightPart.Adress.Flat);
            cmd.Parameters.Add("@Corpus", SqlDbType.NVarChar, 12).Value = ConvertToDBNull(item.Employee_SlightPart.Adress.Corpus);
            cmd.Parameters.Add("@Structure", SqlDbType.NVarChar, 12).Value = ConvertToDBNull(item.Employee_SlightPart.Adress.Structure);
            cmd.Parameters.Add("@Mphone", SqlDbType.VarChar, 20).Value = ConvertToDBNull(item.Employee_SlightPart.Mobilephone);
            cmd.Parameters.Add("@Email", SqlDbType.NVarChar, 50).Value = ConvertToDBNull(item.Employee_SlightPart.Email);
            cmd.Parameters.Add("@FN", SqlDbType.NVarChar, 25).Value = item.Fname;
            cmd.Parameters.Add("@SN", SqlDbType.NVarChar, 25).Value = item.Sname;
            cmd.Parameters.Add("@MN", SqlDbType.NVarChar, 25).Value = item.Mname;
            cmd.Parameters.Add("@Declinated", SqlDbType.Bit).Value = item.IsDeclinated;
            cmd.Parameters.Add("@Departament", SqlDbType.TinyInt).Value = ConvertToDBNull(item.Departament.ID);
            cmd.Parameters.Add("@InnerOffice", SqlDbType.NVarChar, 60).Value = item.Inneroffice;
            cmd.Parameters.Add("@Gend", SqlDbType.Bit).Value = item.Gender;
            cmd.Parameters.Add("@Birth", SqlDbType.Date).Value = ConvertToDBNull(item.Employee_SlightPart.Birthdate);
            cmd.Parameters.Add("@Hire", SqlDbType.Date).Value = ConvertToDBNull(item.Employee_SlightPart.Hiredate);
            cmd.Parameters.Add("@EmployeeStatusID", SqlDbType.NVarChar, 30).Value = item.EmployeeStatus;
            cmd.Parameters.Add("@foto", SqlDbType.VarBinary).Value = ConvertToDBNull(item.Foto);
            cmd.Parameters.Add("@Profile", SqlDbType.TinyInt).Value = (byte)item.Profile;
            var par = cmd.Parameters.Add("@InsertedID", SqlDbType.Int);
            par.Direction = ParameterDirection.Output;
            try
            {
                cmd.ExecuteNonQuery();
                item.ID = (int)cmd.Parameters["@InsertedID"].Value;
                item.Version = Version.Original;
            }
            catch (Exception)
            {
                throw;
            }
        }
        protected override void Edit(Employee item, SqlConnection connection, SqlTransaction tran = null)
        {
            SqlCommand cmd = connection.CreateCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "InnResources.prEditEmployee";
            cmd.Transaction = tran;
            cmd.Parameters.Add("@WPhone", SqlDbType.VarChar, 20).Value = ConvertToDBNull(item.Employee_SlightPart.Workphone);
            cmd.Parameters.Add("@Educ_1", SqlDbType.NVarChar, 150).Value = ConvertToDBNull(item.Employee_SlightPart.Education1);
            cmd.Parameters.Add("@Educ_2", SqlDbType.NVarChar, 150).Value = ConvertToDBNull(item.Employee_SlightPart.Education2);
            cmd.Parameters.Add("@Educ_3", SqlDbType.NVarChar, 150).Value = ConvertToDBNull(item.Employee_SlightPart.Education3);
            cmd.Parameters.Add("@Science", SqlDbType.NVarChar, 250).Value = ConvertToDBNull(item.Employee_SlightPart.Sciencedegree);
            cmd.Parameters.Add("@SettlementID", SqlDbType.Int).Value = ConvertToDBNull(item.Employee_SlightPart.Adress?.Settlement?.ID);
            cmd.Parameters.Add("@StreetPrefix", SqlDbType.NVarChar, 20).Value = ConvertToDBNull(item.Employee_SlightPart.Adress?.StreetPrefix);
            cmd.Parameters.Add("@Street", SqlDbType.NVarChar, 40).Value = ConvertToDBNull(item.Employee_SlightPart.Adress?.Street);
            cmd.Parameters.Add("@Housing", SqlDbType.NVarChar, 8).Value = ConvertToDBNull(item.Employee_SlightPart.Adress?.Housing);
            cmd.Parameters.Add("@Flat", SqlDbType.NVarChar, 8).Value = ConvertToDBNull(item.Employee_SlightPart.Adress?.Flat);
            cmd.Parameters.Add("@Corpus", SqlDbType.NVarChar, 12).Value = ConvertToDBNull(item.Employee_SlightPart.Adress?.Corpus);
            cmd.Parameters.Add("@Structure", SqlDbType.NVarChar, 12).Value = ConvertToDBNull(item.Employee_SlightPart.Adress?.Structure);
            cmd.Parameters.Add("@Mphone", SqlDbType.VarChar, 20).Value = ConvertToDBNull(item.Employee_SlightPart.Mobilephone);
            cmd.Parameters.Add("@Email", SqlDbType.NVarChar, 50).Value = ConvertToDBNull(item.Employee_SlightPart.Email);
            cmd.Parameters.Add("@FN", SqlDbType.NVarChar, 25).Value = item.Fname;
            cmd.Parameters.Add("@SN", SqlDbType.NVarChar, 25).Value = item.Sname;
            cmd.Parameters.Add("@MN", SqlDbType.NVarChar, 25).Value = item.Mname;
            cmd.Parameters.Add("@Declinated", SqlDbType.Bit).Value = item.IsDeclinated;
            cmd.Parameters.Add("@Departament", SqlDbType.TinyInt).Value = ConvertToDBNull(item.Departament.ID);
            cmd.Parameters.Add("@InnerOffice", SqlDbType.NVarChar, 60).Value = item.Inneroffice;
            cmd.Parameters.Add("@Gend", SqlDbType.Bit).Value = item.Gender;
            cmd.Parameters.Add("@Birth", SqlDbType.Date).Value = ConvertToDBNull(item.Employee_SlightPart.Birthdate);
            cmd.Parameters.Add("@Hire", SqlDbType.Date).Value = ConvertToDBNull(item.Employee_SlightPart.Hiredate);
            cmd.Parameters.Add("@EmployeeStatusID", SqlDbType.NVarChar, 30).Value = item.EmployeeStatus;
            cmd.Parameters.Add("@foto", SqlDbType.VarBinary).Value = ConvertToDBNull(item.Foto);
            cmd.Parameters.Add("@Profile", SqlDbType.TinyInt).Value = (byte)item.Profile;
            cmd.Parameters.Add("@EmployeeID", SqlDbType.Int).Value = item.ID;
            try
            {
                cmd.ExecuteNonQuery();
                item.Version = Version.Original;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public override void Delete(Employee item)
        {
            SqlCommand cmd = _storage.DBConnection.CreateCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "InnResources.prDeleteEmployee";
            cmd.Parameters.Add("@EmployeeID", SqlDbType.Int).Value = item.ID;
            try
            {
                cmd.Connection.Open();
                cmd.ExecuteNonQuery();
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                cmd.Connection.Close();
            }
        }
        public override void SaveChanges(Employee item, SqlTransaction tran = null)
        {
            if (item == null) return;
            if (tran == null)
            {
                SqlTransaction transaction = null;
                SqlConnection con = _storage.DBConnection;
                try
                {
                    con.Open();
                    transaction = con.BeginTransaction();
                    _storage.SettlementAccessService.SaveChanges(item?.Employee_SlightPart?.Adress?.Settlement, transaction);
                    base.SaveChanges(item, transaction);
                    transaction.Commit();
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    throw;
                }
                finally
                {
                    con.Close();
                }
            }
            else
            {
                try
                {
                    _storage.SettlementAccessService.SaveChanges(item?.Employee_SlightPart?.Adress?.Settlement, tran);
                    base.SaveChanges(item, tran);
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
        public async Task SavePasswordAsync(Employee item, string password)
        {
            SqlCommand cmd = _storage.DBConnection.CreateCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "InnResources.prEditEmployeePassword";
            cmd.Parameters.Add("@password", SqlDbType.NVarChar, 30).Value = password;
            cmd.Parameters.Add("@EmployeeID", SqlDbType.Int).Value = item.ID;
            try
            {
                cmd.Connection.Open();
                await cmd.ExecuteNonQueryAsync();
                item.Password = password;
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                cmd.Connection.Close();
            }
        }
        private Employee_SlightPart LoadSligthPart(Employee item)
        {
            var cmd = _storage.DBConnection.CreateCommand();
            SqlDataReader rd = null;
            Employee_SlightPart part = null;
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "InnResources.prEmployee_EndLoad";
            cmd.Parameters.Add("@EmployeeID", SqlDbType.Int).Value = item.ID;
            try
            {
                cmd.Connection.Open();
                rd = cmd.ExecuteReader();
                if (rd.HasRows)
                {
                    rd.Read();
                    Adress adress = new Adress(settlement: rd.IsDBNull(7) ? null : App.Storage.SettlementAccessService.GetItemByID(rd.GetInt32(7)),
                                                streetprefix: rd.IsDBNull(9) ? null : rd.GetString(9),
                                                street: rd.IsDBNull(8) ? null : rd.GetString(8),
                                                housing: rd.IsDBNull(5) ? null : rd.GetString(5),
                                                flat: rd.IsDBNull(4) ? null : rd.GetString(4),
                                                corpus: rd.IsDBNull(0) ? null : rd.GetString(0),
                                                structure: rd.IsDBNull(10) ? null : rd.GetString(10));
                    part = new Employee_SlightPart(
                    education1: rd.IsDBNull(1) ? null : rd.GetString(1),
                    education2: rd.IsDBNull(2) ? null : rd.GetString(2),
                    education3: rd.IsDBNull(3) ? null : rd.GetString(3),
                    sciencedegree: rd.IsDBNull(6) ? null : rd.GetString(6),
                    mobilephone: rd.IsDBNull(11) ? null : rd.GetString(11),
                    workphone: rd.IsDBNull(12) ? null : rd.GetString(12),
                    adress: adress,
                    email: rd.IsDBNull(13) ? null : rd.GetString(13),
                    hiredate: rd.IsDBNull(14) ? null : new DateTime?(rd.GetDateTime(14)),
                    birthdate: rd.IsDBNull(15) ? null : new DateTime?(rd.GetDateTime(15)),
                    hidepersonal: rd.GetBoolean(16));
                }
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                rd?.Close();
                cmd.Connection.Close();
            }
            return part;
        }
        public async Task<Employee_SlightPart> LoadSlightPartAsync(Employee item)
        {
            item.UploadStatus = UploadResult.Performing;
            var task = Task.Run<Employee_SlightPart>(() => LoadSligthPart(item));
            var result = await task;
            if (task.IsFaulted) item.UploadStatus = UploadResult.Error;
            else item.UploadStatus = UploadResult.Sucsess;
            return result;
        }
        public override Employee GetItemByID(int id)
        {
            SqlCommand cmd = _storage.DBConnection.CreateCommand();
            cmd.CommandText = "InnResources.prGetEmployeeByID";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("@EmployeeID", SqlDbType.Int).Value = id;
            Employee empl = null;
            try
            {
                cmd.Connection.Open();
                var rd = cmd.ExecuteReader();
                if (rd.HasRows)
                {
                    int colFirstName = rd.GetOrdinal("FirstName");
                    int colMiddleName = rd.GetOrdinal("MiddleName");
                    int colSecondName = rd.GetOrdinal("SecondName");
                    int colDeclinated = rd.GetOrdinal("Declinated");
                    int colDepartament = rd.GetOrdinal("DepartamentID");
                    int colEmployeeID = rd.GetOrdinal("EmployeeID");
                    int colGender = rd.GetOrdinal("Gender");
                    int colInnerOffice = rd.GetOrdinal("InnerOfficeID");
                    int colCondition = rd.GetOrdinal("EmployeeStatusID");
                    int colProfile = rd.GetOrdinal("PermissionProfile");
                    int colPassword = rd.GetOrdinal("UserPassword");
                    int colFoto = rd.GetOrdinal("Foto");
                    int colUpdate = rd.GetOrdinal("UpdateDate");
                    while (rd.Read())
                    {
                        empl = new Employee(id: rd.GetInt32(colEmployeeID),
                                            departament: App.Storage.DepartamentsAccessService.GetItemByID(rd.GetByte(colDepartament)),
                                            office: rd.GetString(colInnerOffice),
                                            firstname: rd.GetString(colFirstName),
                                            middlename: rd.GetString(colMiddleName),
                                            secondname: rd.GetString(colSecondName),
                                            gender: rd.GetBoolean(colGender),
                                            declinated: rd.GetBoolean(colDeclinated),
                                            emplstatus: rd.GetString(colCondition),
                                            foto: rd.IsDBNull(colFoto) ? null : (byte[])rd[colFoto],
                                            profile: (PermissionProfile)rd.GetByte(colProfile),
                                            password: rd.IsDBNull(colPassword) ? null : rd.GetString(colPassword),
                                            vr: Version.Original,
                                            updatedate: rd.GetDateTime(colUpdate));
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                cmd.Connection.Close();
            }
            return empl;
        }
        public async Task<List<int>> LoadAnnualDatesAsync(DateTime date)
        {
            SqlCommand cmd = _storage.DBConnection.CreateCommand();
            cmd.CommandText = "InnResources.prEmployeesAnnualDates";
            cmd.CommandType = CommandType.StoredProcedure;
            SqlDataReader rd = null;
            List<int> dates = new List<int>(5);
            try
            {
                cmd.Connection.Open();
                rd = await cmd.ExecuteReaderAsync();
                if (rd.HasRows)
                {
                    while (rd.Read())
                    {
                        dates.Add(rd.GetInt32(0));
                    }
                }
            }
            catch (Exception)
            {

            }
            finally
            {
                rd?.Close();
                cmd.Connection.Close();
            }
            return dates;
        }
        public EmployeeDataAccess(ILocalStorage storage) : base(storage) { }
    }
    public class EmployeeDataAccessCached : EmployeeDataAccess
    {
        ConcurrentDictionary<int, Employee> _cache = new ConcurrentDictionary<int, Employee>();
        bool _has_full_access;

        public override Employee GetItemByID(int id)
        {
            if (_cache.ContainsKey(id)) return _cache[id];
            else
            {
                var d = base.GetItemByID(id);
                _cache.TryAdd(id, d);
                return d;
            }
        }
        public override ICollection<Employee> Items()
        {
            if (_has_full_access) return _cache.Values;
            else
            {
                SqlCommand cmd = _storage.DBConnection.CreateCommand();
                cmd.CommandText = "InnResources.prGetEmployees_short";
                cmd.CommandType = CommandType.StoredProcedure;
                try
                {
                    cmd.Connection.Open();
                    var rd = cmd.ExecuteReader();
                    if (rd.HasRows)
                    {
                        int colFirstName = rd.GetOrdinal("FirstName");
                        int colMiddleName = rd.GetOrdinal("MiddleName");
                        int colSecondName = rd.GetOrdinal("SecondName");
                        int colDeclinated = rd.GetOrdinal("Declinated");
                        int colDepartament = rd.GetOrdinal("DepartamentID");
                        int colEmployeeID = rd.GetOrdinal("EmployeeID");
                        int colGender = rd.GetOrdinal("Gender");
                        int colInnerOffice = rd.GetOrdinal("InnerOfficeID");
                        int colCondition = rd.GetOrdinal("EmployeeStatusID");
                        int colProfile = rd.GetOrdinal("PermissionProfile");
                        int colPassword = rd.GetOrdinal("UserPassword");
                        int colFoto = rd.GetOrdinal("Foto");
                        int colUpdate = rd.GetOrdinal("UpdateDate");
                        while (rd.Read())
                        {
                            if (!_cache.ContainsKey(rd.GetInt32(colEmployeeID)))
                            {
                                var e = new Employee(id: rd.GetInt32(colEmployeeID),
                                                departament: App.Storage.DepartamentsAccessService.GetItemByID(rd.GetByte(colDepartament)),
                                                office: rd.GetString(colInnerOffice),
                                                firstname: rd.GetString(colFirstName),
                                                middlename: rd.GetString(colMiddleName),
                                                secondname: rd.GetString(colSecondName),
                                                gender: rd.GetBoolean(colGender),
                                                declinated: rd.GetBoolean(colDeclinated),
                                                emplstatus: rd.GetString(colCondition),
                                                foto: rd.IsDBNull(colFoto) ? null : (byte[])rd[colFoto],
                                                profile: (PermissionProfile)rd.GetByte(colProfile),
                                                password: rd.IsDBNull(colPassword) ? null : rd.GetString(colPassword),
                                                vr: Version.Original,
                                                updatedate: rd.GetDateTime(colUpdate));
                                _cache.TryAdd(e.ID, e);
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    throw;
                }
                finally
                {
                    cmd.Connection.Close();
                }
                _has_full_access = true;
                return _cache.Values;
            }
        }
        protected override void Edit(Employee item, SqlConnection connection, SqlTransaction tran = null)
        {
            base.Edit(item, connection, tran);
            EditByID(_cache, item);
        }
        protected override void Add(Employee item, SqlConnection connection, SqlTransaction tran = null)
        {
            base.Add(item, connection, tran);
            _cache.TryAdd(item.ID, item);
        }
        public override void Delete(Employee item)
        {
            base.Delete(item);
            _cache.TryRemove(item.ID, out Employee _);
        }
        public EmployeeDataAccessCached(ILocalStorage storage) : base(storage) { }
    }

    public class ExpertDataAccess : DataAccess<Expert>//TODO: create cashed subclass
    {
        public override ICollection<Expert> Items()
        {
            SqlCommand cmd = _storage.DBConnection.CreateCommand();
            cmd.CommandText = "InnResources.prGetExperts";
            cmd.CommandType = CommandType.StoredProcedure;
            List<Expert> list = new List<Expert>(120); //TODO: remove constant
            try
            {
                cmd.Connection.Open();
                var rd = cmd.ExecuteReader();
                if (rd.HasRows)
                {
                    int colExpertID = rd.GetOrdinal("ExpertID");
                    int colEmployeeID = rd.GetOrdinal("EmployeeID");
                    int colSpecialityID = rd.GetOrdinal("SpecialityID");
                    int colExperience = rd.GetOrdinal("Experience");
                    int colLastAtt = rd.GetOrdinal("LastAttestation");
                    int colClosed = rd.GetOrdinal("IsClosed");
                    int colExpertModify = rd.GetOrdinal("UpdateDate");
                    while (rd.Read())
                    {
                        list.Add(new Expert(id: rd.GetInt32(colExpertID),
                                            speciality: App.Storage.SpecialityAccessService.GetItemByID(rd.GetInt16(colSpecialityID)),
                                            employee: App.Storage.EmployeeAccessService.GetItemByID(rd.GetInt32(colEmployeeID)),
                                            receiptdate: rd.GetDateTime(colExperience),
                                            lastattestationdate: rd.IsDBNull(colLastAtt) ? null : new DateTime?(rd.GetDateTime(colLastAtt)),
                                            closed: rd.GetBoolean(colClosed),
                                            vr: Version.Original,
                                            updatedate: rd.GetDateTime(colExpertModify))
                                );
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                cmd.Connection.Close();
            }
            return list;
        }
        protected override void Add(Expert item, SqlConnection connection, SqlTransaction tran = null)
        {
            SqlCommand cmd = connection.CreateCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "InnResources.prAddExpert";
            cmd.Transaction = tran;
            cmd.Parameters.Add("@SpecialityID", SqlDbType.SmallInt).Value = item.Speciality.ID;
            cmd.Parameters.Add("@EmployeeID", SqlDbType.Int).Value = item.Employee.ID;
            cmd.Parameters.Add("@Experience", SqlDbType.Date).Value = item.ReceiptDate;
            cmd.Parameters.Add("@LastAttestation", SqlDbType.Date).Value = ConvertToDBNull(item.LastAttestationDate);
            cmd.Parameters.Add("@Closed", SqlDbType.Bit).Value = item.IsClosed;
            var par = cmd.Parameters.Add("@InsertedID", SqlDbType.Int);
            par.Direction = ParameterDirection.Output;
            try
            {
                cmd.ExecuteNonQuery();
                item.ID = (int)cmd.Parameters["@InsertedID"].Value;
                item.Version = Version.Original;
            }
            catch (Exception)
            {
                throw;
            }
        }
        protected override void Edit(Expert item, SqlConnection connection, SqlTransaction tran = null)
        {
            SqlCommand cmd = connection.CreateCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "InnResources.prEditExpert";
            cmd.Transaction = tran;
            cmd.Parameters.Add("@Experience", SqlDbType.Date).Value = item.ReceiptDate;
            cmd.Parameters.Add("@LastAttestation", SqlDbType.Date).Value = ConvertToDBNull(item.LastAttestationDate);
            cmd.Parameters.Add("@Closed", SqlDbType.Bit).Value = item.IsClosed;
            cmd.Parameters.Add("@ExpertID", SqlDbType.Int).Value = item.ID;
            try
            {
                cmd.ExecuteNonQuery();
                item.Version = Version.Original;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public override void Delete(Expert item)
        {
            SqlCommand cmd = _storage.DBConnection.CreateCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "InnResources.prDeleteExpert";
            cmd.Parameters.Add("@ExpertID", SqlDbType.Int).Value = item.ID;
            try
            {
                cmd.Connection.Open();
                cmd.ExecuteNonQuery();
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                cmd.Connection.Close();
            }
        }
        public override void SaveChanges(Expert item, SqlTransaction tran = null)
        {
            if (item == null) return;
            if (tran == null)
            {
                SqlTransaction transaction = null;
                SqlConnection con = _storage.DBConnection;
                try
                {
                    con.Open();
                    transaction = con.BeginTransaction();
                    _storage.SpecialityAccessService.SaveChanges(item.Speciality, transaction);
                    _storage.EmployeeAccessService.SaveChanges(item.Employee, transaction);
                    base.SaveChanges(item, transaction);
                    transaction.Commit();
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    throw;
                }
                finally
                {
                    con.Close();
                }
            }
            else
            {
                try
                {
                    _storage.SpecialityAccessService.SaveChanges(item.Speciality, tran);
                    _storage.EmployeeAccessService.SaveChanges(item.Employee, tran);
                    base.SaveChanges(item, tran);
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
        public override Expert GetItemByID(int id)
        {
            Expert expert = null;
            SqlCommand cmd = _storage.DBConnection.CreateCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "InnResources.prGetExpertByID";
            cmd.Parameters.Add("@ExpertID", SqlDbType.Int).Value = id;
            try
            {
                cmd.Connection.Open();
                var rd = cmd.ExecuteReader();
                if (rd.HasRows)
                {
                    int colExpertID = rd.GetOrdinal("ExpertID");
                    int colEmployeeID = rd.GetOrdinal("EmployeeID");
                    int colSpecialityID = rd.GetOrdinal("SpecialityID");
                    int colExperience = rd.GetOrdinal("Experience");
                    int colLastAtt = rd.GetOrdinal("LastAttestation");
                    int colClosed = rd.GetOrdinal("IsClosed");
                    int colExpertModify = rd.GetOrdinal("UpdateDate");
                    while (rd.Read())
                    {
                        expert = new Expert(id: rd.GetInt32(colExpertID),
                                            speciality: App.Storage.SpecialityAccessService.GetItemByID(rd.GetInt16(colSpecialityID)),
                                            employee: App.Storage.EmployeeAccessService.GetItemByID(rd.GetInt32(colEmployeeID)),
                                            receiptdate: rd.GetDateTime(colExperience),
                                            lastattestationdate: rd.IsDBNull(colLastAtt) ? null : new DateTime?(rd.GetDateTime(colLastAtt)),
                                            closed: rd.GetBoolean(colClosed),
                                            vr: Version.Original,
                                            updatedate: rd.GetDateTime(colExpertModify));
                    }
                }
            }
            catch
            {
                throw;
            }
            finally
            {
                cmd.Connection.Close();
            }
            return expert;
        }
        public List<Expert> LoadExpertSpecialities(Employee employee)
        {
            SqlCommand cmd = _storage.DBConnection.CreateCommand();
            cmd.CommandText = "InnResources.prGetExpertsByEmployeeID";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("@EmployeeID", SqlDbType.Int).Value = employee.ID;
            List<Expert> list = new List<Expert>(7);
            try
            {
                cmd.Connection.Open();
                var rd = cmd.ExecuteReader();
                if (rd.HasRows)
                {
                    int colExpertID = rd.GetOrdinal("ExpertID");
                    int colEmployeeID = rd.GetOrdinal("EmployeeID");
                    int colSpecialityID = rd.GetOrdinal("SpecialityID");
                    int colExperience = rd.GetOrdinal("Experience");
                    int colLastAtt = rd.GetOrdinal("LastAttestation");
                    int colClosed = rd.GetOrdinal("IsClosed");
                    int colExpertModify = rd.GetOrdinal("UpdateDate");
                    while (rd.Read())
                    {
                        list.Add(new Expert(id: rd.GetInt32(colExpertID),
                                            speciality: App.Storage.SpecialityAccessService.GetItemByID(rd.GetInt16(colSpecialityID)),
                                            employee: employee,
                                            receiptdate: rd.GetDateTime(colExperience),
                                            lastattestationdate: rd.IsDBNull(colLastAtt) ? null : new DateTime?(rd.GetDateTime(colLastAtt)),
                                            closed: rd.GetBoolean(colClosed),
                                            vr: Version.Original,
                                            updatedate: rd.GetDateTime(colExpertModify))
                                );
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                cmd.Connection.Close();
            }
            return list;
        }
        public async Task<List<Expert>> LoadExpertSpecialitiesAsync(Employee employee)
        {
            return await Task.Run(() => LoadExpertSpecialities(employee));
        }
        public ExpertDataAccess(ILocalStorage storage) : base(storage) { }
    }

    public class OrganizationsDataAccess : DataAccess<Organization> //TODO: create cashed subclass
    {
        protected override void Add(Organization item, SqlConnection connection, SqlTransaction tran = null)
        {
            SqlCommand cmd = connection.CreateCommand();
            cmd.Transaction = tran;
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "OutResources.prAddOrganization";
            cmd.Parameters.Add("@Name", SqlDbType.NVarChar, 200).Value = item.Name;
            cmd.Parameters.Add("@ShortName", SqlDbType.NVarChar, 150).Value = ConvertToDBNull(item.ShortName);
            cmd.Parameters.Add("@Post", SqlDbType.Char, 6).Value = item.PostCode;
            cmd.Parameters.Add("@SettlementID", SqlDbType.Int).Value = item.Adress.Settlement.ID;
            cmd.Parameters.Add("@StreetPrefix", SqlDbType.NVarChar, 10).Value = item.Adress.StreetPrefix;
            cmd.Parameters.Add("@Street", SqlDbType.NVarChar, 40).Value = item.Adress.Street;
            cmd.Parameters.Add("@Housing", SqlDbType.NVarChar, 8).Value = item.Adress.Housing;
            cmd.Parameters.Add("@Office", SqlDbType.NVarChar, 12).Value = ConvertToDBNull(item.Adress.Flat);
            cmd.Parameters.Add("@Corpus", SqlDbType.NVarChar, 12).Value = ConvertToDBNull(item.Adress.Corpus);
            cmd.Parameters.Add("@Structure", SqlDbType.NVarChar, 12).Value = ConvertToDBNull(item.Adress.Structure);
            cmd.Parameters.Add("@Telephone", SqlDbType.VarChar, 20).Value = ConvertToDBNull(item.Telephone);
            cmd.Parameters.Add("@Telephone_2", SqlDbType.VarChar, 20).Value = ConvertToDBNull(item.Telephone2);
            cmd.Parameters.Add("@Fax", SqlDbType.VarChar, 20).Value = ConvertToDBNull(item.Fax);
            cmd.Parameters.Add("@Email", SqlDbType.NVarChar, 50).Value = ConvertToDBNull(item.Email);
            cmd.Parameters.Add("@WebSite", SqlDbType.NVarChar, 50).Value = ConvertToDBNull(item.WebSite);
            var par = cmd.Parameters.Add("@InsertedID", SqlDbType.Int);
            par.Direction = ParameterDirection.Output;
            try
            {
                cmd.ExecuteNonQuery();
                item.ID = (int)cmd.Parameters["@InsertedID"].Value;
                item.Version = Version.Original;
            }
            catch (Exception)
            {
                throw;
            }
        }
        protected override void Edit(Organization item, SqlConnection connection, SqlTransaction tran = null)
        {
            SqlCommand cmd = connection.CreateCommand();
            cmd.Transaction = tran;
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "OutResources.prEditOrganization";
            cmd.Parameters.Add("@Name", SqlDbType.NVarChar, 200).Value = item.Name;
            cmd.Parameters.Add("@ShortName", SqlDbType.NVarChar, 150).Value = ConvertToDBNull(item.ShortName);
            cmd.Parameters.Add("@Post", SqlDbType.Char, 6).Value = item.PostCode;
            cmd.Parameters.Add("@SettlementID", SqlDbType.Int).Value = item.Adress.Settlement.ID;
            cmd.Parameters.Add("@StreetPrefix", SqlDbType.NVarChar, 10).Value = item.Adress.StreetPrefix;
            cmd.Parameters.Add("@Street", SqlDbType.NVarChar, 40).Value = item.Adress.Street;
            cmd.Parameters.Add("@Housing", SqlDbType.NVarChar, 8).Value = item.Adress.Housing;
            cmd.Parameters.Add("@Office", SqlDbType.NVarChar, 12).Value = ConvertToDBNull(item.Adress.Flat);
            cmd.Parameters.Add("@Corpus", SqlDbType.NVarChar, 12).Value = ConvertToDBNull(item.Adress.Corpus);
            cmd.Parameters.Add("@Structure", SqlDbType.NVarChar, 12).Value = ConvertToDBNull(item.Adress.Structure);
            cmd.Parameters.Add("@Telephone", SqlDbType.VarChar, 20).Value = ConvertToDBNull(item.Telephone);
            cmd.Parameters.Add("@Telephone_2", SqlDbType.VarChar, 20).Value = ConvertToDBNull(item.Telephone2);
            cmd.Parameters.Add("@Fax", SqlDbType.VarChar, 20).Value = ConvertToDBNull(item.Fax);
            cmd.Parameters.Add("@Email", SqlDbType.NVarChar, 50).Value = ConvertToDBNull(item.Email);
            cmd.Parameters.Add("@WebSite", SqlDbType.NVarChar, 50).Value = ConvertToDBNull(item.WebSite);
            cmd.Parameters.Add("@StatusID", SqlDbType.NVarChar, 30).Value = item.IsValid;
            cmd.Parameters.Add("@OrganizationID", SqlDbType.Int).Value = item.ID;
            try
            {
                cmd.ExecuteNonQuery();
                item.Version = Version.Original;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public override void Delete(Organization item)
        {
            SqlCommand cmd = _storage.DBConnection.CreateCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "OutResources.prDeleteOrganization";
            cmd.Parameters.Add("@id", SqlDbType.Int).Value = item.ID;
            try
            {
                cmd.Connection.Open();
                cmd.ExecuteNonQuery();
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                cmd.Connection.Close();
            }
        }
        public override void SaveChanges(Organization item, SqlTransaction tran = null)
        {
            if (item == null) return;
            if (tran == null)
            {
                SqlTransaction transaction = null;
                SqlConnection con = _storage.DBConnection;
                try
                {
                    con.Open();
                    transaction = con.BeginTransaction();
                    _storage.SettlementAccessService.SaveChanges(item.Adress.Settlement, transaction);
                    base.SaveChanges(item, transaction);
                    transaction.Commit();
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    throw;
                }
                finally
                {
                    con.Close();
                }
            }
            else
            {
                try
                {
                    _storage.SettlementAccessService.SaveChanges(item.Adress.Settlement, tran);
                    base.SaveChanges(item, tran);
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
        public override ICollection<Organization> Items()
        {
            SqlCommand cmd = _storage.DBConnection.CreateCommand();
            cmd.CommandText = "OutResources.prGetOrganizations";
            cmd.CommandType = CommandType.StoredProcedure;
            List<Organization> list = new List<Organization>(500);
            try
            {
                cmd.Connection.Open();
                var rd = cmd.ExecuteReader();
                if (rd.HasRows)
                {
                    int colID = rd.GetOrdinal("OrganizationID");
                    int colName = rd.GetOrdinal("Name");
                    int colShortName = rd.GetOrdinal("ShortName");
                    int colPost = rd.GetOrdinal("Post");
                    int colSettlement = rd.GetOrdinal("SettlementID");
                    int colStreetPrefix = rd.GetOrdinal("StreetPrefix");
                    int colStreet = rd.GetOrdinal("Street");
                    int colHousing = rd.GetOrdinal("Housing");
                    int colOffice = rd.GetOrdinal("Office");
                    int colCorpus = rd.GetOrdinal("Corpus");
                    int colStructure = rd.GetOrdinal("Structure");
                    int colTel = rd.GetOrdinal("Telephone");
                    int colTel2 = rd.GetOrdinal("Telephone_2");
                    int colFax = rd.GetOrdinal("Fax");
                    int colEmail = rd.GetOrdinal("Email");
                    int colWebSite = rd.GetOrdinal("WebSite");
                    int colStatus = rd.GetOrdinal("IsValid");
                    int colUpdateDate = rd.GetOrdinal("UpdateDate");
                    while (rd.Read())
                    {
                        Adress adr = new Adress(settlement: rd.IsDBNull(colSettlement) ? null : App.Storage.SettlementAccessService.GetItemByID(rd.GetInt32(colSettlement)),
                                                streetprefix: rd[colStreetPrefix] == DBNull.Value ? null : rd.GetString(colStreetPrefix),
                                                street: rd[colStreet] == DBNull.Value ? null : rd.GetString(colStreet),
                                                housing: rd[colHousing] == DBNull.Value ? null : rd.GetString(colHousing),
                                                flat: rd[colOffice] == DBNull.Value ? null : rd.GetString(colOffice),
                                                corpus: rd[colCorpus] == DBNull.Value ? null : rd.GetString(colCorpus),
                                                structure: rd[colStructure] == DBNull.Value ? null : rd.GetString(colStructure)
                                                );
                        list.Add(new Organization(id: rd.GetInt32(colID),
                                                   name: rd.GetString(colName),
                                                   shortname: rd[colShortName] == DBNull.Value ? null : rd.GetString(colShortName),
                                                   postcode: rd.GetString(colPost),
                                                   adress: adr,
                                                   telephone: rd[colTel] == DBNull.Value ? null : rd.GetString(colTel),
                                                   telephone2: rd[colTel2] == DBNull.Value ? null : rd.GetString(colTel2),
                                                   fax: rd[colFax] == DBNull.Value ? null : rd.GetString(colFax),
                                                   email: rd[colEmail] == DBNull.Value ? null : rd.GetString(colEmail),
                                                   website: rd[colWebSite] == DBNull.Value ? null : rd.GetString(colWebSite),
                                                   status: rd.GetBoolean(colStatus),
                                                   vr: Version.Original,
                                                   updatedate: rd.GetDateTime(colUpdateDate))
                                 );
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                cmd.Connection.Close();
            }
            return list;
        }
        public override Organization GetItemByID(int id)
        {
            SqlCommand cmd = _storage.DBConnection.CreateCommand();
            cmd.CommandText = "OutResources.prGetOrganizationByID";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("@OrganizationID", SqlDbType.Int).Value = id;
            Organization organization = null;
            try
            {
                cmd.Connection.Open();
                var rd = cmd.ExecuteReader();
                if (rd.HasRows)
                {
                    int colID = rd.GetOrdinal("OrganizationID");
                    int colName = rd.GetOrdinal("Name");
                    int colShortName = rd.GetOrdinal("ShortName");
                    int colPost = rd.GetOrdinal("Post");
                    int colSettlement = rd.GetOrdinal("SettlementID");
                    int colStreetPrefix = rd.GetOrdinal("StreetPrefix");
                    int colStreet = rd.GetOrdinal("Street");
                    int colHousing = rd.GetOrdinal("Housing");
                    int colOffice = rd.GetOrdinal("Office");
                    int colCorpus = rd.GetOrdinal("Corpus");
                    int colStructure = rd.GetOrdinal("Structure");
                    int colTel = rd.GetOrdinal("Telephone");
                    int colTel2 = rd.GetOrdinal("Telephone_2");
                    int colFax = rd.GetOrdinal("Fax");
                    int colEmail = rd.GetOrdinal("Email");
                    int colWebSite = rd.GetOrdinal("WebSite");
                    int colStatus = rd.GetOrdinal("IsValid");
                    int colUpdateDate = rd.GetOrdinal("UpdateDate");
                    while (rd.Read())
                    {
                        Adress adr = new Adress(settlement: rd.IsDBNull(colSettlement) ? null : App.Storage.SettlementAccessService.GetItemByID(rd.GetInt32(colSettlement)),
                                                streetprefix: rd[colStreetPrefix] == DBNull.Value ? null : rd.GetString(colStreetPrefix),
                                                street: rd[colStreet] == DBNull.Value ? null : rd.GetString(colStreet),
                                                housing: rd[colHousing] == DBNull.Value ? null : rd.GetString(colHousing),
                                                flat: rd[colOffice] == DBNull.Value ? null : rd.GetString(colOffice),
                                                corpus: rd[colCorpus] == DBNull.Value ? null : rd.GetString(colCorpus),
                                                structure: rd[colStructure] == DBNull.Value ? null : rd.GetString(colStructure)
                                                );
                        organization = new Organization(id: rd.GetInt32(colID),
                                                   name: rd.GetString(colName),
                                                   shortname: rd[colShortName] == DBNull.Value ? null : rd.GetString(colShortName),
                                                   postcode: rd.GetString(colPost),
                                                   adress: adr,
                                                   telephone: rd[colTel] == DBNull.Value ? null : rd.GetString(colTel),
                                                   telephone2: rd[colTel2] == DBNull.Value ? null : rd.GetString(colTel2),
                                                   fax: rd[colFax] == DBNull.Value ? null : rd.GetString(colFax),
                                                   email: rd[colEmail] == DBNull.Value ? null : rd.GetString(colEmail),
                                                   website: rd[colWebSite] == DBNull.Value ? null : rd.GetString(colWebSite),
                                                   status: rd.GetBoolean(colStatus),
                                                   vr: Version.Original,
                                                   updatedate: rd.GetDateTime(colUpdateDate));
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                cmd.Connection.Close();
            }
            return organization;
        }
        public OrganizationsDataAccess(ILocalStorage storage) : base(storage) { }
    }

    public class LaboratoryDataAccess : DataAccess<Laboratory>
    {
        public override Laboratory GetItemByID(int id)
        {
            SqlCommand cmd = _storage.DBConnection.CreateCommand();
            cmd.CommandText = "InnResources.prGetLaboratoryByID";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("@LaboratoryID", SqlDbType.Int).Value = id;
            Laboratory laboratory = null;
            try
            {
                cmd.Connection.Open();
                var rd = cmd.ExecuteReader();
                if (rd.HasRows)
                {
                    int colLaboratoryID = rd.GetOrdinal("LaboratoryID");
                    int colName = rd.GetOrdinal("Name");
                    int colShortName = rd.GetOrdinal("ShortName");
                    int colPost = rd.GetOrdinal("Post");
                    int colSettlementID = rd.GetOrdinal("SettlementID");
                    int colStreetPrefix = rd.GetOrdinal("StreetPrefix");
                    int colStreet = rd.GetOrdinal("Street");
                    int colHousing = rd.GetOrdinal("Housing");
                    int colOffice = rd.GetOrdinal("Office");
                    int colCorpus = rd.GetOrdinal("Corpus");
                    int colStructure = rd.GetOrdinal("Structure");
                    int colTel = rd.GetOrdinal("Telephone");
                    int colTel2 = rd.GetOrdinal("Telephone_2");
                    int colFax = rd.GetOrdinal("Fax");
                    int colEmail = rd.GetOrdinal("Email");
                    int colWebSite = rd.GetOrdinal("WebSite");
                    int colPaymentAcc = rd.GetOrdinal("PaymentAccount");
                    int colPersonalAcc = rd.GetOrdinal("PersonalAccount");
                    int colRecipientBank = rd.GetOrdinal("RecipientBank");
                    int colBankCorrespondentAccount = rd.GetOrdinal("BankCorrespondentAccount");
                    int colBIC = rd.GetOrdinal("BIC");
                    int colINN = rd.GetOrdinal("INN");
                    int colKPP = rd.GetOrdinal("KPP");
                    int colKBK = rd.GetOrdinal("KBK");
                    int colOKMTO = rd.GetOrdinal("OKMTO");
                    int colHourPrice = rd.GetOrdinal("HourPrice");
                    int colHourPriceOrderID = rd.GetOrdinal("HourPriceOrderID");
                    int colRegistrationDate = rd.GetOrdinal("RegistrationDate");
                    int colIsValid = rd.GetOrdinal("IsValid");
                    int colUpdateDate = rd.GetOrdinal("UpdateDate");
                    int colPaidoutPercent = rd.GetOrdinal("PaidoutPercent");
                    while (rd.Read())
                    {
                        Adress _add = new Adress(settlement: App.Storage.SettlementAccessService.GetItemByID(rd.GetInt32(colSettlementID)),
                                                 streetprefix: rd.GetString(colStreetPrefix),
                                                 street: rd.GetString(colStreet),
                                                 housing: rd.GetString(colHousing),
                                                 flat: rd.IsDBNull(colOffice) ? null : rd.GetString(colOffice),
                                                 corpus: rd.IsDBNull(colCorpus) ? null : rd.GetString(colCorpus),
                                                 structure: rd.IsDBNull(colStructure) ? null : rd.GetString(colStructure));
                        laboratory = new Laboratory(id: rd.GetInt32(colLaboratoryID),
                                                name: rd.GetString(colName),
                                                shortname: rd.IsDBNull(colShortName) ? null : rd.GetString(colShortName),
                                                postcode: rd.GetString(colPost),
                                                adress: _add,
                                                telephone: rd.GetString(colTel),
                                                telephone2: rd.IsDBNull(colTel2) ? null : rd.GetString(colTel2),
                                                fax: rd.GetString(colFax),
                                                email: rd.IsDBNull(colEmail) ? null : rd.GetString(colEmail),
                                                website: rd.IsDBNull(colWebSite) ? null : rd.GetString(colWebSite),
                                                status: rd.GetBoolean(colIsValid),
                                                vr: Version.Original,
                                                updatedate: rd.GetDateTime(colUpdateDate),
                                                payment_acc: rd.IsDBNull(colPaymentAcc) ? null : rd.GetString(colPaymentAcc),
                                                personal_acc: rd.IsDBNull(colPersonalAcc) ? null : rd.GetString(colPersonalAcc),
                                                bank_recipient: rd.IsDBNull(colRecipientBank) ? null : rd.GetString(colRecipientBank),
                                                bank_corresp: rd.IsDBNull(colBankCorrespondentAccount) ? null : rd.GetString(colBankCorrespondentAccount),
                                                bic: rd.IsDBNull(colBIC) ? null : rd.GetString(colBIC),
                                                inn: rd.IsDBNull(colINN) ? null : rd.GetString(colINN),
                                                kpp: rd.IsDBNull(colKPP) ? null : rd.GetString(colKPP),
                                                kbk: rd.IsDBNull(colKBK) ? null : rd.GetString(colKBK),
                                                oktmo: rd.IsDBNull(colOKMTO) ? null : rd.GetString(colOKMTO),
                                                price_order_id: rd.GetInt32(colHourPriceOrderID),
                                                hourprice: rd.GetDecimal(colHourPrice),
                                                registrationdate: rd.IsDBNull(colRegistrationDate) ? null : new DateTime?(rd.GetDateTime(colRegistrationDate)),
                                                paidoutpercent: rd.GetDouble(colPaidoutPercent));
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                cmd.Connection.Close();
            }
            return laboratory;
        }
        public override ICollection<Laboratory> Items()
        {
            SqlCommand cmd = _storage.DBConnection.CreateCommand();
            cmd.CommandText = "InnResources.prGetLaboratories";
            cmd.CommandType = CommandType.StoredProcedure;
            List<Laboratory> list = new List<Laboratory>();
            try
            {
                cmd.Connection.Open();
                var rd = cmd.ExecuteReader();
                if (rd.HasRows)
                {
                    int colLaboratoryID = rd.GetOrdinal("LaboratoryID");
                    int colName = rd.GetOrdinal("Name");
                    int colShortName = rd.GetOrdinal("ShortName");
                    int colPost = rd.GetOrdinal("Post");
                    int colSettlementID = rd.GetOrdinal("SettlementID");
                    int colStreetPrefix = rd.GetOrdinal("StreetPrefix");
                    int colStreet = rd.GetOrdinal("Street");
                    int colHousing = rd.GetOrdinal("Housing");
                    int colOffice = rd.GetOrdinal("Office");
                    int colCorpus = rd.GetOrdinal("Corpus");
                    int colStructure = rd.GetOrdinal("Structure");
                    int colTel = rd.GetOrdinal("Telephone");
                    int colTel2 = rd.GetOrdinal("Telephone_2");
                    int colFax = rd.GetOrdinal("Fax");
                    int colEmail = rd.GetOrdinal("Email");
                    int colWebSite = rd.GetOrdinal("WebSite");
                    int colPaymentAcc = rd.GetOrdinal("PaymentAccount");
                    int colPersonalAcc = rd.GetOrdinal("PersonalAccount");
                    int colRecipientBank = rd.GetOrdinal("RecipientBank");
                    int colBankCorrespondentAccount = rd.GetOrdinal("BankCorrespondentAccount");
                    int colBIC = rd.GetOrdinal("BIC");
                    int colINN = rd.GetOrdinal("INN");
                    int colKPP = rd.GetOrdinal("KPP");
                    int colKBK = rd.GetOrdinal("KBK");
                    int colOKMTO = rd.GetOrdinal("OKMTO");
                    int colHourPrice = rd.GetOrdinal("HourPrice");
                    int colHourPriceOrderID = rd.GetOrdinal("HourPriceOrderID");
                    int colRegistrationDate = rd.GetOrdinal("RegistrationDate");
                    int colIsValid = rd.GetOrdinal("IsValid");
                    int colUpdateDate = rd.GetOrdinal("UpdateDate");
                    int colPaidoutPercent = rd.GetOrdinal("PaidoutPercent");
                    while (rd.Read())
                    {
                        Adress _add = new Adress(settlement: App.Storage.SettlementAccessService.GetItemByID(rd.GetInt32(colSettlementID)),
                                                 streetprefix: rd.GetString(colStreetPrefix),
                                                 street: rd.GetString(colStreet),
                                                 housing: rd.GetString(colHousing),
                                                 flat: rd.IsDBNull(colOffice) ? null : rd.GetString(colOffice),
                                                 corpus: rd.IsDBNull(colCorpus) ? null : rd.GetString(colCorpus),
                                                 structure: rd.IsDBNull(colStructure) ? null : rd.GetString(colStructure));
                        list.Add(new Laboratory(id: rd.GetInt32(colLaboratoryID),
                                                name: rd.GetString(colName),
                                                shortname: rd.IsDBNull(colShortName) ? null : rd.GetString(colShortName),
                                                postcode: rd.GetString(colPost),
                                                adress: _add,
                                                telephone: rd.GetString(colTel),
                                                telephone2: rd.IsDBNull(colTel2) ? null : rd.GetString(colTel2),
                                                fax: rd.GetString(colFax),
                                                email: rd.IsDBNull(colEmail) ? null : rd.GetString(colEmail),
                                                website: rd.IsDBNull(colWebSite) ? null : rd.GetString(colWebSite),
                                                status: rd.GetBoolean(colIsValid),
                                                vr: Version.Original,
                                                updatedate: rd.GetDateTime(colUpdateDate),
                                                payment_acc: rd.IsDBNull(colPaymentAcc) ? null : rd.GetString(colPaymentAcc),
                                                personal_acc: rd.IsDBNull(colPersonalAcc) ? null : rd.GetString(colPersonalAcc),
                                                bank_recipient: rd.IsDBNull(colRecipientBank) ? null : rd.GetString(colRecipientBank),
                                                bank_corresp: rd.IsDBNull(colBankCorrespondentAccount) ? null : rd.GetString(colBankCorrespondentAccount),
                                                bic: rd.IsDBNull(colBIC) ? null : rd.GetString(colBIC),
                                                inn: rd.IsDBNull(colINN) ? null : rd.GetString(colINN),
                                                kpp: rd.IsDBNull(colKPP) ? null : rd.GetString(colKPP),
                                                kbk: rd.IsDBNull(colKBK) ? null : rd.GetString(colKBK),
                                                oktmo: rd.IsDBNull(colOKMTO) ? null : rd.GetString(colOKMTO),
                                                price_order_id: rd.GetInt32(colHourPriceOrderID),
                                                hourprice: rd.GetDecimal(colHourPrice),
                                                registrationdate: rd.IsDBNull(colRegistrationDate) ? null : new DateTime?(rd.GetDateTime(colRegistrationDate)),
                                                paidoutpercent: rd.GetDouble(colPaidoutPercent))
                                );
                    }
                }

            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                cmd.Connection.Close();
            }
            return list;
        }
        protected override void Add(Laboratory item, SqlConnection connection, SqlTransaction tran = null)
        {
            throw new NotSupportedException();
        }
        protected override void Edit(Laboratory item, SqlConnection connection, SqlTransaction tran = null)
        {
            SqlCommand cmd = connection.CreateCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "InnResources.prEditLaboratory"; //TODO: remove isvalid field
            cmd.Transaction = tran;
            cmd.Parameters.Add("@Name", SqlDbType.NVarChar, 200).Value = item.Name;
            cmd.Parameters.Add("@ShortName", SqlDbType.NVarChar, 150).Value = ConvertToDBNull(item.ShortName);
            cmd.Parameters.Add("@Post", SqlDbType.Char, 6).Value = item.PostCode;
            cmd.Parameters.Add("@SettlementID", SqlDbType.Int).Value = item.Adress.Settlement.ID;
            cmd.Parameters.Add("@StreetPrefix", SqlDbType.NVarChar, 10).Value = item.Adress.StreetPrefix;
            cmd.Parameters.Add("@Street", SqlDbType.NVarChar, 40).Value = item.Adress.Street;
            cmd.Parameters.Add("@Housing", SqlDbType.NVarChar, 8).Value = item.Adress.Housing;
            cmd.Parameters.Add("@Office", SqlDbType.NVarChar, 12).Value = ConvertToDBNull(item.Adress.Flat);
            cmd.Parameters.Add("@Corpus", SqlDbType.NVarChar, 12).Value = ConvertToDBNull(item.Adress.Corpus);
            cmd.Parameters.Add("@Structure", SqlDbType.NVarChar, 12).Value = ConvertToDBNull(item.Adress.Structure);
            cmd.Parameters.Add("@Telephone", SqlDbType.VarChar, 20).Value = ConvertToDBNull(item.Telephone);
            cmd.Parameters.Add("@Telephone_2", SqlDbType.VarChar, 20).Value = ConvertToDBNull(item.Telephone2);
            cmd.Parameters.Add("@Fax", SqlDbType.VarChar, 20).Value = ConvertToDBNull(item.Fax);
            cmd.Parameters.Add("@Email", SqlDbType.NVarChar, 50).Value = ConvertToDBNull(item.Email);
            cmd.Parameters.Add("@WebSite", SqlDbType.NVarChar, 50).Value = ConvertToDBNull(item.WebSite);
            cmd.Parameters.Add("@PaymentAccount", SqlDbType.VarChar).Value = item.PaymentAccount;
            cmd.Parameters.Add("@PersonalAccount", SqlDbType.NVarChar).Value = item.PersonalAccount;
            cmd.Parameters.Add("@RecipientBank", SqlDbType.NVarChar).Value = item.BankRecipient;
            cmd.Parameters.Add("@BankCorrespondentAccount", SqlDbType.NVarChar).Value = item.BankCorrecpondentAccount;
            cmd.Parameters.Add("@BIC", SqlDbType.NVarChar).Value = item.BIC;
            cmd.Parameters.Add("@INN", SqlDbType.NVarChar).Value = item.INN;
            cmd.Parameters.Add("@KPP", SqlDbType.NVarChar).Value = item.KPP;
            cmd.Parameters.Add("@KBK", SqlDbType.NVarChar).Value = item.KBK;
            cmd.Parameters.Add("@OKTMO", SqlDbType.NVarChar).Value = item.OKTMO;
            cmd.Parameters.Add("@HourPrice", SqlDbType.Money).Value = item.HourPrice;
            cmd.Parameters.Add("@HourPriceOrderID", SqlDbType.Int).Value = item.HourPriceOrder;
            cmd.Parameters.Add("@PaidoutPercent", SqlDbType.Float).Value = item.PaidOutPersent;
            cmd.Parameters.Add("@LaboratoryID", SqlDbType.Int).Value = item.ID;
            try
            {
                cmd.ExecuteNonQuery();
                item.Version = Version.Original;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public override void SaveChanges(Laboratory item, SqlTransaction tran = null)
        {
            if (item == null) return;
            if (tran == null)
            {
                SqlTransaction transaction = null;
                SqlConnection con = _storage.DBConnection;
                try
                {
                    con.Open();
                    transaction = con.BeginTransaction();
                    _storage.SettlementAccessService.SaveChanges(item.Adress.Settlement, transaction);
                    base.SaveChanges(item, transaction);
                    transaction.Commit();
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    throw;
                }
                finally
                {
                    con.Close();
                }
            }
            else
            {
                try
                {
                    _storage.SettlementAccessService.SaveChanges(item.Adress.Settlement, tran);
                    base.SaveChanges(item, tran);
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
        public LaboratoryDataAccess(ILocalStorage storage) : base(storage) { }
    }

    public class CustomerDataAccess : DataAccess<Customer>
    {
        public override ICollection<Customer> Items()
        {
            SqlCommand cmd = _storage.DBConnection.CreateCommand();
            cmd.CommandText = "OutResources.prGetCustomers";
            cmd.CommandType = CommandType.StoredProcedure;
            List<Customer> list = new List<Customer>(800); //TODO: remove constant
            try
            {
                cmd.Connection.Open();
                var rd = cmd.ExecuteReader();
                if (rd.HasRows)
                {
                    int colId = rd.GetOrdinal("CustomerID");
                    int colFirstName = rd.GetOrdinal("FirstName");
                    int colSecondName = rd.GetOrdinal("SecondName");
                    int colMiddleName = rd.GetOrdinal("MiddleName");
                    int colDeclinated = rd.GetOrdinal("Declinated");
                    int colGender = rd.GetOrdinal("Gender");
                    int colWorkPhone = rd.GetOrdinal("WorkPhone");
                    int colMobilePhone = rd.GetOrdinal("MobilePhone");
                    int colOrganization = rd.GetOrdinal("OrganizationID");
                    int colOffice = rd.GetOrdinal("Office");
                    int colRank = rd.GetOrdinal("RankID");
                    int colDep = rd.GetOrdinal("Departament");
                    int colEmail = rd.GetOrdinal("Email");
                    int colStatus = rd.GetOrdinal("IsValid");
                    int colUpdateDate = rd.GetOrdinal("UpdateDate");
                    int colPrevId = rd.GetOrdinal("PreviousID");
                    while (rd.Read())
                    {
                        list.Add(new Customer(id: rd.GetInt32(colId),
                                                previd: rd[colPrevId] == DBNull.Value ? null : new int?(rd.GetInt32(colPrevId)),
                                                firstname: rd.GetString(colFirstName),
                                                secondname: rd.GetString(colSecondName),
                                                middlename: rd.GetString(colMiddleName),
                                                declinated: rd.GetBoolean(colDeclinated),
                                                gender: rd.GetBoolean(colGender),
                                                workphone: rd[colWorkPhone] == DBNull.Value ? null : rd.GetString(colWorkPhone),
                                                mobilephone: rd[colMobilePhone] == DBNull.Value ? null : rd.GetString(colMobilePhone),
                                                organization: rd[colOrganization] == DBNull.Value ? null : _storage.OrganizationAccessService.GetItemByID(rd.GetInt32(colOrganization)),
                                                office: rd[colOffice] == DBNull.Value ? null : rd.GetString(colOffice),
                                                rank: rd[colRank] == DBNull.Value ? null : rd.GetString(colRank),
                                                departament: rd[colDep] == DBNull.Value ? null : rd.GetString(colDep),
                                                email: rd[colEmail] == DBNull.Value ? null : rd.GetString(colEmail),
                                                status: rd.GetBoolean(colStatus),
                                                vr: Version.Original,
                                                updatedate: rd.GetDateTime(colUpdateDate))
                                 );
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                cmd.Connection.Close();
            }
            return list;
        }
        protected override void Add(Customer item, SqlConnection connection, SqlTransaction tran = null)
        {
            SqlCommand cmd = connection.CreateCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "OutResources.prAddCustomer";
            cmd.Transaction = tran;
            cmd.Parameters.Add("@FN", SqlDbType.NVarChar, 25).Value = item.Fname;
            cmd.Parameters.Add("@SN", SqlDbType.NVarChar, 25).Value = item.Sname;
            cmd.Parameters.Add("@MN", SqlDbType.NVarChar, 25).Value = item.Mname;
            cmd.Parameters.Add("@Declinated", SqlDbType.Bit).Value = item.IsDeclinated;
            cmd.Parameters.Add("@Gend", SqlDbType.NVarChar, 15).Value = item.Gender;
            cmd.Parameters.Add("@WorkPhone", SqlDbType.VarChar, 20).Value = ConvertToDBNull(item.Workphone);
            cmd.Parameters.Add("@MobilePhone", SqlDbType.VarChar, 20).Value = ConvertToDBNull(item.Mobilephone);
            cmd.Parameters.Add("@OrgID", SqlDbType.Int).Value = ConvertToDBNull(item.Organization?.ID);
            cmd.Parameters.Add("@Email", SqlDbType.NVarChar, 50).Value = ConvertToDBNull(item.Email);
            cmd.Parameters.Add("@Rank", SqlDbType.NVarChar, 100).Value = ConvertToDBNull(item.Rank);
            cmd.Parameters.Add("@Departament", SqlDbType.NVarChar, 100).Value = ConvertToDBNull(item.Departament);
            cmd.Parameters.Add("@Office", SqlDbType.NVarChar, 150).Value = item.Office;
            var par = cmd.Parameters.Add("@InsertedID", SqlDbType.Int);
            par.Direction = ParameterDirection.Output;
            try
            {
                cmd.ExecuteNonQuery();
                item.ID = (int)cmd.Parameters["@InsertedID"].Value;
                item.Version = Version.Original;
            }
            catch (Exception)
            {
                throw;
            }
        }
        protected override void Edit(Customer item, SqlConnection connection, SqlTransaction tran = null)
        {
            SqlCommand cmd = connection.CreateCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "OutResources.prEditCustomer";
            cmd.Transaction = tran;
            cmd.Parameters.Add("@FN", SqlDbType.NVarChar, 25).Value = item.Fname;
            cmd.Parameters.Add("@SN", SqlDbType.NVarChar, 25).Value = item.Sname;
            cmd.Parameters.Add("@MN", SqlDbType.NVarChar, 25).Value = item.Mname;
            cmd.Parameters.Add("@Declinated", SqlDbType.Bit).Value = item.IsDeclinated;
            cmd.Parameters.Add("@Gend", SqlDbType.NVarChar, 15).Value = item.Gender;
            cmd.Parameters.Add("@WorkPhone", SqlDbType.VarChar, 20).Value = ConvertToDBNull(item.Workphone);
            cmd.Parameters.Add("@MobilePhone", SqlDbType.VarChar, 20).Value = ConvertToDBNull(item.Mobilephone);
            cmd.Parameters.Add("@OrgID", SqlDbType.Int).Value = ConvertToDBNull(item.Organization?.ID);
            cmd.Parameters.Add("@Email", SqlDbType.NVarChar, 50).Value = ConvertToDBNull(item.Email);
            cmd.Parameters.Add("@Rank", SqlDbType.NVarChar, 100).Value = ConvertToDBNull(item.Rank);
            cmd.Parameters.Add("@Departament", SqlDbType.NVarChar, 100).Value = ConvertToDBNull(item.Departament);
            cmd.Parameters.Add("@Office", SqlDbType.NVarChar, 150).Value = item.Office;
            cmd.Parameters.Add("@Status", SqlDbType.Bit).Value = item.IsValid;
            cmd.Parameters.Add("@CusIden", SqlDbType.Int).Value = item.ID;
            var par = cmd.Parameters.Add("@NewID", SqlDbType.Int);
            par.Direction = ParameterDirection.Output;
            try
            {
                cmd.ExecuteNonQuery();
                if (cmd.Parameters["@NewID"].Value != DBNull.Value)
                {
                    var _new = (int)cmd.Parameters["NewID"].Value;
                    item.PreviousID = item.ID;
                    item.ID = _new;
                }
                item.Version = Version.Original;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public override void Delete(Customer item)
        {
            SqlCommand cmd = _storage.DBConnection.CreateCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "OurResources.prDeleteCustomer";
            cmd.Parameters.Add("@id", SqlDbType.Int).Value = item.ID;
            try
            {
                cmd.Connection.Open();
                cmd.ExecuteNonQuery();
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                cmd.Connection.Close();
            }
        }
        public override void SaveChanges(Customer item, SqlTransaction tran = null)
        {
            if (item == null) return;
            if (tran == null)
            {
                SqlTransaction transaction = null;
                SqlConnection con = _storage.DBConnection;
                try
                {
                    con.Open();
                    transaction = con.BeginTransaction();
                    _storage.OrganizationAccessService.SaveChanges(item.Organization, transaction);
                    base.SaveChanges(item, transaction);
                    transaction.Commit();
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    throw;
                }
                finally
                {
                    con.Close();
                }
            }
            else
            {
                try
                {
                    _storage.OrganizationAccessService.SaveChanges(item.Organization, tran);
                    base.SaveChanges(item, tran);
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
        public override Customer GetItemByID(int id)
        {
            SqlCommand cmd = _storage.DBConnection.CreateCommand();
            cmd.CommandText = "OutResources.prGetCustomerByID";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("@CustomerID", SqlDbType.Int).Value = id;
            Customer customer = null;
            try
            {
                cmd.Connection.Open();
                var rd = cmd.ExecuteReader();
                if (rd.HasRows)
                {
                    int colId = rd.GetOrdinal("CustomerID");
                    int colFirstName = rd.GetOrdinal("FirstName");
                    int colSecondName = rd.GetOrdinal("SecondName");
                    int colMiddleName = rd.GetOrdinal("MiddleName");
                    int colDeclinated = rd.GetOrdinal("Declinated");
                    int colGender = rd.GetOrdinal("Gender");
                    int colWorkPhone = rd.GetOrdinal("WorkPhone");
                    int colMobilePhone = rd.GetOrdinal("MobilePhone");
                    int colOrganization = rd.GetOrdinal("OrganizationID");
                    int colOffice = rd.GetOrdinal("Office");
                    int colRank = rd.GetOrdinal("RankID");
                    int colDep = rd.GetOrdinal("Departament");
                    int colEmail = rd.GetOrdinal("Email");
                    int colStatus = rd.GetOrdinal("IsValid");
                    int colUpdateDate = rd.GetOrdinal("UpdateDate");
                    int colPrevId = rd.GetOrdinal("PreviousID");
                    while (rd.Read())
                    {
                        customer = new Customer(id: rd.GetInt32(colId),
                                                previd: rd[colPrevId] == DBNull.Value ? null : new int?(rd.GetInt32(colPrevId)),
                                                firstname: rd.GetString(colFirstName),
                                                secondname: rd.GetString(colSecondName),
                                                middlename: rd.GetString(colMiddleName),
                                                declinated: rd.GetBoolean(colDeclinated),
                                                gender: rd.GetBoolean(colGender),
                                                workphone: rd[colWorkPhone] == DBNull.Value ? null : rd.GetString(colWorkPhone),
                                                mobilephone: rd[colMobilePhone] == DBNull.Value ? null : rd.GetString(colMobilePhone),
                                                organization: rd[colOrganization] == DBNull.Value ? null : _storage.OrganizationAccessService.GetItemByID(rd.GetInt32(colOrganization)),
                                                office: rd[colOffice] == DBNull.Value ? null : rd.GetString(colOffice),
                                                rank: rd[colRank] == DBNull.Value ? null : rd.GetString(colRank),
                                                departament: rd[colDep] == DBNull.Value ? null : rd.GetString(colDep),
                                                email: rd[colEmail] == DBNull.Value ? null : rd.GetString(colEmail),
                                                status: rd.GetBoolean(colStatus),
                                                vr: Version.Original,
                                                updatedate: rd.GetDateTime(colUpdateDate)
                                 );
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                cmd.Connection.Close();
            }
            return customer;
        }
        public CustomerDataAccess(ILocalStorage storage) : base(storage) { }
    }

    public class ResolutionDataAccess : DataAccess<Resolution>
    {
        public override Resolution GetItemByID(int id)
        {
            SqlCommand cmd = _storage.DBConnection.CreateCommand();
            cmd.CommandText = "Activity.prGetResolutionByID";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("@ResolutionID", SqlDbType.Int).Value = id;
            Resolution resolution = null;
            try
            {
                cmd.Connection.Open();
                var rd = cmd.ExecuteReader();
                while (rd.Read())
                {
                    resolution = new Resolution(id: rd.GetInt32(0),
                                                registrationdate: rd.GetDateTime(1),
                                                resolutiondate: rd.IsDBNull(2) ? null : new DateTime?(rd.GetDateTime(2)),
                                                resolutiontype: (ResolutionTypes)rd.GetByte(3),
                                                customer: _storage.CustomerAccessService.GetItemByID(rd.GetInt32(5)),
                                                obj: rd.IsDBNull(6) ? null : rd.GetString(6),
                                                prescribe: rd.IsDBNull(7) ? null : rd.GetString(7),
                                                quest: rd.IsDBNull(8) ? null : rd.GetString(8),
                                                nativenumeration: rd.GetBoolean(9),
                                                status: rd.GetString(4),
                                                uidcase: rd.IsDBNull(11) ? null : rd.GetString(11),
                                                casenumber: rd.IsDBNull(10) ? null : rd.GetString(10),
                                                respondent: rd.IsDBNull(14) ? null : rd.GetString(14),
                                                plaintiff: rd.IsDBNull(15) ? null : rd.GetString(15),
                                                typecase: App.Storage.CaseTypes.First(n => n.Code.Equals(rd.GetString(13), StringComparison.Ordinal)),
                                                annotate: rd.IsDBNull(12) ? null : rd.GetString(12),
                                                comment: rd.IsDBNull(16) ? null : rd.GetString(16),
                                                vr: Version.Original
                                                );
                }
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                cmd.Connection.Close();
            }
            return resolution;
        }
        public override ICollection<Resolution> Items()
        {
            throw new NotImplementedException();
        }
        protected override void Add(Resolution item, SqlConnection connection, SqlTransaction tran = null)
        {
            SqlCommand cmd = connection.CreateCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "Activity.prAddResolution";
            cmd.Transaction = tran;
            cmd.Parameters.Add("@RegDate", SqlDbType.Date).Value = item.RegistrationDate;
            cmd.Parameters.Add("@ResolDate", SqlDbType.Date).Value = ConvertToDBNull(item.ResolutionDate);
            cmd.Parameters.Add("@TypeResol", SqlDbType.TinyInt).Value = (int)item.ResolutionType;
            cmd.Parameters.Add("@Status", SqlDbType.NVarChar, 30).Value = item.ResolutionStatus;
            cmd.Parameters.Add("@CustID", SqlDbType.Int).Value = item.Customer.ID;
            cmd.Parameters.Add("@TypeCase", SqlDbType.Char, 1).Value = item.TypeCase.Code;
            cmd.Parameters.Add("@Annotate", SqlDbType.NVarChar, 500).Value = ConvertToDBNull(item.CaseAnnotate);
            cmd.Parameters.Add("@Comment", SqlDbType.NVarChar, 500).Value = ConvertToDBNull(item.Comment);
            cmd.Parameters.Add("@NumberCase", SqlDbType.NVarChar, 50).Value = ConvertToDBNull(item.CaseNumber);
            cmd.Parameters.Add("@Uid", SqlDbType.NVarChar).Value = ConvertToDBNull(item.UidCase);
            cmd.Parameters.Add("@Respondent", SqlDbType.NVarChar, 150).Value = ConvertToDBNull(item.Respondent);
            cmd.Parameters.Add("@Plaintiff", SqlDbType.NVarChar, 150).Value = ConvertToDBNull(item.Plaintiff);
            cmd.Parameters.Add("@PrescribeType", SqlDbType.NVarChar, 200).Value = ConvertToDBNull(item.PrescribeType);
            cmd.Parameters.Add("@Questions", SqlDbType.NVarChar).Value = ConvertToDBNull(item.Questions.WrapCollectionToDBString());
            cmd.Parameters.Add("@Objects", SqlDbType.NVarChar).Value = ConvertToDBNull(item.Objects.WrapCollectionToDBString());
            cmd.Parameters.Add("@IsNativeQNumeration", SqlDbType.Bit).Value = item.NativeQuestionNumeration;
            var par = cmd.Parameters.Add("@InsertedID", SqlDbType.Int);
            par.Direction = ParameterDirection.Output;
            try
            {
                cmd.ExecuteNonQuery();
                item.ID = (int)cmd.Parameters["@InsertedID"].Value;
                item.Version = Version.Original;
            }
            catch (Exception)
            {
                throw;
            }
        }
        protected override void Edit(Resolution item, SqlConnection connection, SqlTransaction tran = null)
        {
            SqlCommand cmd = connection.CreateCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "Activity.prEditResolution";
            cmd.Transaction = tran;
            cmd.Parameters.Add("@RegDate", SqlDbType.Date).Value = item.RegistrationDate;
            cmd.Parameters.Add("@ResolDate", SqlDbType.Date).Value = ConvertToDBNull(item.ResolutionDate);
            cmd.Parameters.Add("@TypeResol", SqlDbType.TinyInt).Value = (int)item.ResolutionType;
            cmd.Parameters.Add("@Status", SqlDbType.NVarChar, 30).Value = item.ResolutionStatus;
            cmd.Parameters.Add("@CustID", SqlDbType.Int).Value = item.Customer.ID;
            cmd.Parameters.Add("@TypeCase", SqlDbType.Char, 1).Value = item.TypeCase.Code;
            cmd.Parameters.Add("@Annotate", SqlDbType.NVarChar, 500).Value = ConvertToDBNull(item.CaseAnnotate);
            cmd.Parameters.Add("@Comment", SqlDbType.NVarChar, 500).Value = ConvertToDBNull(item.Comment);
            cmd.Parameters.Add("@NumberCase", SqlDbType.NVarChar, 50).Value = ConvertToDBNull(item.CaseNumber);
            cmd.Parameters.Add("@Uid", SqlDbType.NVarChar).Value = ConvertToDBNull(item.UidCase);
            cmd.Parameters.Add("@Respondent", SqlDbType.NVarChar, 150).Value = ConvertToDBNull(item.Respondent);
            cmd.Parameters.Add("@Plaintiff", SqlDbType.NVarChar, 150).Value = ConvertToDBNull(item.Plaintiff);
            cmd.Parameters.Add("@PrescribeType", SqlDbType.NVarChar, 200).Value = ConvertToDBNull(item.PrescribeType);
            cmd.Parameters.Add("@Questions", SqlDbType.NVarChar).Value = ConvertToDBNull(item.Questions.WrapCollectionToDBString());
            cmd.Parameters.Add("@Objects", SqlDbType.NVarChar).Value = ConvertToDBNull(item.Objects.WrapCollectionToDBString());
            cmd.Parameters.Add("@IsNativeQNumeration", SqlDbType.Bit).Value = item.NativeQuestionNumeration;
            cmd.Parameters.Add("@ResolIden", SqlDbType.Int).Value = item.ID;
            try
            {
                cmd.ExecuteNonQuery();
                item.Version = Version.Original;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public override void SaveChanges(Resolution item, SqlTransaction tran = null)
        {
            if (item == null) return;
            if (tran == null)
            {
                SqlTransaction transaction = null;
                SqlConnection con = _storage.DBConnection;
                try
                {
                    con.Open();
                    transaction = con.BeginTransaction();
                    _storage.CustomerAccessService.SaveChanges(item.Customer, transaction);
                    base.SaveChanges(item, transaction);
                    foreach (var expertise in item.Expertisies)
                    {
                        _storage.ExpertiseAccessService.SaveChanges(expertise, transaction);
                    }
                    transaction.Commit();
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    throw;
                }
                finally
                {
                    con.Close();
                }
            }
            else
            {
                try
                {
                    _storage.CustomerAccessService.SaveChanges(item.Customer, tran);
                    base.SaveChanges(item, tran);
                    foreach (var expertise in item.Expertisies)
                    {
                        _storage.ExpertiseAccessService.SaveChanges(expertise, tran);
                    }
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
        public override void Delete(Resolution item)
        {
            if (item.Version == Version.New) return;
            SqlCommand cmd = _storage.DBConnection.CreateCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "Activity.prDeleteResolution";
            cmd.Parameters.Add("@id", SqlDbType.Int).Value = item.ID;
            try
            {
                cmd.Connection.Open();
                cmd.ExecuteNonQuery();
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                cmd.Connection.Close();
            }
        }
        public async Task<List<Resolution>> LoadResolutionsAsync(int empid, ExpertiseTypes type = ExpertiseTypes.Unknown, string number = null, DateTime? execdatemin = null,
                                                           DateTime? execdatemax = null, bool? status = null, int evaluation = 0)
        {
            List<Resolution> resols = new List<Resolution>();
            SqlCommand cmd = _storage.DBConnection.CreateCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "Activity.prResolutions";
            cmd.Parameters.Add("@empid", SqlDbType.Int).Value = empid;
            SqlDataReader rd = null;
            if (type != ExpertiseTypes.Unknown) cmd.Parameters.Add("@exptype", SqlDbType.TinyInt).Value = (byte)type;
            if (!string.IsNullOrWhiteSpace(number)) cmd.Parameters.Add("@number", SqlDbType.NVarChar, 5).Value = number;
            if (execdatemin != null) cmd.Parameters.Add("@datemin", SqlDbType.Date).Value = execdatemin;
            if (execdatemax != null) cmd.Parameters.Add("@datemax", SqlDbType.Date).Value = execdatemax;
            if (evaluation != 0) cmd.Parameters.Add("@evaluation", SqlDbType.TinyInt).Value = evaluation;
            if (status != null) cmd.Parameters.Add("@executed", SqlDbType.Bit).Value = status;
            try
            {
                cmd.Connection.Open();
                rd = await cmd.ExecuteReaderAsync();
                if (rd.HasRows)
                {
                    int colExpertiseID = rd.GetOrdinal("ExpertiseID");
                    int colNumber = rd.GetOrdinal("Number");
                    int colExpertiseResult = rd.GetOrdinal("ExpertiseResult");
                    int colStartDate = rd.GetOrdinal("StartDate");
                    int colExecutionDate = rd.GetOrdinal("ExecutionDate");
                    int colExpertiseType = rd.GetOrdinal("TypeExpertise");
                    int colPreviousExpertise = rd.GetOrdinal("PreviousResolution");
                    int colTimelimit = rd.GetOrdinal("Timelimit");
                    int colExpertID = rd.GetOrdinal("ExpertID");
                    int colCustomerID = rd.GetOrdinal("CustomerID");
                    int colPrescribeType = rd.GetOrdinal("PrescribeType");
                    int colObjects = rd.GetOrdinal("ProvidedObjects");
                    int colQuestions = rd.GetOrdinal("Questions");
                    int colRegDate = rd.GetOrdinal("RegistrationDate");
                    int colResolDate = rd.GetOrdinal("ResolutionDate");
                    int colResolutionID = rd.GetOrdinal("ResolutionID");
                    int colResolutionType = rd.GetOrdinal("ResolutionType");
                    int colResolutionStatus = rd.GetOrdinal("ResolutionStatusID");
                    int colAnnotate = rd.GetOrdinal("Annotate");
                    int colCaseComment = rd.GetOrdinal("Comment");
                    int colNumberCase = rd.GetOrdinal("NumberCase");
                    int colUIDCase = rd.GetOrdinal("UIDCase");
                    int colPlaintiff = rd.GetOrdinal("Plaintiff");
                    int colRespondent = rd.GetOrdinal("Respondent");
                    int colCaseType = rd.GetOrdinal("TypeCase");
                    int colNativeQuestions = rd.GetOrdinal("IsNativeQuestionsNumeration");
                    int colMContent = rd.GetOrdinal("Content");
                    int colMDalayDAte = rd.GetOrdinal("DelayDate");
                    int colMDate = rd.GetOrdinal("EntityDate");
                    int colMType = rd.GetOrdinal("EntityType");
                    int colMID = rd.GetOrdinal("ID");
                    int colMReferTo = rd.GetOrdinal("ReferTo");
                    int colMRegDate = rd.GetOrdinal("MovementRegDate");
                    int colMSuspend = rd.GetOrdinal("Suspend");
                    int colMResume = rd.GetOrdinal("Resume");
                    Resolution _resolution = null;
                    Expertise _expertise = null;
                    int expid = 0, resid = 0;
                    while (rd.Read())
                    {
                        resid = rd.GetInt32(colResolutionID);
                        if (!resols.Any(n => n.ID == resid))
                        {
                            _resolution = new Resolution(
                                                id: resid,
                                                registrationdate: rd.GetDateTime(colRegDate),
                                                resolutiondate: rd.IsDBNull(colResolDate) ? null : new DateTime?(rd.GetDateTime(colResolDate)),
                                                resolutiontype: (ResolutionTypes)rd.GetByte(colResolutionType),
                                                customer: App.Storage.CustomerAccessService.GetItemByID(rd.GetInt32(colCustomerID)),
                                                obj: rd.IsDBNull(colObjects) ? null : rd.GetString(colObjects),
                                                quest: rd.IsDBNull(colQuestions) ? null : rd.GetString(colQuestions),
                                                nativenumeration: rd.GetBoolean(colNativeQuestions),
                                                status: rd.GetString(colResolutionStatus),
                                                typecase: App.Storage.CaseTypes.First(n => n.Code.Equals(rd.GetString(colCaseType), StringComparison.Ordinal)),
                                                uidcase: rd.IsDBNull(colUIDCase) ? null : rd.GetString(colUIDCase),
                                                respondent: rd.IsDBNull(colRespondent) ? null : rd.GetString(colRespondent),
                                                plaintiff: rd.IsDBNull(colPlaintiff) ? null : rd.GetString(colPlaintiff),
                                                casenumber: rd.IsDBNull(colNumberCase) ? null : rd.GetString(colNumberCase),
                                                comment: rd.IsDBNull(colCaseComment) ? null : rd.GetString(colCaseComment),
                                                annotate: rd.IsDBNull(colAnnotate) ? null : rd.GetString(colAnnotate),
                                                prescribe: rd.IsDBNull(colPrescribeType) ? null : rd.GetString(colPrescribeType),
                                                vr: Version.Original);
                            resols.Add(_resolution);
                        }
                        if (expid < rd.GetInt32(colExpertiseID)) // sorted
                        {
                            expid = rd.GetInt32(colExpertiseID);
                            _expertise = new Expertise(id: expid,
                                                        number: rd.GetString(colNumber),
                                                        expert: App.Storage.ExpertAccessService.GetItemByID(rd.GetInt32(colExpertID)),
                                                        result: rd.IsDBNull(colExpertiseResult) ? ExpertiseResults.Unknown : (ExpertiseResults)rd.GetByte(colExpertiseResult),
                                                        start: rd.GetDateTime(colStartDate),
                                                        end: rd.IsDBNull(colExecutionDate) ? null : new DateTime?(rd.GetDateTime(colExecutionDate)),
                                                        timelimit: rd.GetByte(colTimelimit),
                                                        type: (ExpertiseTypes)rd.GetByte(colExpertiseType),
                                                        previous: rd.IsDBNull(colPreviousExpertise) ? null : new Int32?(rd.GetInt32(colPreviousExpertise)),
                                                        spendhours: null,
                                                        vr: Version.Original);
                            resols.First(n => n.ID == resid).Expertisies.Add(_expertise);
                        }
                        if (!rd.IsDBNull(colMID))
                        {
                            switch (rd.GetByte(colMType))//1 - Request, 2 - Responce, 3 - Report, 4 - Outcoming Letter, 5 - Incoming Letter
                            {
                                case 1:
                                    var rq = new Request(id: rd.GetInt32(colMID),
                                                            from:  _expertise,
                                                            register: rd.GetDateTime(colMRegDate),
                                                            suspend: rd.GetBoolean(colMSuspend),
                                                            content: rd.IsDBNull(colMContent) ? null : rd.GetString(colMContent),
                                                            create: rd.IsDBNull(colMDate) ? rd.GetDateTime(colMRegDate) : rd.GetDateTime(colMDate),
                                                            version: Version.Original);
                                    _expertise.Movements.Add(rq);
                                    break;
                                case 2:
                                    var res = new Response(id: rd.GetInt32(colMID),
                                                            from: _expertise,
                                                            register: rd.GetDateTime(colMRegDate),
                                                            refer: rd.IsDBNull(colMReferTo) ? 0 : rd.GetInt32(colMReferTo),
                                                            resume: rd.IsDBNull(colMResume) ? false : rd.GetBoolean(colMResume),
                                                            content: rd.IsDBNull(colMContent) ? null : rd.GetString(colMContent),
                                                            create: rd.IsDBNull(colMDate) ? rd.GetDateTime(colMRegDate) : rd.GetDateTime(colMDate),
                                                            version: Version.Original);
                                    _expertise.Movements.Add(res);
                                    break;
                                case 3:
                                    var rep = new Report(id: rd.GetInt32(colMID),
                                                            from: _expertise,
                                                            register: rd.GetDateTime(colMRegDate),
                                                            delay: rd.GetDateTime(colMDalayDAte),
                                                            reason: rd.IsDBNull(colMContent) ? null : rd.GetString(colMContent),
                                                            create: rd.IsDBNull(colMDate) ? rd.GetDateTime(colMRegDate) : rd.GetDateTime(colMDate),
                                                            version: Version.Original);
                                    _expertise.Movements.Add(rep);
                                    break;
                                case 4:
                                    var ol = new OutcomingLetter(id: rd.GetInt32(colMID),
                                                                from: _expertise,
                                                                register: rd.GetDateTime(colMRegDate),
                                                                create: rd.IsDBNull(colMDate) ? rd.GetDateTime(colMRegDate) : rd.GetDateTime(colMDate),
                                                                version: Version.Original);
                                    _expertise.Movements.Add(ol);
                                    break;
                                case 5:
                                    var il = new IncomingLetter(id: rd.GetInt32(colMID),
                                                                from: _expertise,
                                                                register: rd.GetDateTime(colMRegDate),
                                                                create: rd.IsDBNull(colMDate) ? rd.GetDateTime(colMRegDate) : rd.GetDateTime(colMDate),
                                                                version: Version.Original);
                                    _expertise.Movements.Add(il);
                                    break;
                                default:
                                    App.ErrorLogger.LogError("Unknown type of expertise movement. Movement not created");
                                    break;
                            }
                        }
                    }
                    rd.Close();
                }
            }
            catch (Exception ex)
            {
                App.ErrorLogger.LogError(ex.Message);
                throw;
            }
            finally
            {
                rd?.Close();
                cmd.Connection.Close();
            }
            return resols;
        }
        public async Task<List<Resolution>> LoadResolutionsByExpertAsync(int expertid, ExpertiseTypes type = ExpertiseTypes.Unknown, string number = null, DateTime? execdatemin = null,
                                                           DateTime? execdatemax = null, bool? status = null, int evaluation = 0)
        {
            List<Resolution> resols = new List<Resolution>();
            SqlCommand cmd = _storage.DBConnection.CreateCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "Activity.prResolutionsByExperts";
            cmd.Parameters.Add("@expertid", SqlDbType.Int).Value = expertid;
            SqlDataReader rd = null;
            if (type != ExpertiseTypes.Unknown) cmd.Parameters.Add("@exptype", SqlDbType.TinyInt).Value = (byte)type;
            if (!string.IsNullOrWhiteSpace(number)) cmd.Parameters.Add("@number", SqlDbType.NVarChar, 5).Value = number;
            if (execdatemin != null) cmd.Parameters.Add("@datemin", SqlDbType.Date).Value = execdatemin;
            if (execdatemax != null) cmd.Parameters.Add("@datemax", SqlDbType.Date).Value = execdatemax;
            if (evaluation != 0) cmd.Parameters.Add("@evaluation", SqlDbType.TinyInt).Value = evaluation;
            if (status != null) cmd.Parameters.Add("@executed", SqlDbType.Bit).Value = status;
            try
            {
                cmd.Connection.Open();
                rd = await cmd.ExecuteReaderAsync();
                if (rd.HasRows)
                {
                    int colExpertiseID = rd.GetOrdinal("ExpertiseID");
                    int colNumber = rd.GetOrdinal("Number");
                    int colExpertiseResult = rd.GetOrdinal("ExpertiseResult");
                    int colStartDate = rd.GetOrdinal("StartDate");
                    int colExecutionDate = rd.GetOrdinal("ExecutionDate");
                    int colExpertiseType = rd.GetOrdinal("TypeExpertise");
                    int colPreviousExpertise = rd.GetOrdinal("PreviousResolution");
                    int colTimelimit = rd.GetOrdinal("Timelimit");
                    int colExpertID = rd.GetOrdinal("ExpertID");
                    int colCustomerID = rd.GetOrdinal("CustomerID");
                    int colPrescribeType = rd.GetOrdinal("PrescribeType");
                    int colObjects = rd.GetOrdinal("ProvidedObjects");
                    int colQuestions = rd.GetOrdinal("Questions");
                    int colRegDate = rd.GetOrdinal("RegistrationDate");
                    int colResolDate = rd.GetOrdinal("ResolutionDate");
                    int colResolutionID = rd.GetOrdinal("ResolutionID");
                    int colResolutionType = rd.GetOrdinal("ResolutionType");
                    int colResolutionStatus = rd.GetOrdinal("ResolutionStatusID");
                    int colAnnotate = rd.GetOrdinal("Annotate");
                    int colCaseComment = rd.GetOrdinal("Comment");
                    int colNumberCase = rd.GetOrdinal("NumberCase");
                    int colUIDCase = rd.GetOrdinal("UIDCase");
                    int colPlaintiff = rd.GetOrdinal("Plaintiff");
                    int colRespondent = rd.GetOrdinal("Respondent");
                    int colCaseType = rd.GetOrdinal("TypeCase");
                    int colNativeQuestions = rd.GetOrdinal("IsNativeQuestionsNumeration");
                    int colMContent = rd.GetOrdinal("Content");
                    int colMDalayDAte = rd.GetOrdinal("DelayDate");
                    int colMDate = rd.GetOrdinal("EntityDate");
                    int colMType = rd.GetOrdinal("EntityType");
                    int colMID = rd.GetOrdinal("ID");
                    int colMReferTo = rd.GetOrdinal("ReferTo");
                    int colMRegDate = rd.GetOrdinal("MovementRegDate");
                    int colMSuspend = rd.GetOrdinal("Suspend");
                    int colMResume = rd.GetOrdinal("Resume");
                    Resolution _resolution = null;
                    Expertise _expertise = null;
                    int exid = 0, resid = 0;
                    while (rd.Read())
                    {
                        resid = rd.GetInt32(colResolutionID);
                        if (!resols.Any(n => n.ID == resid))
                        {
                            _resolution = new Resolution(
                                                id: resid,
                                                registrationdate: rd.GetDateTime(colRegDate),
                                                resolutiondate: rd.IsDBNull(colResolDate) ? null : new DateTime?(rd.GetDateTime(colResolDate)),
                                                resolutiontype: (ResolutionTypes)rd.GetByte(colResolutionType),
                                                customer: App.Storage.CustomerAccessService.GetItemByID(rd.GetInt32(colCustomerID)),
                                                obj: rd.IsDBNull(colObjects) ? null : rd.GetString(colObjects),
                                                quest: rd.IsDBNull(colQuestions) ? null : rd.GetString(colQuestions),
                                                nativenumeration: rd.GetBoolean(colNativeQuestions),
                                                status: rd.GetString(colResolutionStatus),
                                                typecase: App.Storage.CaseTypes.First(n => n.Code.Equals(rd.GetString(colCaseType), StringComparison.Ordinal)),
                                                uidcase: rd.IsDBNull(colUIDCase) ? null : rd.GetString(colUIDCase),
                                                respondent: rd.IsDBNull(colRespondent) ? null : rd.GetString(colRespondent),
                                                plaintiff: rd.IsDBNull(colPlaintiff) ? null : rd.GetString(colPlaintiff),
                                                casenumber: rd.IsDBNull(colNumberCase) ? null : rd.GetString(colNumberCase),
                                                comment: rd.IsDBNull(colCaseComment) ? null : rd.GetString(colCaseComment),
                                                annotate: rd.IsDBNull(colAnnotate) ? null : rd.GetString(colAnnotate),
                                                prescribe: rd.IsDBNull(colPrescribeType) ? null : rd.GetString(colPrescribeType),
                                                vr: Version.Original);
                            resols.Add(_resolution);
                        }
                        if (exid < rd.GetInt32(colExpertiseID)) // sorted
                        {
                            exid = rd.GetInt32(colExpertiseID);
                            _expertise = new Expertise(id: exid,
                                                        number: rd.GetString(colNumber),
                                                        expert: App.Storage.ExpertAccessService.GetItemByID(rd.GetInt32(colExpertID)),
                                                        result: rd.IsDBNull(colExpertiseResult) ? ExpertiseResults.Unknown : (ExpertiseResults)rd.GetByte(colExpertiseResult),
                                                        start: rd.GetDateTime(colStartDate),
                                                        end: rd.IsDBNull(colExecutionDate) ? null : new DateTime?(rd.GetDateTime(colExecutionDate)),
                                                        timelimit: rd.GetByte(colTimelimit),
                                                        type: (ExpertiseTypes)rd.GetByte(colExpertiseType),
                                                        previous: rd.IsDBNull(colPreviousExpertise) ? null : new Int32?(rd.GetInt32(colPreviousExpertise)),
                                                        spendhours: null,
                                                        vr: Version.Original);
                            resols.First(n => n.ID == resid).Expertisies.Add(_expertise);
                        }
                        if (!rd.IsDBNull(colMID))
                        {
                            switch (rd.GetByte(colMType))//1 - Request, 2 - Responce, 3 - Report, 4 - Outcoming Letter, 5 - Incoming Letter
                            {
                                case 1:
                                    var rq = new Request(id: rd.GetInt32(colMID),
                                                            from: _expertise,
                                                            register: rd.GetDateTime(colMRegDate),
                                                            suspend: rd.GetBoolean(colMSuspend),
                                                            content: rd.IsDBNull(colMContent) ? null : rd.GetString(colMContent),
                                                            create: rd.IsDBNull(colMDate) ? rd.GetDateTime(colMRegDate) : rd.GetDateTime(colMDate),
                                                            version: Version.Original);
                                    _expertise.Movements.Add(rq);
                                    break;
                                case 2:
                                    var res = new Response(id: rd.GetInt32(colMID),
                                                            from: _expertise,
                                                            register: rd.GetDateTime(colMRegDate),
                                                            refer: rd.IsDBNull(colMReferTo) ? 0 : rd.GetInt32(colMReferTo),
                                                            resume: rd.IsDBNull(colMResume) ? false : rd.GetBoolean(colMResume),
                                                            content: rd.IsDBNull(colMContent) ? null : rd.GetString(colMContent),
                                                            create: rd.IsDBNull(colMDate) ? rd.GetDateTime(colMRegDate) : rd.GetDateTime(colMDate),
                                                            version: Version.Original);
                                    _expertise.Movements.Add(res);
                                    break;
                                case 3:
                                    var rep = new Report(id: rd.GetInt32(colMID),
                                                            from: _expertise,
                                                            register: rd.GetDateTime(colMRegDate),
                                                            delay: rd.GetDateTime(colMDalayDAte),
                                                            reason: rd.IsDBNull(colMContent) ? null : rd.GetString(colMContent),
                                                            create: rd.IsDBNull(colMDate) ? rd.GetDateTime(colMRegDate) : rd.GetDateTime(colMDate),
                                                            version: Version.Original);
                                    _expertise.Movements.Add(rep);
                                    break;
                                case 4:
                                    var ol = new OutcomingLetter(id: rd.GetInt32(colMID),
                                                                from: _expertise,
                                                                register: rd.GetDateTime(colMRegDate),
                                                                create: rd.IsDBNull(colMDate) ? rd.GetDateTime(colMRegDate) : rd.GetDateTime(colMDate),
                                                                version: Version.Original);
                                    _expertise.Movements.Add(ol);
                                    break;
                                case 5:
                                    var il = new IncomingLetter(id: rd.GetInt32(colMID),
                                                                from: _expertise,
                                                                register: rd.GetDateTime(colMRegDate),
                                                                create: rd.IsDBNull(colMDate) ? rd.GetDateTime(colMRegDate) : rd.GetDateTime(colMDate),
                                                                version: Version.Original);
                                    _expertise.Movements.Add(il);
                                    break;
                                default:
                                    App.ErrorLogger.LogError("Unknown type of expertise movement. Movement not created");
                                    break;
                            }
                        }
                    }
                    rd.Close();
                }
            }
            //catch (Exception)
            //{
            //    throw;
            //}
            finally
            {
                rd?.Close();
                cmd.Connection.Close();
            }
            return resols;
        }
        public ResolutionDataAccess(ILocalStorage storage) : base(storage) { }
    }

    public class EquipmentDataAccess : DataAccess<Equipment>
    {
        public override Equipment GetItemByID(int id)
        {
            SqlCommand cmd = _storage.DBConnection.CreateCommand();
            cmd.CommandText = "InnResources.prGetEquipmentByID";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("@EqupmentID", SqlDbType.SmallInt).Value = id;
            Equipment equipment = null;
            try
            {
                cmd.Connection.Open();
                var rd = cmd.ExecuteReader();
                if (rd.HasRows)
                {
                    while (rd.Read())
                    {

                        equipment = new Equipment(id: rd.GetInt16(0),
                                                name: rd.GetString(1),
                                                description: !rd.IsDBNull(2) ? rd.GetString(2) : null,
                                                commisiondate: !rd.IsDBNull(3) ? new DateTime?(rd.GetDateTime(3)) : null,
                                                check: !rd.IsDBNull(4) ? new DateTime?(rd.GetDateTime(4)) : null,
                                                istrack: rd.GetBoolean(5),
                                                vr: Version.Original,
                                                updatedate: rd.GetDateTime(6));
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                cmd.Connection.Close();
            }
            return equipment;
        }
        public override ICollection<Equipment> Items()
        {
            SqlCommand cmd = _storage.DBConnection.CreateCommand();
            cmd.CommandText = "InnResources.prGetEquipments";
            cmd.CommandType = CommandType.StoredProcedure;
            List<Equipment> list = new List<Equipment>(55); //TODO: remove constant
            try
            {
                cmd.Connection.Open();
                var rd = cmd.ExecuteReader();
                if (rd.HasRows)
                {
                    int colId = rd.GetOrdinal("EquipmentID");
                    int colEquipmentName = rd.GetOrdinal("EquipmentName");
                    int colDesc = rd.GetOrdinal("Descript");
                    int colCommDate = rd.GetOrdinal("CommissionDate");
                    int colLastCheckDate = rd.GetOrdinal("LastCheckDate");
                    int colStatus = rd.GetOrdinal("IsTraking");
                    int colLastUpdate = rd.GetOrdinal("UpdateDate");
                    while (rd.Read())
                    {

                        list.Add(new Equipment(id: rd.GetInt16(colId),
                                                name: rd.GetString(colEquipmentName),
                                                description: rd[colDesc] != DBNull.Value ? rd.GetString(colDesc) : null,
                                                commisiondate: rd[colCommDate] != DBNull.Value ? new DateTime?(rd.GetDateTime(colCommDate)) : null,
                                                check: rd[colLastCheckDate] != DBNull.Value ? new DateTime?(rd.GetDateTime(colLastCheckDate)) : null,
                                                istrack: rd.GetBoolean(colStatus),
                                                vr: Version.Original,
                                                updatedate: rd.GetDateTime(colLastUpdate)
                                ));
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                cmd.Connection.Close();
            }
            return list;
        }
        protected override void Add(Equipment item, SqlConnection connection, SqlTransaction tran = null)
        {
            SqlCommand cmd = connection.CreateCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "InnResources.prAddEquipment";
            cmd.Transaction = tran;
            cmd.Parameters.Add("@Name", SqlDbType.NVarChar, 100).Value = item.EquipmentName;
            cmd.Parameters.Add("@Descr", SqlDbType.NVarChar, 500).Value = ConvertToDBNull(item.Description);
            cmd.Parameters.Add("@CommDate", SqlDbType.Date).Value = ConvertToDBNull(item.CommisionDate);
            cmd.Parameters.Add("@CheckDate", SqlDbType.Date).Value = ConvertToDBNull(item.LastCheckDate);
            var par = cmd.Parameters.Add("@InsertedID", SqlDbType.Int);
            par.Direction = ParameterDirection.Output;
            try
            {
                cmd.ExecuteNonQuery();
                item.ID = (int)cmd.Parameters["@InsertedID"].Value;
                item.Version = Version.Original;
            }
            catch (Exception)
            {
                throw;
            }
        }
        protected override void Edit(Equipment item, SqlConnection connection, SqlTransaction tran = null)
        {
            SqlCommand cmd = connection.CreateCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "InnResources.prEditEquipment";
            cmd.Transaction = tran;
            cmd.Parameters.Add("@Name", SqlDbType.NVarChar, 100).Value = item.EquipmentName;
            cmd.Parameters.Add("@Descr", SqlDbType.NVarChar, 500).Value = ConvertToDBNull(item.Description);
            cmd.Parameters.Add("@CommDate", SqlDbType.Date).Value = ConvertToDBNull(item.CommisionDate);
            cmd.Parameters.Add("@CheckDate", SqlDbType.Date).Value = ConvertToDBNull(item.LastCheckDate);
            cmd.Parameters.Add("@Status", SqlDbType.NVarChar, 30).Value = item.IsTrack;
            cmd.Parameters.Add("@EquipmantID", SqlDbType.SmallInt).Value = item.ID;
            try
            {
                cmd.ExecuteNonQuery();
                item.Version = Version.Original;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public override void Delete(Equipment item)
        {
            SqlCommand cmd = _storage.DBConnection.CreateCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "InnResources.prDeleteEquipment";
            cmd.Parameters.Add("@id", SqlDbType.SmallInt).Value = item.ID;
            try
            {
                cmd.Connection.Open();
                cmd.ExecuteNonQuery();
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                cmd.Connection.Close();
            }
        }
        public EquipmentDataAccess(ILocalStorage storage) : base(storage) { }
    }

    public class EquipmentUsageDataAccess : DataAccess<EquipmentUsage>
    {
        public override EquipmentUsage GetItemByID(int id)
        {
            SqlCommand cmd = _storage.DBConnection.CreateCommand();
            cmd.CommandText = "Activity.prGetEquipmentUsageByID";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("@UsageID", SqlDbType.Int).Value = id;
            EquipmentUsage usage = null;
            try
            {
                cmd.Connection.Open();
                var rd = cmd.ExecuteReader();
                if (rd.HasRows)
                {

                }
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                cmd.Connection.Close();
            }
            return usage;
        }
        public override ICollection<EquipmentUsage> Items()
        {
            throw new NotImplementedException();
        }
        protected override void Add(EquipmentUsage item, SqlConnection connection, SqlTransaction tran = null)
        {
            SqlCommand cmd = connection.CreateCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "Activity.prAddEquipmentUsage";
            cmd.Transaction = tran;
            cmd.Parameters.Add("@EquipmentID", SqlDbType.SmallInt).Value = item.UsedEquipment.ID;
            cmd.Parameters.Add("@ExpertiseID", SqlDbType.Int).Value = item.FromExpertise.ID;
            cmd.Parameters.Add("@UsageDate", SqlDbType.Date).Value = item.UsageDate;
            cmd.Parameters.Add("@Duration", SqlDbType.TinyInt).Value = item.Duration;
            var par = cmd.Parameters.Add("@InsertedID", SqlDbType.Int);
            par.Direction = ParameterDirection.Output;
            try
            {
                cmd.ExecuteNonQuery();
                item.ID = (int)cmd.Parameters["@InsertedID"].Value;
                item.Version = Version.Original;
            }
            catch (Exception)
            {
                throw;
            }
        }
        protected override void Edit(EquipmentUsage item, SqlConnection connection, SqlTransaction tran = null)
        {
            SqlCommand cmd = connection.CreateCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "Activity.prEditEquipmentUsage";
            cmd.Transaction = tran;
            cmd.Parameters.Add("@EquipmentID", SqlDbType.SmallInt).Value = item.UsedEquipment.ID;
            cmd.Parameters.Add("@ExpertiseID", SqlDbType.Int).Value = item.FromExpertise.ID;
            cmd.Parameters.Add("@UsageDate", SqlDbType.Date).Value = item.UsageDate;
            cmd.Parameters.Add("@Duration", SqlDbType.TinyInt).Value = item.Duration;
            cmd.Parameters.Add("@UsageID", SqlDbType.Int).Value = item.ID;
            try
            {
                cmd.ExecuteNonQuery();
                item.Version = Version.Original;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public override void Delete(EquipmentUsage item)
        {
            SqlCommand cmd = _storage.DBConnection.CreateCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "Activity.prDeleteEquipmentUsage";
            cmd.Parameters.Add("@UsageID", SqlDbType.Int).Value = item.ID;
            try
            {
                cmd.Connection.Open();
                cmd.ExecuteNonQuery();
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                cmd.Connection.Close();
            }
        }
        public override void SaveChanges(EquipmentUsage item, SqlTransaction tran = null)
        {
            if (item == null) return;
            if (tran == null)
            {
                SqlTransaction transaction = null;
                SqlConnection con = _storage.DBConnection;
                try
                {
                    con.Open();
                    transaction = con.BeginTransaction();
                    _storage.EquipmentAccessService.SaveChanges(item.UsedEquipment, transaction);
                    base.SaveChanges(item, transaction);
                    transaction.Commit();
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    throw;
                }
                finally
                {
                    con.Close();
                }
            }
            else
            {
                try
                {
                    _storage.EquipmentAccessService.SaveChanges(item.UsedEquipment, tran);
                    base.SaveChanges(item, tran);
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
        public EquipmentUsageDataAccess(ILocalStorage storage) : base(storage) { }
    }

    public class ExpertiseDataAccess : DataAccess<Expertise>
    {
        private void SaveContentToDB(Expertise item, SqlTransaction transaction)
        {
            foreach (var movement in item.Movements)
            {
                _storage.ExpertiseMovementAccessService.SaveChanges(movement, transaction);
            }
            foreach (var bill in item.Bills)
            {
                _storage.BillAccessService.SaveChanges(bill, transaction);
            }
            foreach (var usage in item.EquipmentUsages)
            {
                _storage.EquipmentUsageAccessService.SaveChanges(usage, transaction);
            }
        }
        public override Expertise GetItemByID(int id) => throw new NotImplementedException();
        public override ICollection<Expertise> Items() => throw new NotImplementedException();
        protected override void Add(Expertise item, SqlConnection connection, SqlTransaction tran = null)
        {
            SqlCommand cmd = connection.CreateCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "Activity.prAddExpertise";
            cmd.Transaction = tran;
            cmd.Parameters.Add("@Num", SqlDbType.VarChar, 5).Value = item.Number;
            cmd.Parameters.Add("@Expert", SqlDbType.Int).Value = item.Expert.ID;
            if (item.ExpertiseResult == ExpertiseResults.Unknown) cmd.Parameters.Add("@Result", SqlDbType.TinyInt).Value = DBNull.Value;
            else cmd.Parameters.Add("@Result", SqlDbType.TinyInt).Value = (byte)item.ExpertiseResult;
            cmd.Parameters.Add("@StartDate", SqlDbType.Date).Value = item.StartDate;
            cmd.Parameters.Add("@ExDate", SqlDbType.Date).Value = ConvertToDBNull(item.EndDate);
            cmd.Parameters.Add("@Limit", SqlDbType.TinyInt).Value = item.TimeLimit;
            cmd.Parameters.Add("@Resol", SqlDbType.Int).Value = item.FromResolution.ID;
            cmd.Parameters.Add("@Type", SqlDbType.TinyInt).Value = (byte)item.ExpertiseType;
            cmd.Parameters.Add("@PreviousResolution", SqlDbType.Int).Value = ConvertToDBNull(item.PreviousExpertise);
            var par = cmd.Parameters.Add("@InsertedID", SqlDbType.Int);
            par.Direction = ParameterDirection.Output;
            try
            {
                cmd.ExecuteNonQuery();
                item.ID = (int)cmd.Parameters["@InsertedID"].Value;
                item.Version = Version.Original;
            }
            catch (Exception)
            {
                throw;
            }
        }
        protected override void Edit(Expertise item, SqlConnection connection, SqlTransaction tran = null)
        {
            SqlCommand cmd = connection.CreateCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "Activity.prEditExpertise";
            cmd.Transaction = tran;
            cmd.Parameters.Add("@Num", SqlDbType.VarChar, 5).Value = item.Number;
            cmd.Parameters.Add("@Expert", SqlDbType.Int).Value = item.Expert.ID;
            if (item.ExpertiseResult == ExpertiseResults.Unknown) cmd.Parameters.Add("@Result", SqlDbType.TinyInt).Value = DBNull.Value;
            else cmd.Parameters.Add("@Result", SqlDbType.TinyInt).Value = (byte)item.ExpertiseResult;
            cmd.Parameters.Add("@StartDate", SqlDbType.Date).Value = item.StartDate;
            cmd.Parameters.Add("@ExDate", SqlDbType.Date).Value = ConvertToDBNull(item.EndDate);
            cmd.Parameters.Add("@Limit", SqlDbType.TinyInt).Value = item.TimeLimit;
            cmd.Parameters.Add("@Resol", SqlDbType.Int).Value = item.FromResolution.ID;
            cmd.Parameters.Add("@Type", SqlDbType.TinyInt).Value = (byte)item.ExpertiseType;
            cmd.Parameters.Add("@PreviousResolution", SqlDbType.Int).Value = ConvertToDBNull(item.PreviousExpertise);
            cmd.Parameters.Add("@SpendHours", SqlDbType.SmallInt).Value = ConvertToDBNull(item.SpendHours);
            cmd.Parameters.Add("@NObject", SqlDbType.SmallInt).Value = ConvertToDBNull(item.ObjectsCount);
            cmd.Parameters.Add("@NCat", SqlDbType.TinyInt).Value = ConvertToDBNull(item.CategoricalAnswers);
            cmd.Parameters.Add("@NVer", SqlDbType.TinyInt).Value = ConvertToDBNull(item.ProbabilityAnswers);
            cmd.Parameters.Add("@NAlt", SqlDbType.TinyInt).Value = ConvertToDBNull(item.AlternativeAnswers);
            cmd.Parameters.Add("@NNPV_Metod", SqlDbType.TinyInt).Value = ConvertToDBNull(item.NPV_MetodAnswers);
            cmd.Parameters.Add("@NNPV_Mater", SqlDbType.TinyInt).Value = ConvertToDBNull(item.NPV_MaterialAnswers);
            cmd.Parameters.Add("@NNPV_Comp", SqlDbType.TinyInt).Value = ConvertToDBNull(item.NPV_CompAnswers);
            cmd.Parameters.Add("@NNPV_Other", SqlDbType.TinyInt).Value = ConvertToDBNull(item.NPV_OtherAnswers);
            cmd.Parameters.Add("@Comment", SqlDbType.NVarChar, 500).Value = ConvertToDBNull(item.Comment);
            cmd.Parameters.Add("@Evaluation", SqlDbType.TinyInt).Value = ConvertToDBNull(item.Evaluation);
            cmd.Parameters.Add("@ExpIden", SqlDbType.Int).Value = item.ID;
            try
            {
                cmd.ExecuteNonQuery();
                item.Version = Version.Original;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public override void Delete(Expertise item)
        {
            if (item.Version == Version.New) return;
            SqlCommand cmd = _storage.DBConnection.CreateCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "Activity.prDeleteExpertise";
            cmd.Parameters.Add("@id", SqlDbType.Int).Value = item.ID;
            try
            {
                cmd.Connection.Open();
                cmd.ExecuteNonQuery();
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                cmd.Connection.Close();
            }
        }
        public override void SaveChanges(Expertise item, SqlTransaction tran = null)
        {
            if (item == null) return;
            if (tran == null)
            {
                SqlTransaction transaction = null;
                SqlConnection con = _storage.DBConnection;
                try
                {
                    con.Open();
                    transaction = con.BeginTransaction();
                    _storage.ExpertAccessService.SaveChanges(item.Expert, transaction);
                    base.SaveChanges(item, transaction);
                    SaveContentToDB(item, transaction);
                    transaction.Commit();
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    throw;
                }
                finally
                {
                    con.Close();
                }
            }
            else
            {
                try
                {
                    _storage.ExpertAccessService.SaveChanges(item.Expert, tran);
                    base.SaveChanges(item, tran);
                    SaveContentToDB(item, tran);
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
        /// <summary>
        /// Завершает загрузку из БД оставшихся полей (кол-во ответов, список счетов и т.п)
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public async Task FinishDownloadAsync(Expertise item)
        {
            var cmd = _storage.DBConnection.CreateCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "Activity.prExpertise_EndLoad";
            cmd.Parameters.Add("@id", SqlDbType.Int).Value = item.ID;
            SqlDataReader rd = null;
            try
            {
                item.UploadStatus = UploadResult.Performing;
                cmd.Connection.Open();
                rd = await cmd.ExecuteReaderAsync();
                if (rd.HasRows)
                {
                    rd.Read();
                    var ver = item.Version;
                    item.SpendHours = rd.IsDBNull(0) ? null : new short?(rd.GetInt16(0));
                    item.ObjectsCount = rd.IsDBNull(1) ? null : new short?(rd.GetInt16(1));
                    item.CategoricalAnswers = rd.IsDBNull(2) ? null : new short?(rd.GetByte(2));
                    item.ProbabilityAnswers = rd.IsDBNull(3) ? null : new short?(rd.GetByte(3));
                    item.AlternativeAnswers = rd.IsDBNull(4) ? null : new short?(rd.GetByte(4));
                    item.NPV_MetodAnswers = rd.IsDBNull(5) ? null : new short?(rd.GetByte(5));
                    item.NPV_MaterialAnswers = rd.IsDBNull(6) ? null : new short?(rd.GetByte(6));
                    item.NPV_CompAnswers = rd.IsDBNull(7) ? null : new short?(rd.GetByte(7));
                    item.NPV_OtherAnswers = rd.IsDBNull(8) ? null : new short?(rd.GetByte(8));
                    item.Comment = rd.IsDBNull(9) ? null : rd.GetString(9);
                    item.Evaluation = rd.IsDBNull(10) ? null : new int?(rd.GetByte(10));
                    if (rd.NextResult())
                    {
                        while (rd.Read())
                        {
                            Bill b = new Bill(id: rd.GetInt32(1),
                                                from: item,
                                                number: rd.GetString(2),
                                                billdate: rd.GetDateTime(0),
                                                paiddate: rd.IsDBNull(6) ? null : new DateTime?(rd.GetDateTime(6)),
                                                payer: rd.IsDBNull(7) ? null : rd.GetString(7),
                                                hours: rd.GetByte(4),
                                                hourprice: rd.GetDecimal(3),
                                                paid: rd.GetDecimal(5),
                                                vr: Version.Original);
                            item.Bills.Add(b);
                        }
                    }
                    if (rd.NextResult())
                    {
                        while (rd.Read())
                        {
                            EquipmentUsage usage = new EquipmentUsage(id: rd.GetInt32(0),
                                                                        from: item,
                                                                        usagedate: rd.GetDateTime(2),
                                                                        duration: rd.GetByte(3),
                                                                        equipment: App.Storage.EquipmentAccessService.GetItemByID(rd.GetInt16(1)),
                                                                        version: Version.Original
                                                                        );
                            item.EquipmentUsages.Add(usage);
                        }
                    }
                    item.UploadStatus = UploadResult.Sucsess;
                    item.Version = ver;
                }
            }
            catch (Exception)
            {
                item.UploadStatus = UploadResult.Error;
            }
            finally
            {
                rd.Close();
                cmd.Connection.Close();
            }
        }
        public async Task<(IEnumerable<Expertise>, Dictionary<string, int>)> LoadExpertiseInWorkAsync(Employee employee)
        {
            var r = await Task.Run<(IEnumerable<Expertise>, Dictionary<string, int>)>(async () =>
            {
                SqlCommand cmd = _storage.DBConnection.CreateCommand();
                cmd.CommandText = "Activity.prActiveExpertiseSummary";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@empid", SqlDbType.Int).Value = employee.ID;
                SqlDataReader rd = null;
                List<Expertise> _expertises = new List<Expertise>(50);
                Dictionary<string, int> _case_occurs = new Dictionary<string, int>(7);
                foreach (var item in _storage.CaseTypes)
                {
                    _case_occurs.Add(item.Code, 0);
                }
                try
                {
                    cmd.Connection.Open();
                    rd = await cmd.ExecuteReaderAsync();
                    if (rd.HasRows)
                    {
                        int colExpertiseID = rd.GetOrdinal("ExpertiseID");
                        int colNumber = rd.GetOrdinal("Number");
                        int colStartDate = rd.GetOrdinal("StartDate");
                        int colExpertiseType = rd.GetOrdinal("TypeExpertise");
                        int colTimelimit = rd.GetOrdinal("Timelimit");
                        int colExpertID = rd.GetOrdinal("ExpertID");
                        int colCaseType = rd.GetOrdinal("TypeCase");
                        int colBillID = rd.GetOrdinal("BillID");
                        int colBillDate = rd.GetOrdinal("BillDate");
                        int colBillNumber = rd.GetOrdinal("BillNumber");
                        int colHourPrice = rd.GetOrdinal("HourPrice");
                        int colNHours = rd.GetOrdinal("NHours");
                        int colPaid = rd.GetOrdinal("Paid");
                        int colPaidDate = rd.GetOrdinal("PaidDate");
                        int colPayer = rd.GetOrdinal("Payer");
                        int colMContent = rd.GetOrdinal("Content");
                        int colMDalayDAte = rd.GetOrdinal("DelayDate");
                        int colMDate = rd.GetOrdinal("EntityDate");
                        int colMType = rd.GetOrdinal("EntityType");
                        int colMID = rd.GetOrdinal("ID");
                        int colMReferTo = rd.GetOrdinal("ReferTo");
                        int colMRegDate = rd.GetOrdinal("RegistrationDate");
                        int colMSuspend = rd.GetOrdinal("Suspend");
                        int colMResume = rd.GetOrdinal("Resume");
                        int curID = 0, curbillID = 0, curMID = 0;
                        Expertise _exp = null;
                        while (rd.Read())
                        {
                            if (curID != rd.GetInt32(colExpertiseID))
                            {
                                curID = rd.GetInt32(colExpertiseID);
                                _exp = new Expertise(id: curID,
                                                     number: rd.GetString(colNumber),
                                                     expert: _storage.ExpertAccessService.GetItemByID(rd.GetInt32(colExpertID)),
                                                     result: ExpertiseResults.Unknown,
                                                     start: rd.GetDateTime(colStartDate),
                                                     end: null,
                                                     timelimit: rd.GetByte(colTimelimit),
                                                     type: (ExpertiseTypes)rd.GetByte(colExpertiseType),
                                                     previous: null,
                                                     spendhours: null,
                                                     vr: Version.Original);
                                _expertises.Add(_exp);
                            }
                            if (!rd.IsDBNull(colBillID) && curbillID != rd.GetInt32(colBillID))
                            {
                                curbillID = rd.GetInt32(colBillID);
                                Bill bill = new Bill(id: curbillID,
                                                    from: _exp,
                                                    number: rd.GetString(colBillNumber),
                                                    billdate: rd.GetDateTime(colBillDate),
                                                    paiddate: rd.IsDBNull(colPaidDate) ? null : new DateTime?(rd.GetDateTime(colPaidDate)),
                                                    payer: rd.IsDBNull(colPayer) ? null : rd.GetString(colPayer),
                                                    hours: rd.GetByte(colNHours),
                                                    hourprice: rd.GetDecimal(colHourPrice),
                                                    paid: rd.GetDecimal(colPaid),
                                                    vr: Version.Original);
                                _exp.Bills.Add(bill);
                            }
                            if (!rd.IsDBNull(colMID) && curMID != rd.GetInt32(colMID))
                            {
                                curMID = rd.GetInt32(colMID);
                                switch (rd.GetByte(colMType))//1 - Request, 2 - Responce, 3 - Report, 4 - Outcoming Letter, 5 - Incoming Letter
                                {
                                    case 1:
                                        var rq = new Request(id: rd.GetInt32(colMID),
                                                                from: _exp,
                                                                register: rd.GetDateTime(colMRegDate),
                                                                suspend: rd.GetBoolean(colMSuspend),
                                                                content: rd.IsDBNull(colMContent) ? null : rd.GetString(colMContent),
                                                                create: rd.IsDBNull(colMDate) ? rd.GetDateTime(colMRegDate) : rd.GetDateTime(colMDate),
                                                                version: Version.Original);
                                        _exp.Movements.Add(rq);
                                        break;
                                    case 2:
                                        var res = new Response(id: rd.GetInt32(colMID),
                                                                from: _exp,
                                                                register: rd.GetDateTime(colMRegDate),
                                                                refer: rd.IsDBNull(colMReferTo) ? 0 : rd.GetInt32(colMReferTo),
                                                                resume: rd.IsDBNull(colMResume) ? false : rd.GetBoolean(colMResume),
                                                                content: rd.IsDBNull(colMContent) ? null : rd.GetString(colMContent),
                                                                create: rd.IsDBNull(colMDate) ? rd.GetDateTime(colMRegDate) : rd.GetDateTime(colMDate),
                                                                version: Version.Original);
                                        _exp.Movements.Add(res);
                                        break;
                                    case 3:
                                        var rep = new Report(id: rd.GetInt32(colMID),
                                                                from: _exp,
                                                                register: rd.GetDateTime(colMRegDate),
                                                                delay: rd.GetDateTime(colMDalayDAte),
                                                                reason: rd.IsDBNull(colMContent) ? null : rd.GetString(colMContent),
                                                                create: rd.IsDBNull(colMDate) ? rd.GetDateTime(colMRegDate) : rd.GetDateTime(colMDate),
                                                                version: Version.Original);
                                        _exp.Movements.Add(rep);
                                        break;
                                    case 4:
                                        var ol = new OutcomingLetter(id: rd.GetInt32(colMID),
                                                                    from: _exp,
                                                                    register: rd.GetDateTime(colMRegDate),
                                                                    create: rd.IsDBNull(colMDate) ? rd.GetDateTime(colMRegDate) : rd.GetDateTime(colMDate),
                                                                    version: Version.Original);
                                        _exp.Movements.Add(ol);
                                        break;
                                    case 5:
                                        var il = new IncomingLetter(id: rd.GetInt32(colMID),
                                                                    from: _exp,
                                                                    register: rd.GetDateTime(colMRegDate),
                                                                    create: rd.IsDBNull(colMDate) ? rd.GetDateTime(colMRegDate) : rd.GetDateTime(colMDate),
                                                                    version: Version.Original);
                                        _exp.Movements.Add(il);
                                        break;
                                    default:
                                        App.ErrorLogger.LogError("Unknown type of expertise movement. Movement not created");
                                        break;
                                }
                            }
                            _case_occurs[rd.GetString(colCaseType)]++;
                        }
                    }
                }
                finally
                {
                    rd?.Close();
                    cmd.Connection.Close();
                }
                return (_expertises, _case_occurs);
            });
            return r;
        }
        public ExpertiseDataAccess(ILocalStorage storage) : base(storage) { }
    }

    public class BillDataAccess : DataAccess<Bill>
    {
        public override Bill GetItemByID(int id) => throw new NotImplementedException();
        public override ICollection<Bill> Items() => throw new NotImplementedException();
        protected override void Add(Bill item, SqlConnection connection, SqlTransaction tran = null)
        {
            SqlCommand cmd = connection.CreateCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "Activity.prAddBill";
            cmd.Transaction = tran;
            cmd.Parameters.Add("@Num", SqlDbType.VarChar, 5).Value = item.Number;
            cmd.Parameters.Add("@ExpertiseID", SqlDbType.Int).Value = item.FromExpertise.ID;
            cmd.Parameters.Add("@BillDate", SqlDbType.Date).Value = item.BillDate;
            cmd.Parameters.Add("@Payer", SqlDbType.NVarChar, 100).Value = item.Payer;
            cmd.Parameters.Add("@Nhours", SqlDbType.TinyInt).Value = item.Hours;
            cmd.Parameters.Add("@HourPrice", SqlDbType.Money).Value = item.HourPrice;
            cmd.Parameters.Add("@Paid", SqlDbType.Money).Value = item.Paid;
            cmd.Parameters.Add("@PaidDate", SqlDbType.Date).Value = ConvertToDBNull(item.PaidDate);
            var par = cmd.Parameters.Add("@InsertedID", SqlDbType.Int);
            par.Direction = ParameterDirection.Output;
            try
            {
                cmd.ExecuteNonQuery();
                item.ID = (int)cmd.Parameters["@InsertedID"].Value;
                item.Version = Version.Original;
            }
            catch (Exception)
            {
                throw;
            }
        }
        protected override void Edit(Bill item, SqlConnection connection, SqlTransaction tran = null)
        {
            SqlCommand cmd = connection.CreateCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "Activity.prAddBill";
            cmd.Transaction = tran;
            cmd.Parameters.Add("@Num", SqlDbType.VarChar, 5).Value = item.Number;
            cmd.Parameters.Add("@ExpertiseID", SqlDbType.Int).Value = item.FromExpertise.ID;
            cmd.Parameters.Add("@BillDate", SqlDbType.Date).Value = item.BillDate;
            cmd.Parameters.Add("@Payer", SqlDbType.NVarChar, 100).Value = item.Payer;
            cmd.Parameters.Add("@Nhours", SqlDbType.TinyInt).Value = item.Hours;
            cmd.Parameters.Add("@HourPrice", SqlDbType.Money).Value = item.HourPrice;
            cmd.Parameters.Add("@Paid", SqlDbType.Money).Value = item.Paid;
            cmd.Parameters.Add("@PaidDate", SqlDbType.Date).Value = ConvertToDBNull(item.PaidDate);
            cmd.Parameters.Add("@BillId", SqlDbType.Int).Value = item.ID;
            try
            {
                cmd.ExecuteNonQuery();
                item.Version = Version.Original;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public override void Delete(Bill item)
        {
            if (item.Version == Version.New) return;
            SqlCommand cmd = _storage.DBConnection.CreateCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "Activity.prDeleteBill";
            cmd.Parameters.Add("@BillID", SqlDbType.Int).Value = item.ID;
            try
            {
                cmd.Connection.Open();
                cmd.ExecuteNonQuery();
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                cmd.Connection.Close();
            }
        }
        public BillDataAccess(ILocalStorage storage) : base(storage) { }
    }

    public class MovementDataAccess : DataAccess<ExpertiseMovement>
    {
        public override ExpertiseMovement GetItemByID(int id) => throw new NotImplementedException();
        public override ICollection<ExpertiseMovement> Items() => throw new NotImplementedException();
        protected override void Add(ExpertiseMovement item, SqlConnection connection, SqlTransaction tran = null)
        {
            switch (item)
            {
                case Request r:
                    AddRequest(r, connection, tran);
                    break;
                case Response r:
                    AddResponse(r, connection, tran);
                    break;
                case Report r:
                    AddReport(r, connection, tran);
                    break;
                case IncomingLetter l:
                    AddIncomingLetter(l, connection, tran);
                    break;
                case OutcomingLetter l:
                    AddOutComingLetter(l, connection, tran);
                    break;
                default:
                    App.ErrorLogger.LogError("Unknown type of ExpertiseMovement");
                    break;
            }
        }
        protected override void Edit(ExpertiseMovement item, SqlConnection connection, SqlTransaction tran = null)
        {
            switch (item)
            {
                case Request r:
                    EditRequest(r, connection, tran);
                    break;
                case Response r:
                    EditResponse(r, connection, tran);
                    break;
                case Report r:
                    EditReport(r, connection, tran);
                    break;
                case IncomingLetter l:
                    EditIncomingLetter(l, connection, tran);
                    break;
                case OutcomingLetter l:
                    EditOutComingLetter(l, connection, tran);
                    break;
                default:
                    App.ErrorLogger.LogError("Unknown type of ExpertiseMovement");
                    break;
            }
            //SqlCommand cmd = connection.CreateCommand();
            //cmd.CommandType = CommandType.StoredProcedure;
            //cmd.CommandText = "Activity.prEditExpertiseMovementEntity";
            //cmd.Transaction = tran;
            //cmd.Parameters.Add("@EntityDate", SqlDbType.Date).Value = ConvertToDBNull(item.CreationDate);
            //cmd.Parameters.Add("@RegistrationDate", SqlDbType.Date).Value = item.RegistrationDate;
            //cmd.Parameters.Add("@DelayDate", SqlDbType.Date).Value = ConvertToDBNull(item.DelayDate);
            //cmd.Parameters.Add("@ReferTo", SqlDbType.Int).Value = ConvertToDBNull(item.ReferTo);
            //cmd.Parameters.Add("@Content", SqlDbType.NVarChar, 1000).Value = ConvertToDBNull(item.Content);
            //cmd.Parameters.Add("@Suspend", SqlDbType.Bit).Value = ConvertToDBNull(item.SuspendExecution);
            //cmd.Parameters.Add("@EntityID", SqlDbType.Int).Value = item.ID;
            //try
            //{
            //    cmd.ExecuteNonQuery();
            //    item.Version = Version.Original;
            //}
            //catch (Exception)
            //{
            //    throw;
            //}
        }
        public override void Delete(ExpertiseMovement item)
        {
            if (item.Version == Version.New) return;
            SqlCommand cmd = _storage.DBConnection.CreateCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "Activity.prDeleteExpertiseMovementEntity";
            cmd.Parameters.Add("@EntityID", SqlDbType.Int).Value = item.ID;
            try
            {
                cmd.Connection.Open();
                cmd.ExecuteNonQuery();
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                cmd.Connection.Close();
            }
        }
        private void AddRequest(Request request, SqlConnection connection, SqlTransaction tran = null)
        {
            SqlCommand cmd = connection.CreateCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "Activity.prAddExpertiseMovementEntity";
            cmd.Transaction = tran;
            cmd.Parameters.Add("@ExpertiseID", SqlDbType.Int).Value = request.FromExpertise.ID;
            cmd.Parameters.Add("@EntityType", SqlDbType.TinyInt).Value = request.GetMovementTypeID();
            cmd.Parameters.Add("@EntityDate", SqlDbType.Date).Value = ConvertToDBNull(request.CreationDate);
            cmd.Parameters.Add("@RegistrationDate", SqlDbType.Date).Value = request.RegistrationDate;
            cmd.Parameters.Add("@DelayDate", SqlDbType.Date).Value = DBNull.Value;
            cmd.Parameters.Add("@ReferTo", SqlDbType.Int).Value = DBNull.Value;
            cmd.Parameters.Add("@Content", SqlDbType.NVarChar, 1000).Value = ConvertToDBNull(request.Content);
            cmd.Parameters.Add("@Suspend", SqlDbType.Bit).Value = ConvertToDBNull(request.SuspendExecution);
            cmd.Parameters.Add("@Resume", SqlDbType.Bit).Value = DBNull.Value;
            var par = cmd.Parameters.Add("@InsertedID", SqlDbType.Int);
            par.Direction = ParameterDirection.Output;
            try
            {
                cmd.ExecuteNonQuery();
                request.ID = (int)cmd.Parameters["@InsertedID"].Value;
                request.Version = Version.Original;
            }
            catch (Exception ex)
            {
                App.ErrorLogger.LogError(ex.Message);
                throw;
            }
        }
        private void AddResponse(Response response, SqlConnection connection, SqlTransaction tran = null)
        {
            SqlCommand cmd = connection.CreateCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "Activity.prAddExpertiseMovementEntity";
            cmd.Transaction = tran;
            cmd.Parameters.Add("@ExpertiseID", SqlDbType.Int).Value = response.FromExpertise.ID;
            cmd.Parameters.Add("@EntityType", SqlDbType.TinyInt).Value = response.GetMovementTypeID();
            cmd.Parameters.Add("@EntityDate", SqlDbType.Date).Value = ConvertToDBNull(response.CreationDate);
            cmd.Parameters.Add("@RegistrationDate", SqlDbType.Date).Value = response.RegistrationDate;
            cmd.Parameters.Add("@DelayDate", SqlDbType.Date).Value = DBNull.Value;
            cmd.Parameters.Add("@ReferTo", SqlDbType.Int).Value = response.ReferTo;
            cmd.Parameters.Add("@Content", SqlDbType.NVarChar, 1000).Value = ConvertToDBNull(response.Content);
            cmd.Parameters.Add("@Suspend", SqlDbType.Bit).Value = DBNull.Value;
            cmd.Parameters.Add("@Resume", SqlDbType.Bit).Value = response.ResumeExecution;
            var par = cmd.Parameters.Add("@InsertedID", SqlDbType.Int);
            par.Direction = ParameterDirection.Output;
            try
            {
                cmd.ExecuteNonQuery();
                response.ID = (int)cmd.Parameters["@InsertedID"].Value;
                response.Version = Version.Original;
            }
            catch (Exception ex)
            {
                App.ErrorLogger.LogError(ex.Message);
                throw;
            }
        }
        private void AddReport(Report report, SqlConnection connection, SqlTransaction tran = null)
        {
            SqlCommand cmd = connection.CreateCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "Activity.prAddExpertiseMovementEntity";
            cmd.Transaction = tran;
            cmd.Parameters.Add("@ExpertiseID", SqlDbType.Int).Value = report.FromExpertise.ID;
            cmd.Parameters.Add("@EntityType", SqlDbType.TinyInt).Value = report.GetMovementTypeID();
            cmd.Parameters.Add("@EntityDate", SqlDbType.Date).Value = ConvertToDBNull(report.CreationDate);
            cmd.Parameters.Add("@RegistrationDate", SqlDbType.Date).Value = report.RegistrationDate;
            cmd.Parameters.Add("@DelayDate", SqlDbType.Date).Value = report.DelayDate;
            cmd.Parameters.Add("@ReferTo", SqlDbType.Int).Value = DBNull.Value;
            cmd.Parameters.Add("@Content", SqlDbType.NVarChar, 1000).Value = ConvertToDBNull(report.Reason);
            cmd.Parameters.Add("@Suspend", SqlDbType.Bit).Value = DBNull.Value;
            cmd.Parameters.Add("@Resume", SqlDbType.Bit).Value = DBNull.Value;
            var par = cmd.Parameters.Add("@InsertedID", SqlDbType.Int);
            par.Direction = ParameterDirection.Output;
            try
            {
                cmd.ExecuteNonQuery();
                report.ID = (int)cmd.Parameters["@InsertedID"].Value;
                report.Version = Version.Original;
            }
            catch (Exception ex)
            {
                App.ErrorLogger.LogError(ex.Message);
                throw;
            }
        }
        private void AddOutComingLetter(OutcomingLetter letter, SqlConnection connection, SqlTransaction tran = null)
        {
            App.ErrorLogger.LogError($"Invoked function {nameof(AddOutComingLetter)} not realize");
        }
        private void AddIncomingLetter(IncomingLetter letter, SqlConnection connection, SqlTransaction tran = null)
        {
            App.ErrorLogger.LogError($"Invoked function {nameof(AddIncomingLetter)} don't realize");
        }
        private void EditRequest(Request request, SqlConnection connection, SqlTransaction tran = null)
        {
            SqlCommand cmd = connection.CreateCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "Activity.prEditExpertiseMovementEntity";
            cmd.Transaction = tran;
            cmd.Parameters.Add("@EntityDate", SqlDbType.Date).Value = ConvertToDBNull(request.CreationDate);
            cmd.Parameters.Add("@RegistrationDate", SqlDbType.Date).Value = request.RegistrationDate;
            cmd.Parameters.Add("@DelayDate", SqlDbType.Date).Value = DBNull.Value;
            cmd.Parameters.Add("@ReferTo", SqlDbType.Int).Value = DBNull.Value;
            cmd.Parameters.Add("@Content", SqlDbType.NVarChar, 1000).Value = ConvertToDBNull(request.Content);
            cmd.Parameters.Add("@Suspend", SqlDbType.Bit).Value = request.SuspendExecution;
            cmd.Parameters.Add("@Resume", SqlDbType.Bit).Value = DBNull.Value;
            cmd.Parameters.Add("@EntityID", SqlDbType.Int).Value = request.ID;
            try
            {
                cmd.ExecuteNonQuery();
                request.Version = Version.Original;
            }
            catch (Exception ex)
            {
                App.ErrorLogger.LogError(ex.Message);
                throw;
            }
        }
        private void EditResponse(Response response, SqlConnection connection, SqlTransaction tran = null)
        {
            SqlCommand cmd = connection.CreateCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "Activity.prEditExpertiseMovementEntity";
            cmd.Transaction = tran;
            cmd.Parameters.Add("@EntityDate", SqlDbType.Date).Value = ConvertToDBNull(response.CreationDate);
            cmd.Parameters.Add("@RegistrationDate", SqlDbType.Date).Value = response.RegistrationDate;
            cmd.Parameters.Add("@DelayDate", SqlDbType.Date).Value = DBNull.Value;
            cmd.Parameters.Add("@ReferTo", SqlDbType.Int).Value = DBNull.Value;
            cmd.Parameters.Add("@Content", SqlDbType.NVarChar, 1000).Value = ConvertToDBNull(response.Content);
            cmd.Parameters.Add("@Suspend", SqlDbType.Bit).Value = DBNull.Value;
            cmd.Parameters.Add("@Resume", SqlDbType.Bit).Value = response.ResumeExecution;
            cmd.Parameters.Add("@EntityID", SqlDbType.Int).Value = response.ID;
            try
            {
                cmd.ExecuteNonQuery();
                response.Version = Version.Original;
            }
            catch (Exception ex)
            {
                App.ErrorLogger.LogError(ex.Message);
                throw;
            }
        }
        private void EditReport(Report report, SqlConnection connection, SqlTransaction tran = null)
        {
            SqlCommand cmd = connection.CreateCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "Activity.prEditExpertiseMovementEntity";
            cmd.Transaction = tran;
            cmd.Parameters.Add("@EntityDate", SqlDbType.Date).Value = ConvertToDBNull(report.CreationDate);
            cmd.Parameters.Add("@RegistrationDate", SqlDbType.Date).Value = report.RegistrationDate;
            cmd.Parameters.Add("@DelayDate", SqlDbType.Date).Value = report.DelayDate;
            cmd.Parameters.Add("@ReferTo", SqlDbType.Int).Value = DBNull.Value;
            cmd.Parameters.Add("@Content", SqlDbType.NVarChar, 1000).Value = ConvertToDBNull(report.Reason);
            cmd.Parameters.Add("@Suspend", SqlDbType.Bit).Value = DBNull.Value;
            cmd.Parameters.Add("@Resume", SqlDbType.Bit).Value = DBNull.Value;
            cmd.Parameters.Add("@EntityID", SqlDbType.Int).Value = report.ID;
            try
            {
                cmd.ExecuteNonQuery();
                report.Version = Version.Original;
            }
            catch (Exception ex)
            {
                App.ErrorLogger.LogError(ex.Message);
                throw;
            }
        }
        private void EditOutComingLetter(OutcomingLetter letter, SqlConnection connection, SqlTransaction tran = null)
        {
            App.ErrorLogger.LogError($"Invoked function {nameof(EditOutComingLetter)} don't realize");
        }
        private void EditIncomingLetter(IncomingLetter letter, SqlConnection connection, SqlTransaction tran = null)
        {
            App.ErrorLogger.LogError($"Invoked function {nameof(EditIncomingLetter)} don't realize");
        }
        public MovementDataAccess(ILocalStorage storage) : base(storage) { }
    }
}
