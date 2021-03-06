﻿using System;
using System.Data;

namespace Light.Data
{
	/// <summary>
	/// Data parameter.
	/// </summary>
	public class DataParameter
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="Light.Data.DataParameter"/> class.
		/// </summary>
		/// <param name="paramName">Parameter name.</param>
		/// <param name="paramValue">Parameter value.</param>
		/// <param name="dbType">Db type.</param>
		/// <param name="direction">Direction.</param>
		public DataParameter (string paramName, object paramValue, string dbType, ParameterDirection direction)
		{
			if (string.IsNullOrEmpty (paramName)) {
				throw new ArgumentNullException ("paramName");
			}
            
			_parameterName = paramName;
			_dbType = dbType;
			_value = paramValue;
			_direction = direction;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Light.Data.DataParameter"/> class.
		/// </summary>
		/// <param name="paramName">Parameter name.</param>
		/// <param name="paramValue">Parameter value.</param>
		public DataParameter (string paramName, object paramValue)
			: this (paramName, paramValue, null, ParameterDirection.Input)
		{

		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Light.Data.DataParameter"/> class.
		/// </summary>
		/// <param name="paramName">Parameter name.</param>
		/// <param name="paramValue">Parameter value.</param>
		/// <param name="direction">Direction.</param>
		public DataParameter (string paramName, object paramValue, ParameterDirection direction)
			: this (paramName, paramValue, null, direction)
		{

		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Light.Data.DataParameter"/> class.
		/// </summary>
		/// <param name="paramName">Parameter name.</param>
		/// <param name="paramValue">Parameter value.</param>
		/// <param name="dbType">Db type.</param>
		public DataParameter (string paramName, object paramValue, string dbType)
			: this (paramName, paramValue, dbType, ParameterDirection.Input)
		{

		}

		IDataParameter _dataParameter = null;

		/// <summary>
		/// Sets the data parameter.
		/// </summary>
		/// <param name="dataParameter">Data parameter.</param>
		internal void SetDataParameter (IDataParameter dataParameter)
		{
			_dataParameter = dataParameter;
		}

		string _parameterName;

		/// <summary>
		/// 参数名称
		/// </summary>
		public string ParameterName {
			get {

				return _parameterName;
			}
			internal set {
				_parameterName = value;
			}
		}

		object _value;

		/// <summary>
		/// Gets or sets the value.
		/// </summary>
		/// <value>The value.</value>
		public object Value {
			get {
				return _value;
			}
			internal set {
				_value = value;
			}
		}

		string _dbType;

		/// <summary>
		/// Gets or sets the DBType.
		/// </summary>
		/// <value>The type of the db.</value>
		public string DbType {
			get {
                
				return _dbType;
			}
			internal set {
				_dbType = value;
			}
		}

		ParameterDirection _direction;

		/// <summary>
		/// Gets or sets the direction.
		/// </summary>
		/// <value>The direction.</value>
		public ParameterDirection Direction {
			get {
				return _direction;
			}
			internal set {
				_direction = value;
			}
		}

		/// <summary>
		/// Gets the output value.
		/// </summary>
		/// <value>The output value.</value>
		public object OutputValue {
			get {
				if (_dataParameter == null)
					return null;
				return _dataParameter.Value;
			}
		}

	}
}
