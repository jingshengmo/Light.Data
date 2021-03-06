﻿using System;
using System.Text;
using System.Collections.Generic;

namespace Light.Data.PostgreAdapter
{
	class PostgreCommandFactory: CommandFactory
	{
		public PostgreCommandFactory ()
		{
			_canInnerPage = true;
			_strictMode = true;
		}

		bool _strictMode;

		public void SetStrictMode (bool strictMode)
		{
			_strictMode = strictMode;
		}

		public override CommandData CreateTruncateCommand (DataTableEntityMapping mapping)
		{
			CommandData data = base.CreateTruncateCommand (mapping);
			if (mapping.IdentityField != null) {
				string restartSeq = string.Format ("alter sequence \"{0}\" restart;", GetIndentitySeq (mapping));
				data.CommandText += restartSeq;
			}
			return data;
		}

		public override string CreateBooleanQuerySql (string fieldName, bool isTrue)
		{
			return string.Format ("{0}={1}", fieldName, isTrue ? "true" : "false");
		}

		public override string CreateDataFieldSql (string fieldName)
		{
			if (_strictMode) {
				return string.Format ("\"{0}\"", fieldName);
			}
			else {
				return fieldName;
			}
		}

		public override string CreateDataTableSql (string tableName)
		{
			if (_strictMode) {
				return string.Format ("\"{0}\"", tableName);
			}
			else {
				return tableName;
			}
		}

		public override string CreateDividedSql (string field, object value, bool forward)
		{
			if (forward) {
				return string.Format ("({0}::float/{1})", field, value);
			}
			else {
				return string.Format ("({0}/{1}::float)", value, field);
			}
		}

		public override string CreateLogSql (string field)
		{
			return string.Format ("ln({0})", field);
		}

		protected override CommandData CreateSelectBaseCommand (DataEntityMapping mapping, string customSelect, QueryExpression query, OrderExpression order, Region region)//, bool distinct)
		{
			CommandData command = base.CreateSelectBaseCommand (mapping, customSelect, query, order, region);
			if (region != null) {
				if (region.Start == 0) {
					command.CommandText = string.Format ("{0} limit {1}", command.CommandText, region.Size);
				}
				else {
					command.CommandText = string.Format ("{0} limit {2} offset {1}", command.CommandText, region.Start, region.Size);
				}
			}
			return command;
		}

		public override CommandData[] CreateBulkInsertCommand (DataTableEntityMapping mapping, Array entitys, int batchCount)
		{
			if (entitys == null || entitys.Length == 0) {
				throw new ArgumentNullException ("entitys");
			}
			if (batchCount <= 0) {
				batchCount = 10;
			}

			int totalCount = entitys.Length;
			List<string> insertList = new List<string> ();
			foreach (DataFieldMapping field in mapping.NoIdentityFields) {
				insertList.Add (CreateDataFieldSql (field.Name));
			}

			string insert = string.Join (",", insertList);
			string insertsql = string.Format ("insert into {0}({1})", CreateDataTableSql (mapping.TableName), insert);

			int createCount = 0;
			int totalCreateCount = 0;
			StringBuilder values = new StringBuilder ();
			int paramIndex = 0;
			List<DataParameter> dataParams = new List<DataParameter> ();
			List<CommandData> commands = new List<CommandData> ();
			foreach (object entity in entitys) {
				List<DataParameter> entityParams = CreateColumnParameter (mapping.NoIdentityFields, entity);
				string[] valueList = new string[entityParams.Count];
				int index = 0;
				foreach (DataParameter dataParameter in entityParams) {
					string paramName = CreateParamName ("P" + paramIndex);
					valueList [index] = paramName;
					dataParameter.ParameterName = paramName;
					dataParams.Add (dataParameter);
					index++;
					paramIndex++;
				}
				string value = string.Join (",", valueList);
				values.AppendFormat ("({0})", value);
				createCount++;
				totalCreateCount++;
				if (createCount == batchCount || totalCreateCount == totalCount || totalCreateCount == totalCount - 1) {
					CommandData command = new CommandData (string.Format ("{0}values{1};", insertsql, values), dataParams);
					commands.Add (command);
					if (totalCreateCount == totalCount) {
						break;
					}
					dataParams = new List<DataParameter> ();
					createCount = 0;
					paramIndex = 0;
					values = new StringBuilder ();
				}
				else {
					values.Append (",");
				}
			}
			return commands.ToArray ();
		}

		public override string CreateCollectionParamsQuerySql (string fieldName, QueryCollectionPredicate predicate, List<DataParameter> dataParameters)
		{
			if (predicate == QueryCollectionPredicate.In || predicate == QueryCollectionPredicate.NotIn) {
				return base.CreateCollectionParamsQuerySql (fieldName, predicate, dataParameters);
			}
			string op = GetQueryCollectionPredicate (predicate);
			if (dataParameters.Count == 0) {
				throw new LightDataException (RE.EnumerableLengthNotAllowIsZero);
			}
			int i = 0;
			StringBuilder sb = new StringBuilder ();
			sb.AppendFormat ("{0} {1} (", fieldName, op);
			foreach (DataParameter dataParameter in dataParameters) {
				if (i > 0)
					sb.Append (" union all ");
				sb.AppendFormat ("select {0}", dataParameter.ParameterName);
				i++;
			}
			sb.Append (")");
			return sb.ToString ();
		}

