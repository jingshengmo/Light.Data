using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Light.Data
{
	/// <summary>
	/// Query.
	/// </summary>
	public interface IQuery<T> : IEnumerable<T> where T : class
	{
		#region IQuery<T> 成员

		/// <summary>
		/// Reset the specified where expression
		/// </summary>
		IQuery<T> WhereReset ();

		/// <summary>
		/// Set the specified where expression.
		/// </summary>T1,
		/// <param name="expression">Expression.</param>
		IQuery<T> Where (Expression<Func<T, bool>> expression);

		/// <summary>
		/// Catch the specified where expression with and.
		/// </summary>
		/// <param name="expression">Expression.</param>
		IQuery<T> WhereWithAnd (Expression<Func<T, bool>> expression);

		/// <summary>
		/// Catch the specified where expression with or.
		/// </summary>
		/// <param name="expression">Expression.</param>
		IQuery<T> WhereWithOr (Expression<Func<T, bool>> expression);

		/// <summary>
		/// Catch the specified asc order by expression.
		/// </summary>
		/// <param name="expression">Expression.</param>
		IQuery<T> OrderByCatch<TKey> (Expression<Func<T, TKey>> expression);

		/// <summary>
		/// Catch the specified desc order by expression.
		/// </summary>
		/// <param name="expression">Expression.</param>
		IQuery<T> OrderByDescendingCatch<TKey> (Expression<Func<T, TKey>> expression);

		/// <summary>
		/// Set the specified asc order by expression.
		/// </summary>
		/// <param name="expression">Expression.</param>
		/// <typeparam name="TKey">The 1st type parameter.</typeparam>
		IQuery<T> OrderBy<TKey> (Expression<Func<T, TKey>> expression);

		/// <summary>
		/// Set the specified desc order by expression.
		/// </summary>
		/// <param name="expression">Expression.</param>
		/// <typeparam name="TKey">The 1st type parameter.</typeparam>
		IQuery<T> OrderByDescending<TKey> (Expression<Func<T, TKey>> expression);

		/// <summary>
		/// Reset the specified order by expression.
		/// </summary>
		IQuery<T> OrderByReset ();

		/// <summary>
		/// Set order by random.
		/// </summary>
		/// <returns>LEnumerable.</returns>
		IQuery<T> OrderByRandom ();

		/// <summary>
		/// Set take datas count.
		/// </summary>
		/// <param name="count">Count.</param>
		IQuery<T> Take (int count);

		/// <summary>
		/// Set from datas index.
		/// </summary>
		/// <returns>JoinTable.</returns>
		/// <param name="index">Index.</param>
		IQuery<T> Skip (int index);

		/// <summary>
		/// Set take datas range.
		/// </summary>
		/// <param name="from">From.</param>
		/// <param name="to">To.</param>
		IQuery<T> Range (int from, int to);

		/// <summary>
		/// Reset take datas range.
		/// </summary>
		IQuery<T> RangeReset ();

		/// <summary>
		/// Sets page size.
		/// </summary>
		/// <param name="page">Page.</param>
		/// <param name="size">Size.</param>
		IQuery<T> PageSize (int page, int size);

		/// <summary>
		/// Set the SafeLevel.
		/// </summary>
		IQuery<T> SafeMode (SafeLevel level);

		/// <summary>
		/// Sets the distinct.
		/// </summary>
		IQuery<T> SetDistinct (bool distinct);

		/// <summary>
		/// Gets the datas count.
		/// </summary>
		/// <value>The count.</value>
		int Count {
			get;
		}

		/// <summary>
		/// Get single instance.
		/// </summary>
		/// <returns>instance.</returns>
		T First ();

		/// <summary>
		/// Elements at index.
		/// </summary>
		/// <returns>instance.</returns>
		/// <param name="index">Index.</param>
		T ElementAt (int index);

		/// <summary>
		/// Gets the data is exists with query expression.
		/// </summary>
		/// <value><c>true</c> if exists; otherwise, <c>false</c>.</value>
		bool Exists {
			get;
		}

		/// <summary>
		/// To the list.
		/// </summary>
		/// <returns>The list.</returns>
		List<T> ToList ();

		/// <summary>
		/// To the array.
		/// </summary>
		/// <returns>The array.</returns>
		T [] ToArray ();

		#endregion

		/// <summary>
		/// All fileds data insert to the special table K.
		/// </summary>
		/// <typeparam name="K">The 1st type parameter.</typeparam>
		int Insert<K> () where K : class, new();

		/// <summary>
		/// Select fileds data insert to the special table K.
		/// </summary>
		/// <param name="expression">Expression.</param>
		/// <typeparam name="K">The 1st type parameter.</typeparam>
		int SelectInsert<K> (Expression<Func<T, K>> expression) where K : class, new();

		/// <summary>
		/// Update datas.
		/// </summary>
		/// <param name="expression">Expression.</param>
		int Update (Expression<Func<T, T>> expression);

		/// <summary>
		/// Delete datas
		/// </summary>
		int Delete ();

		/// <summary>
		/// Create Selector.
		/// </summary>
		/// <param name="expression">Expression.</param>
		/// <typeparam name="TResult">The 1st type parameter.</typeparam>
		ISelect<TResult> Select<TResult> (Expression<Func<T, TResult>> expression) where TResult : class;

		/// <summary>
		/// Inner Join table with specified queryExpression and onExpression.
		/// </summary>
		/// <param name="queryExpression">Query expression.</param>
		/// <param name="onExpression">On expression.</param>
		/// <typeparam name="T1">The 1st type parameter.</typeparam>
		IJoinTable<T, T1> Join<T1> (Expression<Func<T1, bool>> queryExpression, Expression<Func<T, T1, bool>> onExpression) where T1 : class;

		/// <summary>
		/// Inner Join table with specified specified onExpression.
		/// </summary>
		/// <param name="onExpression">On expression.</param>
		/// <typeparam name="T1">The 1st type parameter.</typeparam>
		IJoinTable<T, T1> Join<T1> (Expression<Func<T, T1, bool>> onExpression) where T1 : class;

		/// <summary>
		/// Left Join table with specified queryExpression and onExpression.
		/// </summary>
		/// <param name="queryExpression">Query expression.</param>
		/// <param name="onExpression">On expression.</param>
		/// <typeparam name="T1">The 1st type parameter.</typeparam>
		IJoinTable<T, T1> LeftJoin<T1> (Expression<Func<T1, bool>> queryExpression, Expression<Func<T, T1, bool>> onExpression) where T1 : class;

		/// <summary>
		/// Left Join table with specified onExpression.
		/// </summary>
		/// <param name="onExpression">On expression.</param>
		/// <typeparam name="T1">The 1st type parameter.</typeparam>
		IJoinTable<T, T1> LeftJoin<T1> (Expression<Func<T, T1, bool>> onExpression) where T1 : class;

		/// <summary>
		/// Right Join table with specified queryExpression and onExpression.
		/// </summary>
		/// <param name="queryExpression">Query expression.</param>
		/// <param name="onExpression">On expression.</param>
		/// <typeparam name="T1">The 1st type parameter.</typeparam>
		IJoinTable<T, T1> RightJoin<T1> (Expression<Func<T1, bool>> queryExpression, Expression<Func<T, T1, bool>> onExpression) where T1 : class;

		/// <summary>
		/// Right Join table with specified onExpression.
		/// </summary>
		/// <param name="onExpression">On expression.</param>
		/// <typeparam name="T1">The 1st type parameter.</typeparam>
		IJoinTable<T, T1> RightJoin<T1> (Expression<Func<T, T1, bool>> onExpression) where T1 : class;

		/// <summary>
		/// Inner Join query data with onExpression.
		/// </summary>
		/// <param name="query">Query.</param>
		/// <param name="onExpression">On expression.</param>
		/// <typeparam name="T1">The 1st type parameter.</typeparam>
		IJoinTable<T, T1> Join<T1> (IQuery<T1> query, Expression<Func<T, T1, bool>> onExpression) where T1 : class;

		/// <summary>
		/// Left Join query data with onExpression.
		/// </summary>
		/// <param name="query">Query.</param>
		/// <param name="onExpression">On expression.</param>
		/// <typeparam name="T1">The 1st type parameter.</typeparam>
		IJoinTable<T, T1> LeftJoin<T1> (IQuery<T1> query, Expression<Func<T, T1, bool>> onExpression) where T1 : class;

		/// <summary>
		/// Right Join query data with onExpression.
		/// </summary>
		/// <param name="query">Query.</param>
		/// <param name="onExpression">On expression.</param>
		/// <typeparam name="T1">The 1st type parameter.</typeparam>
		IJoinTable<T, T1> RightJoin<T1> (IQuery<T1> query, Expression<Func<T, T1, bool>> onExpression) where T1 : class;

		/// <summary>
		/// Inner Join aggregate data with onExpression.
		/// </summary>
		/// <param name="aggregate">Aggregate.</param>
		/// <param name="onExpression">On expression.</param>
		/// <typeparam name="T1">The 1st type parameter.</typeparam>
		IJoinTable<T, T1> Join<T1> (IAggregate<T1> aggregate, Expression<Func<T, T1, bool>> onExpression) where T1 : class;

		/// <summary>
		/// Left Join aggregate data with onExpression.
		/// </summary>
		/// <param name="aggregate">Aggregate.</param>
		/// <param name="onExpression">On expression.</param>
		/// <typeparam name="T1">The 1st type parameter.</typeparam>
		IJoinTable<T, T1> LeftJoin<T1> (IAggregate<T1> aggregate, Expression<Func<T, T1, bool>> onExpression) where T1 : class;

		/// <summary>
		/// Right Join aggregate data with onExpression.
		/// </summary>
		/// <param name="aggregate">Aggregate.</param>
		/// <param name="onExpression">On expression.</param>
		/// <typeparam name="T1">The 1st type parameter.</typeparam>
		IJoinTable<T, T1> RightJoin<T1> (IAggregate<T1> aggregate, Expression<Func<T, T1, bool>> onExpression) where T1 : class;

		/// <summary>
		/// Create group by aggregator
		/// </summary>
		/// <returns>The by.</returns>
		/// <param name="expression">Expression.</param>
		/// <typeparam name="K">The 1st type parameter.</typeparam>
		IAggregate<K> GroupBy<K> (Expression<Func<T, K>> expression) where K : class;

		/// <summary>
		/// Select special filed datas
		/// </summary>
		/// <returns>The single field.</returns>
		/// <param name="expression">Expression.</param>
		/// <typeparam name="K">The 1st type parameter.</typeparam>
		IEnumerable<K> QuerySingleField<K> (Expression<Func<T, K>> expression);

		/// <summary>
		/// Select special filed data list
		/// </summary>
		/// <returns>The single field list.</returns>
		/// <param name="expression">Expression.</param>
		/// <typeparam name="K">The 1st type parameter.</typeparam>
		List<K> QuerySingleFieldList<K> (Expression<Func<T, K>> expression);

	}
}

