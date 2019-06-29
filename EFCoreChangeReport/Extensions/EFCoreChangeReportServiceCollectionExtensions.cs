using EFCoreChangeReport.Configuration;
using EFCoreChangeReport.Services;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace EFCoreChangeReport.Extensions
{
    public static class EFCoreChangeReportServiceCollectionExtensions
    {
        public static IServiceCollection AddEFCoreChangeReport(this IServiceCollection services, Action<EFCoreChangeReportConfiguration> configureOptions)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));
            if (configureOptions == null) throw new ArgumentNullException(nameof(configureOptions));

            services.Configure<EFCoreChangeReportConfiguration>(configureOptions);
            services.AddScoped<IEFCoreChangeReportService, EFCoreChangeReportService>();

            return services;
        }

    }
}
