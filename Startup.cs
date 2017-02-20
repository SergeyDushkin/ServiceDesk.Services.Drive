using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using servicedesk.Services.Drive.Dal;
using Microsoft.EntityFrameworkCore;

//using System;
//using System.Security.Cryptography.X509Certificates;
//using System.IO;
//using Microsoft.IdentityModel.Tokens;
//using RawRabbit.Attributes;
//using RawRabbit.Common;
//using RawRabbit.vNext;
//using RawRabbit.vNext.Logging;

namespace servicedesk.Services.Drive
{
    public class Startup
    {
        public string EnvironmentName { get; set; }
        public IConfiguration Configuration { get; set; }

        public Startup(IHostingEnvironment env)
        {
            EnvironmentName = env.EnvironmentName.ToLowerInvariant();
            var builder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables()
                .SetBasePath(env.ContentRootPath);

            Configuration = builder.Build();
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<DriveDbContext>(options => options.UseNpgsql(Configuration.GetConnectionString("DriveDatabase")));

            services.AddCors(x => x.AddPolicy("corsGlobalPolicy", policy => {
                policy.AllowAnyHeader();
                policy.AllowAnyMethod();
                policy.AllowAnyOrigin();
                policy.AllowCredentials();
            }));
            
            //services.AddAuthentication();
            //services.AddAuthorization(options =>
            //{
            //    options.AddPolicy("user", policy => policy.RequireClaim("role", "OPERATOR"));
            //});

            
			//services
			//	.AddRawRabbit(
			//		Configuration.GetSection("RawRabbit"),
			//		ioc => ioc
			//			.AddSingleton(LoggingFactory.ApplicationLogger))
			//			.AddSingleton<IConfigurationEvaluator, AttributeConfigEvaluator>();

            services.AddMvc();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            //System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

            var serilogLogger = new LoggerConfiguration()
                .Enrich.WithProperty("Application", "ServiceDesk.Services.Drive")
                .ReadFrom.Configuration(Configuration)
                .CreateLogger();

            loggerFactory.AddSerilog(serilogLogger);
            loggerFactory.AddConsole();

            app.UseCors("corsGlobalPolicy");

            app.ApplicationServices.GetService<DriveDbContext>().Database.EnsureCreated();

            //app.UseMiddleware(typeof(ErrorHandlingMiddleware));
            //app.UseSecureLinkMiddleware();
            //app.UseJwtBearerAuthentication(GetJwtBearerOptions(env));

            app.UseMvc(routes => routes.MapRoute("default", "{controller=Home}/{action=Get}/{id?}"));
        }

        /*
        private JwtBearerOptions GetJwtBearerOptions(IHostingEnvironment env)
        {
            var cert = new X509Certificate2(Path.Combine(env.ContentRootPath, 
                Configuration.GetSection("Authentication:Certificate").Value), Configuration.GetSection("Authentication:CertificatePassword").Value);

            var options = new JwtBearerOptions
            {
                Authority = Configuration.GetSection("Authentication:Authority").Value,
                Audience = Configuration.GetSection("Authentication:Audience").Value,

                AutomaticAuthenticate = true,
                AutomaticChallenge = true,
                RequireHttpsMetadata = false,

                TokenValidationParameters = { 
                   IssuerSigningKey = new X509SecurityKey(cert), 
                   ValidateIssuerSigningKey = true, 
                   ValidateLifetime = true, 
                   ClockSkew = TimeSpan.Zero,
                   ValidateIssuer = false,
                   ValidateAudience = false
                }
            };

            return options;
        }*/
    }
}
