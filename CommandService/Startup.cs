using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.SignalR;
using CommandService.Hubs;
using MassTransit;
using CommandService.Models;
using MassTransit.Transports.Fabric;
using CommandService.Consumers;

namespace CommandService
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "CommandService", Version = "v1" });
            });
            services.AddCors(options =>
            {
                options.AddPolicy(name: "MySpec",
                                  builder =>
                                  {
                                      builder.WithOrigins("https://localhost:44336")
                                      .AllowCredentials()
                                       .AllowAnyHeader()
                                       .AllowAnyMethod();
                                  });
            });
            services.AddMassTransit(x =>
            {
                x.AddConsumer<EventHubConsumer>();
                x.UsingRabbitMq((context, cfg) =>
                {
                    cfg.Host("localhost", "/", h =>
                    {
                        h.Username("guest");
                        h.Password("guest");
                    });

                    cfg.Message<SocketCommandMessage>(e => e.SetEntityName("command-exchange"));
                    cfg.Publish<SocketCommandMessage>(e => e.ExchangeType = "topic");
                    cfg.Send<SocketCommandMessage>(e => e.UseRoutingKeyFormatter(s => s.Message.Topic));


                    cfg.ReceiveEndpoint("socket-event", e =>
                    {
                        e.ConfigureConsumer<EventHubConsumer>(context);
                    });
                });
            });
            services.AddSignalR();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "CommandService v1"));
            }
            app.UseHttpsRedirection();

            app.UseRouting();
            app.UseCors("MySpec");
            //app.UseMiddleware<CorsMiddleware>();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHub<CommandHub>("/api/connect");
            });
        }
    }
}
