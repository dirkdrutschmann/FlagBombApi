using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;

namespace APIPacBomb
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
            services.AddSingleton<Interfaces.IUserDatabaseService>(provider =>
            {                
                return new Services.UserDatabaseService(
                    Configuration["Db:user"],
                    Configuration["Db:pass"],
                    Configuration["Db:server"],
                    Configuration["Db:database"],
                    provider.GetService<ILoggerFactory>().CreateLogger<Services.DatabaseService>()
                );               
            });

            services.AddSingleton<Interfaces.IGameDatabaseService>(provider => {
                return new Services.GameDatabaseService(
                    Configuration["Db:user"],
                    Configuration["Db:pass"],
                    Configuration["Db:server"],
                    Configuration["Db:database"],
                    provider.GetService<ILoggerFactory>().CreateLogger<Services.GameDatabaseService>()
                );
            });

            services.AddSingleton<Interfaces.ISessionService, Services.SessionService>();

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(option => {
                    option.TokenValidationParameters = new TokenValidationParameters()
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = Configuration["Jwt:Issuer"],
                        ValidAudience = Configuration["Jwt:Issuer"],
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["Jwt:Key"]))
                    };
                });
            
            services.AddControllers().AddNewtonsoftJson(
                options =>
                {
                    options.SerializerSettings.Converters.Add(new Classes.TileJsonConverter());
                }
            );

            services.AddSwaggerGen( opt => 
            {
                opt.SwaggerDoc("1.0", new OpenApiInfo()
                {
                    Version = "1.0",
                    Title = "PacBomb API",
                    Description = "API fuer FlagBomb-Steuerung"
                });
                
                var xmlFilename = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
                opt.IncludeXmlComments(System.IO.Path.Combine(System.AppContext.BaseDirectory, xmlFilename));
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseAuthentication();

            app.UseRouting();

            app.UseSwagger();

            app.UseSwaggerUI(opt => 
            {
                opt.SwaggerEndpoint("/swagger/1.0/swagger.json", "1.0");
                opt.RoutePrefix = string.Empty;
            });

            app.UseAuthorization();

            app.UseWebSockets();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
