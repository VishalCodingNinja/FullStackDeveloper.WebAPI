using System;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.IO;
using System.Reflection;
using FullStackDeveloper.WebAPI.Models;

namespace FullStackDeveloper.WebAPI
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
			//Inject AppSettings
			services.Configure<ApplicationSettings>(Configuration.GetSection("ApplicationSettings"));

			services.AddMvcCore().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

			services.AddDbContext<AuthenticationContext>(options =>
			options.UseSqlServer(Configuration.GetConnectionString("IdentityConnection")));

			services.AddDefaultIdentity<ApplicationUser>()
				.AddRoles<IdentityRole>()
				.AddEntityFrameworkStores<AuthenticationContext>();

			services.Configure<IdentityOptions>(options =>
			{
				options.Password.RequireDigit = false;
				options.Password.RequireNonAlphanumeric = false;
				options.Password.RequireLowercase = false;
				options.Password.RequireUppercase = false;
				options.Password.RequiredLength = 4;
			}
			);

			services.AddCors();

			//Jwt Authentication

			var key = Encoding.UTF8.GetBytes(Configuration["ApplicationSettings:JWT_Secret"].ToString());

			services.AddAuthentication(x =>
			{
				x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
				x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
				x.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
			}).AddJwtBearer(x =>
			{
				x.RequireHttpsMetadata = false;
				x.SaveToken = false;
				x.TokenValidationParameters = new TokenValidationParameters
				{
					ValidateIssuerSigningKey = true,
					IssuerSigningKey = new SymmetricSecurityKey(key),
					ValidateIssuer = false,
					ValidateAudience = false,
					ClockSkew = TimeSpan.Zero
				};
			});
			services.AddSwaggerGen(c =>
			{
				c.SwaggerDoc("v1", new OpenApiInfo
				{
					Version = "v1",
					Title = "FullStackDeveloper.API",
					Description = "Its an API which manages users of FullStackDeveloper",
					TermsOfService = new Uri("https://localhost:4200/terms"),
					Contact = new OpenApiContact
					{
						Name = "Shayne Boyer",
						Email = string.Empty,
						Url = new Uri("https://localhost:4200"),
					},
					License = new OpenApiLicense
					{
						Name = "Under Licensed to FullStackDeveloper",
						Url = new Uri("hhttps://localhost:4200"),
					}
				});

				// Set the comments path for the Swagger JSON and UI.
				var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
				var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
				c.IncludeXmlComments(xmlPath);
			});
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IHostingEnvironment env)
		{
			app.UseSwagger();
			app.UseSwaggerUI(c =>
			{
				c.SwaggerEndpoint("/swagger/v1/swagger.json", "FullStackDeveloperAPI");
				//c.RoutePrefix = string.Empty;
			});
			app.Use(async (ctx, next) =>
			{
				await next();
				if (ctx.Response.StatusCode == 204)
				{
					ctx.Response.ContentLength = 0;
				}
			});
			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}
			app.UseCors(builder =>
			builder.WithOrigins(Configuration["ApplicationSettings:Client_URL"].ToString())
			.AllowAnyHeader()
			.AllowAnyMethod()
			);
			app.UseAuthentication();
			app.UseMvc();
		}
	}
}
