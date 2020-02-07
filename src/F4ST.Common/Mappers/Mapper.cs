using System;
using Mapster;

namespace F4ST.Common.Mappers
{
    public class Mapper : IMapper
    {
        public TTarget Map<TSource, TTarget>(TSource source, TTarget target)
        {
            return source.Adapt(target);
        }

        public TTarget Map<TTarget>(object source)
        {
            return source.Adapt<TTarget>();
        }

        public TypeAdapterSetter<TSource,TTarget> Bind<TSource,TTarget>()
        {
            return TypeAdapterConfig<TSource, TTarget>.NewConfig();
        }
    }
}