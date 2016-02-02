﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Text.RegularExpressions;

namespace Light.Data
{
	internal delegate string GetAliasHandler (object obj);

	abstract class CommandFactory
	{
		protected virtual string CreateCustomFiledName ()
		{
			return string.Format ("field_{0}", Guid.NewGuid ().ToString ("N").Substring (0, 8));
		}

		public virtual string CreateTempParamName ()
		{
			return string.Format ("_param_{0}_", Guid.NewGuid ().ToString ("N"));
		}

		protected string _wildcards = "%";

		Dictionary<QueryPredicate, string> _queryPredicateDict = new Dictionary<QueryPredicate, string> ();

		Dictionary<QueryCollectionPredicate, string> _queryCollectionPredicateDict = new Dictionary<QueryCollectionPredicate, string> ();

		Dictionary<JoinType, string> _joinCollectionPredicateDict = new Dictionary<JoinType, string> ();

		protected void InitialPredicate ()
		{
			_queryPredicateDict [QueryPredicate.Eq] = "=";
			_queryPredicateDict [QueryPredicate.Gt] = ">";
			_queryPredicateDict [QueryPredicate.GtEq] = ">=";
			_queryPredicateDict [QueryPredicate.Lt] = "<";
			_queryPredicateDict [QueryPredicate.LtEq] = "<=";
			_queryPredicateDict [QueryPredicate.NotEq] = "<>";

			_queryCollectionPredicateDict [QueryCollectionPredicate.In] = "in";
			_queryCollectionPredicateDict [QueryCollectionPredicate.NotIn] = "not in";
			_queryCollectionPredicateDict [QueryCollectionPredicate.GtAll] = "> all";
			_queryCollectionPredicateDict [QueryCollectionPredicate.LtAll] = "< all";
			_queryCollectionPredicateDict [QueryCollectionPredicate.GtAny] = "> any";
			_queryCollectionPredicateDict [QueryCollectionPredicate.LtAny] = "< any";

			_joinCollectionPredicateDict [JoinType.InnerJoin] = "inner join";
			_joinCollectionPredicateDict [JoinType.LeftJoin] = "left join";
			_joinCollectionPredicateDict [JoinType.RightJoin] = "right join";
		}

		protected string GetQueryPredicate (QueryPredicate predicate)
		{
			if (_queryPredicateDict.ContainsKey (predicate)) {
				return _queryPredicateDict [predicate];
			}
			else {
				throw new LightDataException (string.Format (RE.UnSupportPredicate, predicate));
			}
		}

		protected string GetQueryCollectionPredicate (QueryCollectionPredicate predicate)
		{
			if (_queryCollectionPredicateDict.ContainsKey (predicate)) {
				return _queryCollectionPredicateDict [predicate];
			}
			else {
				throw new LightDataException (string.Format (RE.UnSupportPredicate, predicate));
			}
		}

		protected bool _canInnerPage = false;

		/// <summary>
		/// 是否支持内分页
		/// </summary>
		public bool CanInnerPager {
			get {
				return _canInnerPage;
			}
		}

		protected CommandFactory ()
		{
			InitialPredicate ();
		}

		#region 增删改操作命令

		/// <summary>
		/// 从字段映射中获取DataParameter集合
		/// </summary>
		/// <param name="mappings">字段映射列表</param>
		/// <param name="source">数据对象</param>
		/// <returns>DataParameter集合</returns>
		protected virtual List<DataParameter> GetDataParameters (IEnumerable<FieldMapping> mappings, object source)
		{
			List<DataParameter> paramList = new List<DataParameter> ();
			foreach (DataFieldMapping field in mappings) {
				if (field is PrimitiveFieldMapping) {
					PrimitiveFieldMapping primitiveFieldMapping = field as PrimitiveFieldMapping;

					object obj = primitiveFieldMapping.Handler.Get (source);
					if (primitiveFieldMapping.SpecifiedHandler != null) {
						bool hasdata = Convert.ToBoolean (primitiveFieldMapping.SpecifiedHandler.Get (source));
						if (!hasdata) {
							obj = null;
						}
					}
					if (obj == null && !primitiveFieldMapping.IsNullable) {
						throw new LightDataException (string.Format (RE.DataValueIsNotAllowEmply, primitiveFieldMapping.Name));
					}
					DataParameter dataParameter = new DataParameter (field.Name, field.ToColumn (obj), field.DBType, ParameterDirection.Input);
					paramList.Add (dataParameter);
				}
				else if (field is EnumFieldMapping) {
					EnumFieldMapping enumFieldMapping = field as EnumFieldMapping;
					object obj = enumFieldMapping.Handler.Get (source);
					if (enumFieldMapping.SpecifiedHandler != null) {
						bool hasdata = Convert.ToBoolean (enumFieldMapping.SpecifiedHandler.Get (source));
						if (!hasdata) {
							obj = null;
						}
					}
					if (obj == null && !enumFieldMapping.IsNullable) {
						throw new LightDataException (string.Format (RE.DataValueIsNotAllowEmply, enumFieldMapping.Name));
					}
					DataParameter dataParameter = new DataParameter (field.Name, field.ToColumn (obj), field.DBType, ParameterDirection.Input);
					paramList.Add (dataParameter);
				}
				else if (field is ComplexFieldMapping) {
					ComplexFieldMapping complexFieldMapping = field as ComplexFieldMapping;
					object obj = complexFieldMapping.Handler.Get (source);
					if (obj == null) {
						throw new LightDataException (string.Format (RE.DataValueIsNotAllowEmply, complexFieldMapping.Name));
					}
					List<DataParameter> subParamList = GetDataParameters (complexFieldMapping.GetFieldMappings (), obj);
					paramList.AddRange (subParamList);
				}
			}
			return paramList;
		}

