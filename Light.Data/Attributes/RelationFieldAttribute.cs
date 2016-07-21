﻿using System;

namespace Light.Data
{
	/// <summary>
	/// Relation field attribute.
	/// </summary>
	[AttributeUsage (AttributeTargets.Property, Inherited = true, AllowMultiple = true)]
	public class RelationFieldAttribute : Attribute
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="Light.Data.RelationFieldAttribute"/> class.
		/// </summary>
		/// <param name="masterKey">Master key.</param>
		/// <param name="relateKey">Relate key.</param>
		public RelationFieldAttribute (string masterKey, string relateKey)
		{
			if (string.IsNullOrEmpty (masterKey)) {
				throw new ArgumentNullException (nameof (masterKey));
			}
			if (string.IsNullOrEmpty (relateKey)) {
				throw new ArgumentNullException (nameof (relateKey));
			}
			MasterKey = masterKey;
			RelateKey = relateKey;
		}

		/// <summary>
		/// Gets the master key.
		/// </summary>
		/// <value>The master key.</value>
		public string MasterKey {
			get;
			private set;
		}

		/// <summary>
		/// Gets the relate key.
		/// </summary>
		/// <value>The relate key.</value>
		public string RelateKey {
			get;
			private set;
		}
	}
}
