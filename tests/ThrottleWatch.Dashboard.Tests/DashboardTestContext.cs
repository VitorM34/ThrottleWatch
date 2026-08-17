using System.Globalization;
using System.Resources;
using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using ThrottleWatch.Dashboard.Localization;

namespace ThrottleWatch.Dashboard.Tests;

public abstract class DashboardTestContext : TestContext
{
    protected DashboardTestContext()
    {
        CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo(LocalizationConstants.DefaultCulture);
        CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(LocalizationConstants.DefaultCulture);
        Services.AddSingleton<IStringLocalizer<SharedResources>>(new SharedResourcesLocalizer());
    }

    private sealed class SharedResourcesLocalizer : IStringLocalizer<SharedResources>
    {
        private static readonly ResourceManager Manager = new(
            "ThrottleWatch.Dashboard.Resources.SharedResources",
            typeof(SharedResources).Assembly);

        public LocalizedString this[string name]
        {
            get
            {
                var value = Manager.GetString(name) ?? name;
                return new LocalizedString(name, value, resourceNotFound: value == name);
            }
        }

        public LocalizedString this[string name, params object[] arguments]
        {
            get
            {
                var format = this[name].Value;
                return new LocalizedString(name, string.Format(CultureInfo.CurrentUICulture, format, arguments));
            }
        }

        public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures) => [];
    }
}
