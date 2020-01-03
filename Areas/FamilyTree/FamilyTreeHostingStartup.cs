using System;
using Microsoft.AspNetCore.Hosting;
using FamilyTreeWebTools.Services;
using FamilyTreeWebTools.Data;
using FamilyTreeWebApp.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

[assembly: HostingStartup(typeof(AspNetCoreApp3.Models.FamilyTreeHostingStartup))]
namespace AspNetCoreApp3.Models
{
  public class FamilyTreeHostingStartup : IHostingStartup
  {
    public void Configure(IWebHostBuilder builder)
    {
      builder.ConfigureServices((context, services) => {
        services.AddDbContext<FamilyTreeDbContext>();
      });

    }
  }
}