// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Xunit;

namespace Autofac.Extras.AggregateService.Test
{
    public class AggregateServiceOpenGenericDeepFixture
    {
        [Fact]
        public void DeeplyNestedOpenGenericIsNotSupported()
        {
            var builder = new ContainerBuilder();

            // while
            // builder.RegisterAggregateService(typeof(IOpenGenericAggregateWithTypeParameter<IOpenGeneric<>>));
            // is not syntactical legal, the following however is.
            var myTrickyType = typeof(IOpenGenericAggregateWithTypeParameter<>).MakeGenericType(typeof(IOpenGeneric<>));

            var action = new Action(() => builder.RegisterAggregateService(myTrickyType));
            Assert.Throws<ArgumentException>(action);
        }
    }
}
