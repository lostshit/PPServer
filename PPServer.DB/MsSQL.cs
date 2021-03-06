﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Data.Common;

namespace PPServer.DB
{
    internal class MsSQL : Database, IDatabase
    {
        public MsSQL()
        {
            this.DBName = "MsSQL";
        }
        /// <summary>
        /// 初始化连接
        /// </summary>
        /// <param name="ConnectionParams">连接参数，0:数据库地址,1:端口号,2:是否使用本地身份验证,3:用户名,4:密码,5:当前数据库,6:其他参数</param>
        /// <returns></returns>
        public bool Init(params object[] ConnectionParams)
        {
            try
            {
                string _DBServer = ConnectionParams[0].ToString().Trim();
                int _port = ConnectionParams[1] == null ? 0 : (int)ConnectionParams[1];
                bool _windowsAuthentication = ConnectionParams[2] == null ? false : (bool)ConnectionParams[2];
                string _userName = ConnectionParams[3] == null ? string.Empty : ConnectionParams[3].ToString().Trim();
                string _password = ConnectionParams[4] == null ? string.Empty : ConnectionParams[4].ToString().Trim();
                string _initDB = ConnectionParams[5] == null ? string.Empty : ConnectionParams[5].ToString().Trim();
                string _otherParams = ConnectionParams[6] == null ? string.Empty : ConnectionParams[6].ToString().Trim().Trim(';');
                string _connStr = "data source=" + _DBServer + ((_port == 0 || _port == 1433) ? "" : ("," + _port.ToString())) + (_windowsAuthentication ? ";Integrated Security=SSPI" : (";user id=" + _userName + ";password=" + _password)) + ";initial catalog=" + _initDB + ";Persist Security Info=False" + (_otherParams == string.Empty ? string.Empty : (";" + _otherParams));

                //测试连接
                using (DbConnection _conn = new SqlConnection(_connStr))
                {
                    _conn.Open();
                    _conn.Close();
                }
                this.ConnectionString = _connStr;
                this.Inited = true;
                WriteToLog("初始化[" + this.DBName + "]数据库连接: " + _DBServer + ((_port == 0 || _port == 1433) ? string.Empty : (":" + _port.ToString())), false);
                return true;
            }
            catch (Exception ex)
            {
                this.Inited = false;
                WriteToLog("初始化[" + this.DBName + "]数据库失败: " + ex.Message, true);
            }
            return false;
        }

