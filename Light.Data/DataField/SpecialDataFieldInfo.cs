﻿using System;
namespace Light.Data
{
	class SpecialDataFieldInfo : LambdaDataFieldInfo, IAliasDataFieldInfo
	{
		DataFieldInfo _fieldInfo;

		//string _aliasName;

		public SpecialDataFieldInfo (DataFieldInfo fieldInfo, string name)
			: base (fieldInfo.TableMapping, true, name)
		{
			_fieldInfo = fieldInfo;
			//_aliasName = name;
		}

		public DataFieldInfo FieldInfo {
			get {
				return _fieldInfo;
			}
		}

		//public string AliasName {
		//	get {
		//		return _aliasName;
		//	}

		//	//set {
		//	//	_aliasName = value;
		//	//}
		//}

		public override DataFieldInfo CreateAliasTableInfo (string aliasTableName)
		{
			DataFieldInfo info = _fieldInfo.CreateAliasTableInfo (aliasTableName);
			SpecialDataFieldInfo newinfo = new SpecialDataFieldInfo (info, FieldName);
			newinfo._aliasTableName = aliasTableName;
			return newinfo;
		}

		internal override string CreateSqlString (CommandFactory factory, bool isFullName, CreateSqlState state)
		{
			//return _fieldInfo.CreateSqlString (factory, isFullName, state);
			if (isFullName) {
				string tableName = this._aliasTableName ?? TableMapping.TableName;
				return factory.CreateFullDataFieldSql (tableName, FieldName);
			}
			else {
				return factory.CreateDataFieldSql (FieldName);
			}
		}

		public string CreateAliasDataFieldSql (CommandFactory factory, bool isFullName, CreateSqlState state)
		{
			string fieldSql = _fieldInfo.CreateSqlString (factory, isFullName, state);
			string sql = factory.CreateAliasFieldSql (fieldSql, FieldName);
			return sql;
		}
	}
}
