using System;
using Mapster;

namespace F4ST.Common.Mappers
{
    public interface IMapper
    {
        TTarget Map<TSource, TTarget>(TSource source, TTarget target);
        TTarget Map<TTarget>(object source);
        TypeAdapterSetter<TSource, TTarget> Bind<TSource, TTarget>();
    }
}