		/// <summary>
		/// 生成数据新增命令
		/// </summary>
		/// <param name="entity">数据实体</param>
		/// <returns>新增命令对象</returns>
		public virtual CommandData CreateInsertCommand (object entity)
		{
			DataTableEntityMapping mapping = DataMapping.GetTableMapping (entity.GetType ());
			List<FieldMapping> fields = new List<FieldMapping> ();
			fields.AddRange (mapping.GetFieldMappings ());
			if (mapping.IdentityField != null) {
				fields.Remove (mapping.IdentityField);
			}
			List<DataParameter> paramList = GetDataParameters (fields, entity);
			string[] insertList = new string[paramList.Count];
			string[] valuesList = new string[paramList.Count];
			int index = 0;
			foreach (DataParameter dataParameter in paramList) {
				insertList [index] = CreateDataFieldSql (dataParameter.ParameterName);
				string paramName = CreateParamName ("P" + index);
				valuesList [index] = paramName;
				dataParameter.ParameterName = paramName;
				index++;
			}
			string insert = string.Join (",", insertList);
			string values = string.Join (",", valuesList);
			string sql = string.Format ("insert into {0}({1})values({2})", CreateDataTableSql (mapping.TableName), insert, values);
			CommandData command = new CommandData (sql, paramList);
			return command;
		}

		/// <summary>
		/// 生成数据更新命令
		/// </summary>
		/// <param name="entity">数据实体</param>
		/// <param name="updatefieldNames">需更新的数据字段</param>
		/// <returns>更新命令对象</returns>
		public virtual CommandData CreateUpdateCommand (object entity, string[] updatefieldNames)
		{
			DataTableEntityMapping mapping = DataMapping.GetTableMapping (entity.GetType ());
			if (mapping.PrimaryKeyFields == null || mapping.PrimaryKeyFields.Length == 0) {
				throw new LightDataException (RE.PrimaryKeyIsNotExist);
			}

			List<FieldMapping> fields = null;

			if (updatefieldNames != null && updatefieldNames.Length > 0) {
				List<FieldMapping> updateFields = new List<FieldMapping> ();
				foreach (string name in updatefieldNames) {
					FieldMapping fm = mapping.FindFieldMapping (name);

					if (fm != null && !updateFields.Contains (fm)) {
						updateFields.Add (fm);
					}
				}
				fields = updateFields;
			}

			if (fields == null) {
				fields = new List<FieldMapping> ();
				fields.AddRange (mapping.GetFieldMappings ());
			}

			if (mapping.IdentityField != null) {
				fields.Remove (mapping.IdentityField);
			}
			foreach (DataFieldMapping primaryField in mapping.PrimaryKeyFields) {
				fields.Remove (primaryField);
			}

			List<DataParameter> columnList = GetDataParameters (fields, entity);
			List<DataParameter> primaryList = GetDataParameters (mapping.PrimaryKeyFields, entity);
			if (columnList.Count == 0) {
				throw new LightDataException (RE.UpdateFieldIsNotExists);
			}

			string[] updateList = new string[columnList.Count];
			string[] whereList = new string[primaryList.Count];
			int paramIndex = 0;
			int index = 0;
			foreach (DataParameter dataParameter in columnList) {
				string paramName = CreateParamName ("P" + paramIndex);
				updateList [index] = string.Format ("{0}={1}", CreateDataFieldSql (dataParameter.ParameterName), paramName);
				dataParameter.ParameterName = paramName;
				paramIndex++;
				index++;
			}
			index = 0;
			foreach (DataParameter dataParameter in primaryList) {
				string paramName = CreateParamName ("P" + paramIndex);
				whereList [index] = string.Format ("{0}={1}", CreateDataFieldSql (dataParameter.ParameterName), paramName);
				dataParameter.ParameterName = paramName;
				paramIndex++;
				index++;
			}
			string update = string.Join (",", updateList);
			string where = string.Join (" and ", whereList);
			string sql = string.Format ("update {0} set {1} where {2}", CreateDataTableSql (mapping.TableName), update, where);
			CommandData command = new CommandData (sql, columnList);
			command.AddParameters (primaryList);
			return command;
		}

		/// <summary>
		/// 生成数据删除命令
		/// </summary>
		/// <param name="entity">数据实体</param>
		/// <returns>删除命令对象</returns>
		public virtual CommandData CreateDeleteCommand (object entity)
		{
			DataTableEntityMapping mapping = DataMapping.GetTableMapping (entity.GetType ());
			if (mapping.PrimaryKeyFields == null || mapping.PrimaryKeyFields.Length == 0) {
				throw new LightDataException (RE.PrimaryKeyIsNotExist);
			}
			List<DataParameter> primaryList = GetDataParameters (mapping.PrimaryKeyFields, entity);
			string[] whereList = new string[primaryList.Count];
			int index = 0;
			foreach (DataParameter dataParameter in primaryList) {
				string paramName = CreateParamName ("P" + index);
				whereList [index] = string.Format ("{0}={1}", CreateDataFieldSql (dataParameter.ParameterName), paramName);
				dataParameter.ParameterName = paramName;
				index++;
			}
			string where = string.Join (" and ", whereList);
			string sql = string.Format ("delete from {0} where {1}", CreateDataTableSql (mapping.TableName), where);
			CommandData command = new CommandData (sql, primaryList);
			return command;
		}

		public virtual CommandData CreateEntityExistsCommand (object entity)
		{
			DataTableEntityMapping mapping = DataMapping.GetTableMapping (entity.GetType ());
			if (mapping.PrimaryKeyFields == null || mapping.PrimaryKeyFields.Length == 0) {
				throw new LightDataException (RE.PrimaryKeyIsNotExist);
			}
			List<DataParameter> primaryList = GetDataParameters (mapping.PrimaryKeyFields, entity);
			string[] whereList = new string[primaryList.Count];
			int index = 0;
			foreach (DataParameter dataParameter in primaryList) {
				string paramName = CreateParamName ("P" + index);
				whereList [index] = string.Format ("{0}={1}", CreateDataFieldSql (dataParameter.ParameterName), paramName);
				dataParameter.ParameterName = paramName;
				index++;
			}
			string where = string.Join (" and ", whereList);
			string sql = string.Format ("select 1 from {0} where {1}", CreateDataTableSql (mapping.TableName), where);
			CommandData command = new CommandData (sql, primaryList);
			return command;
		}

		public virtual CommandData CreateTruncatCommand (DataTableEntityMapping mapping)
		{
			string sql = string.Format ("truncate table {0}", CreateDataTableSql (mapping));
			CommandData command = new CommandData (sql);
			return command;
		}

