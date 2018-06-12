﻿using System;
using System.Linq.Expressions;
using System.Reflection;

namespace ETModel.Reflection
{
	public static class PropertyUtil
	{
		public static string GetPropertyName<TObject>(this TObject type,
				Expression<Func<TObject, object>> propertyRefExpr)
		{
			return GetPropertyNameCore(propertyRefExpr.Body);
		}

		public static string GetName<TObject>(Expression<Func<TObject, object>> propertyRefExpr)
		{
			return GetPropertyNameCore(propertyRefExpr.Body);
		}

		private static string GetPropertyNameCore(Expression propertyRefExpr)
		{
			MemberExpression memberExpr = propertyRefExpr as MemberExpression;
			if (memberExpr == null)
			{
				UnaryExpression unaryExpr = propertyRefExpr as UnaryExpression;
				if (unaryExpr != null && unaryExpr.NodeType == ExpressionType.Convert)
					memberExpr = unaryExpr.Operand as MemberExpression;
			}

			if (memberExpr != null && memberExpr.Member.MemberType == MemberTypes.Property)
				return memberExpr.Member.Name;

			throw new ArgumentException("No property reference expression was found.",
										nameof(propertyRefExpr));
		}
	}
}