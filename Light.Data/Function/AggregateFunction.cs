﻿using System;

namespace Light.Data
{
	/// <summary>
	/// Aggregate function.
	/// </summary>
	public abstract class AggregateFunction
	{
		internal AggregateFunction (DataEntityMapping tableMapping)
		{
			TableMapping = tableMapping;
		}

		internal DataEntityMapping TableMapping {
			get;
			private set;
		}

		/// <summary>
		/// Equal the specified value.
		/// </summary>
		/// <returns>The expression.</returns>
		/// <param name="value">Value.</param>
		public AggregateHavingExpression Eq (object value)
		{
			if (Object.Equals (value, null)) {
				return IsNull ();
			}
			else if (value is System.Collections.IEnumerable && !(value is string)) {
				return In ((System.Collections.IEnumerable)value);
			}
			else {
				return SingleParam (QueryPredicate.Eq, value);
			}
		}

		/// <summary>
		/// Less than or equal the specified value.
		/// </summary>
		/// <returns>The expression.</returns>
		/// <param name="value">Value.</param>
		public AggregateHavingExpression LtEq (object value)
		{
			return SingleParam (QueryPredicate.LtEq, value);
		}

		/// <summary>
		/// Less than the specified value.
		/// </summary>
		/// <returns>The expression.</returns>
		/// <param name="value">Value.</param>
		public AggregateHavingExpression Lt (object value)
		{
			return SingleParam (QueryPredicate.Lt, value);
		}

		/// <summary>
		/// Greate than the specified value.
		/// </summary>
		/// <returns>The expression.</returns>
		/// <param name="value">Value.</param>
		public AggregateHavingExpression Gt (object value)
		{
			return SingleParam (QueryPredicate.Gt, value);
		}

		/// <summary>
		/// Greate than or equal the specified value.
		/// </summary>
		/// <returns>The expression.</returns>
		/// <param name="value">Value.</param>
		public AggregateHavingExpression GtEq (object value)
		{
			return SingleParam (QueryPredicate.GtEq, value);
		}

		/// <summary>
		/// Not equal the specified value.
		/// </summary>
		/// <returns>The expression.</returns>
		/// <param name="value">Value.</param>
		public AggregateHavingExpression NotEq (object value)
		{
			if (Object.Equals (value, null)) {
				return IsNotNull ();
			}
			else if (value is System.Collections.IEnumerable && !(value is string)) {
				return NotIn ((System.Collections.IEnumerable)value);
			}
			else {
				return SingleParam (QueryPredicate.NotEq, value);
			}
		}

		/// <summary>
		/// In the specified values.
		/// </summary>
		/// <returns>The expression</returns>
		/// <param name="values">Values.</param>
		public AggregateHavingExpression In (System.Collections.IEnumerable values)
		{
			return CollectionParams (QueryCollectionPredicate.In, values);
		}

		/// <summary>
		/// Not in the specified values.
		/// </summary>
		/// <returns>The expression</returns>
		/// <param name="values">Values.</param>
		public AggregateHavingExpression NotIn (System.Collections.IEnumerable values)
		{
			return CollectionParams (QueryCollectionPredicate.NotIn, values);
		}

		/// <summary>
		/// In the specified field and expression.
		/// </summary>
		/// <returns>The expression</returns>
		/// <param name="field">Field.</param>
		/// <param name="expression">Expression.</param>
		public AggregateHavingExpression In (DataFieldInfo field, QueryExpression expression)
		{
			return CollectionParams (QueryCollectionPredicate.In, field, expression);
		}

		/// <summary>
		/// In the specified field.
		/// </summary>
		/// <returns>The expression</returns>
		/// <param name="field">Field.</param>
		public AggregateHavingExpression In (DataFieldInfo field)
		{
			return In (field, null);
		}

		/// <summary>
		/// Not in the specified field and expression.
		/// </summary>
		/// <returns>The expression</returns>
		/// <param name="field">Field.</param>
		/// <param name="expression">Expression.</param>
		public AggregateHavingExpression NotIn (DataFieldInfo field, QueryExpression expression)
		{
			return CollectionParams (QueryCollectionPredicate.NotIn, field, expression);
		}

