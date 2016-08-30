﻿using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Light.Data
{
	public class LightJoinTable<T, T1, T2> : IJoinTable<T, T1, T2>
		where T : class//, new()
		where T1 : class//, new()
		where T2 : class//, new()
	{
		QueryExpression _query;

		internal QueryExpression Query {
			get {
				return _query;
			}
		}

		OrderExpression _order;

		internal OrderExpression Order {
			get {
				return _order;
			}
		}

		Region _region;

		internal Region Region {
			get {
				return _region;
			}
		}

		DataContext _context;

		internal DataContext Context {
			get {
				return _context;
			}
		}

		SafeLevel _level = SafeLevel.None;

		internal SafeLevel Level {
			get {
				return _level;
			}
		}

		List<IJoinModel> _modelList = new List<IJoinModel> ();

		internal List<IJoinModel> ModelList {
			get {
				return _modelList;
			}
		}

		List<IMap> _maps = new List<IMap> ();

		internal List<IMap> Maps {
			get {
				return _maps;
			}
		}

		internal LightJoinTable (LightJoinTable<T, T1> query1, JoinType joinType, Expression<Func<T2, bool>> queryExpression, Expression<Func<T, T1, T2, bool>> onExpression)
		{
			_query = query1.Query;
			_order = query1.Order;
			_region = query1.Region;
			_context = query1.Context;
			_level = query1.Level;
			_modelList.AddRange (query1.ModelList);
			_maps.AddRange (query1.Maps);
			DataEntityMapping entityMapping = DataEntityMapping.GetEntityMapping (typeof (T2));
			_maps.Add (entityMapping.GetRelationMap ());
			QueryExpression subQuery;
			DataFieldExpression on;
			if (queryExpression != null) {
				subQuery = LambdaExpressionExtend.ResolveLambdaQueryExpression (queryExpression);
			}
			else {
				subQuery = null;
			}
			if (onExpression != null) {
				on = LambdaExpressionExtend.ResolvelambdaOnExpression (onExpression, _maps);
			}
			else {
				throw new LightDataException (RE.OnExpressionNotExists);
			}

			JoinConnect connect = new JoinConnect (joinType, on);
			EntityJoinModel model = new EntityJoinModel (entityMapping, "T2", connect, subQuery, null);
			_modelList.Add (model);
		}

		internal LightJoinTable (LightJoinTable<T, T1> query1, JoinType joinType, IAggregate<T2> aggregate, Expression<Func<T, T1, T2, bool>> onExpression)
		{
			_query = null;
			_order = null;
			_region = query1.Region;
			_context = query1.Context;
			_level = query1.Level;
			_modelList.AddRange (query1.ModelList);
			_maps.AddRange (query1.Maps);
			AggregateGroupData data = aggregate.GetGroupData ();
			_maps.Add (new AggregateMap (data.Model));
			DataFieldExpression on;

			if (onExpression != null) {
				on = LambdaExpressionExtend.ResolvelambdaOnExpression (onExpression, _maps);
			}
			else {
				throw new LightDataException (RE.OnExpressionNotExists);
			}

			JoinConnect connect = new JoinConnect (joinType, on);
			AggregateJoinModel model = new AggregateJoinModel (data.Model, "T2", connect, data.Query, data.Having, data.Order);
			_modelList.Add (model);
		}

		public IJoinTable<T, T1, T2, T3> Join<T3> (Expression<Func<T3, bool>> queryExpression, Expression<Func<T, T1, T2, T3, bool>> onExpression) where T3 : class//, new()
		{
			return new LightJoinTable<T, T1, T2, T3> (this, JoinType.InnerJoin, queryExpression, onExpression);
		}

		public IJoinTable<T, T1, T2, T3> Join<T3> (Expression<Func<T, T1, T2, T3, bool>> onExpression) where T3 : class//, new()
		{
			return new LightJoinTable<T, T1, T2, T3> (this, JoinType.InnerJoin, null, onExpression);
		}

		public IJoinTable<T, T1, T2, T3> LeftJoin<T3> (Expression<Func<T3, bool>> queryExpression, Expression<Func<T, T1, T2, T3, bool>> onExpression) where T3 : class//, new()
		{
			return new LightJoinTable<T, T1, T2, T3> (this, JoinType.LeftJoin, queryExpression, onExpression);
		}

		public IJoinTable<T, T1, T2, T3> LeftJoin<T3> (Expression<Func<T, T1, T2, T3, bool>> onExpression) where T3 : class//, new()
		{
			return new LightJoinTable<T, T1, T2, T3> (this, JoinType.LeftJoin, null, onExpression);
		}

		public IJoinTable<T, T1, T2, T3> RightJoin<T3> (Expression<Func<T3, bool>> queryExpression, Expression<Func<T, T1, T2, T3, bool>> onExpression) where T3 : class//, new()
		{
			return new LightJoinTable<T, T1, T2, T3> (this, JoinType.RightJoin, queryExpression, onExpression);
		}

		public IJoinTable<T, T1, T2, T3> RightJoin<T3> (Expression<Func<T, T1, T2, T3, bool>> onExpression) where T3 : class//, new()
		{
			return new LightJoinTable<T, T1, T2, T3> (this, JoinType.RightJoin, null, onExpression);
		}

		/// <summary>
		/// Reset the specified where expression
		/// </summary>
		/// <returns>LEnumerable.</returns>
		public IJoinTable<T, T1, T2> WhereReset ()
		{
			_query = null;
			return this;
		}

		/// <summary>
		/// Where the specified expression.
		/// </summary>T1,
		/// <param name="expression">Expression.</param>
		public IJoinTable<T, T1, T2> Where (Expression<Func<T, T1, T2, bool>> expression)
		{
			var queryExpression = LambdaExpressionExtend.ResolveLambdaMutliQueryExpression (expression, _maps);
			_query = queryExpression;
			return this;
		}

		/// <summary>
		/// Catch the specified where expression with and.
		/// </summary>
		/// <returns>LEnumerable.</returns>
		/// <param name="expression">Expression.</param>
		public IJoinTable<T, T1, T2> WhereWithAnd (Expression<Func<T, T1, T2, bool>> expression)
		{
			var queryExpression = LambdaExpressionExtend.ResolveLambdaMutliQueryExpression (expression, _maps);
			_query = QueryExpression.And (_query, queryExpression);
			return this;
		}

		/// <summary>
		/// Catch the specified where expression with or.
		/// </summary>
		/// <returns>LEnumerables.</returns>
		/// <param name="expression">Expression.</param>
		public IJoinTable<T, T1, T2> WhereWithOr (Expression<Func<T, T1, T2, bool>> expression)
		{
			var queryExpression = LambdaExpressionExtend.ResolveLambdaMutliQueryExpression (expression, _maps);
			_query = QueryExpression.Or (_query, queryExpression);
			return this;
		}

		/// <summary>
		/// Catch the specified order by expression.
		/// </summary>
		/// <returns>LEnumerable.</returns>
		/// <param name="expression">Expression.</param>
		public IJoinTable<T, T1, T2> OrderByCatch<TKey> (Expression<Func<T, T1, T2, TKey>> expression)
		{
			var orderExpression = LambdaExpressionExtend.ResolveLambdaMutliOrderByExpression (expression, OrderType.ASC, _maps);
			_order = OrderExpression.Catch (_order, orderExpression);
			return this;
		}

		/// <summary>
		/// Catch the specified order by expression.
		/// </summary>
		/// <returns>LEnumerable.</returns>
		/// <param name="expression">Expression.</param>
		public IJoinTable<T, T1, T2> OrderByDescendingCatch<TKey> (Expression<Func<T, T1, T2, TKey>> expression)
		{
			var orderExpression = LambdaExpressionExtend.ResolveLambdaMutliOrderByExpression (expression, OrderType.DESC, _maps);
			_order = OrderExpression.Catch (_order, orderExpression);
			return this;
		}

		/// <summary>
		/// Orders the by.
		/// </summary>
		/// <returns>The by.</returns>
		/// <param name="expression">Expression.</param>
		/// <typeparam name="TKey">The 1st type parameter.</typeparam>
		public IJoinTable<T, T1, T2> OrderBy<TKey> (Expression<Func<T, T1, T2, TKey>> expression)
		{
			var orderExpression = LambdaExpressionExtend.ResolveLambdaMutliOrderByExpression (expression, OrderType.ASC, _maps);
			_order = orderExpression;
			return this;
		}

		/// <summary>
		/// Orders the by.
		/// </summary>
		/// <returns>The by.</returns>
		/// <param name="expression">Expression.</param>
		/// <typeparam name="TKey">The 1st type parameter.</typeparam>
		public IJoinTable<T, T1, T2> OrderByDescending<TKey> (Expression<Func<T, T1, T2, TKey>> expression)
		{
			var orderExpression = LambdaExpressionExtend.ResolveLambdaMutliOrderByExpression (expression, OrderType.DESC, _maps);
			_order = orderExpression;
			return this;
		}

		/// <summary>
		/// Reset the specified order by expression.
		/// </summary>
		/// <returns>LEnumerable.</returns>
		public IJoinTable<T, T1, T2> OrderByReset ()
		{
			_order = null;
			return this;
		}

		/// <summary>
		/// Set order by random.
		/// </summary>
		/// <returns>LEnumerable.</returns>
		//public IJoinTable<T, T1, T2> OrderByRandom ()
		//{
		//	_order = new RandomOrderExpression (DataEntityMapping.GetEntityMapping (typeof (T)));
		//	return this;
		//}

		/// <summary>
		/// Take the datas count.
		/// </summary>
		/// <returns>LEnumerable.</returns>
		/// <param name="count">Count.</param>
		public IJoinTable<T, T1, T2> Take (int count)
		{
			int start;
			int size = count;
			if (_region == null) {
				start = 0;
			}
			else {
				start = _region.Start;
			}
			_region = new Region (start, size);
			//if (_region == null) {
			//	_region = new Region (0, count);
			//}
			//else {
			//	_region.Size = count;
			//}
			return this;
		}

		/// <summary>
		/// Skip the specified index.
		/// </summary>
		/// <returns>LEnumerable.</returns>
		/// <param name="index">Index.</param>
		public IJoinTable<T, T1, T2> Skip (int index)
		{
			int start = index;
			int size;
			if (_region == null) {
				size = int.MaxValue;
			}
			else {
				size = _region.Size;
			}
			_region = new Region (start, size);
			//if (_region == null) {
			//	_region = new Region (index, int.MaxValue);
			//}
			//else {
			//	_region.Start = index;
			//}
			return this;
		}

		/// <summary>
		/// Range the specified from and to.
		/// </summary>
		/// <returns>LEnumerable.</returns>
		/// <param name="from">From.</param>
		/// <param name="to">To.</param>
		public IJoinTable<T, T1, T2> Range (int from, int to)
		{
			int start = from;
			int size = to - from;
			_region = new Region (start, size);
			//if (_region == null) {
			//	_region = new Region (start, size);
			//}
			//else {
			//	_region.Start = start;
			//	_region.Size = size;
			//}
			return this;
		}

		/// <summary>
		/// reset the range
		/// </summary>
		/// <returns>LEnumerable.</returns>
		public IJoinTable<T, T1, T2> RangeReset ()
		{
			_region = null;
			return this;
		}

		/// <summary>
		/// Sets page size.
		/// </summary>
		/// <returns>LEnumerable.</returns>
		/// <param name="page">Page.</param>
		/// <param name="size">Size.</param>
		public IJoinTable<T, T1, T2> PageSize (int page, int size)
		{
			if (page < 1) {
				throw new ArgumentOutOfRangeException (nameof (page));
			}
			if (size < 1) {
				throw new ArgumentOutOfRangeException (nameof (size));
			}
			page--;
			int start = page * size;
			_region = new Region (start, size);
			//if (_region == null) {
			//	_region = new Region (start, size);
			//}
			//else {
			//	_region.Start = start;
			//	_region.Size = size;
			//}
			return this;
		}

		/// <summary>
		/// Safes the mode.
		/// </summary>
		/// <returns>LEnumerable.</returns>
		/// <param name="level">Level.</param>
		public IJoinTable<T, T1, T2> SafeMode (SafeLevel level)
		{
			_level = level;
			return this;
		}

		public ISelect<TResult> Select<TResult> (Expression<Func<T, T1, T2, TResult>> expression) where TResult : class
		{
			JoinSelector selector = LambdaExpressionExtend.CreateMutliSelector (expression, _maps) as JoinSelector;
			Delegate dele = expression.Compile ();
			LightJoinSelect<TResult> selectable = new LightJoinSelect<TResult> (_context, dele, selector, _modelList.ToArray (), _query, _order, _region, _level);
			return selectable;
		}
	}
}

