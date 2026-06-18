// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Autofac.Extras.AggregateService.Test.Stubs;

/// <summary>
/// A sample service interface.
/// </summary>
public interface IMyService
{
    DateTime SomeDateValue
    {
        get;
    }

    ISomeDependency? SomeDependency
    {
        get;
    }

    int SomeIntValue
    {
        get;
    }

    string? SomeStringValue
    {
        get;
    }
}