		/// <summary>
		/// Not in the specified field.
		/// </summary>
		/// <returns>The expression</returns>
		/// <param name="field">Field.</param>
		public AggregateHavingExpression NotIn (DataFieldInfo field)
		{
			return NotIn (field, null);
		}

		/// <summary>
		/// Greater than all the specified values.
		/// </summary>
		/// <returns>The expression</returns>
		/// <param name="values">Values.</param>
		public AggregateHavingExpression GtAll (System.Collections.IEnumerable values)
		{
			return CollectionParams (QueryCollectionPredicate.GtAll, values);
		}

		/// <summary>
		/// Greater than all the specified field and expression.
		/// </summary>
		/// <returns>The expression</returns>
		/// <param name="field">Field.</param>
		/// <param name="expression">Expression.</param>
		public AggregateHavingExpression GtAll (DataFieldInfo field, QueryExpression expression)
		{
			return CollectionParams (QueryCollectionPredicate.GtAll, field, expression);
		}

		/// <summary>
		/// Greater than all the specified field.
		/// </summary>
		/// <returns>The expression</returns>
		/// <param name="field">Field.</param>
		public AggregateHavingExpression GtAll (DataFieldInfo field)
		{
			return GtAll (field, null);
		}

		/// <summary>
		/// Greater than all the specified values.
		/// </summary>
		/// <returns>The expression</returns>
		/// <param name="values">Values.</param>
		public AggregateHavingExpression LtAll (System.Collections.IEnumerable values)
		{
			return CollectionParams (QueryCollectionPredicate.LtAll, values);
		}

		/// <summary>
		/// Less than all the specified field and expression.
		/// </summary>
		/// <returns>The expression</returns>
		/// <param name="field">Field.</param>
		/// <param name="expression">Expression.</param>
		public AggregateHavingExpression LtAll (DataFieldInfo field, QueryExpression expression)
		{
			return CollectionParams (QueryCollectionPredicate.LtAll, field, expression);
		}

		/// <summary>
		/// Less than all the specified field.
		/// </summary>
		/// <returns>The expression</returns>
		/// <param name="field">Field.</param>
		public AggregateHavingExpression LtAll (DataFieldInfo field)
		{
			return LtAll (field, null);
		}

		/// <summary>
		/// Greater than any the specified values.
		/// </summary>
		/// <returns>The expression</returns>
		/// <param name="values">Values.</param>
		public AggregateHavingExpression GtAny (System.Collections.IEnumerable values)
		{
			return CollectionParams (QueryCollectionPredicate.GtAny, values);
		}

		/// <summary>
		/// Greater than any the specified field and expression.
		/// </summary>
		/// <returns>The expression</returns>
		/// <param name="field">Field.</param>
		/// <param name="expression">Expression.</param>
		public AggregateHavingExpression GtAny (DataFieldInfo field, QueryExpression expression)
		{
			return CollectionParams (QueryCollectionPredicate.GtAny, field, expression);
		}

		/// <summary>
		/// Greater than any the specified field.
		/// </summary>
		/// <returns>The expression</returns>
		/// <param name="field">Field.</param>
		public AggregateHavingExpression GtAny (DataFieldInfo field)
		{
			return GtAny (field, null);
		}

		/// <summary>
		/// Less than any the specified values.
		/// </summary>
		/// <returns>The expression</returns>
		/// <param name="values">Values.</param>
		public AggregateHavingExpression LtAny (System.Collections.IEnumerable values)
		{
			return CollectionParams (QueryCollectionPredicate.LtAny, values);
		}

		/// <summary>
		/// Less than any the specified field and expression.
		/// </summary>
		/// <returns>The expression</returns>
		/// <param name="field">Field.</param>
		/// <param name="expression">Expression.</param>
		public AggregateHavingExpression LtAny (DataFieldInfo field, QueryExpression expression)
		{
			return CollectionParams (QueryCollectionPredicate.LtAny, field, expression);
		}

		/// <summary>
		/// Less than any the specified field.
		/// </summary>
		/// <returns>The expression</returns>
		/// <param name="field">Field.</param>
		public AggregateHavingExpression LtAny (DataFieldInfo field)
		{
			return LtAny (field, null);
		}