		#endregion

		#region 主命令语句块

		//		public virtual string GetSelectString (DataEntityMapping mapping, out int count)
		//		{
		//			string[] names = mapping.GetFieldNames ();
		//			string[] values = new string[names.Length];
		//			for (int i = 0; i < names.Length; i++) {
		//				values [i] = CreateDataFieldSql (names [i]);
		//			}
		//			count = names.Length;
		//			return string.Join (",", values);
		//		}

		public virtual string GetSelectString (DataEntityMapping mapping)
		{
//			int count;
//			return GetSelectString (mapping, out count);
			string[] names = mapping.GetFieldNames ();
			string[] values = new string[names.Length];
			for (int i = 0; i < names.Length; i++) {
				values [i] = CreateDataFieldSql (names [i]);
			}
			return string.Join (",", values);
		}

		public virtual string GetQueryString (QueryExpression query, out DataParameter[] parameters, bool fullFieldName = false)
		{
			string queryString = null;
			parameters = null;
			if (query != null) {
				queryString = string.Format ("where {0}", query.CreateSqlString (this, fullFieldName, out parameters));
			}
			return queryString;
		}

		public virtual string GetHavingString (AggregateHavingExpression having, out DataParameter[] parameters, List<AggregateFunctionInfo> functions)
		{
			string havingString = null;
			parameters = null;
			if (having != null) {
				havingString = string.Format ("having {0}", having.CreateSqlString (this, false, out parameters, new GetAliasHandler (delegate(object obj) {
					string alias = null;
					if (obj is AggregateFunction) {
						foreach (AggregateFunctionInfo info in functions) {
							if (Object.ReferenceEquals (obj, info.Function)) {
								alias = info.Name;
							}
						}
					}
					else {
						throw new LightDataException (RE.UnknowHavingType);
					}
					return alias;
				})));
			}
			return havingString;
		}

		public virtual string GetOrderString (OrderExpression order, bool fullFieldName = false)
		{
			string orderString = null;
			DataParameter[] parameters = null;
			if (order != null) {
				orderString = string.Format ("order by {0}", order.CreateSqlString (this, fullFieldName, out parameters));
			}
			return orderString;
		}

		public virtual string GetOrderString (OrderExpression order, out DataParameter[] parameters, List<DataFieldInfo> fields, List<AggregateFunctionInfo> functions)
		{
			string orderString = null;
			parameters = null;
			if (order != null) {
				orderString = string.Format ("order by {0}", order.CreateSqlString (this, false, out parameters, new GetAliasHandler (delegate(object obj) {
					string alias = null;
					if (obj is DataFieldInfo) {
						foreach (DataFieldInfo info in fields) {
							if (Object.ReferenceEquals (obj, info)) {
								AliasDataFieldInfo aliasInfo = info as AliasDataFieldInfo;
								if (!Object.Equals (aliasInfo, null)) {
									alias = aliasInfo.Alias;
								}
								else {
									alias = info.FieldName;
								}
								break;
							}
						}
					}
					else if (obj is AggregateFunction) {
						foreach (AggregateFunctionInfo info in functions) {
							if (Object.ReferenceEquals (obj, info.Function)) {
								alias = info.Name;
								break;
							}
						}
					}
					else {
						throw new LightDataException (RE.UnknowOrderType);
					}
					return alias;
				})));
			}
			return orderString;
		}

		public virtual string GetOnString (DataFieldExpression on, bool fullFieldName = true)
		{
			string onString = null;
			DataParameter[] parameters = null;
			if (on != null) {
				onString = string.Format ("on {0}", on.CreateSqlString (this, fullFieldName, out parameters));
			}
			return onString;
		}

		/// <summary>
		/// 创建查询命令
		/// </summary>
		/// <param name="mapping">数据表映射</param>
		/// <param name="query">查询表达式</param>
		/// <param name="order">排序表达式</param>
		/// <param name="region">查询范围,如非空则生成内分页语句</param>
		/// <returns>查询命令对象</returns>
		public virtual CommandData CreateSelectCommand (DataEntityMapping mapping, QueryExpression query, OrderExpression order, Region region)
		{
			if (region != null && !_canInnerPage) {
				throw new LightDataException (RE.DataBaseNotSupportInnerPage);
			}
			string select = GetSelectString (mapping);
			return this.CreateSelectBaseCommand (mapping, select, query, order, region);
		}

		public virtual CommandData CreateSelectSingleFieldCommand (DataFieldInfo fieldinfo, QueryExpression query, OrderExpression order, bool distinct, Region region)
		{
			if (region != null && !_canInnerPage) {
				throw new LightDataException (RE.DataBaseNotSupportInnerPage);
			}
			DataFieldMapping fieldMapping = fieldinfo.DataField;
			if (fieldMapping is PrimitiveFieldMapping || fieldMapping is EnumFieldMapping || fieldMapping is CustomFieldMapping) {
				DataEntityMapping mapping = fieldMapping.EntityMapping;
				string select = fieldinfo.CreateDataFieldSql (this);
				if (distinct) {
					select = "distinct " + select;
				}
				return CreateSelectBaseCommand (mapping, select, query, order, region);
			}
			else {
				throw new LightDataException (RE.OnlyPrimitiveFieldCanSelectSingle);
			}
		}

		/// <summary>
		/// 创建自定查询内容的命令
		/// </summary>
		/// <param name="mapping">数据表映射</param>
		/// <param name="customSelect">查询输出的内容</param>
		/// <param name="query">查询表达式</param>
		/// <param name="order">排序表达式</param>
		/// <param name="region">查询范围</param>
		/// <returns></returns>
		protected virtual CommandData CreateSelectBaseCommand (DataEntityMapping mapping, string customSelect, QueryExpression query, OrderExpression order, Region region)//, bool distinct)
		{
			StringBuilder sql = new StringBuilder ();
			DataParameter[] parameters;
			string queryString = GetQueryString (query, out parameters);
			string orderString = GetOrderString (order);
			sql.AppendFormat ("select {0} from {1}", customSelect, CreateDataTableSql (mapping.TableName));//, distinct ? "distinct " : string.Empty);
			if (!string.IsNullOrEmpty (queryString)) {
				sql.AppendFormat (" {0}", queryString);
			}
			if (!string.IsNullOrEmpty (orderString)) {
				sql.AppendFormat (" {0}", orderString);
			}
			CommandData command = new CommandData (sql.ToString (), parameters);
			command.TransParamName = true;
			return command;
		}

