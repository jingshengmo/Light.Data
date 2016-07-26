﻿using System;

namespace Light.Data
{
	class DataFieldQueryExpression : QueryExpression
	{
		DataFieldInfo _fieldInfo;

		QueryPredicate _predicate;

		DataFieldInfo _relateFieldInfo;

		bool _isReverse;

		bool _isSameTable;

		public DataFieldQueryExpression (DataFieldInfo fieldInfo, QueryPredicate predicate, DataFieldInfo relateFieldInfo, bool isReverse)
			: base (fieldInfo.TableMapping)
		{
			_fieldInfo = fieldInfo;
			_predicate = predicate;
			_relateFieldInfo = relateFieldInfo;
			_isReverse = isReverse;
			_isSameTable = Object.Equals (fieldInfo.TableMapping, relateFieldInfo.TableMapping);
		}

		//internal override string CreateSqlString (CommandFactory factory, bool fullFieldName, out DataParameter[] dataParameters)
		//{
		//	dataParameters = null;
		//	return factory.CreateRelationTableSql (_fieldInfo.CreateDataFieldSql (factory, false), _predicate, _isReverse, _relateFieldInfo.CreateDataFieldSql (factory, !_isSameTable));
		//}

		internal override string CreateSqlString (CommandFactory factory, bool fullFieldName, out DataParameter [] dataParameters)
		{
			//dataParameters = null;
			DataParameter [] dataParameters1;
			DataParameter [] dataParameters2;
			string sql = factory.CreateRelationTableSql (_fieldInfo.CreateDataFieldSql (factory, fullFieldName, out dataParameters1), _predicate, _isReverse, _relateFieldInfo.CreateDataFieldSql (factory, fullFieldName ? true : !_isSameTable, out dataParameters2));
			dataParameters = DataParameter.ConcatDataParameters (dataParameters1, dataParameters2);
			return sql;
		}

		//protected override bool EqualsDetail (QueryExpression expression)
		//{
		//	if (base.EqualsDetail (expression)) {
		//		DataFieldQueryExpression target = expression as DataFieldQueryExpression;
		//		return this._fieldInfo.Equals (target._fieldInfo)
		//		&& this._relateFieldInfo.Equals (target._relateFieldInfo)
		//		&& this._predicate == target._predicate
		//		&& this._isReverse == target._isReverse;
		//	}
		//	else {
		//		return false;
		//	}
		//}
	}
}
