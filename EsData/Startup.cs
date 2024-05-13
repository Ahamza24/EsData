using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using EsData.Data;
using System.Text.Json.Serialization;
using System.Text.Json;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Identity;
using EsData.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace EsData
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
            services.AddControllers()
                .AddNewtonsoftJson(x =>
                {
                    x.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                });


            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "EsData", Version = "v1" });
            });

            services.AddDbContext<EsDataContext>(options =>
                    options.UseSqlServer(Configuration.GetConnectionString("EsDataContext"))
                .EnableSensitiveDataLogging()
                .LogTo(x => System.Diagnostics.Debug.WriteLine(x)));

            #region Add CORS  
            services.AddCors(options => options.AddPolicy("Cors", builder =>
            {
                builder
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader();
            }));
            #endregion

            services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                options.SignIn.RequireConfirmedAccount = false;
            })
                .AddEntityFrameworkStores<EsDataContext>()
                .AddDefaultTokenProviders();

            services.AddAuthentication(o =>
            {
                o.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                o.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters =
                     new TokenValidationParameters
                     {
                         ValidateIssuer = true,
                         ValidateAudience = true,
                         ValidateLifetime = true,
                         ValidateIssuerSigningKey = true,

                         ValidIssuer = Configuration["JWT:ValidIssuer"],
                         ValidAudience = Configuration["JWT:ValidAudience"],
                         IssuerSigningKey =
                          JwtSecurityKey.Create(Configuration["JWT:Secret"]),

                         ClockSkew = TimeSpan.Zero
                     };
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "EsData v1"));
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            this.CreateInitialRolesAndUsersAsync(userManager, roleManager)
               .Wait();
        }

        private async Task CreateInitialRolesAndUsersAsync(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            try
            {
                string adminRoleName = Roles.Admin;
                if (!await roleManager.RoleExistsAsync(adminRoleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(adminRoleName));
                }

                string staffRoleName = Roles.Worker;
                if (!await roleManager.RoleExistsAsync(staffRoleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(staffRoleName));
                }

                string premiumRoleName = Roles.Premium;
                if (!await roleManager.RoleExistsAsync(premiumRoleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(premiumRoleName));
                }

                var user = new ApplicationUser();
                user.UserName = "admin@webapimovie.bolton.ac.uk";
                user.Email = user.UserName;
                user.Firstname = "Jen";
                user.Surename = "Erik";

                string password = "Pa$$w0rd!";

                if (await userManager.FindByNameAsync(user.UserName) == null)
                {
                    var createSuccess = await userManager.CreateAsync(user, password);
                    if (createSuccess.Succeeded)
                    {
                        await userManager.AddToRoleAsync(user, adminRoleName);
                        await userManager.SetLockoutEnabledAsync(user, false);
                    }
                    else
                    {
                        throw new Exception(createSuccess.Errors.FirstOrDefault().ToString());
                    }
                }

            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }
    }
}
