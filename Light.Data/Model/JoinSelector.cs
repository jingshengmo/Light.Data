﻿using System;
using System.Collections.Generic;

namespace Light.Data
{
	class JoinSelector : ISelector
	{
		Dictionary<string, DataFieldInfo> infoDict = new Dictionary<string, DataFieldInfo> ();

		//List<DataFieldInfo> infoList = new List<DataFieldInfo> ();

		public void SetDataEntity (DataEntityMapping entityMapping)
		{
			if (entityMapping == null)
				throw new ArgumentNullException (nameof (entityMapping));
			foreach (DataFieldMapping fieldMapping in entityMapping.DataEntityFields) {
				if (fieldMapping != null) {
					DataFieldInfo field = new DataFieldInfo (fieldMapping);
					//					this.infoList [info.FieldName] = info;
					AliasDataFieldInfo aliasField = new AliasDataFieldInfo (field, field.FieldName);
					this.infoDict [aliasField.Alias] = aliasField;
				}
			}
		}

		public void SetDataField (DataFieldInfo field)
		{
			if (Object.Equals (field, null))
				throw new ArgumentNullException (nameof (field));
			//			this.infoList [field.FieldName] = field;
			AliasDataFieldInfo aliasField = new AliasDataFieldInfo (field, field.FieldName);
			this.infoDict [aliasField.Alias] = aliasField;
		}

		//internal void SetInnerDataField (DataFieldInfo field)
		//{
		//	if (Object.Equals (field, null))
		//		throw new ArgumentNullException (nameof (field));
		//	this.infoList.Add(field);
		//}

		public void SetAliasDataField (AliasDataFieldInfo aliasField)
		{
			if (Object.Equals (aliasField, null))
				throw new ArgumentNullException (nameof (aliasField));
			this.infoDict [aliasField.Alias] = aliasField;
		}

		public List<DataFieldInfo> GetFieldInfos ()
		{
			//if (infoList.Count > 0) {
			//	List<DataFieldInfo> infos = new List<DataFieldInfo> (this.infoList);
			//	return infos;
			//}
			//else {
			List<DataFieldInfo> infos = new List<DataFieldInfo> (this.infoDict.Values);
			return infos;
			//}
		}

		/// <summary>
		/// Clones the with except DataEntityMapping,bucause mpping will referer the owin.
		/// </summary>
		/// <returns>The with except clone.</returns>
		/// <param name="exceptMappings">Except mappings.</param>
		internal JoinSelector CloneWithExcept (DataEntityMapping [] exceptMappings)
		{
			JoinSelector target = new JoinSelector ();
			foreach (KeyValuePair<string, DataFieldInfo> kv in this.infoDict) {
				DataEntityMapping mapping = kv.Value.TableMapping;
				bool isexcept = false;
				if (exceptMappings != null && exceptMappings.Length > 0) {
					foreach (DataEntityMapping exceptMapping in exceptMappings) {
						if (exceptMapping == mapping) {
							isexcept = true;
							break;
						}
					}
				}
				if (!isexcept) {
					target.infoDict [kv.Key] = kv.Value;
				}
			}
			return target;
		}

		public string CreateSelectString (CommandFactory factory, out DataParameter [] dataParameters)
		{
			string [] selectList = new string [this.infoDict.Count];
			int index = 0;
			List<DataParameter> innerParameters = null;
			foreach (DataFieldInfo fieldInfo in this.infoDict.Values) {
				AliasDataFieldInfo aliasInfo = fieldInfo as AliasDataFieldInfo;
				DataParameter [] dataParameters1 = null;
				if (!Object.Equals (aliasInfo, null)) {
					selectList [index] = aliasInfo.CreateAliasDataFieldSql (factory, true, out dataParameters1);
				}
				else {
					selectList [index] = fieldInfo.CreateDataFieldSql (factory, true, out dataParameters1);
				}
				if (dataParameters1 != null && dataParameters1.Length > 0) {
					if (innerParameters == null) {
						innerParameters = new List<DataParameter> ();
					}
					innerParameters.AddRange (dataParameters1);
				}
				index++;
			}
			string customSelect = string.Join (",", selectList);
			dataParameters = innerParameters != null ? innerParameters.ToArray () : null;
			return customSelect;
		}
	}
}
