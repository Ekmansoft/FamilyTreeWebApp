using Ekmansoft.FamilyTree.WebApp.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

[assembly: HostingStartup(typeof(AspNetCoreApp3.Models.FamilyTreeHostingStartup))]
namespace AspNetCoreApp3.Models
{
  public class FamilyTreeHostingStartup : IHostingStartup
  {
    public void Configure(IWebHostBuilder builder)
    {
      builder.ConfigureServices((context, services) =>
      {
        services.AddDbContext<FamilyTreeDbContext>();
      });

    }
  }
}