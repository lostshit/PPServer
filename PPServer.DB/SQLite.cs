﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.Common;
using System.Data.SQLite;

namespace PPServer.DB
{
    internal class SQLite : Database, IDatabase
    {
        /// <summary>
        /// 构造SQLite数据库操作对象
        /// </summary>
        public SQLite()
        {
            this.DBName = "SQLite";
        }
        /// <summary>
        /// 初始化连接
        /// </summary>
        /// <param name="ConnectionParams">连接参数，0:数据库文件地址，1:密码</param>
        /// <returns></returns>
        public bool Init(params object[] ConnectionParams)
        {
            string _DBFilePath = ConnectionParams[0].ToString().Trim();
            try
            {                
                string _password = (ConnectionParams.Length==1 || ConnectionParams[1] == null) ? null : ConnectionParams[1].ToString().Trim();
                SQLiteConnectionStringBuilder _connStr = new SQLiteConnectionStringBuilder();
                _connStr.DataSource = _DBFilePath;
                if (!string.IsNullOrEmpty(_password))
                    _connStr.Password = _password;
                //测试连接
                using (DbConnection _conn = new SQLiteConnection(_connStr.ToString()))
                {
                    _conn.Open();
                    _conn.Close();
                }
                this.ConnectionString = _connStr.ToString();
                this.Inited = true;
                WriteToLog("初始化[" + this.DBName + "]数据库连接: " + _DBFilePath);
                return true;
            }
            catch (Exception ex)
            {
                this.Inited = false;
                WriteToLog("初始化[" + this.DBName + "]数据库\"" + _DBFilePath + "\"失败: " + ex.Message, true);
            }
            return false;
        }
        #region 基础执行方法
        public int ExecuteSQL(string SQL, DbParameter[] Params = null)
        {
            try
            {
                DealNullParam(ref Params);  //处理空数据
                using (SQLiteConnection conn = new SQLiteConnection(ConnectionString))
                {
                    conn.Open();
                    SQLiteCommand _cmd = new SQLiteCommand(SQL, conn);
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
                SQLiteConnection _conn = new SQLiteConnection(ConnectionString);
                SQLiteCommand _cmd = new SQLiteCommand(SQL, _conn);
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
                using (SQLiteConnection conn = new SQLiteConnection(ConnectionString))
                {
                    conn.Open();
                    SQLiteCommand _cmd = new SQLiteCommand(SQL, conn);
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
                using (SQLiteConnection conn = new SQLiteConnection(ConnectionString))
                {
                    conn.Open();
                    SQLiteCommand _cmd = new SQLiteCommand(SQL, conn);
                    _cmd.CommandType = IsProcedure ? CommandType.StoredProcedure : CommandType.Text;
                    if (Params != null)
                        _cmd.Parameters.AddRange(Params);
                    SQLiteDataAdapter _adapter = new SQLiteDataAdapter(_cmd);
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
            return new SQLiteParameter(Name, Value == null ? DBNull.Value : Value);
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
