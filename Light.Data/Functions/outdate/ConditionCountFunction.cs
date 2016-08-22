﻿using System;

namespace Light.Data
{
	class ConditionCountFunction : AggregateData
	{
		QueryExpression _expression;

		DataFieldInfo _fieldinfo;

		bool _isDistinct;

		//internal ConditionCountFunction (QueryExpression expression, bool isDistinct)
		//	: base (expression.TableMapping)
		//{
		//	_expression = expression;
		//	_isDistinct = isDistinct;
		//}

		internal ConditionCountFunction (DataFieldInfo fieldinfo, QueryExpression expression, bool isDistinct)
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
		//	string sql = factory.CreateConditionCountSql (expressionString, !Object.Equals (this._fieldinfo, null) ? _fieldinfo.CreateSqlString (factory, isFullName, out dataParameters2) : null, _isDistinct);
		//	dataParameters = DataParameter.ConcatDataParameters (dataParameters1, dataParameters2);
		//	return sql;
		//}

		internal override string CreateSqlString (CommandFactory factory, bool isFullName, CreateSqlState state)
		{
			string expressionString = _expression.CreateSqlString (factory, isFullName, state);
			string sql = factory.CreateConditionCountSql (expressionString, !Object.Equals (this._fieldinfo, null) ? _fieldinfo.CreateSqlString (factory, isFullName, state) : null, _isDistinct);
			return sql;
		}

		//protected override bool EqualsDetail (AggregateFunction function)
		//{
		//	if ((this.TableMapping == null && function.TableMapping == null) || base.EqualsDetail (function)) {
		//		ConditionCountFunction target = function as ConditionCountFunction;
		//		bool f1 = Object.Equals (this._fieldinfo, null);
		//		bool f2 = Object.Equals (target._fieldinfo, null);
		//		if ((f1 && f2) || (!f1 && !f2)) {
		//			return this._isDistinct == target._isDistinct && this._expression.Equals (target._expression);
		//		}
		//		else {
		//			return false;
		//		}
		//	}
		//	else {
		//		return false;
		//	}
		//}
	}
}