		public virtual CommandData CreateSelectJoinTableCommand (JoinSelector selector, List<JoinModel> modelList, QueryExpression query, OrderExpression order)
		{
			List<DataFieldInfo> fields = selector.GetFieldInfos ();
			string[] selectList = new string[fields.Count];
			int index = 0;
			foreach (DataFieldInfo fieldInfo in fields) {
				AliasDataFieldInfo aliasInfo = fieldInfo as AliasDataFieldInfo;
				if (!Object.Equals (aliasInfo, null)) {
					selectList [index] = aliasInfo.CreateAliasDataFieldSql (this, false);
				}
				else {
					selectList [index] = fieldInfo.CreateDataFieldSql (this, true);
				}
				index++;
			}
			string customSelect = string.Join (",", selectList);
			return CreateSelectJoinTableCommand (customSelect, modelList, query, order);
		}

		/// <summary>
		/// Creates the select join table command.
		/// </summary>
		/// <returns>The select join table command.</returns>
		/// <param name="customSelect">Custom select.</param>
		/// <param name="modelList">Model list.</param>
		/// <param name="query">Query.</param>
		/// <param name="order">Order.</param>
		public virtual CommandData CreateSelectJoinTableCommand (string customSelect, List<JoinModel> modelList, QueryExpression query, OrderExpression order)
		{
			DataEntityMapping mapping = modelList [0].Mapping;
			List<DataParameter> listParameters = new List<DataParameter> ();
//			List<string> selectList = new List<string> ();
			StringBuilder tables = new StringBuilder ();
			foreach (JoinModel model in modelList) {
//				if (model.SelectAllField) {
//					foreach (string item in model.Mapping.GetFieldNames()) {
//						string fieldName = CreateFullDataFieldSql (model.Mapping.TableName, item);
//						selectList.Add (fieldName);
//					}
//				}
//				else {
//					DataFieldInfo[] infos = model.GetFields ();
//					if (infos != null && infos.Length > 0) {
//						foreach (DataFieldInfo info in infos) {
//							string fieldName = info.CreateDataFieldSql (this, true);
//							selectList.Add (fieldName);
//						}
//					}
//				}
				if (model.Connect != null) {
					tables.AppendFormat (" {0} ", _joinCollectionPredicateDict [model.Connect.Type]);
				}
				if (model.Query != null || model.Order != null) {
					DataParameter[] mparameters;
					string mqueryString = GetQueryString (model.Query, out mparameters);
					string morderString = GetOrderString (model.Order);
					tables.AppendFormat ("(select * from {0}", CreateDataTableSql (model.Mapping.TableName));
					if (!string.IsNullOrEmpty (mqueryString)) {
						tables.AppendFormat (" {0}", mqueryString);
					}
					if (!string.IsNullOrEmpty (morderString)) {
						tables.AppendFormat (" {0}", morderString);
					}
					tables.AppendFormat (") as {0}", CreateDataTableSql (model.Mapping.TableName));
					if (mparameters != null) {
						listParameters.AddRange (mparameters);
					}
				}
				else {
					tables.Append (CreateDataTableSql (model.Mapping.TableName));
				}
				if (model.Connect != null && model.Connect.On != null) {
					string onString = GetOnString (model.Connect.On);
					if (!string.IsNullOrEmpty (onString)) {
						tables.AppendFormat (" {0}", onString);
					}
				}
			}
			StringBuilder sql = new StringBuilder ();
			DataParameter[] parameters;
			string queryString = GetQueryString (query, out parameters, true);
			string orderString = GetOrderString (order, true);
			if (parameters != null) {
				listParameters.AddRange (parameters);
			}
//			if (string.IsNullOrEmpty (customSelect)) {
//				customSelect = string.Join (",", selectList);
//			}
			sql.AppendFormat ("select {0} from {1}", customSelect, tables);
			if (!string.IsNullOrEmpty (queryString)) {
				sql.AppendFormat (" {0}", queryString);
			}
			if (!string.IsNullOrEmpty (orderString)) {
				sql.AppendFormat (" {0}", orderString);
			}
			CommandData command = new CommandData (sql.ToString (), listParameters);
			command.TransParamName = true;
			return command;
		}

		/// <summary>
		/// 创建内容Exists查询命令
		/// </summary>
		/// <param name="mapping">数据表映射</param>
		/// <param name="query">查询表达式</param>
		/// <returns></returns>
		public virtual CommandData CreateExistsCommand (DataEntityMapping mapping, QueryExpression query)
		{
			Region region = null;
			if (_canInnerPage) {
				region = new Region (0, 1);
			}
			return this.CreateSelectBaseCommand (mapping, "1", query, null, region);
		}

		public virtual CommandData CreateAggregateCommand (DataFieldMapping fieldMapping, AggregateType aggregateType, QueryExpression query, bool distinct)
		{
			DataEntityMapping mapping = fieldMapping.EntityMapping;
			if (aggregateType != AggregateType.COUNT) {
				TypeCode code = Type.GetTypeCode (fieldMapping.ObjectType);
				if (aggregateType == AggregateType.MAX || aggregateType == AggregateType.MIN) {
					if (code == TypeCode.Char || code == TypeCode.DBNull || code == TypeCode.Object || code == TypeCode.String) {
						throw new LightDataException (RE.TheTypeOfAggregationFieldIsNotRight);
					}
				}
				else {
					if (code == TypeCode.Char || code == TypeCode.DBNull || code == TypeCode.Object || code == TypeCode.String || code == TypeCode.DateTime) {
						throw new LightDataException (RE.TheTypeOfAggregationFieldIsNotRight);
					}
				}
			}

			string function = null;
			switch (aggregateType) {
			case AggregateType.COUNT:
				function = CreateCountSql (CreateDataFieldSql (fieldMapping.Name), distinct);
				break;
			case AggregateType.SUM:
				function = CreateSumSql (CreateDataFieldSql (fieldMapping.Name), distinct);
				break;
			case AggregateType.AVG:
				function = CreateAvgSql (CreateDataFieldSql (fieldMapping.Name), distinct);
				break;
			case AggregateType.MAX:
				function = CreateMaxSql (CreateDataFieldSql (fieldMapping.Name));
				break;
			case AggregateType.MIN:
				function = CreateMinSql (CreateDataFieldSql (fieldMapping.Name));
				break;
			}
			return CreateSelectBaseCommand (mapping, function, query, null, null);//, false);
		}

