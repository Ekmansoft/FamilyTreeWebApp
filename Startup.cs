using AspNetCoreWebApp3.Models;
using FamilyTreeWebApp.Data;
using FamilyTreeWebTools.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System;
using System.Diagnostics;
using System.IO;

namespace FamilyTreeWebApp
{
  public class Startup
  {
    private static readonly TraceSource trace = new TraceSource("Startup", SourceLevels.Information);
    public Startup(IConfiguration configuration)
    {
      trace.TraceInformation("Startup-1");
      Configuration = configuration;
      trace.TraceInformation("Startup-2");
    }

    public IConfiguration Configuration { get; }

    // This method gets called by the runtime. Use this method to add services to the container.
    public void ConfigureServices(IServiceCollection services)
    {
      trace.TraceInformation("ConfigureServices-start");
      services.Configure<CookiePolicyOptions>(options =>
      {
        // This lambda determines whether user consent for non-essential cookies is needed for a given request.
        options.CheckConsentNeeded = context => true;
        options.MinimumSameSitePolicy = SameSiteMode.None;
      });


      trace.TraceInformation("ConfigureService App:" + Configuration["Geni:ClientId"]);
      Action<WebAppIdentity> appId = (opt =>
      {
        opt.AppId = Configuration["Geni:ClientId"];
        opt.AppSecret = Configuration["Geni:ClientSecret"];
        trace.TraceInformation("ConfigureService App:" + opt.AppId + ":" + opt.AppSecret);
      });
      services.Configure(appId);
      services.AddSingleton(resolver => resolver.GetRequiredService<IOptions<WebAppIdentity>>().Value);

      trace.TraceInformation("ConfigureService Email:" + Configuration["EmailSendSource:Address"] + ":" +
        Configuration["EmailSendSource:CredentialAddress"]);
      Action<EmailSendSource> emailSendSource = (opt =>
      {
        opt.Address = Configuration["EmailSendSource:Address"];
        opt.CredentialAddress = Configuration["EmailSendSource:CredentialAddress"];
        opt.CredentialPassword = Configuration["EmailSendSource:CredentialPassword"];
        trace.TraceInformation("ConfigureService Email:" + opt.Address + ":" + opt.CredentialAddress);
      });
      services.Configure(emailSendSource);
      services.AddSingleton(resolver => resolver.GetRequiredService<IOptions<EmailSendSource>>().Value);

      services.AddDbContext<FamilyTreeDbContext>();
      //services.AddDbContext<ProfileDbContext>();
      services.AddDistributedMemoryCache();
      services.AddSession(options =>
      {
        // Set a short timeout for easy testing.
        options.IdleTimeout = TimeSpan.FromMinutes(60);
        options.Cookie.HttpOnly = true;
        //options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        //options.Cookie.SameSite = SameSiteMode.Strict;

        // Make the session cookie essential
        options.Cookie.IsEssential = true;
      });

      //services.AddRazorPages();

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
      services.AddMvc().AddRazorPagesOptions(options =>
          {
            //options.AllowAreas = true;
            options.Conventions.AuthorizeAreaFolder("Identity", "/Account/Manage");
            options.Conventions.AuthorizeAreaPage("Identity", "/Account/Logout");
            options.Conventions.AuthorizeAreaFolder("Identity", "/FamilyTree");
            //options.Conventions.AuthorizeAreaPage("Identity", "/FamilyTree/UploadFiles/Upload");
          }); ;

      services.AddDataProtection()
        .PersistKeysToFileSystem(new DirectoryInfo("./persisting-keys"));

      //services.AddSingleton<IEmailSender, EmailSender>();
      trace.TraceInformation("ConfigureServices-end");
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
      trace.TraceInformation("Configure-start");
      if (!env.IsDevelopment())
      {
        app.UseExceptionHandler("/Error");
        // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
        app.UseHsts();
      }

      app.UseHttpsRedirection();
      app.UseStaticFiles();
      app.UseSession();
      app.UseRouting();

      app.UseForwardedHeaders(new ForwardedHeadersOptions
      {
        ForwardedHeaders = Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedFor
      });


      app.UseAuthentication();
      app.UseAuthorization();

      app.UseEndpoints(endpoints =>
      {
        endpoints.MapRazorPages();
      });
      //app.MapRazorPages();
      //app.Run();
      trace.TraceInformation("Configure-end");
    }
  }

}
