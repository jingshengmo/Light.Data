﻿using System;
namespace Light.Data
{
	abstract class LambdaDataFieldInfo : DataFieldInfo
	{
		public LambdaDataFieldInfo (DataEntityMapping mapping)
			: base (mapping)
		{
		}

		public LambdaDataFieldInfo (DataEntityMapping mapping, bool customName, string name)
			: base (mapping, customName, name)
		{
		}

		internal override string DBType {
			get {
				return string.Empty;
			}
		}

		internal override object ToParameter (object value)
		{
			return value;
		}

	}
}