		public virtual CommandData CreateAggregateCountCommand (DataEntityMapping mapping, QueryExpression query)
		{
			string select = CreateCountAllSql ();
			return CreateSelectBaseCommand (mapping, select, query, null, null);//, false);
		}

		public virtual CommandData CreateAggregateJoinCountCommand (List<JoinModel> modelList, QueryExpression query)
		{
			string select = CreateCountAllSql ();
			return CreateSelectJoinTableCommand (select, modelList, query, null);
		}

		public virtual CommandData CreateDeleteMassCommand (DataTableEntityMapping mapping, QueryExpression query)
		{
			StringBuilder sql = new StringBuilder ();
			DataParameter[] parameters;
			string queryString = GetQueryString (query, out parameters);

			sql.AppendFormat ("delete from {0}", CreateDataTableSql (mapping.TableName));
			if (!string.IsNullOrEmpty (queryString)) {
				sql.AppendFormat (" {0}", queryString);
			}
			CommandData command = new CommandData (sql.ToString (), parameters);
			command.TransParamName = true;
			return command;
		}

		public virtual CommandData CreateSelectInsertCommand (DataTableEntityMapping insertMapping, DataFieldInfo[] insertFields, DataTableEntityMapping selectMapping, SelectFieldInfo[] selectFields, QueryExpression query, OrderExpression order)
		{
			StringBuilder sql = new StringBuilder ();
			string insert = null;
			string select = null;
			int insertCount;
			int selectCount = 0;
			if (insertFields != null && insertFields.Length > 0) {
				insertCount = insertFields.Length;
				string[] insertFieldNames = new string[insertFields.Length];
				for (int i = 0; i < insertFields.Length; i++) {
					if (!insertMapping.Equals (insertFields [i].TableMapping)) {
						throw new LightDataException (RE.FieldIsNotMatchDataMapping);
					}
					if (insertFields [i] is CommonDataFieldInfo) {
						throw new LightDataException (RE.InsertFieldIsNotDataFieldInfo);
					}
					if (insertFields [i] is ExtendDataFieldInfo) {
						throw new LightDataException (RE.InsertFieldIsNotDataFieldInfo);
					}
					insertFieldNames [i] = insertFields [i].CreateDataFieldSql (this);
					insert = string.Join (",", insertFieldNames);
				}
			}
			else {
				insertCount = insertMapping.FieldCount;
				insert = GetSelectString (insertMapping);
			}
			List<DataParameter> totalParameters = new List<DataParameter> ();
			if (selectFields != null && selectFields.Length > 0) {
				selectCount = selectFields.Length;
				string[] selectFieldNames = new string[selectFields.Length];
				
				for (int i = 0; i < selectFields.Length; i++) {
//					if (!(selectFields [i] is CommonDataFieldInfo) && !selectMapping.Equals (selectFields [i].TableMapping)) {
//						throw new LightDataException (RE.FieldIsNotMatchDataMapping);
//					}
					DataParameter dp;
					selectFieldNames [i] = selectFields [i].CreateDataFieldSql (this,out dp);
					select = string.Join (",", selectFieldNames);
					if (dp != null) {
						totalParameters.Add (dp);
					}
				}
			}
			else {
				selectCount = selectMapping.FieldCount;
				select = GetSelectString (selectMapping);
			}

			if (insertCount != selectCount) {
				throw new LightDataException (RE.SelectFiledsCountNotEquidInsertFiledCount);
			}

			DataParameter[] parameters;
			string queryString = GetQueryString (query, out parameters);
			string orderString = GetOrderString (order);
			sql.AppendFormat ("insert into {0}({1})", CreateDataTableSql (insertMapping.TableName), insert);
			sql.AppendFormat ("select {0} from {1}", select, CreateDataTableSql (selectMapping.TableName));
			if (!string.IsNullOrEmpty (queryString)) {
				sql.AppendFormat (" {0}", queryString);
			}
			if (!string.IsNullOrEmpty (orderString)) {
				sql.AppendFormat (" {0}", orderString);
			}
			if (parameters != null && parameters.Length > 0) {
				totalParameters.AddRange (parameters);
			}
			CommandData command = new CommandData (sql.ToString (), totalParameters);
			command.TransParamName = true;
			return command;
		}

		public virtual CommandData CreateUpdateMassCommand (DataTableEntityMapping mapping, UpdateSetValue[] updateSetValues, QueryExpression query)
		{
			StringBuilder sql = new StringBuilder ();
			DataParameter[] parameters;
			string queryString = GetQueryString (query, out parameters);

			int length = updateSetValues.Length;
			DataParameter[] setparameters = new DataParameter[length];
			string[] setList = new string[length];

			for (int i = 0; i < length; i++) {
				if (!mapping.Equals (updateSetValues [i].DataField.DataField.EntityMapping)) {
					throw new LightDataException (RE.UpdateFieldTypeIsError);
				}
				setparameters [i] = updateSetValues [i].CreateDataParameter (this);
				setList [i] = string.Format ("{0}={1}", updateSetValues [i].DataField.CreateDataFieldSql (this), setparameters [i].ParameterName);
			}
			string setString = string.Join (",", setList);
			sql.AppendFormat ("update {0} set {1}", CreateDataTableSql (mapping.TableName), setString);
			if (!string.IsNullOrEmpty (queryString)) {
				sql.AppendFormat (" {0}", queryString);
			}

			CommandData command = new CommandData (sql.ToString (), parameters);
			command.AddParameters (setparameters);
			command.TransParamName = true;
			return command;
		}