		/// <summary>
		/// Between the specified fromValue and toValue.
		/// </summary>
		/// <returns>The expression</returns>
		/// <param name="fromValue">From value.</param>
		/// <param name="toValue">To value.</param>
		public AggregateHavingExpression Between (object fromValue, object toValue)
		{
			return BetweenParams (true, fromValue, toValue);
		}

		/// <summary>
		/// Nots the between fromValue and toValue.
		/// </summary>
		/// <returns>The expression</returns>
		/// <param name="fromValue">From value.</param>
		/// <param name="toValue">To value.</param>
		public AggregateHavingExpression NotBetween (object fromValue, object toValue)
		{
			return BetweenParams (false, fromValue, toValue);
		}

		/// <summary>
		/// Determines whether this field is null.
		/// </summary>
		/// <returns>The expression</returns>
		public AggregateHavingExpression IsNull ()
		{
			return Null (true);
		}

		/// <summary>
		/// Determines whether this field is not null.
		/// </summary>
		/// <returns>The expression</returns>
		public AggregateHavingExpression IsNotNull ()
		{
			return Null (false);
		}

		private AggregateHavingExpression SingleParam (QueryPredicate predicate, object value)
		{
			return SingleParam (predicate, value, false);
		}

		private AggregateHavingExpression SingleParam (QueryPredicate predicate, object value, bool isReverse)
		{
			if (Object.Equals (value, null)) {
				throw new ArgumentNullException ("value");
			}
			AggregateHavingExpression exp = new SingleParamAggregateExpression (this, predicate, value, isReverse);
			return exp;
		}

		private AggregateHavingExpression CollectionParams (QueryCollectionPredicate predicate, System.Collections.IEnumerable values)
		{
			if (Object.Equals (values, null)) {
				throw new ArgumentNullException ("values");
			}
			AggregateHavingExpression exp = new CollectionParamsAggregateExpression (this, predicate, values);
			return exp;
		}

		private AggregateHavingExpression CollectionParams (QueryCollectionPredicate predicate, DataFieldInfo field, QueryExpression expression)
		{
			if (Object.Equals (field, null)) {
				throw new ArgumentNullException ("field");
			}
			if (expression == null) {
				throw new ArgumentNullException ("expression");
			}
			AggregateHavingExpression exp = new SubAggregateExpression (this, predicate, field, expression);
			return exp;
		}

		private AggregateHavingExpression BetweenParams (bool isNot, object fromValue, object toValue)
		{
			if (Object.Equals (fromValue, null)) {
				throw new ArgumentNullException ("fromValue");
			}
			if (Object.Equals (toValue, null)) {
				throw new ArgumentNullException ("toValue");
			}
			AggregateHavingExpression exp = new BetweenParamsAggregateExpression (this, isNot, fromValue, toValue);
			return exp;
		}

		private AggregateHavingExpression Null (bool isNull)
		{
			AggregateHavingExpression exp = new NullAggregateExpression (this, isNull);
			return exp;
		}

		/// <param name="field">Field.</param>
		/// <param name="value">Value.</param>
		public static AggregateHavingExpression operator == (AggregateFunction field, object value)
		{
			if (value == null)
				return field.IsNull ();
			if (value is System.Collections.IEnumerable && value.GetType () != typeof(string))
				return field.In ((System.Collections.IEnumerable)value);
			return field.Eq (value);
		}

		/// <param name="field">Field.</param>
		/// <param name="value">Value.</param>
		public static AggregateHavingExpression operator != (AggregateFunction field, object value)
		{
			if (value == null)
				return field.IsNotNull ();
			if (value is System.Collections.IEnumerable && value.GetType () != typeof(string))
				return field.NotIn ((System.Collections.IEnumerable)value);
			return field.NotEq (value);
		}

		/// <param name="field">Field.</param>
		/// <param name="value">Value.</param>
		public static AggregateHavingExpression operator > (AggregateFunction field, object value)
		{
			return field.Gt (value);
		}

		/// <param name="field">Field.</param>
		/// <param name="value">Value.</param>
		public static AggregateHavingExpression operator >= (AggregateFunction field, object value)
		{
			return field.GtEq (value);
		}

