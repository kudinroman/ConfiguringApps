using ConfiguringApps.Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using System.IO;

public class Startup
{
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<UptimeService>();
        services.AddMvc().AddMvcOptions(options => {
            options.RespectBrowserAcceptHeader = true;
        });
    }

    public void ConfigureDevelopmentServices(IServiceCollection services)
    {
        services.AddSingleton<UptimeService>();
        services.AddMvc();
    }

    public void Configure(IApplicationBuilder app, IHostingEnvironment env)
    {
        if ((Configuration.GetSection("ShortCircuitMiddleware")?
            .GetValue<bool>("EnableBrowserShortCircuit")).Value)
        {
            app.UseMiddleware<BrowserTypeMiddleware>();
            app.UseMiddleware<ShortCircuitMiddleware>();
        }

        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
            app.UseStatusCodePages();
            app.UseBrowserLink();
        }
        else
        {
            app.UseExceptionHandler("/Home/Error");
        }

        app.UseDefaultFiles();
        app.UseStaticFiles();

        // добавляем поддержку каталога node_modules
        app.UseFileServer(new FileServerOptions()
        {
            FileProvider = new PhysicalFileProvider(
                Path.Combine(env.ContentRootPath, "node_modules")
            ),
            RequestPath = "/node_modules",
            EnableDirectoryBrowsing = false
        });

        app.UseMvc(routes =>
        {
            routes.MapRoute(
            name: "default",
            template: "{controller=Home}/{action=Index}/{id?}");
        });
    }

    public void ConfigureDevelopment(IApplicationBuilder app,
    IHostingEnvironment env)
    {
        app.UseDeveloperExceptionPage();
        app.UseStatusCodePages();
        app.UseBrowserLink();
        app.UseStaticFiles();
        app.UseMvcWithDefaultRoute();
    }
}