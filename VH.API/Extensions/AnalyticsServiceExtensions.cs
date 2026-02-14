using VH.Services.Interfaces;
using VH.Services.Services;

namespace VH.API.Extensions
{
    public static class AnalyticsServiceExtensions
    {
        public static IServiceCollection AddAnalyticsServices(this IServiceCollection services)
        {
            services.AddScoped<IAlertaConsumoService, AlertaConsumoService>();
            services.AddScoped<IDashboardAnalyticsService, DashboardAnalyticsService>();
            
            return services;
        }
    }
}
