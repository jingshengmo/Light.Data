﻿using System;
using System.Text.RegularExpressions;

namespace Light.Data
{
	class EnumFieldMapping : DataFieldMapping
	{
		readonly object _minValue;

		readonly object _defaultValue;

		Type _nullableType;

		public Type NullableType {
			get {
				return _nullableType;
			}
		}

		//bool auto;

		public EnumFieldMapping (Type type, string fieldName, string indexName, DataMapping mapping, bool isNullable, string dbType, object defaultValue)
			: base (type, fieldName, indexName, mapping, isNullable, dbType)
		{
			Type itemstype = Type.GetType ("System.Nullable`1");
			_nullableType = itemstype.MakeGenericType (type);
			Array values = Enum.GetValues (ObjectType);
			object value = values.GetValue (0);
			_minValue = Convert.ChangeType (value, _typeCode);

			//_minValue = Convert.ChangeType (value, _typeCode);
			//if (defaultValue != null) {
			//	string str = defaultValue as String;
			//	if (str != null) {
			//		object dvalue = Enum.Parse (type, str, true);
			//		_defaultValue = Convert.ChangeType (dvalue, _typeCode);
			//	}
			//	else if (defaultValue.GetType () == type) {
			//		_defaultValue = Convert.ChangeType (defaultValue, _typeCode);
			//	}
			//}

			//if (defaultValue != null && defaultValue.GetType () == type) {
			//	_defaultValue = Convert.ChangeType (defaultValue, _typeCode);
			//}
			if (defaultValue != null) {
				Type defaultValueType = defaultValue.GetType ();
				if (defaultValueType == type) {
					_defaultValue = Convert.ChangeType (defaultValue, _typeCode);
				}
				else {
					throw new LightDataException (string.Format (RE.EnumDefaultValueType, fieldName, defaultValue));
				}
			}
		}

		public EnumFieldMapping (Type type, string fieldName, string indexName, DataMapping mapping, bool isNullable)
			: base (type, fieldName, indexName, mapping, isNullable, null)
		{
			Type itemstype = Type.GetType ("System.Nullable`1");
			_nullableType = itemstype.MakeGenericType (type);
			Array values = Enum.GetValues (ObjectType);
			object value = values.GetValue (0);
			_minValue = Convert.ChangeType (value, _typeCode);
		}

		public override object ToProperty (object value)
		{
			if (Object.Equals (value, DBNull.Value) || Object.Equals (value, null)) {
				return null;
			}
			else {
				//if (auto) {
				//	//string str = value as string;
				//	//if (str != null) {
				//	//	return Enum.Parse (ObjectType, str);
				//	//}
				//	//else {
				//	Type type = value.GetType ();
				//	TypeCode code = Type.GetTypeCode (type);
				//	if (code != this._typeCode) {
				//		value = Convert.ChangeType (value, this._typeCode);
				//	}
				//	return value;
				//	//}
				//}
				//else {
				//	Type type = value.GetType ();
				//	TypeCode code = Type.GetTypeCode (type);
				//	if (code != this._typeCode) {
				//		value = Convert.ChangeType (value, this._typeCode);
				//	}
				//	return value;
				//}
				//Type type = value.GetType ();
				//TypeCode code = Type.GetTypeCode (type);
				//if (code != this._typeCode) {
				//	value = Convert.ChangeType (value, this._typeCode);
				//}
				//value = Convert.ChangeType (value, _objectType);
				value = Enum.ToObject (_objectType, value);
				return value;
			}
		}

		public override object ToParameter (object value)
		{
			if (Object.Equals (value, null) || Object.Equals (value, DBNull.Value)) {
				return null;
			}
			else {
				return Convert.ChangeType (value, _typeCode);
			}
		}

		#region implemented abstract members of DataFieldMapping

		public override object ToColumn (object value)
		{
			if (Object.Equals (value, DBNull.Value) || Object.Equals (value, null)) {
				if (IsNullable) {
					return null;
				}
				else {
					if (_defaultValue != null) {
						return _defaultValue;
					}
					else {
						return _minValue;
					}
				}
			}
			else {
				return Convert.ChangeType (value, _typeCode);
			}
		}

		#endregion
	}
}
