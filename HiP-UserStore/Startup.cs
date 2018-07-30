using System;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSwag.AspNetCore;
using PaderbornUniversity.SILab.Hip.DataStore;
using PaderbornUniversity.SILab.Hip.EventSourcing;
using PaderbornUniversity.SILab.Hip.EventSourcing.EventStoreLlp;
using PaderbornUniversity.SILab.Hip.UserStore.Core;
using PaderbornUniversity.SILab.Hip.UserStore.Model;
using PaderbornUniversity.SILab.Hip.UserStore.Utility;
using PaderbornUniversity.SILab.Hip.Webservice;
using PaderbornUniversity.SILab.Hip.Webservice.Logging;

namespace PaderbornUniversity.SILab.Hip.UserStore
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();

            //Initialize ResourceTypes
            ResourceTypes.Initialize();
            ActionTypes.Initialize();
        }

        public IConfigurationRoot Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services
                .Configure<EndpointConfig>(Configuration.GetSection("Endpoints"))
                .Configure<DataStoreConfig>(Configuration.GetSection("Endpoints"))
                .Configure<EventStoreConfig>(Configuration.GetSection("EventStore"))
                .Configure<UserStoreAuthConfig>(Configuration.GetSection("Auth"))
                .Configure<UploadPhotoConfig>(Configuration.GetSection("UploadingPhoto"))
                .Configure<PredefinedAvatarsConfig>(Configuration.GetSection("PredefinedAvatars"))
                .Configure<CorsConfig>(Configuration)
                .Configure<LoggingConfig>(Configuration.GetSection("HiPLoggerConfig"));

            services
                .AddSingleton<IEventStore, EventSourcing.EventStoreLlp.EventStore>()
                .AddSingleton<EventStoreService>()
                .AddSingleton<CacheDatabaseManager>()
                .AddSingleton<InMemoryCache>()
                .AddSingleton<DataStoreService>()
                .AddSingleton<IDomainIndex, EntityIndex>()
                .AddSingleton<IDomainIndex, ExhibitsVisitedIndex>()
                .AddSingleton<IDomainIndex, UserIndex>();

            var serviceProvider = services.BuildServiceProvider(); // allows us to actually get the configured services
            var authConfig = serviceProvider.GetService<IOptions<UserStoreAuthConfig>>();
            Utility.Auth.Initialize(authConfig);

            // Configure authentication
            services
                .AddAuthentication(options => options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.Audience = authConfig.Value.Audience;
                    options.Authority = authConfig.Value.Authority;
                });

            // Configure authorization
            var domain = authConfig.Value.Authority;
            services.AddAuthorization(options =>
            {
                options.AddPolicy("read:datastore",
                    policy => policy.Requirements.Add(new HasScopeRequirement("read:datastore", domain)));
                options.AddPolicy("write:datastore",
                    policy => policy.Requirements.Add(new HasScopeRequirement("write:datastore", domain)));
                options.AddPolicy("write:cms",
                    policy => policy.Requirements.Add(new HasScopeRequirement("write:cms", domain)));
            });

            services.AddCors();
            services.AddMvc();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory,
            IOptions<CorsConfig> corsConfig, IOptions<EndpointConfig> endpointConfig, IOptions<LoggingConfig> loggingConfig)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"))
                         .AddDebug()
                         .AddHipLogger(loggingConfig.Value);

            ILogger logger = loggerFactory.CreateLogger("ApplicationStartup");
            try
            {
                // CacheDatabaseManager should start up immediately (not only when injected into a controller or
                // something), so we manually request an instance here
                app.ApplicationServices.GetService<CacheDatabaseManager>();

                // Ensures that "Request.Scheme" is correctly set to "https" in our nginx-environment
                app.UseRequestSchemeFixer();

                // Use CORS (important: must be before app.UseMvc())
                app.UseCors(builder =>
                    {
                        var corsEnvConf = corsConfig.Value.Cors[env.EnvironmentName];
                        builder
                            .WithOrigins(corsEnvConf.Origins)
                            .WithMethods(corsEnvConf.Methods)
                            .WithHeaders(corsEnvConf.Headers)
                            .WithExposedHeaders(corsEnvConf.ExposedHeaders);
                    });

                app.UseAuthentication();
                app.UseMvc();
                app.UseSwaggerUiHip();

                logger.LogInformation("UserStore started successfully");
            }
            catch (Exception e)
            {
                logger.LogCritical($"UserStore Startup Failed:{e.Message}");
                throw;
            }

        }
    }
}
