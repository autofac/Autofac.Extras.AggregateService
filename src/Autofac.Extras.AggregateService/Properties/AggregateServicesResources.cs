using System.Globalization;
using System.Reflection;
using System.Resources;

namespace Autofac.Extras.AggregateService.Properties
{
    internal static class AggregateServicesResources
    {
        private static readonly ResourceManager ResourceManager
            = new ResourceManager(
                "Autofac.Extras.AggregateService.Properties.AggregateServicesResources",
                typeof(AggregateServicesResources).GetTypeInfo().Assembly);

        public static string GetString(string name) => ResourceManager.GetString(name, CultureInfo.CurrentCulture);
    }
}