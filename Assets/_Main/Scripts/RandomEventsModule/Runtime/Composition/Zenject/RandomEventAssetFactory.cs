using System;
using System.Collections.Generic;
using System.Linq;

namespace RandomEventsModule
{
    public static class RandomEventAssetFactory
    {
        public static List<TResult> CreateAll<TAsset, TResult>(this IEnumerable<TAsset> assets, Func<TAsset, TResult> create)
            where TAsset : class =>
            assets
                .Where(asset => asset != null)
                .Select(create)
                .ToList();
    }
}