		public virtual CommandData CreateDynamicAggregateCommand (DataEntityMapping mapping, List<DataFieldInfo> fields, List<AggregateFunctionInfo> functions, QueryExpression query, AggregateHavingExpression having, OrderExpression order)
		{
			if (fields == null || fields.Count == 0) {
				throw new LightDataException (RE.DynamicAggregateFieldIsNotExists);
			}
			StringBuilder sql = new StringBuilder ();

			List<DataParameter> parameterlist = new List<DataParameter> ();
			string[] selectList = new string[fields.Count + functions.Count];
			string[] groupbyList = new string[fields.Count];
			int index = 0;

			foreach (DataFieldInfo fieldInfo in fields) {
				if (!mapping.Equals (fieldInfo.TableMapping)) {
					throw new LightDataException (RE.DataMappingIsNotMatchAggregateField);
				}
				string groupbyField = fieldInfo.CreateDataFieldSql (this);
				groupbyList [index] = groupbyField;
				AliasDataFieldInfo aliasInfo = fieldInfo as AliasDataFieldInfo;
				if (!Object.Equals (aliasInfo, null)) {
					selectList [index] = aliasInfo.CreateAliasDataFieldSql (this, false);
				}
				else {
					selectList [index] = groupbyField;
				}
				index++;
			}

			foreach (AggregateFunctionInfo functionInfo in functions) {
				AggregateFunction function = functionInfo.Function;
				if (function.TableMapping != null && !mapping.Equals (function.TableMapping)) {
					throw new LightDataException (RE.DataMappingIsNotMatchAggregateField);
				}
				DataParameter[] aggparameters;
				string aggField = function.CreateSqlString (this, false, out aggparameters);
				string selectField = CreateAliasSql (aggField, functionInfo.Name);
				selectList [index] = selectField;
				parameterlist.AddRange (aggparameters);
				index++;
			}
			string select = string.Join (",", selectList);
			string groupby = string.Join (",", groupbyList);
			sql.AppendFormat ("select {0} from {1}", select, CreateDataTableSql (mapping.TableName));

			DataParameter[] queryparameters = null;
			string queryString = GetQueryString (query, out queryparameters);
			DataParameter[] havingparameters = null;
			string havingString = GetHavingString (having, out havingparameters, functions);
			DataParameter[] orderbyparameters = null;
			string orderString = GetOrderString (order, out orderbyparameters, fields, functions);

			if (!string.IsNullOrEmpty (queryString)) {
				sql.AppendFormat (" {0}", queryString);
				parameterlist.AddRange (queryparameters);
			}

			sql.AppendFormat (" group by {0}", groupby);

			if (!string.IsNullOrEmpty (havingString)) {
				sql.AppendFormat (" {0}", havingString);
				parameterlist.AddRange (havingparameters);
			}

			if (!string.IsNullOrEmpty (orderString)) {
				sql.AppendFormat (" {0}", orderString);
				parameterlist.AddRange (orderbyparameters);
			}

			CommandData command = new CommandData (sql.ToString (), parameterlist);
			command.TransParamName = true;
			return command;
		}

		public virtual CommandData[] CreateBulkInsertCommand (Array entitys, int batchCount)
		{
			if (entitys == null || entitys.Length == 0) {
				throw new ArgumentNullException ("entitys");
			}
			if (batchCount <= 0) {
				batchCount = 10;
			}
			object tmpEntity = entitys.GetValue (0);
			DataTableEntityMapping mapping = DataMapping.GetTableMapping (tmpEntity.GetType ());
			List<FieldMapping> fields = new List<FieldMapping> ();
			int totalCount = entitys.Length;
			fields.AddRange (mapping.GetFieldMappings ());
			if (mapping.IdentityField != null) {
				fields.Remove (mapping.IdentityField);
			}
			List<DataParameter> paramList = GetDataParameters (fields, tmpEntity);
			List<string> insertList = new List<string> ();
			foreach (DataParameter dataParameter in paramList) {
				insertList.Add (CreateDataFieldSql (dataParameter.ParameterName));
			}
			string insertsql = string.Format ("insert into {0}({1})", CreateDataTableSql (mapping.TableName), string.Join (",", insertList));

			int createCount = 0;
			int totalCreateCount = 0;

			StringBuilder totalSql = new StringBuilder ();
			int paramIndex = 0;
			List<DataParameter> dataParams = new List<DataParameter> ();
			List<CommandData> commands = new List<CommandData> ();
			int i = 0;
			foreach (object entity in entitys) {
				List<DataParameter> entityParams = i == 0 ? paramList : GetDataParameters (fields, entity);
				string[] valueList = new string[paramList.Count];
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
				totalSql.AppendFormat ("{0}values({1});", insertsql, value);
				createCount++;
				totalCreateCount++;
				if (createCount == batchCount || totalCreateCount == totalCount) {
					CommandData command = new CommandData (totalSql.ToString (), dataParams);
					commands.Add (command);
					if (totalCreateCount == totalCount) {
						break;
					}
					dataParams = new List<DataParameter> ();
					createCount = 0;
					paramIndex = 0;
					totalSql = new StringBuilder ();
				}
				i++;
			}
			return commands.ToArray ();
		}

		public virtual CommandData CreateIdentityCommand (DataTableEntityMapping mapping)
		{
			string sql = CreateIdentitySql (mapping);
			if (!string.IsNullOrEmpty (sql)) {
				CommandData command = new CommandData (sql, null);
				command.TransParamName = true;
				return command;
			}
			else {
				return null;
			}
		}

		#endregion

		#region 基本语句块

		public virtual string CreateCatchExpressionSql (string expressionString1, string expressionString2, CatchOperatorsType operatorType)
		{
			return string.Format ("({0} {2} {1})", expressionString1, expressionString2, operatorType.ToString ().ToLower ());
		}

		public virtual string CreateCatchExpressionSql (string[] expressionStrings)
		{
			return string.Join (",", expressionStrings);
		}