		public override string CreateRandomOrderBySql (DataEntityMapping mapping, string aliasName, bool fullFieldName)
		{
			return "random()";
		}

		protected override string CreateIdentitySql (DataTableEntityMapping mapping)
		{
			if (mapping.IdentityField != null) {
				return string.Format ("select currval('\"{0}\"');", GetIndentitySeq (mapping));
			}
			else {
				return string.Empty;
			}
		}

		private static string GetIndentitySeq (DataTableEntityMapping mapping)
		{
			if (mapping.IdentityField == null) {
				throw new LightDataException (RE.DataTableNotIdentityField);
			}
			string seq;
			string postgreIdentity = mapping.ExtentParams ["PostgreIdentitySeq"];
			if (!string.IsNullOrEmpty (postgreIdentity)) {
				seq = postgreIdentity;
			}
			else {
				seq = string.Format ("{0}_{1}_seq", mapping.TableName, mapping.IdentityField.Name);
			}
			return seq;
		}

		public override string CreateMatchSql (string field, bool left, bool right)
		{
			StringBuilder sb = new StringBuilder ();
			if (left) {
				sb.AppendFormat ("'{0}'||", _wildcards);
			}
			sb.Append (field);
			if (right) {
				sb.AppendFormat ("||'{0}'", _wildcards);
			}
			return sb.ToString ();
		}

		public override string CreateDateSql (string field, string format)
		{
			if (string.IsNullOrEmpty (format)) {
				return string.Format ("date_trunc('day',{0})", field);
			}
			else {
				string format1 = format.ToUpper ();
				string sqlformat;
				switch (format1) {
				case "YMD":
					sqlformat = "YYYYMMDD";
					break;
				case "YM":
					sqlformat = "YYYYMM";
					break;
				case "Y-M-D":
					sqlformat = "YYYY-MM-DD";
					break;
				case "Y-M":
					sqlformat = "YYYY-MM";
					break;
				case "M-D-Y":
					sqlformat = "MM-DD-YYYY";
					break;
				case "D-M-Y":
					sqlformat = "DD-MM-YYYY";
					break;
				case "Y/M/D":
					sqlformat = "YYYY/MM/DD";
					break;
				case "Y/M":
					sqlformat = "YYYY/MM";
					break;
				case "M/D/Y":
					sqlformat = "MM/DD/YYYY";
					break;
				case "D/M/Y":
					sqlformat = "DD/MM/YYYY";
					break;
				default:
					throw new LightDataException (string.Format (RE.UnsupportDateFormat, format));
				}
				return string.Format ("to_char({0},'{1}')", field, sqlformat);
			}
		}

		public override string CreateYearSql (string field)
		{
//			return string.Format ("date_part('year',{0})", field);
			return string.Format ("extract(year from {0})::int4", field);
		}

		public override string CreateMonthSql (string field)
		{
//			return string.Format ("date_part('month',{0})", field);
			return string.Format ("extract(month from {0})::int4", field);
		}

		public override string CreateDaySql (string field)
		{
//			return string.Format ("date_part('day',{0})", field);
			return string.Format ("extract(day from {0})::int4", field);
		}

		public override string CreateHourSql (string field)
		{
//			return string.Format ("date_part('hour',{0})", field);
			return string.Format ("extract(hour from {0})::int4", field);
		}

		public override string CreateMinuteSql (string field)
		{
//			return string.Format ("date_part('minute',{0})", field);
			return string.Format ("extract(minute from {0})::int4", field);
		}

		public override string CreateSecondSql (string field)
		{
//			return string.Format ("date_part('second',{0})", field);
			return string.Format ("extract(second from {0})::int4", field);
		}

		public override string CreateWeekSql (string field)
		{
//			return string.Format ("date_part('week',{0})", field);
			return string.Format ("extract(week from {0})::int4", field);
		}

		public override string CreateWeekDaySql (string field)
		{
//			return string.Format ("dayofweek({0})-1", field);
			return string.Format ("extract(dow from {0})::int4", field);
		}

		public override string CreateLengthSql (string field)
		{
			return string.Format ("length({0})", field);
		}

		public override string CreateSubStringSql (string field, int start, int size)
		{
			start++;
			if (size == 0) {
				return string.Format ("substring({0} from {1})", field, start);
			}
			else {
				return string.Format ("substring({0} from {1} for {2})", field, start, size);
			}
		}

		public override string CreateDataBaseTimeSql ()
		{
			return "current_time";
		}

		public override string CreateParamName (string name)
		{
			if (name == null)
				throw new ArgumentNullException ("name");
			if (!name.StartsWith (":")) {
				return ":" + name;
			}
			else {
				return name;
			}
		}

		public override string GetHavingString (AggregateHavingExpression having, out DataParameter[] parameters, List<AggregateFunctionInfo> functions)
		{
			string havingString = null;
			parameters = null;
			if (having != null) {
				havingString = string.Format ("having {0}", having.CreateSqlString (this, false, out parameters, new GetAliasHandler (delegate(object obj) {
					return null;
				})));
			}
			return havingString;
		}
	}
}

