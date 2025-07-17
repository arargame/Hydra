using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Hydra.Utils
{
    public static class ExpressionHelper
    {
        public static string? GetPropertyPath<T>(Expression<Func<T, object?>>? expression)
        {
            if (expression == null)
                return null;

            Expression body = expression.Body;

            // Eğer Convert varsa kaldır (boxing gibi durumlar için)
            if (body is UnaryExpression unaryExpression && unaryExpression.NodeType == ExpressionType.Convert)
                body = unaryExpression.Operand;

            var path = new StringBuilder();

            while (body is MemberExpression memberExpression)
            {
                if (path.Length > 0)
                    path.Insert(0, ".");

                path.Insert(0, memberExpression.Member.Name);
                body = memberExpression.Expression;
            }

            return path.ToString();
        }

        public static string GetExpressionBody<T>(Expression<Func<T, bool>> expression)
        {
            return expression.Body.ToString(); 
        }

        public static string GetExpressionBody<T, TResult>(Expression<Func<T, TResult>> expression)
        {
            return expression.Body.ToString();
        }


        public static string GetFilterDescription<T>(Expression<Func<T, bool>>? filter)
        {
            if (filter == null)
                return "[No Filter]";

            return filter.Body.ToString();
        }


    }
}
