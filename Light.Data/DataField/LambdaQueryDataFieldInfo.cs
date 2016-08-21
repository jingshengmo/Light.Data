﻿using System;
namespace Light.Data
{
	class LambdaQueryDataFieldInfo : LambdaDataFieldInfo
	{
		QueryExpression _query;

		public LambdaQueryDataFieldInfo (QueryExpression query)
			: base (query.TableMapping)
		{
			_query = query;
		}

		internal override string CreateSqlString (CommandFactory factory, bool isFullName, out DataParameter [] dataParameters)
		{
			return _query.CreateSqlString (factory, isFullName, out dataParameters);
		}

		internal override string CreateSqlString (CommandFactory factory, bool isFullName, CreateSqlState state)
		{
			string sql = state.GetDataSql (this, isFullName);
			if (sql != null) {
				return sql;
			}

			sql = _query.CreateSqlString (factory, isFullName, state);

			state.SetDataSql (this, isFullName, sql);
			return sql;
		}
	}
}

