using FMCApp.Constants;
using FMCApp.Core.Repositories;
using FMCApp.Core.Resources;
using FMCApp.Core.Services;
using FMCApp.Entity.Constants;
using FMCApp.Entity.DbContexts;
using FMCApp.Entity.Entities.Identity;
using FMCApp.ExceptionHandling;
using FMCApp.Middlewares;
using FMCApp.Services;
using IdentityModel;
using IdentityServer4.EntityFramework.Options;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.MSSqlServer;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace FMCApp.Helpers
{
    public static class StartupHelpers
    {
        public static void RegisterDbContexts(this IServiceCollection services, IConfigurationRoot configuration)
        {
            var migrationsAssembly = typeof(Startup).GetTypeInfo().Assembly.GetName().Name;

            var operationalStoreOptions = new OperationalStoreOptions();
            services.AddSingleton(operationalStoreOptions);

            var storeOptions = new ConfigurationStoreOptions();
            services.AddSingleton(storeOptions);

            services.AddDbContext<AdminDbContext>(options => options.UseSqlServer(configuration.GetConnectionString(ConfigurationConsts.AdminConnectionStringKey), optionsSql => optionsSql.MigrationsAssembly(migrationsAssembly)));
        }

        public static void RegisterDbContextsStaging(this IServiceCollection services)
        {
            var databaseName = Guid.NewGuid().ToString();

            var operationalStoreOptions = new OperationalStoreOptions();
            services.AddSingleton(operationalStoreOptions);

            var storeOptions = new ConfigurationStoreOptions();
            services.AddSingleton(storeOptions);

            services.AddDbContext<AdminDbContext>(optionsBuilder => optionsBuilder.UseInMemoryDatabase(databaseName));
        }

        public static void UseSecurityHeaders(this IApplicationBuilder app)
        {
            app.UseForwardedHeaders(new ForwardedHeadersOptions()
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            });
            app.UseHsts(options => options.MaxAge(days: 365));
            app.UseXXssProtection(options => options.EnabledWithBlockMode());
            app.UseXContentTypeOptions();
            app.UseXfo(options => options.SameOrigin());
            app.UseReferrerPolicy(options => options.NoReferrer());
            var allowCspUrls = new List<string>
            {
                "https://fonts.googleapis.com/",
                "https://fonts.gstatic.com/"
            };

            app.UseCsp(options =>
            {
                options.FontSources(configuration =>
                {
                    configuration.SelfSrc = true;
                    configuration.CustomSources = allowCspUrls;
                });

                //TODO: consider remove unsafe sources - currently using for toastr inline scripts in Notification.cshtml
                options.ScriptSources(configuration =>
                {
                    configuration.SelfSrc = true;
                    configuration.UnsafeInlineSrc = true;
                    configuration.UnsafeEvalSrc = true;
                });

                options.StyleSources(configuration =>
                {
                    configuration.SelfSrc = true;
                    configuration.CustomSources = allowCspUrls;
                    configuration.UnsafeInlineSrc = true;
                });
            });
        }

        public static void ConfigureAuthentification(this IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseAuthentication();

            if (env.IsStaging())
            {
                app.UseMiddleware<AuthenticatedTestRequestMiddleware>();
            }
        }

        public static void ConfigureLocalization(this IApplicationBuilder app)
        {
            var options = app.ApplicationServices.GetService<IOptions<RequestLocalizationOptions>>();
            app.UseRequestLocalization(options.Value);
        }

        public static void AddLogging(this IApplicationBuilder app, ILoggerFactory loggerFactory, IConfigurationRoot configuration)
        {
            loggerFactory.AddConsole(configuration.GetSection(ConfigurationConsts.LoggingSectionKey));
            loggerFactory.AddDebug();

            var columnOptions = new ColumnOptions();

            // Don't include the Properties XML column.
            columnOptions.Store.Remove(StandardColumn.Properties);

            // Do include the log event data as JSON.
            columnOptions.Store.Add(StandardColumn.LogEvent);

            Log.Logger = new LoggerConfiguration()
                .WriteTo.MSSqlServer(configuration.GetConnectionString(ConfigurationConsts.AdminConnectionStringKey),
                    TableConsts.Logging,
                    columnOptions: columnOptions,
                    restrictedToMinimumLevel: LogEventLevel.Error)
                .CreateLogger();
        }

        public static void AddDbContexts(this IServiceCollection services, IHostingEnvironment hostingEnvironment, IConfigurationRoot configuration)
        {
            if (hostingEnvironment.IsStaging())
            {
                services.RegisterDbContextsStaging();
            }
            else
            {
                services.RegisterDbContexts(configuration);
            }
        }

        public static void AddAuthorizationPolicies(this IServiceCollection services)
        {
            services.AddAuthorization(options =>
            {
                options.AddPolicy(AuthorizationConsts.AdministrationPolicy,
                    policy => policy.RequireRole(AuthorizationConsts.AdministrationRole));
            });
        }

        public static void AddServices(this IServiceCollection services)
        {
            //Repositories
            services.AddTransient<IClientRepository, ClientRepository>();
            services.AddTransient<IIdentityResourceRepository, IdentityResourceRepository>();
            services.AddTransient<IIdentityRepository, IdentityRepository>();
            services.AddTransient<IApiResourceRepository, ApiResourceRepository>();
            services.AddTransient<IPersistedGrantRepository, PersistedGrantRepository>();
            services.AddTransient<ILogRepository, LogRepository>();

            //Services
            services.AddTransient<ILogService, LogService>();
            services.AddTransient<IClientService, ClientService>();
            services.AddTransient<IApiResourceService, ApiResourceService>();
            services.AddTransient<IIdentityResourceService, IdentityResourceService>();
            services.AddTransient<IPersistedGrantService, PersistedGrantService>();
            services.AddTransient<IIdentityService, IdentityService>();

            //Resources
            services.AddScoped<IApiResourceServiceResources, ApiResourceServiceResources>();
            services.AddScoped<IClientServiceResources, ClientServiceResources>();
            services.AddScoped<IIdentityResourceServiceResources, IdentityResourceServiceResources>();
            services.AddScoped<IIdentityServiceResources, IdentityServiceResources>();
            services.AddScoped<IPersistedGrantServiceResources, PersistedGrantServiceResources>();

            services.AddTransient<IEmailSender, EmailSender>();

            //Exception handling
            services.AddScoped<ControllerExceptionFilterAttribute>();
        }

        public static void AddMvcLocalization(this IServiceCollection services)
        {
            services.AddSingleton<ITempDataProvider, CookieTempDataProvider>();

            services.AddLocalization(opts => { opts.ResourcesPath = ConfigurationConsts.ResourcesPath; });

            services.AddMvc()
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_1)
                .AddViewLocalization(
                    LanguageViewLocationExpanderFormat.Suffix,
                    opts => { opts.ResourcesPath = ConfigurationConsts.ResourcesPath; })
                .AddDataAnnotationsLocalization();

            services.Configure<RequestLocalizationOptions>(
                opts =>
                {
                    var supportedCultures = new[]
                    {
                        new CultureInfo("en-US"),
                        new CultureInfo("en")
                    };

                    opts.DefaultRequestCulture = new RequestCulture("en");
                    opts.SupportedCultures = supportedCultures;
                    opts.SupportedUICultures = supportedCultures;
                });

        }

        public static void AddAuthentication(this IServiceCollection services, IHostingEnvironment hostingEnvironment)
        {
            services.AddIdentity<UserIdentity, UserIdentityRole>()
                .AddEntityFrameworkStores<AdminDbContext>()
                .AddDefaultTokenProviders();

            //For integration tests use only cookie middleware
            //if (hostingEnvironment.IsStaging())
            //{
            //    services.AddAuthentication(options =>
            //    {
            //        options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            //        options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;

            //        options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            //        options.DefaultForbidScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            //        options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            //        options.DefaultSignOutScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            //    })
            //        .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme,
            //            options => { options.Cookie.Name = AuthorizationConsts.IdentityAdminCookieName; });
            //}
            //else
            //{
            //    services.AddAuthentication(options =>
            //    {
            //        options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            //        options.DefaultChallengeScheme = AuthorizationConsts.OidcAuthenticationScheme;

            //        options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            //        options.DefaultForbidScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            //        options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            //        options.DefaultSignOutScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            //    })
            //        .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme,
            //            options => { options.Cookie.Name = AuthorizationConsts.IdentityAdminCookieName; })
            //        .AddOpenIdConnect(AuthorizationConsts.OidcAuthenticationScheme, options =>
            //        {
            //            options.Authority = AuthorizationConsts.IdentityServerBaseUrl;
            //            options.RequireHttpsMetadata = false;

            //            options.ClientId = AuthorizationConsts.OidcClientId;

            //            options.Scope.Clear();
            //            options.Scope.Add(AuthorizationConsts.ScopeOpenId);
            //            options.Scope.Add(AuthorizationConsts.ScopeProfile);
            //            options.Scope.Add(AuthorizationConsts.ScopeEmail);
            //            options.Scope.Add(AuthorizationConsts.ScopeRoles);

            //            options.SaveTokens = true;

            //            options.TokenValidationParameters = new TokenValidationParameters
            //            {
            //                NameClaimType = JwtClaimTypes.Name,
            //                RoleClaimType = JwtClaimTypes.Role,
            //            };

            //            options.Events = new OpenIdConnectEvents
            //            {
            //                OnMessageReceived = OnMessageReceived,
            //                OnRedirectToIdentityProvider = OnRedirectToIdentityProvider
            //            };
            //        });
            //}
        }
        public static void AddIdentityServer(this IServiceCollection services, IHostingEnvironment hostingEnvironment, IConfigurationRoot configuration)
        {
            string connectionString = configuration.GetConnectionString("AdminConnection");
            var migrationsAssembly = typeof(Startup).GetTypeInfo().Assembly.GetName().Name;
            var builder = services.AddIdentityServer(options =>
            {
                options.Events.RaiseErrorEvents = true;
                options.Events.RaiseInformationEvents = true;
                options.Events.RaiseFailureEvents = true;
                options.Events.RaiseSuccessEvents = true;
            })
                .AddAspNetIdentity<UserIdentity>()
                // this adds the config data from DB (clients, resources)
                .AddConfigurationStore(options =>
                {
                    options.ConfigureDbContext = b =>
                        b.UseSqlServer(connectionString,
                            sql => sql.MigrationsAssembly(migrationsAssembly));
                })
                // this adds the operational data from DB (codes, tokens, consents)
                .AddOperationalStore(options =>
                {
                    options.ConfigureDbContext = b =>
                        b.UseSqlServer(connectionString,
                            sql => sql.MigrationsAssembly(migrationsAssembly));

                    // this enables automatic token cleanup. this is optional.
                    options.EnableTokenCleanup = true;

#if DEBUG
                    options.TokenCleanupInterval = 15; // frequency in seconds to cleanup stale grants. 15 is useful during debugging
#endif                
                });

            if (hostingEnvironment.IsDevelopment())
            {
                builder.AddDeveloperSigningCredential();
            }
            else
            {
                throw new Exception("need to configure key material");
            }
        }



        private static Task OnMessageReceived(MessageReceivedContext context)
        {
            context.Properties.IsPersistent = true;
            context.Properties.ExpiresUtc = new DateTimeOffset(DateTime.Now.AddHours(12));

            return Task.FromResult(0);
        }

        private static Task OnRedirectToIdentityProvider(RedirectContext n)
        {
            n.ProtocolMessage.RedirectUri = AuthorizationConsts.IdentityAdminRedirectUri;

            return Task.FromResult(0);
        }
    }
}
