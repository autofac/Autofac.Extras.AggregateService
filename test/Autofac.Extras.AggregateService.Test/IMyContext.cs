// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;

namespace Autofac.Extras.AggregateService.Test
{
    /// <summary>
    /// Interface illustrating an aggregate service context with supported and unsupported
    /// method signatures.
    /// </summary>
    public interface IMyContext
    {
        // Supported
        IMyService MyService { get; }

        // Unsupported
        IMyService PropertyWithSetter { get; set; }

        // Supported
        IMyService GetMyService();

        // Supported
        IMyService GetMyService(int someValue);

        // Supported
        IMyService GetMyService(string someOtherValue);

        // Supported
        IMyService GetMyService(DateTime someDate, int someInt);

        // Unsupported
        void MethodWithoutReturnValue();
    }
}
