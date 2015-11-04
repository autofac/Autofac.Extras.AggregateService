namespace Autofac.Extras.AggregateService.Test
{
    public interface ISubService : ISuperService
    {
        ISomeOtherDependency SomeOtherDependency { get; }
    }
}
