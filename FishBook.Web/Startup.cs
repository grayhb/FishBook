using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FishBook.DAL;
using FishBook.DAL.Interfaces;
using FishBook.DAL.Models;
using FishBook.DAL.Repositories;
using FishBook.DAL.Services;
using FishBook.Web.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;

namespace FishBook.Web
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
            services.AddControllersWithViews();

            #region DB Contexts

            var connectionString = Configuration.GetConnectionString("FishBookConnection");

            services.AddDbContext<FishBookContext>(opt =>
            {
                opt.UseSqlServer(connectionString);
            });

            #endregion

            #region Identity

            services.TryAddSingleton<ISystemClock, SystemClock>();

            var builder = services.AddIdentityCore<User>();
            var identityBuilder = new IdentityBuilder(builder.UserType, builder.Services);

            identityBuilder.AddEntityFrameworkStores<FishBookContext>();
            identityBuilder.AddSignInManager<SignInManager<User>>();

            #endregion

            #region Services

            services.AddScoped<IJwtGenerator, JwtGenerator>();
            //services.AddScoped<IUserService, UserService>();
            //services.AddScoped<IImageService, ImageService>();

            #endregion

            #region Repositories

            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IPhotoRepository, PhotoRepository>();

            #endregion

            #region Jwt Auth

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["TokenKey"]));

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(
                opt =>
                {
                    opt.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = key,
                        ValidateAudience = false,
                        ValidateIssuer = false,
                    };
                });
            
            #endregion

            services.AddCors(x =>
            {
                x.AddPolicy("CorsPolicy", builder =>
                {
                    builder.WithOrigins("http://localhost:8080", "http://192.168.1.98:8080").AllowAnyMethod().AllowAnyHeader().AllowCredentials();
                });
            });

            #region Services
            
            services.AddScoped<IPhotoService, PhotoService>();
            services.AddScoped<IUserService, UserService>();

            #endregion

            services.AddHttpContextAccessor();

            services.AddControllersWithViews().AddRazorRuntimeCompilation();

            services.AddMvc(option =>
            {
                option.EnableEndpointRouting = false;
                var policy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .Build();
                option.Filters.Add(new AuthorizeFilter(policy));
            }).SetCompatibilityVersion(CompatibilityVersion.Latest);

            services.AddSwaggerGen();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {

            app.UseDeveloperExceptionPage();

            app.UseStaticFiles();

            app.UseRouting();

            app.UseCors("CorsPolicy");

            app.UseAuthentication();

            app.UseMvcWithDefaultRoute();

            app.UseSwagger();
            app.UseSwaggerUI();
        }
    }
}
