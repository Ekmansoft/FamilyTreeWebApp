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

  public class ExtendedFamilyInfo
  {
    public ExtendedFamilyInfo(string title)
    {
      Title = title;
      Spouses = new List<SimpleProfileInfo>();
      Children = new List<SimpleProfileInfo>();
    }

    public string Title;
    public IList<SimpleProfileInfo> Spouses;
    public IList<SimpleProfileInfo> Children;
  }
  public class ExtendedProfileInfo
  {
    public ExtendedProfileInfo()
    {
      MainProfile = new SimpleProfileInfo();
      SpouseInFamilies = new List<ExtendedFamilyInfo>();
      ChildInFamilies = new List<ExtendedFamilyInfo>();
    }

    public SimpleProfileInfo MainProfile;
    public IList<ExtendedFamilyInfo> SpouseInFamilies;
    public IList<ExtendedFamilyInfo> ChildInFamilies;
  }

  public class SimpleProfileInfo
  {

    public SimpleProfileInfo()
    {
      Name = "";
      Id = "";
      BirthDate = "";
      DeathDate = "";
      BirthPlace = "";
      DeathPlace = "";
      Url = "";

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

    public static string GetEventDateString(IndividualEventClass ev)
    {
      if (ev != null)
      {
        FamilyDateTimeClass date = ev.GetDate();

        if (date != null)
        {
          return date.ToString();
        }
      }
      return "";
    }
    public static string GetEventPlaceString(IndividualEventClass ev)
    {
      if (ev != null)
      {
        AddressClass address = ev.GetAddress();

        if (address != null)
        {
          return address.ToString();
        }

        PlaceStructureClass place = ev.GetPlace();

        if (place != null)
        {
          return place.ToString();
        }
      }
      return "";
    }

    public GeniShowProfileModel(WebAppIdentity appId, UserManager<IdentityUser> userManager, FamilyTreeDbContext context)
    {
      _context = context;
      _userManager = userManager;
      _appId = appId;
    }

    public static SimpleProfileInfo GetProfileInfo(IndividualClass profile)
    {
      SimpleProfileInfo Result = new SimpleProfileInfo();

      Result.Name = profile.GetName();
      Result.Id = profile.GetXrefName();
      IndividualEventClass birthEvent = profile.GetEvent(IndividualEventClass.EventType.Birth);
      IndividualEventClass deathEvent = profile.GetEvent(IndividualEventClass.EventType.Death);
      Result.BirthDate = GetEventDateString(birthEvent);
      Result.DeathDate = GetEventDateString(deathEvent);
      Result.BirthPlace = GetEventPlaceString(birthEvent);
      Result.DeathPlace = GetEventPlaceString(deathEvent);

      IList<string> urls = profile.GetUrlList();

      if (urls.Count > 0)
      {
        Result.Url = urls[0];
      }
      return Result;
    }

    public string Message { get; set; }

    public IndividualClass Profile { get; set; }

    public ExtendedProfileInfo ExtendedProfile { get; set; }

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
        return Page();
      }
      Profile = webTree.GetFamilyTree().GetIndividual(profileId);
      if (Profile == null)
      {
        Message = "Profile " + profileId + " not found";
        return Page();
      }
      ProfileData = GetProfileInfo(Profile);
      ExtendedProfile = new ExtendedProfileInfo();
      ExtendedProfile.MainProfile = GetProfileInfo(Profile);

      IList<FamilyXrefClass> spouseFamilies = Profile.GetFamilySpouseList();
      int familyCount = 1;
      foreach (FamilyXrefClass spouseFamXref in spouseFamilies)
      {
        ExtendedFamilyInfo family = new ExtendedFamilyInfo("Own family " + familyCount);

        // Add spouses in this marriage / relation
        FamilyClass SpouseFamily = webTree.GetFamilyTree().GetFamily(spouseFamXref.GetXrefName());
        IList<IndividualXrefClass> SpouseXrefs = SpouseFamily.GetParentList();
        foreach (IndividualXrefClass spouseXref in SpouseXrefs)
        {
          // Skip current profile when listing spouses
          if (spouseXref.GetXrefName() != profileId)
          {
            IndividualClass Spouse = webTree.GetFamilyTree().GetIndividual(spouseXref.GetXrefName());
            family.Spouses.Add(GetProfileInfo(Spouse));
          }
        }

        // Add children in this relation
        IList<IndividualXrefClass> ChildXrefs = SpouseFamily.GetChildList();
        foreach (IndividualXrefClass childXref in ChildXrefs)
        {
          // Skip current profile when listing children
          if (childXref.GetXrefName() != profileId)
          {
            IndividualClass Child = webTree.GetFamilyTree().GetIndividual(childXref.GetXrefName());
            family.Children.Add(GetProfileInfo(Child));
          }
        }
        ExtendedProfile.SpouseInFamilies.Add(family);
        familyCount++;
      }
      IList<FamilyXrefClass> childFamilies = Profile.GetFamilyChildList();
      familyCount = 1;
      foreach (FamilyXrefClass focusFamXref in childFamilies)
      {
        ExtendedFamilyInfo family = new ExtendedFamilyInfo("Raised in family " + familyCount);
        FamilyClass FocusFamily = webTree.GetFamilyTree().GetFamily(focusFamXref.GetXrefName());

        // Add siblings in this family
        IList<IndividualXrefClass> SpouseXrefs = FocusFamily.GetParentList();
        foreach (IndividualXrefClass spouseXref in SpouseXrefs)
        {
          // Skip current profile when listing spouses
          if (spouseXref.GetXrefName() != profileId)
          {
            IndividualClass Spouse = webTree.GetFamilyTree().GetIndividual(spouseXref.GetXrefName());
            family.Spouses.Add(GetProfileInfo(Spouse));
          }
        }

        // Add children in this relation
        IList<IndividualXrefClass> ChildXrefs = FocusFamily.GetChildList();
        foreach (IndividualXrefClass childXref in ChildXrefs)
        {
          // Skip current profile when listing children
          if (childXref.GetXrefName() != profileId)
          {
            IndividualClass Child = webTree.GetFamilyTree().GetIndividual(childXref.GetXrefName());
            family.Children.Add(GetProfileInfo(Child));
          }
        }
        ExtendedProfile.ChildInFamilies.Add(family);
        familyCount++;
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
