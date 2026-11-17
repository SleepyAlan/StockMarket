namespace StockMarket.Web;

public static class DependencyInjection
{
    public static IServiceCollection AddWebServices(
        this IServiceCollection services,
        IWebHostEnvironment environment)
    {
        var mvcBuilder = services.AddControllersWithViews();
        if (environment.IsDevelopment() || environment.EnvironmentName == "Testing")
        {
            mvcBuilder.AddRazorRuntimeCompilation();
        }
        return services;
    }
}
