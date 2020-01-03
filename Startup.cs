using AspNetCoreWebApp3.Models;
using FamilyTreeWebTools.Data;
using FamilyTreeWebApp.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using Microsoft.Extensions.Options;

namespace FamilyTreeWebApp
{
  public class Startup
  {
    //private string _geniClientId = null;
    //private string _geniClientSecret = null;
    public Startup(IConfiguration configuration)
    {
      Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    // This method gets called by the runtime. Use this method to add services to the container.
    public void ConfigureServices(IServiceCollection services)
    {
      services.AddDistributedMemoryCache();

      //_geniClientId = Configuration["Geni:ClientId"];
      //_geniClientSecret = Configuration["Geni:ClientSecret"];

      //WebAppIdentity appIdInfo = new WebAppIdentity { AppId = Configuration["Geni:ClientId"], AppSecret = Configuration["Geni:ClientSecret"] };

      Action<WebAppIdentity> appId = (opt =>
      {
        opt.AppId = Configuration["Geni:ClientId"];
        opt.AppSecret = Configuration["Geni:ClientSecret"];
      });
      services.Configure(appId);
      services.AddSingleton(resolver => resolver.GetRequiredService<IOptions<WebAppIdentity>>().Value);

      services.AddSession(options =>
      {
        // Set a short timeout for easy testing.
        options.IdleTimeout = TimeSpan.FromSeconds(10);
        options.Cookie.HttpOnly = true;
        // Make the session cookie essential
        options.Cookie.IsEssential = true;
      });

      services.AddRazorPages();

      //services.AddIdentity<IdentityUser, IdentityRole>().AddEntityFrameworkStores<IdentityContext>().AddDefaultTokenProviders();

      services.Configure<IdentityOptions>(options =>
      {
        // Default Password settings.
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequireUppercase = true;
        options.Password.RequiredLength = 6;
        options.Password.RequiredUniqueChars = 1;
      });
      services.AddDbContext<IdentityContext>();

      services.ConfigureApplicationCookie(options =>
      {
        options.Cookie.Name = "ImproveYourTree";
        options.Cookie.HttpOnly = true;
        options.ExpireTimeSpan = TimeSpan.FromDays(30);
        options.LoginPath = $"/Identity/Account/Login";
        options.LogoutPath = $"/Identity/Account/Logout";
        options.AccessDeniedPath = $"/Identity/Account/AccessDenied";
        options.ReturnUrlParameter = CookieAuthenticationDefaults.ReturnUrlParameter;
        options.SlidingExpiration = true;
      });
      services.AddMvc();

    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
      if (env.IsDevelopment())
      {
        app.UseDeveloperExceptionPage();
      }
      else
      {
        app.UseExceptionHandler("/Error");
        // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
        app.UseHsts();
      }

      app.UseHttpsRedirection();
      app.UseStaticFiles();
      app.UseSession();
      app.UseRouting();

      //app.UseForwardedHeaders(new ForwardedHeadersOptions
      //{
      //  ForwardedHeaders = Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedFor
      //});


      app.UseAuthentication();
      app.UseAuthorization();

      app.UseEndpoints(endpoints =>
      {
        endpoints.MapRazorPages();
      });
    }
  }

}
