// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Autofac.Extras.AggregateService.Test.Stubs;

public class MyService : IMyService
{
    public MyService()
    {
    }

    public MyService(int someIntValue)
    {
        SomeIntValue = someIntValue;
    }

    public MyService(string? someStringValue, ISomeDependency someDependency)
    {
        SomeStringValue = someStringValue;
        SomeDependency = someDependency;
    }

    public MyService(DateTime someDate, int someInt, ISomeDependency someDependency)
    {
        SomeDateValue = someDate;
        SomeIntValue = someInt;
        SomeDependency = someDependency;
    }

    public int SomeIntValue
    {
        get;
        private set;
    }

    public string? SomeStringValue
    {
        get;
        private set;
    }

    public DateTime SomeDateValue
    {
        get; private set;
    }

    public ISomeDependency? SomeDependency
    {
        get; private set;
    }
}
