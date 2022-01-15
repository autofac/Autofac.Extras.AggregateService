// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Autofac.Extras.AggregateService.Test
{
    public interface IOpenGenericAggregate
    {
        IOpenGeneric<T> GetOpenGeneric<T>();

        IOpenGeneric<string> GetResolvedGeneric();

        IPassThroughOpenGeneric<T> UseOpenGenericParameter<T>(IOpenGeneric<T> openGeneric);

        IOpenGeneric<T> TooManyParameters<T>(IOpenGeneric<T> a, string b, string c, string d, string e, string f, string g, string h, string i, string j, string k, string l, string m, string n, string o, string p, string q, string r);
    }
}
