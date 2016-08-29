﻿
namespace Light.Data
{
	/// <summary>
	/// Constant select field info.
	/// </summary>
	class ConstantSelectFieldInfo : SelectFieldInfo
	{
		readonly object _value;

		/// <summary>
		/// Initializes a new instance of the <see cref="Light.Data.ConstantSelectFieldInfo"/> class.
		/// </summary>
		/// <param name="value">Value.</param>
		public ConstantSelectFieldInfo (object value)
		{
			_value = value;
		}

		#region implemented abstract members of SelectFieldInfo
		/// <summary>
		/// Creates the data field sql.
		/// </summary>
		/// <returns>The data field sql.</returns>
		/// <param name="factory">Factory.</param>
		/// <param name="dataParameters">Data parameter.</param>
		//internal override string CreateSqlString (CommandFactory factory, out DataParameter [] dataParameters)
		//{
		//	if (_value != null) {
		//		string pn = factory.CreateTempParamName ();
		//		DataParameter dataParameter = new DataParameter (pn, _value);
		//		dataParameters = new [] { dataParameter };
		//		return pn;
		//	}
		//	else {
		//		dataParameters = null;
		//		return factory.CreateNullSql ();
		//	}
		//}

		internal override string CreateSqlString (CommandFactory factory, CreateSqlState state)
		{
			if (_value != null) {
				string pn = state.AddDataParameter (_value);
				return pn;
			}
			else {
				return factory.CreateNullSql ();
			}
		}

		/// <summary>
		/// Gets the table mapping.
		/// </summary>
		/// <value>The table mapping.</value>
		internal override DataEntityMapping TableMapping {
			get {
				return null;
			}
		}

		#endregion
	}
}
