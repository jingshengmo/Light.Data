﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Light.Data
{
	class AccessCommandFactory : CommandFactory
	{
		public override string CreateDataFieldSql (string fieldName)
		{
			return string.Format ("[{0}]", fieldName);
		}

		public void UseAccessWildcards ()
		{
			_wildcards = "*";
		}

		public override string GetHavingString (AggregateHavingExpression having, out DataParameter[] parameters, List<AggregateFunctionInfo> functions)
		{
			string havingString = null;
			parameters = null;
			if (having != null) {
				havingString = string.Format ("having {0}", having.CreateSqlString (this, false, out parameters, new GetAliasHandler (delegate {
					return null;
				})));
			}
			return havingString;
		}

		public override string GetOrderString (OrderExpression order, out DataParameter[] parameters, List<DataFieldInfo> fields, List<AggregateFunctionInfo> functions)
		{
			string orderString = null;
			parameters = null;
			if (order != null) {
				orderString = string.Format ("order by {0}", order.CreateSqlString (this, false, out parameters, new GetAliasHandler (delegate {
					return null;
				})));
			}
			return orderString;
		}

		public override string CreateConditionCountSql (string expressionSql, string fieldName, bool isDistinct)
		{
			return string.Format ("count({2}iif({0},{1},null))", expressionSql, !string.IsNullOrEmpty (fieldName) ? CreateDataFieldSql (fieldName) : "1", isDistinct ? "distinct " : "");
		}

		public override string CreateConditionSumSql (string expressionSql, string fieldName, bool isDistinct)
		{
			return string.Format ("sum({2}iif({0},{1},0))", expressionSql, CreateDataFieldSql (fieldName), isDistinct ? "distinct " : "");
		}

		public override string CreateConditionAvgSql (string expressionSql, string fieldName, bool isDistinct)
		{
			return string.Format ("avg({2}iif({0},cdbl({1}),0,)", expressionSql, CreateDataFieldSql (fieldName), isDistinct ? "distinct " : "");
		}

		/// <summary>
		/// 创建内容Exists查询命令
		/// </summary>
		/// <param name="mapping">数据表映射</param>
		/// <param name="query">查询表达式</param>
		/// <returns></returns>
		public override CommandData CreateExistsCommand (DataEntityMapping mapping, QueryExpression query)
		{
			return this.CreateSelectBaseCommand (mapping, "top 1", query, null, null);
		}

		public override string CreateDataTableSql (string tableName)
		{
			return string.Format ("[{0}]", tableName);
		}

		public override string CreateRandomOrderBySql (DataEntityMapping mapping, string aliasName, bool fullFieldName)
		{
			Random rnd = new Random (unchecked((int)DateTime.Now.Ticks));
			int intRandomNumber = rnd.Next () * -1;

			DataFieldMapping keyfield = null;
			string fieldNames = null;
			string tableName = aliasName ?? mapping.TableName;
			DataTableEntityMapping tableMapping = mapping as DataTableEntityMapping;
			if (tableMapping != null) {
				if (tableMapping.HasIdentity) {
					fieldNames = CreateRandomField (tableMapping.IdentityField, tableName, fullFieldName);
				}
				else if (tableMapping.HasPrimaryKey) {
					List<string> list = new List<string> ();
					foreach (DataFieldMapping item in tableMapping.PrimaryKeyFields) {
						string name = CreateRandomField (item, tableName, fullFieldName);
						list.Add (name);
					}
					fieldNames = string.Join ("*", list);
				}
			}
			if (keyfield == null) {
				DataFieldMapping stringField = null;
				DataFieldMapping numberField = null;
				foreach (DataFieldMapping field in mapping.DataEntityFields) {
					if (field.TypeCode == TypeCode.Boolean || field.TypeCode == TypeCode.DBNull || field.TypeCode == TypeCode.Empty || field.TypeCode == TypeCode.Object) {
						continue;
					}
					else if (field.TypeCode == TypeCode.String) {
						if (stringField != null) {
							stringField = field;
						}
					}
					else {
						numberField = field;
						break;
					}
				}
				if (numberField != null) {
					fieldNames = CreateRandomField (numberField, tableName, fullFieldName);
				}
				else if (stringField != null) {
					fieldNames = CreateRandomField (stringField, tableName, fullFieldName);
				}
			}
			if (fieldNames != null) {
				return string.Format ("rnd({0}*{1})", intRandomNumber, fieldNames);
			}
			else {
				throw new LightDataException (RE.DataFieldIsNotStringType);
			}
		}

		private string CreateRandomField (FieldMapping keyfield, string tableName, bool fullFieldName)
		{
			string field;
			if (fullFieldName) {
				field = CreateFullDataFieldSql (tableName, keyfield.Name);
			}
			else {
				field = CreateDataFieldSql (keyfield.Name);
			}
			if (keyfield.TypeCode == TypeCode.String) {
				field = string.Format ("len({0})", field);
			}
			return field;
		}

		public override string CreateMatchSql (string field, bool left, bool right)
		{
			StringBuilder sb = new StringBuilder ();
			if (left) {
				sb.AppendFormat ("'{0}'+", _wildcards);
			}
			sb.Append (field);
			if (right) {
				sb.AppendFormat ("+'{0}'", _wildcards);
			}
			return sb.ToString ();
		}

		public override string CreateDateSql (string field, string format)
		{
			if (string.IsNullOrEmpty (format)) {
				return string.Format ("cdate(format({0}),'yyyy-mm-dd')", field);
			}
			else {
				string format1 = format.ToUpper ();
				string sqlformat;
				switch (format1) {
				case "YMD":
					sqlformat = "yyyymmdd";
					break;
				case "YM":
					sqlformat = "yyyymm";
					break;
				case "Y-M-D":
					sqlformat = "yyyy-mm-dd";
					break;
				case "Y-M":
					sqlformat = "yyyy-mm";
					break;
				case "M-D-Y":
					sqlformat = "mm-dd-yyyy";
					break;
				case "D-M-Y":
					sqlformat = "dd-mm-yyyy";
					break;
				case "Y/M/D":
					sqlformat = "yyyy/mm/dd";
					break;
				case "Y/M":
					sqlformat = "yyyy/mm";
					break;
				case "M/D/Y":
					sqlformat = "mm/dd/yyyy";
					break;
				case "D/M/Y":
					sqlformat = "dd/mm/yyyy";
					break;
				default:
					throw new LightDataException (string.Format (RE.UnsupportDateFormat, format));
				}
				return string.Format ("format({0},'{1}')", field, sqlformat);
			}
		}

		public override string CreateYearSql (string field)
		{
			return string.Format ("year({0})", field);
		}

		public override string CreateMonthSql (string field)
		{
			return string.Format ("month({0})", field);
		}

		public override string CreateDaySql (string field)
		{
			return string.Format ("day({0})", field);
		}

		public override string CreateHourSql (string field)
		{
			return string.Format ("hour({0})", field);
		}

		public override string CreateMinuteSql (string field)
		{
			return string.Format ("minute({0})", field);
		}

		public override string CreateSecondSql (string field)
		{
			return string.Format ("second({0})", field);
		}

		public override string CreateWeekSql (string field)
		{
			return string.Format ("val(format({0},'ww'))", field);
		}

		public override string CreateWeekDaySql (string field)
		{
			return string.Format ("weekday({0})", field);
		}

		public override string CreateLengthSql (string field)
		{
			return string.Format ("len({0})", field);
		}

		public override string CreateDataBaseTimeSql ()
		{
			return "now()";
		}

		public override string CreateSubStringSql (string field, int start, int size)
		{
			start++;
			if (size == 0) {
				return string.Format ("mid({0},{1})", field, start);
			}
			else {
				return string.Format ("mid({0},{1},{2})", field, start, size);
			}
		}

		public override string CreateModSql (string field, object value, bool forward)
		{
			if (forward) {
				return string.Format ("({0} mod {1})", field, value);
			}
			else {
				return string.Format ("({0} mod {1})", value, field);
			}
		}

		public override string CreateAtanSql (string field)
		{
			return string.Format ("atn({0})", field);
		}
	}
}