        #region 基础执行方法
        public int ExecuteSQL(string SQL, DbParameter[] Params = null)
        {
            try
            {
                DealNullParam(ref Params);  //处理空数据
                using (SqlConnection conn = new SqlConnection(ConnectionString))
                {
                    conn.Open();
                    SqlCommand _cmd = new SqlCommand(SQL, conn);
                    _cmd.CommandType = CommandType.Text;
                    if (Params != null)
                        _cmd.Parameters.AddRange(Params);
                    int _result = _cmd.ExecuteNonQuery();
                    conn.Close();
                    _cmd.Dispose();
                    conn.Dispose();
                    WriteToLog(string.Format("[{0}]执行SQL:{1},参数:{2}", this.DBName, SQL, (Params == null || Params.Length == 0) ? "无" : (string.Join(",", Params.Select(t => t.ParameterName + "='" + t.Value.ToString() + "'")))));
                    return _result;
                }
            }
            catch (Exception ex)
            {
                WriteToLog(string.Format("[{0}]执行SQL:{1},参数:{2},产生异常:{3}", this.DBName, SQL, (Params == null || Params.Length == 0) ? "无" : (string.Join(",", Params.Select(t => t.ParameterName + "='" + t.Value.ToString() + "'"))), ex.Message), true);
            }
            return 0;
        }
        public DbDataReader ExecuteReader(string SQL, DbParameter[] Params = null, bool IsProcedure = false)
        {
            try
            {
                DealNullParam(ref Params);  //处理空数据
                SqlConnection _conn = new SqlConnection(ConnectionString);
                SqlCommand _cmd = new SqlCommand(SQL, _conn);
                _cmd.CommandType = IsProcedure ? CommandType.StoredProcedure : CommandType.Text;
                if (Params != null)
                    _cmd.Parameters.AddRange(Params);
                _conn.Open();
                WriteToLog(string.Format("[{0}]执行Reader:{1},参数:{2}", this.DBName, SQL, (Params == null || Params.Length == 0) ? "无" : (string.Join(",", Params.Select(t => t.ParameterName + "='" + t.Value.ToString() + "'")))));
                return _cmd.ExecuteReader(CommandBehavior.CloseConnection);
            }
            catch (Exception ex)
            {
                WriteToLog(string.Format("[{0}]执行SQLReader:{1},参数:{2},产生异常:{3}", this.DBName, SQL, (Params == null || Params.Length == 0) ? "无" : (string.Join(",", Params.Select(t => t.ParameterName + "='" + t.Value.ToString() + "'"))), ex.Message), true);
            }
            return null;
        }
        public object ExecuteScalar(string SQL, DbParameter[] Params = null)
        {
            try
            {
                DealNullParam(ref Params);  //处理空数据
                using (SqlConnection conn = new SqlConnection(ConnectionString))
                {
                    conn.Open();
                    SqlCommand _cmd = new SqlCommand(SQL, conn);
                    _cmd.CommandType = CommandType.Text;
                    if (Params != null)
                        _cmd.Parameters.AddRange(Params);
                    object _result = _cmd.ExecuteScalar();
                    conn.Close();
                    _cmd.Dispose();
                    conn.Dispose();
                    WriteToLog(string.Format("[{0}]执行Scalar:{1},参数:{2}", this.DBName, SQL, (Params == null || Params.Length == 0) ? "无" : (string.Join(",", Params.Select(t => t.ParameterName + "='" + t.Value.ToString() + "'")))));
                    return _result;
                }
            }
            catch (Exception ex)
            {
                WriteToLog(string.Format("[{0}]执行Scalar:{1},参数:{2},产生异常:{3}", this.DBName, SQL, (Params == null || Params.Length == 0) ? "无" : (string.Join(",", Params.Select(t => t.ParameterName + "='" + t.Value.ToString() + "'"))), ex.Message), true);
            }
            return null;
        }
        public DataSet ExecuteDataSet(string SQL, DbParameter[] Params = null, bool IsProcedure = false)
        {
            try
            {
                DealNullParam(ref Params);  //处理空数据
                using (SqlConnection conn = new SqlConnection(ConnectionString))
                {
                    conn.Open();
                    SqlCommand _cmd = new SqlCommand(SQL, conn);
                    _cmd.CommandType = IsProcedure ? CommandType.StoredProcedure : CommandType.Text;
                    if (Params != null)
                        _cmd.Parameters.AddRange(Params);
                    SqlDataAdapter _adapter = new SqlDataAdapter(_cmd);
                    DataSet _result = new DataSet();
                    _adapter.Fill(_result);
                    conn.Close();
                    _cmd.Dispose();
                    conn.Dispose();
                    WriteToLog(string.Format("[{0}]执行DataSet:{1},参数:{2}", this.DBName, SQL, (Params == null || Params.Length == 0) ? "无" : (string.Join(",", Params.Select(t => t.ParameterName + "='" + t.Value.ToString() + "'")))));
                    return _result;
                }
            }
            catch (Exception ex)
            {
                WriteToLog(string.Format("[{0}]执行DataSet:{1},参数:{2},产生异常:{3}", this.DBName, SQL, (Params == null || Params.Length == 0) ? "无" : (string.Join(",", Params.Select(t => t.ParameterName + "='" + t.Value.ToString() + "'"))), ex.Message), true);
            }
            return null;
        }

        /// <summary>
        /// 创建参数
        /// </summary>
        /// <param name="Name">参数名称</param>
        /// <param name="Value">参数值</param>
        /// <returns></returns>
        public DbParameter CreateParameter(string Name, object Value)
        {
            return new SqlParameter(Name, Value==null?DBNull.Value:Value);
        }
        #endregion

