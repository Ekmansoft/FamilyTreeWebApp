using FamilyTreeWebTools.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Diagnostics;

namespace FamilyTreeWebApp.Data
{
  public class FamilyTreeDbContext : DbContext
  {
    static readonly TraceSource trace = new TraceSource("FamilyTreeDbContext", SourceLevels.Warning);
    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
      var configuration = new ConfigurationBuilder().SetBasePath(AppDomain.CurrentDomain.BaseDirectory).AddJsonFile("appsettings.json").Build();

      string sqlServerString = configuration.GetConnectionString("FamilyTreeDbContextConnection");

      if (!string.IsNullOrEmpty(sqlServerString))
      {
        options.UseSqlServer(sqlServerString);
        trace.TraceInformation("Initialized database sqlServer:" + sqlServerString);
      }
      else
      {
        string mySqlServerString = configuration.GetConnectionString("FamilyTreeDbContextConnectionMySql");

        if (!string.IsNullOrEmpty(mySqlServerString))
        {
          options.UseMySql(mySqlServerString, MySqlServerVersion.AutoDetect(mySqlServerString));
          trace.TraceInformation("Initialized database mySql:" + mySqlServerString);
        }
        else
        {
          trace.TraceData(TraceEventType.Warning, 0, "No configured database provider");
        }
      }
      //options.UseMySql(configuration.GetConnectionString("FamilyTreeDbContextConnection"));
      //options.UseSqlite(configuration.GetConnectionString("DefaultConnection"));
      //options.UseMySql(configuration.GetConnectionString("MySqlConnection"));
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
      modelBuilder.Entity<Profile>()
          .HasMany<Issue>(p => p.Issues);
      modelBuilder.Entity<Issue>()
          .HasMany<IssueLink>(i => i.IssueLinks);
    }

    public DbSet<Analysis> Analyses { get; set; }

    public DbSet<Issue> Issues { get; set; }

    public DbSet<IssueLink> IssueLinks { get; set; }

    public DbSet<FamilyTree> FamilyTrees { get; set; }

    public DbSet<Profile> Profiles { get; set; }

    public DbSet<UserInformation> UserInformations { get; set; }

    public DbSet<AppSetting> AppSettings { get; set; }

  }
}
