using CommandService.Models;
using MassTransit;
using MassTransit.Transports.Fabric;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using SimpleService.Consumers;

namespace SimpleService
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
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "SimpleService", Version = "v1" });
            });
            services.AddScoped<CommandManagerConsumer>();
            services.AddMassTransit(x =>
            {
                x.AddConsumer<CommandManagerConsumer>();
                x.UsingRabbitMq((context, cfg) =>
                {
                    cfg.Host("localhost", "/", h =>
                    {
                        h.Username("guest");
                        h.Password("guest");
                    });

                    cfg.Message<SocketCommandMessage>(e => e.SetEntityName("command-exchange"));
                    cfg.Publish<SocketCommandMessage>(e => e.ExchangeType = ExchangeType.Topic.ToString());
                    cfg.Send<SocketCommandMessage>(e => e.UseRoutingKeyFormatter(s => s.Message.Topic));

                    cfg.ReceiveEndpoint("command-queue", e =>
                    {
                        e.ConfigureConsumeTopology = false;
                        e.Bind("command-exchange", a =>
                        {
                            a.RoutingKey = "Core.SimpleService.#";
                            a.ExchangeType = "topic";
                        });
                        e.ConfigureConsumer<CommandManagerConsumer>(context);
                    });

                });
            });

            services.AddMediatR(typeof(Startup));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "SimpleService v1"));
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
