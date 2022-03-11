using DeviceService.Core.Data.DataContext;
using DeviceService.Core.Entities;
using DeviceService.Core.Helpers.Common;
using DeviceService.Core.Helpers.ConfigurationSettings;
using DeviceService.Core.Helpers.Extensions;
using DeviceService.Core.Helpers.Filters.ActionFilters;
using DeviceService.Core.Helpers.Logging.Logger;
using DeviceService.Core.Helpers.RoleBasedAccess;
using DeviceService.Core.Interfaces.Repositories;
using DeviceService.Core.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DeviceService.API
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            ConfigurationSettingsHelper.Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // using identitycore and not identity cos we dont want to use cookies
            IdentityBuilder builder = services.AddIdentityCore<User>(opt => {
                opt.Password.RequireDigit = false;
                opt.Password.RequiredLength = 4;
                opt.Password.RequireNonAlphanumeric = false;
                opt.Password.RequireUppercase = false;
                opt.Password.RequireLowercase = false;
            });

            // so that we may be able to query for the users and have their roles pulled back at the same time as well
            builder = new IdentityBuilder(builder.UserType, typeof(Role), builder.Services);
            builder.AddEntityFrameworkStores<DeviceContext>();
            //If we used AddIdentity only in the preceeding block, we wouldn't nedd to add the following cos it would do it automatically for us
            builder.AddRoleValidator<RoleValidator<Role>>();
            builder.AddRoleManager<RoleManager<Role>>();
            builder.AddSignInManager<SignInManager<User>>();
            builder.AddDefaultTokenProviders();
            services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(Configuration.GetSection("AppSettings:Secret").Value)),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ClockSkew = TimeSpan.Zero
                };

                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Query["access_token"];

                        // If the request is for our hub...
                        var path = context.HttpContext.Request.Path;
                        if (!string.IsNullOrEmpty(accessToken) &&
                            (path.StartsWithSegments("/signalR/init")))
                        {
                            // Read the token out of the query string
                            context.Token = accessToken;
                        }
                        return Task.CompletedTask;
                    }
                };
            });

            services.AddCors(options =>
            {
                options.AddPolicy("CORSPolicy", builder =>
                {
                    builder.AllowAnyMethod().AllowAnyHeader().AllowAnyOrigin();
                });
            });

            services.AddControllers(opt =>
            {
                opt.Filters.Add(typeof(ValidationActionFilter));
                opt.ReturnHttpNotAcceptable = false;
            })
            .AddNewtonsoftJson(opt =>
            {
                opt.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
                opt.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Error;
            })
            .ConfigureApiBehaviorOptions(opt =>
            {
                opt.SuppressModelStateInvalidFilter = true;
            });

            services.AddDbContext<DeviceContext>(options => options.UseLazyLoadingProxies(false));
            services.AddHttpContextAccessor();
            services.AddServiceLibraryServices();

            //REGISTER SERVICES HERE
            //FOR AUTHORIZATION
            services.AddTransient<IAuthorizationPolicyProvider, FunctionalityNamePolicy>();
            services.AddTransient<IAuthorizationHandler, FunctionalityNameHandler>();
            //OTHERS
            services.AddScoped<IAuditReportActivityRepository, AuditReportActivityRepository>();
            services.AddScoped<IAuditReportRepository, AuditReportRepository>();
            services.AddScoped<IAuthRepository, AuthRepository>();
            services.AddScoped<ICloudinaryRepository, CloudinaryRepository>();
            services.AddScoped<IDeviceOperationRepository, DeviceOperationRepository>();
            services.AddScoped<IDeviceRepository, DeviceRepository>();
            services.AddScoped<IDeviceTypeRepository, DeviceTypeRepository>();
            services.AddScoped<IGlobalRepository, GlobalRepository>();
            services.AddScoped<IRoleManagementRepository, RoleManagementRepository>();
            services.AddScoped<IUserRepository, UserRepository>();

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "DeviceService.API", Version = "v1", Description = "CBNeNaira API helps Customers/Merchants to Deposit/Withdraw Naira/eNaira To/Fro their BankAccount/ewallet" });
                // Set the comments path for the Swagger JSON and UI.
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);

                c.AddSecurityDefinition("Basic", new OpenApiSecurityScheme
                {
                    Description =
                    "Authorization header using the Basic Scheme (Username and Password). \r\n\r\n Enter 'Basic' [space] and then your token in the text input below.\r\n\r\nExample: \"Basic 12345abcdef\"",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Basic"
                });

                c.ResolveConflictingActions(apiDescriptions => apiDescriptions.First());
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IHttpContextAccessor httpContextAccessor, ILogger<Startup> logger)
        {
            LogWriter.Logger = logger;
            MyHttpContextAccessor.HttpContextAccessor = httpContextAccessor;

            //FOR LOGGING HTTP WEB REQUEST AND RESPONSES
            app.AddCustomRequestAndResponseLoggingMiddleware(); //PUT THIS ALWAYS AT THE START OF THE PIPELINE INCASE OF UNHANDLED EXCEPTIONS. BECAUSE UNHANDLED EXCEPTIONS CAUSE UNEXPECTED ERRORS WITH THIS MIDDLEWARE
            //FOR CATCHING UNHANDLED EXCEPTIONS
            app.AddCustomExceptionHandlerMiddleware();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            app.UseRouting();
            app.UseCors("CORSPolicy");

            if (env.IsDevelopment())
            {
                // Enable middleware to serve generated Swagger as a JSON endpoint.
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("./v1/swagger.json", "DeviceService.API v1"));
            }

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                //endpoints.MapControllerRoute("swagger", "{controller=swagger}/{action=Index}");
            });
        }
    }
}
