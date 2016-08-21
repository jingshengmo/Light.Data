﻿using System;
using System.Collections;
using System.Collections.Generic;

namespace Light.Data
{
	/// <summary>
	/// LSelectable.
	/// </summary>
	public class LightSelect<K> : ISelect<K> where K : class
	{
		readonly Type _type;

		readonly QueryExpression _query;

		readonly OrderExpression _order;

		readonly Region _region;

		readonly DataContext _context;

		readonly SafeLevel _level;

		readonly Delegate _dele;

		readonly ISelector _selector;

		internal LightSelect (DataContext context, Delegate dele, ISelector selector, Type type, QueryExpression query, OrderExpression order, Region region, SafeLevel level)
		{
			_context = context;
			_dele = dele;
			_selector = selector;
			_type = type;
			_query = query;
			_order = order;
			_region = region;
			_level = level;
		}

		public IEnumerator<K> GetEnumerator ()
		{
			foreach (object item in _context.QueryMappingData (_type, _selector, _query, _order, _region, _level)) {
				object obj = _dele.DynamicInvoke (item);
				yield return obj as K;
			}
		}

		IEnumerator IEnumerable.GetEnumerator ()
		{
			foreach (object item in _context.QueryMappingData (_type, _selector, _query, _order, _region, _level)) {
				object obj = _dele.DynamicInvoke (item);
				yield return obj;
			}
		}

		public List<K> ToList ()
		{
			List<K> list = new List<K> ();
			foreach (object item in _context.QueryMappingData (_type, _selector, _query, _order, _region, _level)) {
				object obj = _dele.DynamicInvoke (item);
				list.Add (obj as K);
			}
			return list;
		}
	}
}

