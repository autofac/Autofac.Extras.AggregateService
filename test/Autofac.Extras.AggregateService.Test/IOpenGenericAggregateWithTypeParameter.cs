namespace Autofac.Extras.AggregateService.Test
{
    public interface IOpenGenericAggregateWithTypeParameter<T>
    {
        T SomeProperty { get; }
        IOpenGeneric<T> OpenGeneric { get; }
    }
}
