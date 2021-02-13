using FamilyTreeLibrary.FamilyData;
using FamilyTreeWebApp.Data;
using FamilyTreeWebApp.Services;
using FamilyTreeWebTools.Data;
using FamilyTreeWebTools.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;
using System.Diagnostics;

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
      MarriageDate = "";
    }

    public string Title;
    public IList<SimpleProfileInfo> Spouses;
    public IList<SimpleProfileInfo> Children;
    public string MarriageDate;
  }
  public class ExtendedProfileInfo
  {
    public ExtendedProfileInfo()
    {
      //Title = "";
      MainProfile = new SimpleProfileInfo();
      SpouseInFamilies = new List<ExtendedFamilyInfo>();
      ChildInFamilies = new List<ExtendedFamilyInfo>();
    }

    //public string Title;
    public SimpleProfileInfo MainProfile;
    public IList<ExtendedFamilyInfo> SpouseInFamilies;
    public IList<ExtendedFamilyInfo> ChildInFamilies;
  }

  public class SimpleProfileInfo
  {

    public SimpleProfileInfo()
    {
      Name = "";
      Sex = "";
      Id = "";
      BirthDate = "";
      DeathDate = "";
      BirthPlace = "";
      DeathPlace = "";
      Url = "";

    }
    public string Name { get; set; }
    public string Sex { get; set; }
    public string Id { get; set; }
    public string BirthDate { get; set; }
    public string BirthPlace { get; set; }
    public string DeathDate { get; set; }
    public string DeathPlace { get; set; }
    public string Url { get; set; }
  }

  public class GeniShowProfileModel : PageModel
  {
    private static readonly TraceSource trace = new TraceSource("GeniShowProfileModel", SourceLevels.Warning);
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

      trace.TraceData(TraceEventType.Information, 0, "Profile " + profile.GetXrefName() + " " + profile.GetName());
      Result.Name = profile.GetName();
      Result.Id = profile.GetXrefName();
      IndividualEventClass birthEvent = profile.GetEvent(IndividualEventClass.EventType.Birth);
      IndividualEventClass deathEvent = profile.GetEvent(IndividualEventClass.EventType.Death);
      Result.BirthDate = GetEventDateString(birthEvent);
      Result.DeathDate = GetEventDateString(deathEvent);
      Result.BirthPlace = GetEventPlaceString(birthEvent);
      Result.DeathPlace = GetEventPlaceString(deathEvent);
      switch (profile.GetSex())
      {
        case IndividualClass.IndividualSexType.Unknown:
          Result.Sex = "-";
          break;
        case IndividualClass.IndividualSexType.Male:
          Result.Sex = "m";
          break;
        case IndividualClass.IndividualSexType.Female:
          Result.Sex = "f";
          break;
      };

      IList<string> urls = profile.GetUrlList();

      if (urls.Count > 0)
      {
        Result.Url = urls[0];
      }
      return Result;
    }

    public static string GetLocalLink(string id)
    {
      return "./ShowProfile?treeName=Geni.com&&profileId=" + id;
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
      foreach (FamilyXrefClass focusFamXref in spouseFamilies)
      {
        ExtendedFamilyInfo family = new ExtendedFamilyInfo("F" + familyCount);

        // Add spouses in this marriage / relation
        FamilyClass FocusFamily = webTree.GetFamilyTree().GetFamily(focusFamXref.GetXrefName());
        IList<IndividualXrefClass> SpouseXrefs = FocusFamily.GetParentList();
        IndividualEventClass marriage = FocusFamily.GetEvent(IndividualEventClass.EventType.FamMarriage);
        if ((marriage != null) && marriage.GetDate().ValidDate())
        {
          DateTime marriageDate = marriage.GetDate().ToDateTime();
          family.MarriageDate = marriageDate.ToString();
        }
        foreach (IndividualXrefClass spouseXref in SpouseXrefs)
        {
          trace.TraceData(TraceEventType.Information, 0, "Spouse " + spouseXref.GetXrefName() + " " + familyCount);
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
          trace.TraceData(TraceEventType.Information, 0, "Child " + childXref.GetXrefName() + " " + familyCount);
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
        ExtendedFamilyInfo family = new ExtendedFamilyInfo("");
        FamilyClass FocusFamily = webTree.GetFamilyTree().GetFamily(focusFamXref.GetXrefName());

        IndividualEventClass marriage = FocusFamily.GetEvent(IndividualEventClass.EventType.FamMarriage);
        if ((marriage != null) && marriage.GetDate().ValidDate())
        {
          DateTime marriageDate = marriage.GetDate().ToDateTime();
          family.MarriageDate = marriageDate.ToString();
        }
        // Add siblings in this family
        IList<IndividualXrefClass> SpouseXrefs = FocusFamily.GetParentList();
        foreach (IndividualXrefClass spouseXref in SpouseXrefs)
        {
          trace.TraceData(TraceEventType.Information, 0, "Parent " + spouseXref.GetXrefName() + " " + familyCount);
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
          trace.TraceData(TraceEventType.Information, 0, "Raised in fam child " + childXref.GetXrefName() + " " + familyCount);
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
