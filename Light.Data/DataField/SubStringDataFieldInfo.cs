﻿using System;

namespace Light.Data
{
	class SubStringDataFieldInfo : ExtendDataFieldInfo
	{
		object _start;

		object _size;

		internal SubStringDataFieldInfo (DataFieldInfo info, object start, object size)
			: base (info)
		{
			//if (start < 0) {
			//	throw new ArgumentOutOfRangeException ("start");
			//}
			//if (size < 0) {
			//	throw new ArgumentOutOfRangeException ("size");
			//}
			_start = start;
			_size = size;
		}

		//internal override string CreateDataFieldSql (CommandFactory factory, bool isFullName)
		//{
		//	string field = BaseFieldInfo.CreateDataFieldSql (factory, isFullName);
		//	//object start = _start;
		//	//object size = _size;
		//	object start = LambdaExpressionExtend.ConvertObject (_start, factory, isFullName, false);
		//	object size = LambdaExpressionExtend.ConvertObject (_size, factory, isFullName, false);
		//	return factory.CreateSubStringSql (field, start, size);
		//}

		internal override string CreateDataFieldSql (CommandFactory factory, bool isFullName, out DataParameter [] dataParameters)
		{
			DataParameter [] dataParameters1 = null;
			DataParameter [] dataParameters2 = null;
			DataParameter [] dataParameters3 = null;
			string field = BaseFieldInfo.CreateDataFieldSql (factory, isFullName, out dataParameters1);
			//object start = LambdaExpressionExtend.ConvertLambdaObject (_start, factory, isFullName, false, out dataParameters2);
			//object size = LambdaExpressionExtend.ConvertLambdaObject (_size, factory, isFullName, false, out dataParameters3);
			object start = _start;
			object size = _size;
			string sql = factory.CreateSubStringSql (field, start, size);
			dataParameters = DataParameter.ConcatDataParameters (dataParameters1, dataParameters2, dataParameters3);
			return sql;
		}

		internal override string DBType {
			get {
				return "string";
			}
		}

		internal override object ToParameter (object value)
		{
			if (value is string) {
				return value;
			}
			else {
				return value.ToString ();
			}
		}

		//protected override bool EqualsDetail (DataFieldInfo info)
		//{
		//	if (base.EqualsDetail (info)) {
		//		SubStringDataFieldInfo target = info as SubStringDataFieldInfo;
		//		if (!Object.Equals (target, null)) {
		//			return this._start == target._start && this._size == target._size;
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
