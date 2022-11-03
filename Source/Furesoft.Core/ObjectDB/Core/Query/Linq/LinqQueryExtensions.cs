﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Furesoft.Core.ObjectDB.Api.Query;
using Furesoft.Core.ObjectDB.Exceptions;

namespace Furesoft.Core.ObjectDB.Core.Query.Linq;

	internal static class LinqQueryExtensions
	{
		public static ILinqQuery<TSource> Where<TSource>(this ILinqQuery<TSource> self,
														 Expression<Func<TSource, bool>> expression)
		{
			return Process(self,
						   query => new WhereClauseVisitor().Process(expression),
						   data => data.UnoptimizedWhere(expression.Compile())
				);
		}

		public static int Count<TSource>(this ILinqQuery<TSource> self)
		{
			if (self == null)
				throw new ArgumentNullException("self");

			return self is LinqQuery<TSource> query
				? query.Count
				: Enumerable.Count(self);
		}

		private static ILinqQuery<TSource> Process<TSource>(
			ILinqQuery<TSource> query,
			Func<LinqQuery<TSource>, IQueryBuilderRecord> queryProcessor,
			Func<ILinqQueryInternal<TSource>, IEnumerable<TSource>> fallbackProcessor)
		{
			if (query == null)
				throw new ArgumentNullException("query");

			if (!(query is LinqQuery<TSource> candidate))
			{
				return
					new UnoptimizedQuery<TSource>(fallbackProcessor((ILinqQueryInternal<TSource>)EnsureLinqQuery(query)));
			}
			try
			{
				var record = queryProcessor(candidate);
				return new LinqQuery<TSource>(candidate, record);
			}
			catch (LinqQueryException)
			{
				return new UnoptimizedQuery<TSource>(fallbackProcessor(candidate));
			}
		}

		private static ILinqQuery<TSource> EnsureLinqQuery<TSource>(ILinqQuery<TSource> query)
		{
			return !(query is PlaceHolderQuery<TSource> placeHolderQuery)
				? query
				: new LinqQuery<TSource>(placeHolderQuery.QueryFactory);
		}

		private static ILinqQuery<TSource> ProcessOrderBy<TSource, TKey>(
			ILinqQuery<TSource> query,
			ExpressionQueryBuilder visitor,
			Expression<Func<TSource, TKey>> expression,
			Func<ILinqQueryInternal<TSource>, IEnumerable<TSource>> fallbackProcessor)
		{
			return Process(query, q => visitor.Process(expression), fallbackProcessor);
		}

		public static ILinqQuery<TSource> OrderBy<TSource, TKey>(this ILinqQuery<TSource> self,
																 Expression<Func<TSource, TKey>> expression)
		{
			return ProcessOrderBy(self, new OrderByAscendingClauseVisitor(), expression,
								  data => data.OrderBy(expression.Compile()));
		}

		public static ILinqQuery<TSource> OrderByDescending<TSource, TKey>(this ILinqQuery<TSource> self,
																		   Expression<Func<TSource, TKey>> expression)
		{
			return ProcessOrderBy(self, new OrderByDescendingClauseVisitor(), expression,
								  data => data.OrderByDescending(expression.Compile()));
		}

		public static ILinqQuery<TSource> ThenBy<TSource, TKey>(this ILinqQuery<TSource> self,
																Expression<Func<TSource, TKey>> expression)
		{
			return ProcessOrderBy(self, new OrderByAscendingClauseVisitor(), expression,
								  data => data.UnoptimizedThenBy(expression.Compile()));
		}

		public static ILinqQuery<TSource> ThenByDescending<TSource, TKey>(this ILinqQuery<TSource> self,
																		  Expression<Func<TSource, TKey>> expression)
		{
			return ProcessOrderBy(self, new OrderByDescendingClauseVisitor(), expression,
								  data => data.UnoptimizedThenByDescending(expression.Compile()));
		}

		public static ILinqQuery<TRet> Select<TSource, TRet>(this ILinqQuery<TSource> self, Func<TSource, TRet> selector)
		{
			if (self is PlaceHolderQuery<TSource> placeHolderQuery)
				return new LinqQuery<TRet>(placeHolderQuery.QueryFactory);

			return new UnoptimizedQuery<TRet>(Enumerable.Select(self, selector));
		}

		public static ILinqQueryable<TSource> AsQueryable<TSource>(this ILinqQuery<TSource> self)
		{
			return new LinqQueryable<TSource>(self);
		}
	}