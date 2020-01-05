using System;
using System.Diagnostics;
using AspNetCoreWebApp3.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

[assembly: HostingStartup(typeof(AspNetCoreWebApp3.Areas.Identity.IdentityHostingStartup))]
namespace AspNetCoreWebApp3.Areas.Identity
{
  public class IdentityHostingStartup : IHostingStartup
  {
    static readonly TraceSource trace = new TraceSource("IdentityHostingStartup", SourceLevels.Information);
    public void Configure(IWebHostBuilder builder)
    {
      builder.ConfigureServices((context, services) =>
      {
        string sqlServerString = context.Configuration.GetConnectionString("IdentityContextConnection");

        if (!string.IsNullOrEmpty(sqlServerString))
        {
          services.AddDbContext<IdentityContext>(options =>
              options.UseSqlServer(sqlServerString));
          trace.TraceInformation("Initialized database sqlServer:" + sqlServerString);
        }
        else
        {
          string mySqlServerString = context.Configuration.GetConnectionString("IdentityContextConnectionMySql");

          if (!string.IsNullOrEmpty(mySqlServerString))
          {
            services.AddDbContext<IdentityContext>(options =>
                options.UseSqlServer(mySqlServerString));
            trace.TraceInformation("Initialized database mySql:" + mySqlServerString);
          }
          else
          {
            trace.TraceData(TraceEventType.Warning, 0,  "No configured database ");
          }
        }

        services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = false)
            .AddEntityFrameworkStores<IdentityContext>();
      });
    }
  }
}