using GalaxyCheck.Internal;
using System;
using System.Linq.Expressions;

namespace GalaxyCheck.Gens.AutoGenHelpers
{
    internal record PathResolutionError(
        string Expression,
        string Error);

    internal class PathResolver
    {
        public static Either<string, PathResolutionError> FromExpression<T, TMember>(
            Expression<Func<T, TMember>> expression)
        {
            return new Left<string, PathResolutionError>("$.Property");
        }
    }
}