        #region 扩展执行方法
        public DbDataReader RunProcedureReader(string ProcName, DbParameter[] Params = null)
        {
            return ExecuteReader(ProcName, Params, true);
        }
        public DataSet RunProcedureDataSet(string ProcName, DbParameter[] Params = null)
        {
            return ExecuteDataSet(ProcName, Params, true);
        }
        public int Insert(string TableName, DbParameter[] Params)
        {
            string _sql = string.Format("INSERT INTO {0}({1}) VALUES({2})", TableName, string.Join(",", Params.Select(t => { return "[" + t.ParameterName.Trim('@') + "]"; })), string.Join(",", Params.Select(t => { return t.ParameterName; })));
            return ExecuteSQL(_sql, Params);
        }
        public int Update(string TableName, DbParameter[] Params, DbParameter[] WhereParams)
        {
            string _sql = string.Format("UPDATE {0} SET {1} WHERE {2}", TableName, string.Join(",", Params.Select(t => { return "[" + t.ParameterName.Trim('@') + "]" + "=" + t.ParameterName; })), string.Join(" AND ", WhereParams.Select(t => { return "[" + t.ParameterName.Trim('@') + "]" + "=" + t.ParameterName; })));
            List<DbParameter> _params = new List<DbParameter>();
            _params.AddRange(Params);
            _params.AddRange(WhereParams);
            return ExecuteSQL(_sql, _params.ToArray());
        }
        public int Delete(string TableName, DbParameter[] Params)
        {
            string _sql = string.Format("DELETE FROM {0} WHERE {1}", TableName, string.Join(" AND ", Params.Select(t => { return "[" + t.ParameterName.Trim('@') + "]" + "=" + t.ParameterName; })));
            return ExecuteSQL(_sql, Params);
        }
        public DbDataReader SelectReader(string TableName, string Fields = null, DbParameter[] WhereParams = null, string Order = null)
        {
            string _sql = string.Format("SELECT {0} FROM {1}" + (WhereParams == null ? string.Empty : " WHERE {2}") + (string.IsNullOrEmpty(Order) ? string.Empty : " ORDER BY {3}"), string.IsNullOrEmpty(Fields) ? "*" : Fields, TableName, string.Join(" AND ", WhereParams.Select(t => { return "[" + t.ParameterName.Trim('@') + "]" + "=" + t.ParameterName; })), Order);
            return ExecuteReader(_sql, WhereParams);
        }
        public DbDataReader SelectReader(string TableName, string Fields = null, string Where = null, string Order = null)
        {
            string _sql = string.Format("SELECT {0} FROM {1}" + (string.IsNullOrEmpty(Where) ? string.Empty : " WHERE {2}") + (string.IsNullOrEmpty(Order) ? string.Empty : " ORDER BY {3}"), string.IsNullOrEmpty(Fields) ? "*" : Fields, TableName, Where, Order);
            return ExecuteReader(_sql);
        }
        public DataSet SelectDataSet(string TableName, string Fields = null, DbParameter[] WhereParams = null, string Order = null)
        {
            string _sql = string.Format("SELECT {0} FROM {1}" + (WhereParams == null ? string.Empty : " WHERE {2}") + (string.IsNullOrEmpty(Order) ? string.Empty : " ORDER BY {3}"), string.IsNullOrEmpty(Fields) ? "*" : Fields, TableName, string.Join(" AND ", WhereParams.Select(t => { return "[" + t.ParameterName.Trim('@') + "]" + "=" + t.ParameterName; })), Order);
            return ExecuteDataSet(_sql, WhereParams);
        }
        public DataSet SelectDataSet(string TableName, string Fields = null, string Where = null, string Order = null)
        {
            string _sql = string.Format("SELECT {0} FROM {1}" + (string.IsNullOrEmpty(Where) ? string.Empty : " WHERE {2}") + (string.IsNullOrEmpty(Order) ? string.Empty : " ORDER BY {3}"), string.IsNullOrEmpty(Fields) ? "*" : Fields, TableName, Where, Order);
            return ExecuteDataSet(_sql);
        }
        #endregion
    }
}
