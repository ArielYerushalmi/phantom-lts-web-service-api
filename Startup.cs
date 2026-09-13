using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using LtsWebServiceAPI.Services;

namespace LtsWebServiceAPI
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            // Registered as a singleton (not scoped/transient) because LtsClientServer holds
            // the actual live state for the session - the Fleck WebSocket server, the current
            // client connection and the pipeline consumer all need to survive across the
            // separate Start/Subscribe/Stop HTTP requests that drive them.
            services.AddSingleton<IDataReciever, LtsClientServer>();

            services.AddCors(o => o.AddPolicy("Policy", builder =>
            {
                builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
            }));
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            var wsOptions = new WebSocketOptions() { KeepAliveInterval = TimeSpan.FromSeconds(2) };
            app.UseWebSockets(wsOptions);
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseRouting();
            app.UseCors("Policy");
            app.UseHttpsRedirection();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

        }   
    }
}
