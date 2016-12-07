﻿using System;
using System.Text;

namespace Light.Data
{
	class AggregateJoinModel : IJoinModel
	{
		readonly JoinConnect _connect;

		public JoinConnect Connect {
			get {
				return _connect;
			}
		}

		readonly AggregateModel _model = null;

		public AggregateModel Model {
			get {
				return _model;
			}
		}

		readonly IJoinTableMapping _joinMapping = null;

		public IJoinTableMapping JoinMapping {
			get {
				return _joinMapping;
			}
		}

		readonly QueryExpression _query = null;

		public QueryExpression Query {
			get {
				return _query;
			}
		}

		readonly OrderExpression _order = null;

		public OrderExpression Order {
			get {
				return _order;
			}
		}

		readonly QueryExpression _having = null;

		public QueryExpression Having {
			get {
				return _having;
			}
		}

		string _aliasTableName;

		public string AliasTableName {
			get {
				return _aliasTableName;
			}
		}

		public AggregateJoinModel (AggregateModel model, string aliasTableName, JoinConnect connect, QueryExpression query, QueryExpression having, OrderExpression order)
		{
			this._model = model;
			this._connect = connect;
			this._query = query;
			this._having = having;
			this._order = order;
			this._aliasTableName = aliasTableName;
			this._joinMapping = model.OutputMapping;
		}

		public string CreateSqlString (CommandFactory factory, CreateSqlState state)
		{
			StringBuilder sb = new StringBuilder ();

			CommandData command = factory.CreateAggregateTableCommand (_model.EntityMapping, _model. GetAggregateDataFieldInfos (), _query, _having, null, null, state);
			//string sql = string.Concat ("(", command.CommandText, ")");
			string aliasName = _aliasTableName ?? _model.EntityMapping.TableName;
			sb.Append (factory.CreateAliasQuerySql (command.CommandText, aliasName));
			return sb.ToString ();
		}
	}
}

