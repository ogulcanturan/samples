using Microsoft.EntityFrameworkCore;

namespace Sample.Api
{
    public static class QueryableExtensions
    {
        public const string WithNoLockTag = "WNL";

        public static IQueryable<T> WithNoLock<T>(this IQueryable<T> query) where T : class
        {
            return query.TagWith(WithNoLockTag).AsNoTracking();
        }
    }
}