﻿using System.Collections.Generic;
using System.Collections;

namespace Light.Data
{
	class QueryState
	{
		//SingleRelationFieldMapping collectionRelateReferFieldMapping;

		//public SingleRelationFieldMapping CollectionRelateReferFieldMapping {
		//	get {
		//		return collectionRelateReferFieldMapping;
		//	}
		//}

		//object collectionRelateReferFieldValue;

		//public object CollectionRelateReferFieldValue {
		//	get {
		//		return collectionRelateReferFieldValue;
		//	}
		//}

		RelationMap relationMap;

		//readonly Dictionary<DataEntityMapping, object> joinDatas = new Dictionary<DataEntityMapping, object> ();

		readonly Dictionary<string, object> joinDatas = new Dictionary<string, object> ();

		readonly Dictionary<string, object> extendDatas = new Dictionary<string, object> ();

		//readonly Dictionary<DataEntityMapping, Hashtable> queryDatas = new Dictionary<DataEntityMapping, Hashtable> ();

		//public void SetCollectionValue (SingleRelationFieldMapping collectionFieldName, object value)
		//{
		//	this.collectionRelateReferFieldMapping = collectionFieldName;
		//	this.collectionRelateReferFieldValue = value;
		//}

		//public bool GetQueryData (DataEntityMapping mapping, object key, out object value)
		//{
		//	value = null;
		//	Hashtable table;
		//	if (!queryDatas.TryGetValue (mapping, out table)) {
		//		return false;
		//	}
		//	if (table.Contains (key)) {
		//		value = table [key];
		//		return true;
		//	}
		//	else {
		//		return false;
		//	}
		//}

		//public void SetQueryData (DataEntityMapping mapping, object key, object value)
		//{
		//	Hashtable table;
		//	if (!queryDatas.TryGetValue (mapping, out table)) {
		//		table = new Hashtable ();
		//		queryDatas.Add (mapping, table);
		//	}
		//	table.Add (key, value);
		//}

		public void InitialJoinData ()
		{
			this.joinDatas.Clear ();
			if (this.extendDatas.Count > 0) {
				foreach (KeyValuePair<string, object> kvs in this.extendDatas) {
					joinDatas.Add (kvs.Key, kvs.Value);
				}
			}
		}

		public void SetRelationMap (RelationMap relationMap)
		{
			this.relationMap = relationMap;
		}

		ISelector selector;

		public void SetSelector (ISelector selector)
		{
			this.selector = selector;
		}

		//public void SetRootJoinData (DataEntityMapping mapping, object value)
		//{
		//	if (mapping == relationMap.RootMapping) {
		//		joinDatas [mapping] = value;
		//	}
		//}

		//public string GetRootAliasName ()
		//{
		//	return this.relationMap.RootAliasName;
		//}

		//public void SetJoinData (SingleRelationFieldMapping mapping, object value)
		//{
		//	if (!joinDatas.ContainsKey (mapping.RelateMapping)) {
		//		joinDatas [mapping.RelateMapping] = value;
		//	}
		//}



		//public bool GetJoinData (SingleRelationFieldMapping mapping, out object value)
		//{
		//	if (joinDatas.TryGetValue (mapping.RelateMapping, out value)) {
		//		return true;
		//	}
		//	else {
		//		return false;
		//	}
		//}

		public void SetExtendData (string fieldPath, object value)
		{
			extendDatas [fieldPath] = value;
		}


		public void SetJoinData (string fieldPath, object value)
		{
			joinDatas [fieldPath] = value;
		}

		public bool GetJoinData (string fieldPath, out object value)
		{
			string m;
			if (relationMap.TryGetCycleFieldPath (fieldPath, out m)) {
				return joinDatas.TryGetValue (m, out value);
			}
			else {
				return joinDatas.TryGetValue (fieldPath, out value);
			}
		}

		public string GetAliasName (string fieldPath)
		{
			string alias;
			if (this.relationMap.CheckValid (fieldPath, out alias)) {
				return alias;
			}
			else {
				throw new LightDataException ("");
			}
		}

		//public bool CheckJoinData (SingleRelationFieldMapping mapping, out string aliasName)
		//{
		//	return this.relationMap.CheckValid (mapping, out aliasName);
		//}
	}
}
