// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Autofac.Extras.AggregateService.Test
{
    public interface ISubService : ISuperService
    {
        ISomeOtherDependency SomeOtherDependency { get; }
    }
}
