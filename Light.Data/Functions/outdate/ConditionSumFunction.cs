﻿using System;

namespace Light.Data
{
	class ConditionSumFunction : AggregateData
	{
		QueryExpression _expression;

		DataFieldInfo _fieldinfo;

		bool _isDistinct;

		internal ConditionSumFunction (DataFieldInfo fieldinfo, QueryExpression expression, bool isDistinct)
			: base (fieldinfo.TableMapping)
		{
			_expression = expression;
			_fieldinfo = fieldinfo;
			_isDistinct = isDistinct;
		}

		//internal override string CreateSqlString (CommandFactory factory, bool isFullName, out DataParameter [] dataParameters)
		//{
		//	DataParameter [] dataParameters1 = null;
		//	DataParameter [] dataParameters2 = null;
		//	string expressionString = _expression.CreateSqlString (factory, isFullName, out dataParameters1);
		//	string sql = factory.CreateConditionSumSql (expressionString, !Object.Equals (this._fieldinfo, null) ? _fieldinfo.CreateSqlString (factory, isFullName, out dataParameters2) : null, _isDistinct);
		//	dataParameters = DataParameter.ConcatDataParameters (dataParameters1, dataParameters2);
		//	return sql;
		//}

		internal override string CreateSqlString (CommandFactory factory, bool isFullName, CreateSqlState state)
		{
			string expressionString = _expression.CreateSqlString (factory, isFullName, state);
			string sql = factory.CreateConditionSumSql (expressionString, !Object.Equals (this._fieldinfo, null) ? _fieldinfo.CreateSqlString (factory, isFullName, state) : null, _isDistinct);
			return sql;
		}


		//protected override bool EqualsDetail (AggregateFunction function)
		//{
		//	if (base.EqualsDetail (function)) {
		//		ConditionSumFunction target = function as ConditionSumFunction;
		//		return this._fieldinfo.Equals (target._fieldinfo) && this._isDistinct == target._isDistinct && this._expression.Equals (target._expression);
		//	}
		//	else {
		//		return false;
		//	}
		//}
	}
}