		/// <param name="field">Field.</param>
		/// <param name="value">Value.</param>
		public static AggregateHavingExpression operator < (AggregateFunction field, object value)
		{
			return field.Lt (value);
		}

		/// <param name="field">Field.</param>
		/// <param name="value">Value.</param>
		public static AggregateHavingExpression operator <= (AggregateFunction field, object value)
		{
			return field.LtEq (value);
		}

		/// <summary>
		/// Order by asc.
		/// </summary>
		/// <returns>The expression</returns>
		public OrderExpression OrderByAsc ()
		{
			return OrderBy (OrderType.ASC);
		}

		/// <summary>
		/// Order by desc.
		/// </summary>
		/// <returns>The expression</returns>
		public OrderExpression OrderByDesc ()
		{
			return OrderBy (OrderType.DESC);
		}

		private OrderExpression OrderBy (OrderType type)
		{
			OrderExpression exp = new AggregateOrderExpression (this, type);
			return exp;
		}

		/// <summary>
		/// Creates the sql string.
		/// </summary>
		/// <returns>The sql string.</returns>
		/// <param name="factory">Factory.</param>
		/// <param name="fullFieldName">If set to <c>true</c> full field name.</param>
		/// <param name="dataParameters">Data parameters.</param>
		internal abstract string CreateSqlString (CommandFactory factory, bool fullFieldName, out DataParameter[] dataParameters);

		/// <summary>
		/// Count Function.
		/// </summary>
		public static AggregateFunction Count ()
		{
			return new CountAllFunction ();
		}

		/// <summary>
		/// Count Function in the specified expression.
		/// </summary>
		/// <returns>The function</returns>
		/// <param name="expression">Expression.</param>
		public static AggregateFunction Count (QueryExpression expression)
		{
			if (expression == null) {
				throw new ArgumentNullException ("expression");
			}
			return new ConditionCountFunction (expression.TableMapping, expression, null, false);
		}

		/// <summary>
		/// Count Function in the specified expression, fieldInfo and isDistinct.
		/// </summary>
		/// <returns>The function</returns>
		/// <param name="expression">Expression.</param>
		/// <param name="fieldInfo">Field info.</param>
		/// <param name="isDistinct">If set to <c>true</c> is distinct.</param>
		public static AggregateFunction Count (QueryExpression expression, DataFieldInfo fieldInfo, bool isDistinct)
		{
			if (expression == null) {
				throw new ArgumentNullException ("expression");
			}
			if (Object.Equals (fieldInfo, null)) {
				throw new ArgumentNullException ("fieldInfo");
			}
			return new ConditionCountFunction (fieldInfo.TableMapping, expression, fieldInfo, isDistinct);
		}

		/// <summary>
		/// Count Function in the specified expression and fieldInfo.
		/// </summary>
		/// <returns>The function</returns>
		/// <param name="expression">Expression.</param>
		/// <param name="fieldInfo">Field info.</param>
		public static AggregateFunction Count (QueryExpression expression, DataFieldInfo fieldInfo)
		{
			return Count (expression, fieldInfo, false);
		}

		/// <summary>
		/// Count Function in the fieldInfo.
		/// </summary>
		/// <returns>The function</returns>
		/// <param name="fieldInfo">Field info.</param>
		public static AggregateFunction Count (DataFieldInfo fieldInfo)
		{
			return Count (fieldInfo, false);
		}

		/// <summary>
		/// Count Function in the specified fieldInfo and isDistinct.
		/// </summary>
		/// <returns>The function</returns>
		/// <param name="fieldInfo">Field info.</param>
		/// <param name="isDistinct">If set to <c>true</c> is distinct.</param>
		public static AggregateFunction Count (DataFieldInfo fieldInfo, bool isDistinct)
		{
			if (Object.Equals (fieldInfo, null)) {
				throw new ArgumentNullException ("fieldInfo");
			}
			return new CountFunction (fieldInfo.TableMapping, fieldInfo, isDistinct);
		}

