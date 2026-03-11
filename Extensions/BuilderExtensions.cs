using HeadHunterJobPopularTagsMonitor.HttpClients;

namespace HeadHunterJobPopularTagsMonitor.Extensions
{
    public static class BuilderExtensions
    {
        public static IServiceCollection AddHeadHunterHttpClient(this IServiceCollection services)
        {
            services.AddHttpClient<HeadHunterHttpService>()
                .SetHandlerLifetime(TimeSpan.FromMinutes(5));

            return services;
        }
    }
}
