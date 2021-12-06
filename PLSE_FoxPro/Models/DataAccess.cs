using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;

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
            public virtual void Delete(T item) { throw new InvalidOperationException(); }
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
            protected override void Edit(Settlement item, SqlConnection connection, SqlTransaction tran = null) //TODO: implement
            {
                throw new NotSupportedException();    
                //base.Edit(item, connection, tran);
                //_cache.EditByID(item);
            }
            public override void Delete(Settlement item)
            {
                base.Delete(item);
                _cache.TryRemove(item.ID, out Settlement _);
            }
            public SettlementsDataAccessCached(ILocalStorage storage) : base(storage) { }
        }
}
