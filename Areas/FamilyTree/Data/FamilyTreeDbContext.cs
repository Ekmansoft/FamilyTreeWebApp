using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using FamilyTreeWebTools.Data;

namespace FamilyTreeWebApp.Data
{
  public class FamilyTreeDbContext : DbContext
  {
    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
	    var configuration = new ConfigurationBuilder().SetBasePath(AppDomain.CurrentDomain.BaseDirectory).AddJsonFile("appsettings.json").Build();

      options.UseMySql(configuration.GetConnectionString("FamilyTreeDbContextConnection"));
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
