using System.Linq.Expressions;

namespace Hydra.Utils
{
    public static class ORMHelper
    {
        public static string GetMemberNameToUseThenInclude<T, T2>(Expression<Func<T, object>> expression, Expression<Func<T2, object>> expression2) where T : class where T2 : class
        {
            var str = ReflectionHelper.GetMemberName<T>(expression);

            var str2 = ReflectionHelper.GetMemberName<T2>(expression2);

            return $"{str}.{str2}";
        }
    }
}
