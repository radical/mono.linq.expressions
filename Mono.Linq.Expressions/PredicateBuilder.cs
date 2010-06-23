//
// PredicateBuilder.cs
//
// Author:
//   Jb Evain (jbevain@novell.com)
//
// (C) 2010 Novell, Inc. (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
// inspired by http://www.albahari.com/nutshell/predicatebuilder.aspx
//

using System;
using System.Linq.Expressions;

namespace Mono.Linq.Expressions {

	public static class PredicateBuilder {

		public static Expression<Func<T, bool>> True<T> ()
		{
			return Expression.Lambda<Func<T, bool>> (Expression.Constant (true), Expression.Parameter (typeof (T)));
		}

		public static Expression<Func<T, bool>> False<T> ()
		{
			return Expression.Lambda<Func<T, bool>> (Expression.Constant (false), Expression.Parameter (typeof (T)));
		}

		public static Expression<Func<T, bool>> OrElse<T> (this Expression<Func<T, bool>> self, Expression<Func<T, bool>> expression)
		{
			return Combine (self, expression, Expression.OrElse);
		}

		public static Expression<Func<T, bool>> AndAlso<T> (this Expression<Func<T, bool>> self, Expression<Func<T, bool>> expression)
		{
			return Combine (self, expression, Expression.AndAlso);
		}

		static Expression<Func<T, bool>> Combine<T> (Expression<Func<T, bool>> self, Expression<Func<T, bool>> expression, Func<Expression, Expression, Expression> selector)
		{
			CheckSelfAndExpression (self, expression);

			var parameter = CreateParameterFrom (self);

			return Expression.Lambda<Func<T, bool>> (
				selector (
					RewriteFor (self.Body, parameter),
					RewriteFor (expression.Body, parameter)),
				parameter);
		}

		static Expression RewriteFor (Expression expression, ParameterExpression parameter)
		{
			return new ParameterRewriter (parameter).Visit (expression);
		}

		class ParameterRewriter : ExpressionVisitor {

			readonly ParameterExpression parameter;

			public ParameterRewriter (ParameterExpression parameter)
			{
				this.parameter = parameter;
			}

			protected override Expression VisitParameter (ParameterExpression expression)
			{
				return parameter;
			}
		}

		static ParameterExpression CreateParameterFrom<T> (Expression<Func<T, bool>> left)
		{
			var template = left.Parameters [0];

			return Expression.Parameter (template.Type, template.Name);
		}

		static void CheckSelfAndExpression<T> (Expression<Func<T, bool>> self, Expression<Func<T, bool>> expression)
		{
			if (self == null)
				throw new ArgumentNullException ("self");
			if (expression == null)
				throw new ArgumentNullException ("expression");
		}
	}
}