		public virtual string CreateSingleParamSql (string fieldName, QueryPredicate predicate, bool isReverse, DataParameter dataParameter)
		{
			StringBuilder sb = new StringBuilder ();
			string op = GetQueryPredicate (predicate);
			if (!isReverse) {
				sb.AppendFormat ("{0}{2}{1}", fieldName, dataParameter.ParameterName, op);
			}
			else {
				sb.AppendFormat ("{1}{2}{0}", fieldName, dataParameter.ParameterName, op);
			}
			return sb.ToString ();
		}

		public virtual string CreateRelationTableSql (string fieldName, QueryPredicate predicate, bool isReverse, string relationFieldName)
		{
			StringBuilder sb = new StringBuilder ();
			string op = GetQueryPredicate (predicate);
			if (!isReverse) {
				sb.AppendFormat ("{0}{2}{1}", fieldName, relationFieldName, op);
			}
			else {
				sb.AppendFormat ("{1}{2}{0}", fieldName, relationFieldName, op);
			}
			return sb.ToString ();
		}

		public virtual string CreateCollectionParamsQuerySql (string fieldName, QueryCollectionPredicate predicate, List<DataParameter> dataParameters)
		{
			string op = GetQueryCollectionPredicate (predicate);
			if (dataParameters.Count == 0) {
				throw new LightDataException (RE.EnumerableLengthNotAllowIsZero);
			}
			int i = 0;
			StringBuilder sb = new StringBuilder ();
			sb.AppendFormat ("{0} {1} (", fieldName, op);
			foreach (DataParameter dataParameter in dataParameters) {
				if (i > 0)
					sb.Append (",");
				sb.Append (dataParameter.ParameterName);
				i++;
			}
			sb.Append (")");
			return sb.ToString ();
		}

		public virtual string CreateExistsQuerySql (string queryTableName, string whereString, bool isNot)
		{
			return string.Format ("{2}exists (select 1 from {0} where {1})", queryTableName, whereString, isNot ? "not " : string.Empty);
		}

		public virtual string CreateSubQuerySql (string fieldName, QueryCollectionPredicate predicate, string queryfieldName, string queryTableName, string whereString)
		{
			StringBuilder sb = new StringBuilder ();
			string op = GetQueryCollectionPredicate (predicate);
			sb.AppendFormat ("{0} {3} (select {1} from {2}", fieldName, queryfieldName, queryTableName, op);
			if (!string.IsNullOrEmpty (whereString)) {
				sb.AppendFormat (" where {0}", whereString);
			}
			sb.Append (")");
			return sb.ToString ();
		}

		public virtual string CreateBetweenParamsQuerySql (string fieldName, bool isNot, DataParameter fromParam, DataParameter toParam)
		{
			StringBuilder sb = new StringBuilder ();
			sb.AppendFormat ("{0} {3}between {1} and {2}", fieldName, fromParam.ParameterName, toParam.ParameterName, isNot ? string.Empty : "not ");
			return sb.ToString ();
		}

		public virtual string CreateCollectionMatchQuerySql (string fieldName, bool isReverse, bool starts, bool ends, bool isNot, List<DataParameter> dataParameters)
		{
			if (dataParameters.Count == 0) {
				throw new LightDataException (RE.EnumerableLengthNotAllowIsZero);
			}
			int i = 0;
			int length = dataParameters.Count;
			StringBuilder sb = new StringBuilder ();
			if (length > 1) {
				sb.Append ("(");
			}
			foreach (DataParameter item in dataParameters) {
				if (i > 0) {
					if (isNot) {
						sb.Append (" and ");
					}
					else {
						sb.Append (" or ");
					}
				}
				if (!isReverse) {
					sb.AppendFormat ("{0} {2}like {1}", fieldName, item.ParameterName, isNot ? "not " : string.Empty);
					string value = item.Value.ToString ();
					if (starts && !value.StartsWith (_wildcards)) {
						value = _wildcards + value;
					}
					if (ends && !value.EndsWith (_wildcards)) {
						value = value + _wildcards;
					}
					item.Value = value;
				}
				else {
					sb.AppendFormat ("{1} {2}like {0}", fieldName, item.ParameterName, isNot ? "not " : string.Empty);
				}
				i++;
			}
			if (length > 1) {
				sb.Append (")");
			}
			return sb.ToString ();
		}

		public virtual string CreateNullQuerySql (string fieldName, bool isNull)
		{
			return string.Format ("{0} is{1} null", fieldName, isNull ? string.Empty : " not");
		}

		public virtual string CreateBooleanQuerySql (string fieldName, bool isTrue)
		{
			return string.Format ("{0}={1}", fieldName, isTrue ? "1" : "0");
		}

		public virtual string CreateOrderBySql (string fieldName, OrderType orderType)
		{
			return string.Format ("{0} {1}", fieldName, orderType.ToString ().ToLower ());
		}

		public virtual string CreateRandomOrderBySql (DataEntityMapping mapping, bool fullFieldName)
		{
			return "newid()";
		}

		protected virtual string CreateIdentitySql (DataTableEntityMapping mapping)
		{
			if (mapping.IdentityField != null) {
				return "select @@Identity;";
			}
			else {
				return string.Empty;
			}
		}

		public virtual string CreateCountAllSql ()
		{
			return "count(1)";
		}

		public virtual string CreateConditionCountSql (string expressionSql, string fieldName, bool isDistinct)
		{
			return string.Format ("count({2}case when {0} then {1} else null end)", expressionSql, !string.IsNullOrEmpty (fieldName) ? fieldName : "1", isDistinct ? "distinct " : "");
		}

		public virtual string CreateCountSql (string fieldName, bool isDistinct)
		{
			return string.Format ("count({1}{0})", fieldName, isDistinct ? "distinct " : "");
		}

		public virtual string CreateSumSql (string fieldName, bool isDistinct)
		{
			return string.Format ("sum({1}{0})", fieldName, isDistinct ? "distinct " : "");
		}

		public virtual string CreateConditionSumSql (string expressionSql, string fieldName, bool isDistinct)
		{
			return string.Format ("sum({2}case when {0} then {1} else null end)", expressionSql, fieldName, isDistinct ? "distinct " : "");
		}

		public virtual string CreateAvgSql (string fieldName, bool isDistinct)
		{
			return string.Format ("avg({1}{0})", fieldName, isDistinct ? "distinct " : "");
		}

