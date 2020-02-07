using System;
using Mapster;

namespace F4St.Common.Mappers
{
    public static class MapperExt
    {
        public static TTarget MapTo<TTarget>(this object source)
        {
            return source.Adapt<TTarget>();
        }

        public static TTarget MapTo<TSource, TTarget>(this TSource source)
        {
            return source.Adapt<TSource, TTarget>();
        }

        public static TTarget MapTo<TSource, TTarget>(this TSource source, TTarget destination)
        {
            return (TTarget) source.Adapt(destination, typeof(TSource), typeof(TTarget));
        }
    }
}