using Ekmansoft.FamilyTree.WebApp.Data;
using Ekmansoft.FamilyTree.WebApp.Services;
using Ekmansoft.FamilyTree.WebTools.Data;
using Ekmansoft.FamilyTree.WebTools.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Diagnostics;

namespace FamilyTreeServices.Pages
{
  [Authorize]
  public class SearchPersonModel : PageModel
  {
    private readonly static TraceSource trace = new TraceSource("Search", SourceLevels.Information);
    public string Message { get; set; }

    private readonly FamilyTreeDbContext _context;
    private readonly WebAppIdentity _appId;
    private readonly UserManager<IdentityUser> _userManager;

    public SearchPersonModel(FamilyTreeDbContext context, UserManager<IdentityUser> userManager, WebAppIdentity appId)
    {
      _userManager = userManager;
      _context = context;
      _appId = appId;
    }

    [BindProperty]
    public IList<FamilyWebTree.SimplePerson> SimplePeople { get; set; }

    [TempData]
    public string SearchString { get; set; }

    private string GetGeniId(string searchString)
    {
      string geniIdString = "";

      foreach (char ch in searchString)
      {
        if (char.IsDigit(ch))
        {
          geniIdString += ch;
        }
        else
        {
          if (char.IsLetter(ch))
          {
            geniIdString = "";
          }
        }
      }
      if (geniIdString.Length > 5)
      {
        trace.TraceData(TraceEventType.Information, 0, "Numbers string returned " + geniIdString.Length + " " + geniIdString);
        return geniIdString;
      }
      return null;
    }


    public void OnGet(string SearchString)
    {
      Message = "Search.... person (get)";

      var GedcomFilename = HttpContext.Session.GetString("GedcomFilename");

      SimplePeople = new List<FamilyWebTree.SimplePerson>();

      if ((SearchString != null) && (SearchString.Length > 0))
      {
        string userId = _userManager.GetUserId(this.User);

        WebAuthentication authenticationClass = null;

        if (string.IsNullOrEmpty(GedcomFilename) || (GedcomFilename == "Geni.com"))
        {
          authenticationClass = FamilyDbContextClass.CreateWebAuthentication(_context, userId, _appId);
        }

        FamilyWebTree webTree = new FamilyWebTree(GedcomFilename, authenticationClass);

        if (webTree != null)
        {
          string guidId = GetGeniId(SearchString);
          int peopleCount = 0;

          if (!string.IsNullOrEmpty(guidId))
          {
            string profileId = "profile-g" + guidId;
            trace.TraceData(TraceEventType.Information, 0, "Search guid " + guidId + " => " + profileId);
            Ekmansoft.FamilyTree.Library.FamilyData.IndividualClass person = webTree.GetFamilyTree().GetIndividual(profileId);

            if (person != null)
            {
              trace.TraceData(TraceEventType.Information, 0, "Found person from guid " + person.GetName() + " " + person.GetXrefName());
              FamilyWebTree.SimplePerson simplePerson = new FamilyWebTree.SimplePerson(person.GetName(), person.GetXrefName(),
                "(" + person.GetDate(Ekmansoft.FamilyTree.Library.FamilyData.IndividualEventClass.EventType.Birth) + " - " +
                person.GetDate(Ekmansoft.FamilyTree.Library.FamilyData.IndividualEventClass.EventType.Death) + ")");

              SimplePeople.Add(simplePerson);
              peopleCount++;
            }
            else
            {
              trace.TraceData(TraceEventType.Warning, 0, "Noone found from guid " + guidId);
            }
          }

          if (SimplePeople.Count == 0)
          {
            IEnumerator<FamilyWebTree.SimplePerson> results = webTree.SearchPerson(SearchString);

            while (results.MoveNext())
            {
              SimplePeople.Add(results.Current);
              peopleCount++;
            }
          }
          webTree.Dispose();
          trace.TraceData(TraceEventType.Information, 0, "Found " + peopleCount);
        }
        else
        {
          trace.TraceData(TraceEventType.Error, 0, "Error webtree == null ");
        }
      }

    }
  }
}
