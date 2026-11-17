using Serilog;
using StockMarket.Web;

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

    builder.Services.AddWebServices(builder.Environment);

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

    //app.UseAuthentication(); // TODO: Add authentication
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