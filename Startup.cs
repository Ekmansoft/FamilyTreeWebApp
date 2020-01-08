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
using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.DataProtection;
using System.IO;
using Microsoft.AspNetCore.Mvc;

namespace FamilyTreeWebApp
{
  //public class EmailSender : IEmailSender
  //{
  //  public Task SendEmailAsync(string email, string subject, string message)
  //  {
  //    return Task.CompletedTask;
  //  }
  //}
  public class Startup
  {
    private static readonly TraceSource trace = new TraceSource("Startup", SourceLevels.Information);
    //private string _geniClientId = null;
    //private string _geniClientSecret = null;
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
      trace.TraceInformation("ConfigureServ-1");
      services.Configure<CookiePolicyOptions>(options =>
      {
        // This lambda determines whether user consent for non-essential cookies is needed for a given request.
        options.CheckConsentNeeded = context => true;
        options.MinimumSameSitePolicy = SameSiteMode.None;
      });

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

      Action<EmailSendSource> emailSendSource = (opt =>
      {
        opt.Address = Configuration["EmailSendSource:Address"];
        opt.CredentialAddress = Configuration["EmailSendSource:CredentialAddress"];
        opt.CredentialPassword = Configuration["EmailSendSource:CredentialPassword"];
      });
      services.Configure(emailSendSource);
      services.AddSingleton(resolver => resolver.GetRequiredService<IOptions<EmailSendSource>>().Value);

      services.AddDbContext<FamilyTreeDbContext>();
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
      services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_3_0).
          AddRazorPagesOptions(options =>
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
      trace.TraceInformation("ConfigureServ-2");
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
      trace.TraceInformation("Configure-1");
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

      trace.TraceInformation("Configure-2");
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
      trace.TraceInformation("Configure-3");
    }
  }

}
