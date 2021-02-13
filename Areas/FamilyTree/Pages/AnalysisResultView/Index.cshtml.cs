using FamilyTreeWebApp.Data;
using FamilyTreeWebApp.Services;
//using Microsoft.EntityFrameworkCore;
using FamilyTreeWebTools.Data;
using FamilyTreeWebTools.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
//using System.Threading.Tasks;
//using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;
//using Microsoft.AspNetCore.Identity.UI;
using System.Globalization;
using System.Linq;

namespace FamilyTreeServices.Pages.AnalysisResultView
{
  [Authorize]
  public class IndexModel : PageModel
  {
    private readonly WebAppIdentity _appId;
    private readonly EmailSendSource _emailSendSource;
    public static string FormatDateString(DateTime time)
    {
      DateTime now = DateTime.Now;

      TimeSpan delta = now - time;

      if (delta.TotalDays < 7)
      {
        return time.ToString("HH:mm dddd", CultureInfo.InvariantCulture);
      }
      if (delta.TotalDays < 30)
      {
        return time.ToString("MM-dd", CultureInfo.InvariantCulture);
      }
      return time.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
    }

    public static string FormatDeltaString(TimeSpan delta)
    {
      string result = "";
      int fieldCount = 0;

      if (delta.TotalDays >= 1)
      {
        result += Math.Truncate(delta.TotalDays) + "d ";
        delta = delta.Add(TimeSpan.FromDays(-Math.Truncate(delta.TotalDays)));
        fieldCount++;
      }
      if (delta.TotalHours >= 1)
      {
        result += Math.Truncate(delta.TotalHours) + "h ";
        delta = delta.Add(TimeSpan.FromHours(-Math.Truncate(delta.TotalHours)));
        fieldCount++;
      }
      if ((fieldCount < 2) && (delta.TotalMinutes >= 1))
      {
        result += Math.Truncate(delta.TotalMinutes) + "m ";
        delta = delta.Add(TimeSpan.FromMinutes(-Math.Truncate(delta.TotalMinutes)));
        fieldCount++;
      }
      if ((fieldCount < 2) && (delta.TotalSeconds >= 1))
      {
        result += Math.Truncate(delta.TotalSeconds) + "s ";
        fieldCount++;
      }
      result = result.Trim(' ');

      if (result.Length == 0)
      {
        result = "0s";
      }
      return "(" + result + ")";
    }

    public static bool InProgress(DateTime endTime, int JobId)
    {
      if (endTime.Year == 1)
      {
        int progress = ProgressDbClass.Instance.GetProgress(JobId);
        if (progress >= 0)
        {
          return true;
        }
      }
      return false;
    }

    public static bool HasFinished(DateTime endTime)
    {
      if (endTime.Year == 1)
      {
        return false;
      }
      return true;
    }

    public static string FormatDatesString(DateTime startTime, DateTime endTime, int JobId, int StartCount)
    {
      if (endTime.Year == 1)
      {
        int progress = ProgressDbClass.Instance.GetProgress(JobId);
        if (progress >= 0)
        {
          return FormatDateString(startTime) + "\nworking:" + progress + "%";
        }
        if (StartCount < 5)
        {
          return FormatDateString(startTime) + "\nwaiting...";
        }
        if (StartCount == 1000)
        {
          return FormatDateString(startTime) + "\npaused";
        }
        return FormatDateString(startTime) + "\nFailed " + StartCount + " times...";
      }
      TimeSpan delta = endTime - startTime;
      if (delta.TotalDays < 300)
      {
        return FormatDateString(startTime) + "\n" + FormatDeltaString(delta);
      }
      return FormatDateString(startTime) + " - " + FormatDateString(endTime);
    }


