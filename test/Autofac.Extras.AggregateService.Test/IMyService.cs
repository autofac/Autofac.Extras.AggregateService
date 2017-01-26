using System;

namespace Autofac.Extras.AggregateService.Test
{
    /// <summary>
    /// A sample service interface.
    /// </summary>
    public interface IMyService
    {
        DateTime SomeDateValue { get; }

        ISomeDependency SomeDependency { get; }

        int SomeIntValue { get; }

        string SomeStringValue { get; }
    }
}