		/// <summary>
		/// Sum Function in the fieldInfo.
		/// </summary>
		/// <returns>The function</returns>
		/// <param name="fieldInfo">Field info.</param>
		public static AggregateFunction Sum (DataFieldInfo fieldInfo)
		{
			return Sum (fieldInfo, false);
		}

		/// <summary>
		/// Sum Function in the specified fieldInfo and isDistinct.
		/// </summary>
		/// <returns>The function</returns>
		/// <param name="fieldInfo">Field info.</param>
		/// <param name="isDistinct">If set to <c>true</c> is distinct.</param>
		public static AggregateFunction Sum (DataFieldInfo fieldInfo, bool isDistinct)
		{
			if (Object.Equals (fieldInfo, null)) {
				throw new ArgumentNullException ("fieldInfo");
			}
			return new SumFunction (fieldInfo.TableMapping, fieldInfo, isDistinct);
		}

		/// <summary>
		/// Sum Function in the specified expression and fieldInfo.
		/// </summary>
		/// <returns>The function</returns>
		/// <param name="expression">Expression.</param>
		/// <param name="fieldInfo">Field info.</param>
		public static AggregateFunction Sum (QueryExpression expression, DataFieldInfo fieldInfo)
		{
			return Sum (expression, fieldInfo, false);
		}

		/// <summary>
		/// Sum Function in the specified expression, fieldInfo and isDistinct.
		/// </summary>
		/// <returns>The function</returns>
		/// <param name="expression">Expression.</param>
		/// <param name="fieldInfo">Field info.</param>
		/// <param name="isDistinct">If set to <c>true</c> is distinct.</param>
		public static AggregateFunction Sum (QueryExpression expression, DataFieldInfo fieldInfo, bool isDistinct)
		{
			if (expression == null) {
				throw new ArgumentNullException ("expression");
			}
			if (Object.Equals (fieldInfo, null)) {
				throw new ArgumentNullException ("fieldInfo");
			}
			return new ConditionSumFunction (fieldInfo.TableMapping, expression, fieldInfo, isDistinct);
		}

		/// <summary>
		/// Avg Function in the fieldInfo.
		/// </summary>
		/// <returns>The function</returns>
		/// <param name="fieldInfo">Field info.</param>
		public static AggregateFunction Avg (DataFieldInfo fieldInfo)
		{
			return Avg (fieldInfo, false);
		}

		/// <summary>
		/// Avg Function in the specified fieldInfo and isDistinct.
		/// </summary>
		/// <returns>The function</returns>
		/// <param name="fieldInfo">Field info.</param>
		/// <param name="isDistinct">If set to <c>true</c> is distinct.</param>
		public static AggregateFunction Avg (DataFieldInfo fieldInfo, bool isDistinct)
		{
			if (Object.Equals (fieldInfo, null)) {
				throw new ArgumentNullException ("fieldInfo");
			}
			return new AvgFunction (fieldInfo.TableMapping, fieldInfo, isDistinct);
		}

		/// <summary>
		/// Sum Function in the specified expression and fieldInfo.
		/// </summary>
		/// <returns>The function</returns>
		/// <param name="expression">Expression.</param>
		/// <param name="fieldInfo">Field info.</param>
		public static AggregateFunction Avg (QueryExpression expression, DataFieldInfo fieldInfo)
		{
			return Avg (expression, fieldInfo, false);
		}

		/// <summary>
		/// Avg Function in the specified expression, fieldInfo and isDistinct.
		/// </summary>
		/// <returns>The function</returns>
		/// <param name="expression">Expression.</param>
		/// <param name="fieldInfo">Field info.</param>
		/// <param name="isDistinct">If set to <c>true</c> is distinct.</param>
		public static AggregateFunction Avg (QueryExpression expression, DataFieldInfo fieldInfo, bool isDistinct)
		{
			if (expression == null) {
				throw new ArgumentNullException ("expression");
			}
			if (Object.Equals (fieldInfo, null)) {
				throw new ArgumentNullException ("fieldInfo");
			}
			return new ConditionAvgFunction (fieldInfo.TableMapping, expression, fieldInfo, isDistinct);
		}