    public static string CheckGedcomLink(Analysis analysis)
    {
      string json = analysis.Results;
      if ((json == null) || (json.Length == 0))
      {
        return "";
      }

      AnalysisResults results = AnalysisResults.FromJson(json);

      if (results != null)
      {

        string result = "";

        if ((results.ExportedGedcomName != null) && (results.ExportedGedcomName.Length > 0))
        {
          result += "<a href=\"/FamilyTree/UploadFiles/Download?id=" + analysis.Id + "\">Gedcom</a>";
        }
        return result;
      }
      return "(no result data)";
    }

    public static bool CheckDatabaseLink(Analysis analysis)
    {
      string json = analysis.Results;
      if ((json == null) || (json.Length == 0))
      {
        return false;
      }

      AnalysisResults results = AnalysisResults.FromJson(json);

      if (results != null)
      {
        if (results.DbResults != null)
        {
          return true;
        }
      }
      return false;
    }

    public static string CheckJsonLink(Analysis analysis)
    {
      if ((analysis.Settings == null) || (analysis.Settings.Length == 0))
      {
        return "";
      }
      if ((analysis.Results == null) || (analysis.Results.Length == 0))
      {
        return "";
      }

      AnalysisSettings settings = AnalysisSettings.FromJson(analysis.Settings);

      if (settings != null)
      {
        string result = "";

        if (settings.ExportJson)
        {
          result += "<a href=\"/FamilyTree/UploadFiles/Download?jsonId=" + analysis.Id + "\">Json</a>";
        }
        if (settings.ExportKml)
        {
          result += "&nbsp;<a href=\"/FamilyTree/UploadFiles/Download?kmlId=" + analysis.Id + "\">Kml</a>";
        }
        return result;
      }
      return "(no settings data)";
    }

    public static string FormatSettingsString(string json)
    {
      if ((json == null) || (json.Length == 0))
      {
        return "";
      }

      AnalysisSettings settings = AnalysisSettings.FromJson(json);

      if (settings != null)
      {
        string result = "";

        if (settings.CheckWholeFile)
        {
          result += "Whole file";
        }
        else
        {
          result += settings.GenerationsBack + "/" + settings.GenerationsForward + " generations";
        }

        if (settings.DuplicateCheck)
        {
          result += ", DupCheck";
        }
        if (settings.ExportGedcom)
        {
          result += ", Export GEDCOM";
        }
        if (settings.ExportJson)
        {
          result += ", Export Json";
        }
        if (settings.UpdateDatabase)
        {
          result += ", Database";
        }
        if (settings.SendEmail)
        {
          result += ", Email";
        }

        return result;
      }
      return "(no data)";
    }

    public static string FormatResultsString(string json)
    {
      if ((json == null) || (json.Length == 0))
      {
        return "";
      }
      AnalysisResults results = AnalysisResults.FromJson(json);

      if (results != null)
      {
        string result = "Searched " + results.SearchedProfiles + " profiles and " +
          results.SearchedFamilies + " families, found " + results.NoOfIssues + " issues in " + results.NoOfProfiles + " profiles";

        return result;
      }
      return "(no data)";

    }
    private readonly FamilyTreeDbContext _context;
    private readonly UserManager<IdentityUser> _userManager;

    public IndexModel(FamilyTreeDbContext context, UserManager<IdentityUser> userManager, WebAppIdentity appId, EmailSendSource emailSendSource)
    {
      _context = context;
      _userManager = userManager;
      _appId = appId;
      _emailSendSource = emailSendSource;
    }

    public IList<Analysis> Analysis { get; set; }

    public void OnGet(string userId)
    {
      if ((userId != null) && (userId == "-1"))
      {
        Analysis = _context.Analyses.OrderByDescending(a => a.Id).ToList();
      }
      else
      {
        var curUser = _userManager.GetUserId(User);

        Analysis = _context.Analyses.Where(a => a.UserId == curUser).OrderByDescending(a => a.Id).ToList();
      }
      FamilyDbContextClass.StartupCheck(_context, _appId, _emailSendSource);
    }
  }
}
