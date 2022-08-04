using GalaxyCheck.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace GalaxyCheck.Gens.ReflectedGenHelpers
{
    internal class PathResolver
    {
        public static Either<(string path, string expression), string> FromExpression<T, TMember>(
            Expression<Func<T, TMember>> expression)
        {
            var path = GetPath(expression);

            if (path == null)
            {
                return new Right<(string path, string expression), string>(expression.ToString());
            }

            return new Left<(string path, string expression), string>(($"$.{path}", expression.ToString()));
        }

        private static string? GetPath<T, TMember>(Expression<Func<T, TMember>> expr)
        {
            var stack = new Stack<string>();

            MemberExpression? me;
            switch (expr.Body.NodeType)
            {
                case ExpressionType.Convert:
                case ExpressionType.ConvertChecked:
                    me = ((expr.Body is UnaryExpression ue) ? ue.Operand : null) as MemberExpression;
                    break;
                default:
                    me = expr.Body as MemberExpression;
                    break;
            }

            while (me != null)
            {
                stack.Push(me.Member.Name);
                me = me.Expression as MemberExpression;
            }

            if (stack.Any() == false)
            {
                return null;
            }

            return string.Join(".", stack.ToArray());
        }
    }
}
