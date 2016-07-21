﻿
using System;

namespace Light.Data
{
	class BooleanQueryExpression : QueryExpression, ISupportNotDefine
	{
		DataFieldInfo _fieldInfo;

		bool _isTrue;

		public BooleanQueryExpression (DataFieldInfo fieldInfo, bool isTrue)
			: base (fieldInfo.TableMapping)
		{
			_fieldInfo = fieldInfo;
			_isTrue = isTrue;
		}

		//internal override string CreateSqlString (CommandFactory factory, bool fullFieldName, out DataParameter[] dataParameters)
		//{
		//	dataParameters = null;
		//	return factory.CreateBooleanQuerySql (_fieldInfo.CreateDataFieldSql (factory, fullFieldName), _isTrue);
		//}

		internal override string CreateSqlString (CommandFactory factory, bool fullFieldName, out DataParameter [] dataParameters)
		{
			return factory.CreateBooleanQuerySql (_fieldInfo.CreateDataFieldSql (factory, fullFieldName, out dataParameters), _isTrue);
		}

		protected override bool EqualsDetail (QueryExpression expression)
		{
			if (base.EqualsDetail (expression)) {
				BooleanQueryExpression target = expression as BooleanQueryExpression;
				return this._fieldInfo.Equals (target._fieldInfo)
				&& this._isTrue == target._isTrue;
			}
			else {
				return false;
			}
		}

		public void SetNot ()
		{
			_isTrue = !_isTrue;
		}
	}
}