		/// <summary>
		/// Max Function in the specified fieldInfo.
		/// </summary>
		/// <param name="fieldInfo">Field info.</param>
		public static AggregateFunction Max (DataFieldInfo fieldInfo)
		{
			if (Object.Equals (fieldInfo, null)) {
				throw new ArgumentNullException ("fieldInfo");
			}
			return new MaxFunction (fieldInfo.TableMapping, fieldInfo);
		}

		/// <summary>
		/// Max Function in the specified expression and fieldInfo.
		/// </summary>
		/// <param name="expression">Expression.</param>
		/// <param name="fieldInfo">Field info.</param>
		public static AggregateFunction Max (QueryExpression expression, DataFieldInfo fieldInfo)
		{
			if (Object.Equals (fieldInfo, null)) {
				throw new ArgumentNullException ("fieldInfo");
			}
			return new ConditionMaxFunction (fieldInfo.TableMapping, expression, fieldInfo);
		}

		/// <summary>
		/// Minimum Function in the specified fieldInfo.
		/// </summary>
		/// <param name="fieldInfo">Field info.</param>
		public static AggregateFunction Min (DataFieldInfo fieldInfo)
		{
			if (Object.Equals (fieldInfo, null)) {
				throw new ArgumentNullException ("fieldInfo");
			}
			return new MinFunction (fieldInfo.TableMapping, fieldInfo);
		}

		/// <summary>
		/// Minimum Function in the specified expression and fieldInfo.
		/// </summary>
		/// <param name="expression">Expression.</param>
		/// <param name="fieldInfo">Field info.</param>
		public static AggregateFunction Min (QueryExpression expression, DataFieldInfo fieldInfo)
		{
			if (Object.Equals (fieldInfo, null)) {
				throw new ArgumentNullException ("fieldInfo");
			}
			return new ConditionMinFunction (fieldInfo.TableMapping, expression, fieldInfo);
		}

//		internal virtual AggregateFunction CreateAliasTableFunction(string aliasTableName)
//		{
//			return this;
//		}

		/// <summary>
		/// Determines whether the specified <see cref="Light.Data.AggregateFunction"/> is equal to the current <see cref="Light.Data.AggregateFunction"/>.
		/// </summary>
		/// <param name="target">The <see cref="Light.Data.AggregateFunction"/> to compare with the current <see cref="Light.Data.AggregateFunction"/>.</param>
		/// <returns><c>true</c> if the specified <see cref="Light.Data.AggregateFunction"/> is equal to the current
		/// <see cref="Light.Data.AggregateFunction"/>; otherwise, <c>false</c>.</returns>
		public virtual bool Equals (AggregateFunction target)
		{
			if (Object.Equals (target, null)) {
				return false;
			}
			if (Object.ReferenceEquals (this, target)) {
				return true;
			}
			else {
				if (this.GetType () == target.GetType ()) {
					return EqualsDetail (target);
				}
				else {
					return false;
				}
			}
		}

		/// <summary>
		/// Equalses the detail.
		/// </summary>
		/// <returns><c>true</c>, if detail was equalsed, <c>false</c> otherwise.</returns>
		/// <param name="function">Function.</param>
		protected virtual bool EqualsDetail (AggregateFunction function)
		{
			return Object.Equals (this.TableMapping, function.TableMapping);
		}

		/// <summary>
		/// Determines whether the specified <see cref="System.Object"/> is equal to the current <see cref="Light.Data.AggregateFunction"/>.
		/// </summary>
		/// <param name="obj">The <see cref="System.Object"/> to compare with the current <see cref="Light.Data.AggregateFunction"/>.</param>
		/// <returns><c>true</c> if the specified <see cref="System.Object"/> is equal to the current
		/// <see cref="Light.Data.AggregateFunction"/>; otherwise, <c>false</c>.</returns>
		public override bool Equals (object obj)
		{
			return object.ReferenceEquals (this, obj);
		}

		/// <summary>
		/// Serves as a hash function for a <see cref="Light.Data.AggregateFunction"/> object.
		/// </summary>
		/// <returns>A hash code for this instance that is suitable for use in hashing algorithms and data structures such as a hash table.</returns>
		public override int GetHashCode ()
		{
			return base.GetHashCode ();
		}
	}
}
