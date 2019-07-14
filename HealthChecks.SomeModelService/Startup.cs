using System.Linq;
using System.Threading.Tasks;
using Autofac;
using HealthChecks.Configuration;
using HealthChecks.SomeModelService.Health;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;

namespace HealthChecks.SomeModelService
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        /// <summary>
        /// Set up any services and other general configuration here
        /// </summary>
        /// <param name="services"></param>
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors();
            services.AddHealthChecks()
                .AddMySql(connectionString: Configuration["Data:ConnectionStrings:MySql"])
                .AddRedis(redisConnectionString: Configuration["Data:ConnectionStrings:Redis"],
                    failureStatus: HealthStatus.Degraded)
                .AddCheck<ApiHealthCheck>("api");
            //.AddCheck<SecondaryHealthCheck>("secondary");
            //add additional healthchecks here, as needed
            //as described here https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks


            //Api Base Configuration
            services.AddMvcCore()
                .AddAuthorization()
                .AddJsonFormatters();

            //add the overall configuration to the IoC
            services.AddSingleton(Configuration);

            //add our specific configurations to the IoC
            var identityConfiguration = new IdentityConfiguration();
            Configuration.GetSection("Identity").Bind(identityConfiguration);
            services.AddSingleton(identityConfiguration);

            if (identityConfiguration.Enabled)
            {
                services
                    .AddAuthentication("Bearer")
                    .AddJwtBearer(
                        "Bearer",
                        options =>
                        {
                            options.Authority = identityConfiguration.ServerUrl;
                            options.RequireHttpsMetadata = identityConfiguration.RequireHttpsMetadata;
                            options.Audience = identityConfiguration.Audience;
                        });
            }

            var awsOptions = Configuration.GetAWSOptions();
            services.AddDefaultAWSOptions(awsOptions);
            ConfigureAwsServices(services);
        }

         private void ConfigureAwsServices(
            IServiceCollection services)
        {
            var localStackConfiguration = new LocalStackConfiguration();
            Configuration.GetSection("LocalStack").Bind(localStackConfiguration);
        }


        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IApplicationLifetime applicationLifetime)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseCors(builder =>
                builder
                    .AllowAnyOrigin()
                    .AllowAnyHeader()
                    .AllowAnyMethod()
            );

            app.UseHealthChecks("/health", new HealthCheckOptions()
            {
                ResponseWriter = WriteHealthCheckResponse
            });

            //Use Identity and Access Control
            app.UseAuthentication();
            
            //Use MVC / API
            app.UseMvc();
        }

        private static Task WriteHealthCheckResponse(HttpContext httpContext,
            HealthReport result)
        {
            httpContext.Response.ContentType = "application/json";

            var json = new JObject(
                new JProperty("status", result.Status.ToString()),
                new JProperty("results", new JObject(result.Entries.Select(pair =>
                    new JProperty(pair.Key, new JObject(
                        new JProperty("status", pair.Value.Status.ToString()),
                        new JProperty("description", pair.Value.Description),
                        new JProperty("data", new JObject(pair.Value.Data.Select(
                            p => new JProperty(p.Key, p.Value))))))))));
            return httpContext.Response.WriteAsync(
                json.ToString(Formatting.Indented));
        }

        /// <summary>
        /// Used to configure your container as needed.
        /// </summary>
        /// <param name="builder"></param>
        public void ConfigureContainer(ContainerBuilder builder)
        {
            builder.RegisterModule(new DependencyModule());
        }
    }
}
