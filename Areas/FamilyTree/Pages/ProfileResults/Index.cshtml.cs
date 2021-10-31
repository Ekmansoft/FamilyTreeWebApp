using FamilyTreeWebApp.Data;
using FamilyTreeWebTools.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace FamilyTreeServices.Pages.ProfileResults
{
  public class IndexModel : PageModel
  {
    private readonly FamilyTreeDbContext _context;
    private static TraceSource trace = new TraceSource("ProfileResults", SourceLevels.Warning);

    public IndexModel(FamilyTreeDbContext context)
    {
      _context = context;
    }



    public class IssueView
    {
      public string Description;
      public string Status;
      public int Id;
      public IList<string> Parameters;
    }

    public class ProfileView
    {
      public int ProfileId;
      public string Name;
      public string Birth;
      public string Death;
      public string Url;
      public string RelationDistance;
      public IList<IssueView> Issues;
    }

    public enum FilterType
    {
      InexactDates,
      MissingBirthDate,
      MissingDeathDate,
      MissingDeathDateEmigrated,
      MissingMarriageDate,
      UnknownSex,
      UnknownLocation,
      //EventErrors,
      ParentIssues,
      MissingPartner,
      FewChildren,
      Twins,
      //ChildIssues,
      UnreasonableDates,
      OldPrivateProfile,
      Duplicate,
      MissingPartnerMitigated,
      MissingMother,
      MissingFather,
      Hidden,
      AllStates
    }

    public class FilterSettings
    {
      public FilterSettings()
      {
        values = new List<bool>();

        foreach (FilterType type in Enum.GetValues(typeof(FilterType)))
        {
          values.Add(true);
        }


      }

      public IList<bool> values;

      public void Update(FilterType type, bool value)
      {
        values[(int)type] = value;
      }
      public bool Get(FilterType type)
      {
        return values[(int)type];
      }

      public override string ToString()
      {
        return string.Join(",", values);
      }
      public void FromString(string str)
      {
        string[] splits = str.Split(",");

        if (splits.Length == Enum.GetValues(typeof(FilterType)).Length)
        {
          foreach (FilterType type in Enum.GetValues(typeof(FilterType)))
          {
            //result = String.Join(",", Get(type));
            bool value = false;
            if (bool.TryParse(splits[(int)type], out value))
            {
              Update(type, value);
            }
          }
        }
      }
    }
    [BindProperty]
    public FilterSettings FilterSettingOnPage { get; set; }
    [BindProperty]
    public bool AllStates { get; set; }
    [BindProperty]
    public AnalysisResults Results { get; set; }
    [BindProperty]
    public Analysis analysisData { set; get; }
    [BindProperty]
    public int NumberOfItems { set; get; }
    [BindProperty]
    public int ItemsPerPage { set; get; } = 500;

    FilterType Categorize(Issue.IssueType type)
    {
      switch (type)
      {
        case Issue.IssueType.ParentLimitMin:
        case Issue.IssueType.MotherLimitMax:
        case Issue.IssueType.FatherLimitMax:
        case Issue.IssueType.EventLimitMin:
        case Issue.IssueType.EventLimitMax:
          return FilterType.UnreasonableDates;
        case Issue.IssueType.OldPrivateProfile:
          return FilterType.OldPrivateProfile;
        case Issue.IssueType.ParentsMissing:
        case Issue.IssueType.ParentsProblem:
        case Issue.IssueType.MarriageProblem:
        case Issue.IssueType.NoOfChildrenMax:
        case Issue.IssueType.DaysBetweenChildren:
          return FilterType.ParentIssues;
        case Issue.IssueType.MissingMother:
          return FilterType.MissingMother;
        case Issue.IssueType.MissingFather:
          return FilterType.MissingFather;
        case Issue.IssueType.Twins:
          return FilterType.Twins;
        case Issue.IssueType.NoOfChildrenMin:
          return FilterType.FewChildren;
        case Issue.IssueType.MissingPartner:
          return FilterType.MissingPartner;
        case Issue.IssueType.MissingPartnerMitigated:
          return FilterType.MissingPartnerMitigated;
        case Issue.IssueType.UnknownBirth:
        case Issue.IssueType.UnknownBirthDeath:
          return FilterType.MissingBirthDate;
        case Issue.IssueType.UnknownDeath:
          return FilterType.MissingDeathDate;
        case Issue.IssueType.UnknownDeathEmigrated:
          return FilterType.MissingDeathDateEmigrated;
        case Issue.IssueType.UnknownSex:
          return FilterType.UnknownSex;
        case Issue.IssueType.MissingWeddingDate:
          return FilterType.MissingMarriageDate;
        case Issue.IssueType.InexactBirthDeath:
          return FilterType.InexactDates;
        case Issue.IssueType.UnknownLocation:
        case Issue.IssueType.ShortAddress:
          return FilterType.UnknownLocation;
        case Issue.IssueType.DuplicateCheck:
          return FilterType.Duplicate;
        case Issue.IssueType.GenerationLimited:
          return FilterType.Hidden;
      }
      return FilterType.Hidden;
    }

    bool IsVisible(Issue.IssueType type, FilterSettings settings)
    {
      return settings.Get(Categorize(type));
    }

    public IDictionary<int, ProfileView> Profiles { get; set; }


    /*public static string CreateUrl(int AnalysisId, int PageNo)
    {
      return "<a href=\"./Index?AnalysisId=" + AnalysisId + "&Page=" + PageNo + "\">Page " + PageNo + "</a>\n";
    }*/

    public List<SelectListItem> GetPageSizeSelectList()
    {
      List<SelectListItem> listItems = new List<SelectListItem>();
      IList<int> listItemsInt = new List<int>();
      //bool selectedFound = false;

      trace.TraceData(TraceEventType.Information, 0, "Selected1 = " + NumberOfItems);
      listItemsInt.Add(100);
      listItemsInt.Add(250);
      listItemsInt.Add(500);
      listItemsInt.Add(1000);

      foreach (int item in listItemsInt)
      {
        SelectListItem listitem = new SelectListItem(item.ToString(), item.ToString());
        if (item == NumberOfItems)
        {
          listitem.Selected = true;
          trace.TraceData(TraceEventType.Information, 0, "Selected = " + NumberOfItems);
          //selectedFound = true;
        }
        listItems.Add(listitem);
      }
      //if (!selectedFound)
      //{
      //  SelectListItem listitem = new SelectListItem(NumberOfItems.ToString(), NumberOfItems.ToString());
      //  listitem.Selected = true;
      //  trace.TraceData(TraceEventType.Information, 0, "Selected2 = " + NumberOfItems);
      //  listItems.Add(listitem);
      //}
      return listItems;
    }


    public int GetPageNumber(int ItemNo)
    {
      return (ItemNo / ItemsPerPage);
    }

    public int UpdateItemsPerPage(string itemsPerPageStr)
    {
      int pageSize = 0;

      if (Int32.TryParse(itemsPerPageStr, out pageSize))
      {
        if (pageSize != 0)
        {
          return pageSize;
        }
        else
        {
          trace.TraceData(TraceEventType.Warning, 0, "Error parsing page size [" + itemsPerPageStr + "]");
        }
      }
      else
      {
        trace.TraceData(TraceEventType.Warning, 0, "Error TryParse page size [" + itemsPerPageStr + "]");
      }
      return 500;
    }

    private string ExtractId(string url)
    {
      int ix = url.LastIndexOf('/');

      if ((ix > 0) && (ix < url.Length))
      {
        return url.Substring(ix + 1);
      }
      return null;
    }

    private string CreateCompareLink(string url1, string url2)
    {
      string id1 = ExtractId(url1);
      string id2 = ExtractId(url2);

      if ((id1 != null) && (id2 != null))
      {
        return "https://www.geni.com/merge/compare/" + id1 + "?return=match&to=" + id2;
      }
      return null;
    }


    public void DoWork(int AnalysisId, int Page, FilterSettings filterSettings)
    {
      trace.TraceData(TraceEventType.Information, 0, "AnalysisId = " + AnalysisId);

      foreach (FilterType type in Enum.GetValues(typeof(FilterType)))
      {
        trace.TraceData(TraceEventType.Information, 0, ": " + type.ToString() + " " + filterSettings.Get(type));
      }

      IQueryable<Analysis> analysis = _context.Analyses.Where(i => i.Id == AnalysisId);

      Results = new AnalysisResults();

      if (analysis != null)
      {
        analysisData = analysis.First<Analysis>();

        if ((analysisData != null) && (analysisData.Results != null))
        {
          Results = AnalysisResults.FromJson(analysisData.Results);
        }
      }
      IList<int> filterList = new List<int>();

      foreach (Issue.IssueType type in Enum.GetValues(typeof(Issue.IssueType)))
      {
        if (IsVisible(type, filterSettings))
        {
          filterList.Add((int)type);
        }
      }

      trace.TraceData(TraceEventType.Information, 0, "Running query Id:" + AnalysisId + " page:" + Page + " filter length:" + filterList.Count);

      var iLinks = (from il in _context.IssueLinks
                    from i in _context.Issues
                    from p in _context.Profiles
                    where (il.AnalysisId == AnalysisId) && (il.IssueId == i.Id) && (i.ProfileId == p.Id) && filterList.AsEnumerable().Contains((int)i.Type)
                    orderby il.Time
                    select new { p.Name, p.Birth, p.Death, p.Url, i.Id, i.Parameters, i.Status, i.ProfileId, i.Description, i.Type, il.RelationDistance }).ToList();

      NumberOfItems = iLinks.Count;

      trace.TraceData(TraceEventType.Information, 0, "result " + NumberOfItems);

      Profiles = new Dictionary<int, ProfileView>();

      ProfileView pv = null;
      int profileNo = 0, issueNo = 0, itemNumber = 0;

      foreach (var link in iLinks)
      {
        if (AllStates || (link.Status == Issue.IssueStatus.Identified))
        {
          if (Page == GetPageNumber(itemNumber++))
          {
            if (!Profiles.ContainsKey(link.ProfileId))
            {
              pv = new ProfileView
              {
                Name = link.Name,
                Birth = link.Birth,
                Death = link.Death,
                ProfileId = link.ProfileId,
                RelationDistance = link.RelationDistance,
                Url = link.Url,
                Issues = new List<IssueView>()
              };
              profileNo++;
              Profiles.Add(link.ProfileId, pv);
            }
            else
            {
              pv = Profiles[link.ProfileId];
            }

            IssueView issueView = new IssueView
            {
              Description = link.Description,
              Id = link.Id,
              Status = link.Status.ToString(),
              Parameters = new List<string>()
            };
            //v.problems += prob.Description + "  <br/>\n\r\n";
            if ((link.Parameters != null) && (link.Parameters.Length > 0))
            {
              string[] parameters = link.Parameters.Split(";");

              foreach (string param in parameters)
              {
                if (param.Length > 0)
                {
                  //string CompareLink = CreateCompareLink(link.Url, param);
                  string CompareLink = "./IssueResults/MergeDuplicate?id=" + link.Id;

                  if (CompareLink != null)
                  {
                    issueView.Parameters.Add(CompareLink);
                  }
                  else
                  {
                    issueView.Parameters.Add(param);
                  }
                }
              }
            }
            issueNo++;
            pv.Issues.Add(issueView);
          }
        }
      }
      trace.TraceData(TraceEventType.Information, 0, "DidWork() " + profileNo + " " + issueNo);
    }

    public void OnGet(int AnalysisId, int? PageNumber)
    {
      trace.TraceData(TraceEventType.Information, 0, "OnGet() " + AnalysisId + " page " + PageNumber);
      //FilterSettings filterSettings = new FilterSettings();
      FilterSettingOnPage = new FilterSettings();
      int RealPage = 0;

      if (PageNumber != null)
      {
        RealPage = (int)PageNumber;
      }

      string filterStr = null;
      string itemsPerPageStr = null;
      Request.Cookies.TryGetValue("FilterSettings", out filterStr);
      Request.Cookies.TryGetValue("ItemsPerPage", out itemsPerPageStr);

      if (!string.IsNullOrEmpty(filterStr))
      {
        trace.TraceData(TraceEventType.Information, 0, "Cookie:" + filterStr);
        FilterSettingOnPage.FromString(filterStr);
      }
      if (!string.IsNullOrEmpty(itemsPerPageStr))
      {
        ItemsPerPage = UpdateItemsPerPage(itemsPerPageStr);
      }


      DoWork(AnalysisId, RealPage, FilterSettingOnPage);
    }
    public void OnPost(int AnalysisId, bool ParentIssues,
                                       bool MissingMother,
                                       bool MissingFather,
                                       bool MissingPartner,
                                       bool MissingPartnerMitigated,
                                       bool Duplicates,
                                       bool Twins,
                                       bool FewChildren,
                                       bool MissingBirthDate,
                                       bool MissingDeathDate,
                                       bool MissingDeathDateEmigrated,
                                       bool MissingMarriageDate,
                                       bool UnknownSex,
                                       bool InexactDates,
                                       bool UnreasonableDates,
                                       bool OldPrivateProfile,
                                       bool UnknownLocation,
                                       bool Hidden,
                                       bool AllStates,
                                       string PageSize)
    {
      trace.TraceData(TraceEventType.Information, 0, "OnPost() " + AnalysisId);
      FilterSettingOnPage = new FilterSettings();

      FilterSettingOnPage.Update(FilterType.ParentIssues, ParentIssues);
      FilterSettingOnPage.Update(FilterType.MissingMother, MissingMother);
      FilterSettingOnPage.Update(FilterType.MissingFather, MissingFather);
      FilterSettingOnPage.Update(FilterType.MissingPartner, MissingPartner);
      FilterSettingOnPage.Update(FilterType.MissingPartnerMitigated, MissingPartnerMitigated);
      FilterSettingOnPage.Update(FilterType.Duplicate, Duplicates);
      FilterSettingOnPage.Update(FilterType.MissingBirthDate, MissingBirthDate);
      FilterSettingOnPage.Update(FilterType.MissingDeathDate, MissingDeathDate);
      FilterSettingOnPage.Update(FilterType.MissingDeathDateEmigrated, MissingDeathDateEmigrated);
      FilterSettingOnPage.Update(FilterType.MissingMarriageDate, MissingMarriageDate);
      FilterSettingOnPage.Update(FilterType.UnknownSex, UnknownSex);
      FilterSettingOnPage.Update(FilterType.InexactDates, InexactDates);
      FilterSettingOnPage.Update(FilterType.Twins, Twins);
      FilterSettingOnPage.Update(FilterType.FewChildren, FewChildren);
      FilterSettingOnPage.Update(FilterType.UnreasonableDates, UnreasonableDates);
      FilterSettingOnPage.Update(FilterType.OldPrivateProfile, OldPrivateProfile);
      FilterSettingOnPage.Update(FilterType.UnknownLocation, UnknownLocation);
      FilterSettingOnPage.Update(FilterType.Hidden, Hidden);
      FilterSettingOnPage.Update(FilterType.AllStates, AllStates);
      this.AllStates = AllStates;
      //this.ItemsPerPage = Convert.ToInt32(PageSize);
      trace.TraceData(TraceEventType.Warning, 0, "Page size = " + PageSize + " " + NumberOfItems);

      NumberOfItems = UpdateItemsPerPage(PageSize);

      trace.TraceData(TraceEventType.Warning, 0, "Page size = " + PageSize + " " + NumberOfItems);

      CookieOptions option = new CookieOptions();
      option.Expires = DateTime.Now.AddYears(5);

      //(Cooki filterCookie = new HttpCookie();
      trace.TraceData(TraceEventType.Information, 0, "Cookie:" + FilterSettingOnPage.ToString());
      Response.Cookies.Append("FilterSettings", FilterSettingOnPage.ToString(), option);
      Response.Cookies.Append("ItemsPerPage", ItemsPerPage.ToString(), option);
      DoWork(AnalysisId, 0, FilterSettingOnPage);
    }
  }
}
