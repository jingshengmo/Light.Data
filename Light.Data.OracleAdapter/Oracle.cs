﻿using System;
using System.Data;
using Oracle.ManagedDataAccess.Client;

namespace Light.Data.OracleAdapter
{
	class Oracle : Database
	{
		public Oracle ()
		{
			_factory = new OracleCommandFactory ();
		}

		#region IDatabase 成员

		public override IDbConnection CreateConnection ()
		{
			return new OracleConnection ();
		}

		public override IDbConnection CreateConnection (string connectionString)
		{
			return new OracleConnection (connectionString);
		}

		public override IDbDataAdapter CreateDataAdapter (IDbCommand cmd)
		{
			OracleDataAdapter da = new OracleDataAdapter ();
			da.SelectCommand = (OracleCommand)cmd;
			return da;
		}

		public override IDbCommand CreateCommand (string sql)
		{
			OracleCommand command = new OracleCommand ();
			command.CommandText = sql;
			command.CommandTimeout = _commandTimeOut;
			command.BindByName = true;
			return command;
		}


		public override IDbCommand CreateCommand ()
		{
			OracleCommand command = new OracleCommand ();
			command.CommandTimeout = _commandTimeOut;
			command.BindByName = true;

			return command;
		}

		public override IDataParameter CreateParameter (string name, object value, string dbType, ParameterDirection direction)
		{
			string parameterName = name;
			if (!parameterName.StartsWith (":")) {
				parameterName = ":" + parameterName;
			}
			if (value is bool) {
				value = (bool)value ? 1 : 0;
			}
			OracleParameter sp = new OracleParameter (parameterName, value);
			if (value == null)
				sp.Value = DBNull.Value;
			sp.Direction = direction;
			OracleDbType oracletype;
			DbType dType;
			int size;
			if (!string.IsNullOrEmpty (dbType)) {
				if (ParseOracleType (dbType, out oracletype)) {
					sp.OracleDbType = oracletype;
				}
				else if (Utility.ParseDbType (dbType, out dType)) {
					sp.DbType = dType;
				}
				if (Utility.ParseSize (dbType, out size)) {
					sp.Size = size;
				}
			}
			return sp;
		}

		public override void FormatStoredProcedureParameter (IDataParameter dataParmeter)
		{
			if (dataParmeter.ParameterName.StartsWith (":")) {
				dataParmeter.ParameterName = dataParmeter.ParameterName.Substring (1);
			}
		}

		#endregion


		private static bool ParseOracleType (string dbType, out OracleDbType type)
		{
			type = OracleDbType.Varchar2;
			int index = dbType.IndexOf ('(');
			string typeString;
			if (index < 0) {
				typeString = dbType;
			}
			else if (index == 0) {
				return false;
			}
			else {
				typeString = dbType.Substring (0, index);
			}
			try {
				type = (OracleDbType)Enum.Parse (typeof(OracleDbType), typeString, true);
				return true;
			} catch {
				return false;
			}
		}

		public override void SetExtendParams (ExtendParamCollection extendParams)
		{
//			ExtendParamsCollection extendParams = new ExtendParamsCollection (arguments);

			if (extendParams ["InnerPager"] != null) {
				if (extendParams ["InnerPager"].ToLower () == "true") {
					InnerPager = true;
				}
			}

			if (extendParams ["TimeOut"] != null) {
				int timeout;
				if (int.TryParse (extendParams ["TimeOut"], out timeout)) {
					CommandTimeOut = timeout;
				}
			}

			if (extendParams ["RoundScale"] != null) {
				byte roundScale;
				if (byte.TryParse (extendParams ["RoundScale"], out roundScale)) {
					OracleCommandFactory oracleFactory = _factory as OracleCommandFactory;
					oracleFactory.SetRoundScale (roundScale);
				}
			}

			if (extendParams ["InnerPager"] != null) {
				if (extendParams ["InnerPager"].ToLower () == "true") {
					InnerPager = true;
				}
			}

			if (extendParams ["OracleIdentityAuto"] != null) {
				bool oracleIdentityAuto;
				if (bool.TryParse (extendParams ["OracleIdentityAuto"], out oracleIdentityAuto)) {
					OracleCommandFactory oracleFactory = _factory as OracleCommandFactory;
					oracleFactory.SetIdentityAuto (oracleIdentityAuto);
				}
			}

			if (extendParams ["StrictMode"] != null) {
				bool strictMode;
				if (bool.TryParse (extendParams ["StrictMode"], out strictMode)) {
					OracleCommandFactory oracleFactory = _factory as OracleCommandFactory;
					oracleFactory.SetStrictMode (strictMode);
				}
			}

		}


	}
}
