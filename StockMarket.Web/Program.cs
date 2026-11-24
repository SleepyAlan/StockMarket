using Microsoft.AspNetCore.Identity;
using Serilog;
using StockMarket.Application;
using StockMarket.Application.Abstractions.Identity;
using StockMarket.Infrastructure;
using StockMarket.Infrastructure.Identity;
using StockMarket.Infrastructure.Persistence;
using StockMarket.Web;
using StockMarket.Web.Services;

var builder = WebApplication.CreateBuilder(args);
if (builder.Environment.IsDevelopment() || builder.Environment.EnvironmentName == "Testing")
{
    Log.Logger = new LoggerConfiguration()
        .WriteTo.Console()
        .CreateBootstrapLogger();
}

try
{
    Log.Information("Starting web application.");

    // Serilog service
    builder.Services.AddSerilog((services, config) =>
    {
        config.ReadFrom
            .Configuration(builder.Configuration)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext();
    });

    // Presentation, Application & Infrastructure services
    builder.Services.AddWebServices(builder.Environment);
    builder.Services.AddApplicationServices();
    builder.Services.AddInfrastructure(builder.Configuration);

    // Identity
    builder.Services
        .AddIdentity<ApplicationUser, IdentityRole<Guid>>()
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddDefaultTokenProviders();
    builder.Services.AddHttpContextAccessor(); // Current Authenticated user.
    builder.Services.AddScoped<ICurrentUserService, HttpContextCurrentUserService>();

    var app = builder.Build();

    app.UseSerilogRequestLogging();

    if (app.Environment.IsDevelopment() || builder.Environment.EnvironmentName == "Testing")
    {
        app.UseDeveloperExceptionPage();
    }
    else
    {
        app.UseExceptionHandler("/Home/Error");
        app.UseHsts();
    }

    app.UseHttpsRedirection();
    app.UseRouting();

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapStaticAssets();

    app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}")
        .WithStaticAssets();


    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Host terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}