		public virtual string CreateConditionAvgSql (string expressionSql, string fieldName, bool isDistinct)
		{
			return string.Format ("avg({2}case when {0} then {1} else null end)", expressionSql, fieldName, isDistinct ? "distinct " : "");
		}

		public virtual string CreateMaxSql (string fieldName)
		{
			return string.Format ("max({0})", fieldName);
		}

		public virtual string CreateConditionMaxSql (string expressionSql, string fieldName)
		{
			return string.Format ("max(case when {0} then {1} else null end)", expressionSql, fieldName);
		}

		public virtual string CreateMinSql (string fieldName)
		{
			return string.Format ("min({0})", fieldName);
		}

		public virtual string CreateConditionMinSql (string expressionSql, string fieldName)
		{
			return string.Format ("min(case when {0} then {1} else null end)", expressionSql, fieldName);
		}

		public virtual string CreateAliasSql (string field, string alias)
		{
			return string.Format ("{0} as {1}", field, CreateDataFieldSql (alias));
		}

		public virtual string CreateDataFieldSql (string fieldName)
		{
			return fieldName;
		}

		public virtual string CreateDataTableSql (string tableName)
		{
			return tableName;
		}

		public virtual string CreateDataTableSql (DataEntityMapping table)
		{
			return CreateDataTableSql (table.TableName);
		}

		public virtual string CreateFullDataFieldSql (string tableName, string fieldName)
		{
			return string.Format ("{0}.{1}", CreateDataTableSql (tableName), CreateDataFieldSql (fieldName));
		}

		public virtual string CreateMatchSql (string field, bool left, bool right)
		{
			throw new NotImplementedException ();
		}

		public virtual string CreateDateSql (string field, string format)
		{
			throw new NotImplementedException ();
		}

		public virtual string CreateYearSql (string field)
		{
			throw new NotImplementedException ();
		}

		public virtual string CreateMonthSql (string field)
		{
			throw new NotImplementedException ();
		}

		public virtual string CreateDaySql (string field)
		{
			throw new NotImplementedException ();
		}

		public virtual string CreateHourSql (string field)
		{
			throw new NotImplementedException ();
		}

		public virtual string CreateMinuteSql (string field)
		{
			throw new NotImplementedException ();
		}

		public virtual string CreateSecondSql (string field)
		{
			throw new NotImplementedException ();
		}

		public virtual string CreateWeekSql (string field)
		{
			throw new NotImplementedException ();
		}

		public virtual string CreateWeekDaySql (string field)
		{
			throw new NotImplementedException ();
		}

		public virtual string CreateLengthSql (string field)
		{
			throw new NotImplementedException ();
		}

		public virtual string CreateSubStringSql (string field, int start, int size)
		{
			throw new NotImplementedException ();
		}

		public virtual string CreateDataBaseTimeSql ()
		{
			throw new NotImplementedException ();
		}

		public virtual string CreateStringSql (string value)
		{
			if (value.IndexOf ('\\') >= 0) {
				value = value.Replace ("\\", "\\\\");
			}
			if (value.IndexOf ('\'') >= 0) {
				value = value.Replace ("\'", "\\'");
			}
			return string.Format ("'{0}'", value);
		}

		public virtual string CreateNullSql ()
		{
			return "null";
		}

		public virtual string CreateNumberSql (object value)
		{
			return value.ToString ();
		}

		public virtual string CreateBooleanSql (bool value)
		{
			return value ? "true" : "false";
		}

		public virtual string CreatePlusSql (string field, object value, bool forward)
		{
			if (forward) {
				return string.Format ("({0}+{1})", field, value);
			}
			else {
				return string.Format ("({0}+{1})", value, field);
			}
		}

		public virtual string CreateMinusSql (string field, object value, bool forward)
		{
			if (forward) {
				return string.Format ("({0}-{1})", field, value);
			}
			else {
				return string.Format ("({0}-{1})", value, field);
			}
		}

		public virtual string CreateMultiplySql (string field, object value, bool forward)
		{
			if (forward) {
				return string.Format ("({0}*{1})", field, value);
			}
			else {
				return string.Format ("({0}*{1})", value, field);
			}
		}

		public virtual string CreateDividedSql (string field, object value, bool forward)
		{
			if (forward) {
				return string.Format ("({0}/{1})", field, value);
			}
			else {
				return string.Format ("({0}/{1})", value, field);
			}
		}

		public virtual string CreateModSql (string field, object value, bool forward)
		{
			if (forward) {
				return string.Format ("({0}%{1})", field, value);
			}
			else {
				return string.Format ("({0}%{1})", value, field);
			}
		}

		public virtual string CreatePowerSql (string field, object value, bool forward)
		{
			if (forward) {
				return string.Format ("({0}^{1})", field, value);
			}
			else {
				return string.Format ("({0}^{1})", value, field);
			}
		}

		public virtual string CreateAbsSql (string field)
		{
			return string.Format ("abs({0})", field);
		}

		public virtual string CreateLogSql (string field)
		{
			return string.Format ("ln({0})", field);
		}

		public virtual string CreateExpSql (string field)
		{
			return string.Format ("exp({0})", field);
		}

		public virtual string CreateSinSql (string field)
		{
			return string.Format ("sin({0})", field);
		}

		public virtual string CreateCosSql (string field)
		{
			return string.Format ("cos({0})", field);
		}

		public virtual string CreateTanSql (string field)
		{
			return string.Format ("tan({0})", field);
		}

		public virtual string CreateAtanSql (string field)
		{
			return string.Format ("atan({0})", field);
		}

		#endregion


		public virtual string CreateJoinOnMatchSql (string leftField, QueryPredicate predicate, string rightField)
		{
			StringBuilder sb = new StringBuilder ();
			string op = GetQueryPredicate (predicate);
			sb.AppendFormat ("{0}{2}{1}", leftField, rightField, op);
			return sb.ToString ();
		}

		public virtual string CreateParamName (string name)
		{
			if (!name.StartsWith ("?")) {
				return "?" + name;
			}
			else {
				return name;
			}
		}
	}
}