﻿using System.Linq;
using Fuxion.Factories;
using Fuxion.Identity;
namespace System.Collections.Generic
{
    public static class System_Extensions
    {
        public static IEnumerable<TSource> AuthorizedTo<TSource>(this IEnumerable<TSource> source, params IFunction[] functions)
        {
            var im = Factory.Get<IdentityManager>();
            var pre = im.GetCurrent().FilterExpression<TSource>(functions);
            if (pre == null) return source;
            return source is IQueryable<TSource>
                ? ((IQueryable<TSource>)source).Where(pre)
                : source.Where(pre.Compile());
        }
        public static IQueryable<TSource> AuthorizedTo<TSource>(this IQueryable<TSource> source, params IFunction[] functions)
        {
            var im = Factory.Get<IdentityManager>();
            var pre = im.GetCurrent().FilterExpression<TSource>(functions);
            if (pre == null) return source;
            return source.Where(pre);
        }
    }
}