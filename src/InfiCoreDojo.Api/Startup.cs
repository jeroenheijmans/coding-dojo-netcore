﻿using System;
using System.Collections.Generic;
using System.Linq;
using InfiCoreDojo.DataAccess;
using InfiCoreDojo.DataAccess.InMemory;
using InfiCoreDojo.DataAccess.JsonFiles;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Swashbuckle.AspNetCore.Swagger;

namespace InfiCoreDojo.Api
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // Bootstrap dependency injection here.
        // Bootstrap frameworks and middleware here.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();
            services.AddRouting();
            services.AddSwaggerGen(c => { c.SwaggerDoc("v1", new Info { Title = "Infi Dojo API", Version = "v1" }); });

            if (Configuration.GetValue<bool>("UseInMemoryData"))
            {
                // Option A: in-memory data, resets on reboot
                services.AddSingleton<InMemoryDatabase>(InMemoryDatabase.Instance);
                services.AddScoped<ILevelDal, InMemoryLevelDal>();
                services.AddScoped<IPlayerDal, InMemoryPlayerDal>();
            }
            else
            {
                // Option B: json files based data, persists through reboot
                services.AddScoped<ILevelDal, JsonFileLevelDal>();
                services.AddScoped<IPlayerDal, JsonFilePlayerDal>();
            }
        }

        // Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseDefaultFiles();
            app.UseStaticFiles();

            app.UseSwagger(); // Json endpoint (required for UI)
            app.UseSwaggerUI(c => { c.SwaggerEndpoint("/swagger/v1/swagger.json", "InfiCoreDojoApi"); });

            // All other routes are done via attributes on Controllers (matter of taste).
            app.UseMvc(routes => routes.MapRoute("default", "{controller=home}/{action=Index}"));
        }
    }
}