using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using FamilyTreeWebTools.Data;
using FamilyTreeWebTools.Services;
using FamilyTreeWebApp.Data;
using Microsoft.AspNetCore.Identity;
using FamilyTreeWebApp.Services;
using FamilyTreeLibrary.FamilyData;
using FamilyTreeTools.FamilyTreeSanityCheck;

namespace FamilyTreeServices.Pages
{
  [Authorize]

  public class SimpleProfileInfo
  {

    public SimpleProfileInfo(string id)
    {
      Name = "";
      Id = id;
      BirthDate = "";
      DeathDate = "";
      BirthPlace = "";
      DeathPlace = "";

    }
    public string Name { get; set; }
    public string Id { get; set; }
    public string BirthDate { get; set; }
    public string BirthPlace { get; set; }
    public string DeathDate { get; set; }
    public string DeathPlace { get; set; }
    public string Url { get; set; }
  }

  public class GeniShowProfileModel : PageModel
  {
    private static readonly TraceSource trace = new TraceSource("GeniShowProfileModel", SourceLevels.Verbose);
    private readonly FamilyTreeDbContext _context;
    private readonly WebAppIdentity _appId;
    private readonly UserManager<IdentityUser> _userManager;

    public GeniShowProfileModel(WebAppIdentity appId, UserManager<IdentityUser> userManager, FamilyTreeDbContext context)
    {
      _context = context;
      _userManager = userManager;
      _appId = appId;
    }

    public static SimpleProfileInfo GetProfileInfo(IndividualClass profile)
    {
      SimpleProfileInfo Result = new SimpleProfileInfo(profile.GetXrefName());

      Result.Name = profile.GetName();
      Result.BirthDate = AncestorStatistics.GetEventDateString(profile, IndividualEventClass.EventType.Birth);
      Result.DeathDate = AncestorStatistics.GetEventDateString(profile, IndividualEventClass.EventType.Death);
      //Result.BirthPlace = "";
      //Result.BirthPlace = "";

      IList<string> urls = profile.GetUrlList();

      if (urls.Count > 0)
      {
        Result.Url = urls[0];
      }
      return Result;
    }

    public string Message { get; set; }

    public IndividualClass Profile { get; set; }

    public SimpleProfileInfo ProfileData { get; set; }

    public IActionResult OnGet(string treeName, string profileId)
    {
      trace.TraceData(TraceEventType.Information, 0, "GeniShowProfileModel.OnGet():" + _appId.AppId + " " + profileId);

      if (profileId == null)
      {
        return NotFound();
      }


      string userId = _userManager.GetUserId(this.User);

      WebAuthentication authenticationClass = FamilyDbContextClass.CreateWebAuthentication(_context, userId, _appId);

      FamilyWebTree webTree = new FamilyWebTree(treeName, authenticationClass);

      if (profileId == null)
      {
        Message = "No profile id";
        return NotFound();
      }
      Profile = webTree.GetFamilyTree().GetIndividual(profileId);
      if (Profile == null)
      {
        Message = "Profile " + profileId + " not found";
        return Page();
      }

      return Page();
    }
    public IActionResult OnPost()
    {
      Message = "Your Login to Geni. post";
      WebAuthentication appAuthentication = new WebAuthentication(_userManager.GetUserId(this.User), _appId.AppId, _appId.AppSecret, 
        FamilyDbContextClass.UpdateGeniAuthentication);
      trace.TraceData(TraceEventType.Information, 0, "GeniShowProfileModel.OnPost() " + _appId.AppId);
      return Page();
    }
  }
}
