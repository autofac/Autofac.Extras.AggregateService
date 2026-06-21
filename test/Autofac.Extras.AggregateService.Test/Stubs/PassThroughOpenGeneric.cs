// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Autofac.Extras.AggregateService.Test.Stubs;

public class PassThroughOpenGeneric<T> : IPassThroughOpenGeneric<T>
{
    public PassThroughOpenGeneric(IOpenGeneric<T> openGeneric)
    {
        OpenGeneric = openGeneric;
    }

    public IOpenGeneric<T> OpenGeneric
    {
        get;
    }
}
