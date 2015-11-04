namespace Autofac.Extras.AggregateService.Test
{
    public interface IOpenGenericAggregate
    {
        IOpenGeneric<T> GetOpenGeneric<T>();

        IOpenGeneric<string> GetResolvedGeneric();
    }
}
