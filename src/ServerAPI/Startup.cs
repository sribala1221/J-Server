using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;
using GenerateTables.Models;
using JwtDb.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using ServerAPI.Authentication;
using ServerAPI.Extensions;
using ServerAPI.Hubs;
using ServerAPI.Policies;
using System.Text;
using Microsoft.AspNetCore.Mvc.Cors.Internal;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Serilog;
using Serilog.Core;

namespace ServerAPI {
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public class Startup
    {

        private SymmetricSecurityKey _signingKey;
        public static string ConnectionString { get; private set; }
        private IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            ConnectionString = Configuration.GetConnectionString("DefaultConnection");
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMemoryCache();

            services.AddHttpContextAccessor();
            services.Replace(ServiceDescriptor.Singleton(typeof(ILogger<>), typeof(TimedLogger<>)));
            
            // Handle authentication - maybe will be rolled to IdentityServer when available
            _signingKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(Configuration["SecretKey"]));
            services.AddDbContext<JwtDbContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));
            services.AddScoped<IJwtFactory, JwtFactory>();

            // jwt wire up
            // Get options from app settings
            IConfigurationSection jwtAppSettingOptions = Configuration.GetSection(nameof(JwtIssuerOptions));

            // Configure JwtIssuerOptions
            services.Configure<JwtIssuerOptions>(options =>
            {
                options.Issuer = jwtAppSettingOptions[nameof(JwtIssuerOptions.Issuer)];
                options.Audience = jwtAppSettingOptions[nameof(JwtIssuerOptions.Audience)];
                options.SigningCredentials = new SigningCredentials(_signingKey, SecurityAlgorithms.HmacSha256);
            });

            // This is important for *creating* users; probably not relevant here
            services.AddIdentity<AppUser, IdentityRole>(o =>
                {
                    // configure identity options
                    o.Password.RequireDigit = false;
                    o.Password.RequireLowercase = false;
                    o.Password.RequireUppercase = false;
                    o.Password.RequireNonAlphanumeric = false;
                    o.Password.RequiredLength = 6;
                })
                .AddEntityFrameworkStores<JwtDbContext>()
                .AddDefaultTokenProviders();
            services.AddAuthorization(options =>
            {
                options.AddPolicy("ApiUser", policy => 
                    policy.RequireClaim("rol", "atims"));
                options.AddPolicy("FuncPermission",
                    policy => policy.AddRequirements(new FuncPermissionRequirement()));
            });

            TokenValidationParameters tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = jwtAppSettingOptions[nameof(JwtIssuerOptions.Issuer)],

                ValidateAudience = true,
                ValidAudience = jwtAppSettingOptions[nameof(JwtIssuerOptions.Audience)],

                ValidateIssuerSigningKey = true,
                IssuerSigningKey = _signingKey,

                RequireExpirationTime = false,
                ValidateLifetime = false,
                ClockSkew = TimeSpan.Zero
            };

            services.AddAuthentication(o =>
            {
                o.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                o.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;

            }).AddJwtBearer(options => options.TokenValidationParameters = tokenValidationParameters);

            // Configure Serilog
            Logger log = new LoggerConfiguration().WriteTo.Console().CreateLogger();
            Log.Logger = log;
            Log.Information($"{DateTime.Now} The global logger has been configured");

            // Add framework services
            services.AddCors(options => options.AddPolicy("AllowAnyOrigin", builder => builder.AllowAnyOrigin()));

            services.Configure<MvcOptions>(options => options.Filters.Add(new CorsAuthorizationFilterFactory("AllowAnyOrigin")));
            services.AddMvc(config =>
            {
                AuthorizationPolicy policy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .Build();
                config.Filters.Add(new AuthorizeFilter(policy));
            }).SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            services.AddHsts(options =>
            {
                options.Preload = true;
                options.IncludeSubDomains = true;
                options.MaxAge = TimeSpan.FromDays(365);
            });

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v2",
                    new OpenApiInfo
                    {
                        Title = "JMS Server API", Version = "v2",
                        Contact = new OpenApiContact
                            { Name = "ATIMS", Email = "info@atims.com", Url = new Uri("https://www.atims.com") }
                    });
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    In =  ParameterLocation.Header,
                    Description = "Please enter into field the word 'Bearer' following by space and JWT",
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey
                });
                // Set the comments path for the Swagger JSON and UI.
                string xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                string xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);
            });
            
            // Eventually
            // services.AddDistributedMemoryCache();

            services.AddDbContext<AAtims>(options =>
            {
                options.UseSqlServer(ConnectionString);
                options.EnableSensitiveDataLogging();
            });

            services.AddTransient<IAuthorizationHandler, FuncPermissionHandler>();
            services.AddTransient<IUserPermissionPolicy, UserPermissionPolicy>();

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            // Register all the Interface and services under namespace ServerAPI.Services for DI          
            services.AddScopedImplementations();
            services.AddSignalR();

            services.ConfigureAudit(Configuration)
                .AddMvc(_ => _.AddAudit())
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IHttpContextAccessor contextAccessor)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();

                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v2/swagger.json", "JMS API V2"));

                app.UseMiddleware<StackifyMiddleware.RequestTracerMiddleware>();
            } else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }
            
            app.UseCors(builder => builder
                .SetIsOriginAllowed(host => true).AllowCredentials()
                .AllowAnyMethod()
                .AllowAnyHeader());
            app.UseHttpsRedirection();
            app.UseAuthentication();

            app.UseDefaultFiles();
            app.UseStaticFiles(new StaticFileOptions //To provide a path for files 
            {
                RequestPath = new PathString("/atims_dir"),
                FileProvider = new PhysicalFileProvider(Configuration.GetValue<string>("SiteVariables:PhotoPath"))
            });
            app.UseStaticFiles();
            app.UseMvc();
            app.UseCors("AllowAnyOrigin");
            app.UseSignalR(routes => routes.MapHub<AtimsHub>("/atimshub"));

            app.UseAuditCorrelationId(contextAccessor);
        }
    }
}

