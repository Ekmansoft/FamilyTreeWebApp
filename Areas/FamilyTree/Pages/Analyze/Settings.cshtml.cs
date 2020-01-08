using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using FamilyTreeLibrary.FamilyTreeStore;
//using FamilyStudioData.Controllers;
using FamilyTreeWebTools.Services;
using FamilyTreeWebTools.Data;
using FamilyTreeWebApp.Data;
using FamilyTreeWebApp.Services;

namespace FamilyTreeServices.Pages
{
  [Authorize]
  public class AnalyzeSettingsModel : PageModel
  {
    private static readonly TraceSource trace = new TraceSource("AnalyzeSettings", SourceLevels.Information);

    private readonly FamilyTreeDbContext _context;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly WebAppIdentity _appId;
    private readonly EmailSendSource _emailSendSource;

    public AnalyzeSettingsModel(FamilyTreeDbContext context, UserManager<IdentityUser> userManager, WebAppIdentity appId, EmailSendSource emailSendSource)
    {
      _userManager = userManager;
      _context = context;
      PageLoading = true;
      _appId = appId;
      _emailSendSource = emailSendSource;
    }

    public string Message { get; set; }

    //[BindProperty]
    //public string geni_access_token { get; set; }
    //[BindProperty]
    //public string geni_expires_in { get; set; }
    //[BindProperty]
    //public string gedcom_file_name { get; set; }

    [BindProperty]
    public AnalysisSettings Settings { get; set; }
    [BindProperty]
    public bool PageLoading { get; set; }
    [BindProperty]
    public bool TreeIsFinite { get; set; }

    /*public async Task<string> GetCurrentUserEmail() 
    {
      IdentityUser usr = await GetCurrentUserAsync();
      trace.TraceData(TraceEventType.Information, 0, "usr=" + usr);
      trace.TraceData(TraceEventType.Information, 0, "usr.mail:" + usr.Email);
      return usr?.Email;
    }    */
    //private Task<IdentityUser> GetCurrentUserAsync() => _userManager.GetUserAsync(HttpContext.User);

    public IActionResult OnGet(string StartPersonXref)
    {
      trace.TraceData(TraceEventType.Information, 0, "AnalyzeSettingsModel.OnGet() start StartPersonXref:" + StartPersonXref);
      PageLoading = true;

      //string geni_access_token2 = HttpContext.Session.GetString("geni_access_token");
      //string token_expires_in2 = HttpContext.Session.GetString("token_expires_in");
      //string geni_refresh_token = HttpContext.Session.GetString("geni_refresh_token");
      string GedcomFilename2 = HttpContext.Session.GetString("GedcomFilename");
      string OriginalFilename2 = HttpContext.Session.GetString("OriginalFilename");

      trace.TraceData(TraceEventType.Information, 0, "Filename:" + GedcomFilename2 + " original file:" + OriginalFilename2 + ", startPerson:" + StartPersonXref);
      //trace.TraceData(TraceEventType.Information, 0, "gat:" + geni_access_token2 + ", gatei:" + token_expires_in2 + ", grt:" + geni_refresh_token + ", gfn:" + GedcomFilename2 + ", ofn:" + OriginalFilename2 + ", spn:" + StartPersonXref);

      //var curUser = _userManager.GetUserId(User);

      TreeIsFinite = true;

      WebAuthentication authenticationClass = null;
      if (string.IsNullOrEmpty(GedcomFilename2) || (GedcomFilename2 == "Geni.com"))
      {
        string userId = _userManager.GetUserId(this.User);

        authenticationClass = FamilyDbContextClass.CreateWebAuthentication(_context, userId, _appId);

        TreeIsFinite = false;
      }

      FamilyWebTree webTree = new FamilyWebTree(GedcomFilename2, authenticationClass);

      if(webTree == null)
      {
        //trace.TraceData(TraceEventType.Error, 0, "AnalyzeSettingsModel.OnGet() error webtree=null: Geni info: " + geni_access_token2 + " expiresIn:" + token_expires_in2);
        trace.TraceData(TraceEventType.Error, 0, "AnalyzeSettingsModel.OnGet() error webtree=null: Gedcom info: " + GedcomFilename2 + " orig:" + OriginalFilename2);
        Message = "No family tree found! Please check that you have enabled cookies!";
        return Page();
      }
      PageLoading = false;

      FamilyTreeContentClass content = webTree.GetFamilyTree().GetContents();
      if (content.families > 0)
      {
        Message = "Loaded family tree with " + content.individuals + " profiles and " + content.families + " families in.";
      }
      else
      {
        Message = "Loaded family tree with " + content.individuals + " profiles in.";
      }

      if ((StartPersonXref != null) && (StartPersonXref.Length > 0))
      {
        webTree.SetStartPerson(StartPersonXref);
      }

      if (!webTree.IsValid())
      {
        if (!String.IsNullOrEmpty(GedcomFilename2))
        {
          Message = "File upload error (Are cookies enabled?)";
        }
        else
        {
          Message = "Geni login failed (Are cookies enabled?)";
        }
        webTree.Dispose();
        return Redirect("/FamilyTree/UploadFiles/UploadFailed");
      }

      /*if((recheck_analysis_id != null) && (recheck_analysis_id > 0))
      {
        FileAnalysis analysis = new FileAnalysis(Settings, start_person_xref, _userManager, this.User);
        FamilyTreeContentClass contents = await analysis.UpdateAnalysis(recheck_analysis_id, geni_access_token, token_expires_in);

      }*/

      Settings = new AnalysisSettings();
      Settings.StartPersonXref = webTree.GetCurrentStartPerson();
      Settings.StartPersonName = webTree.GetPersonName(Settings.StartPersonXref);

      HttpContext.Session.SetString("StartPersonXref", webTree.GetCurrentStartPerson());
      HttpContext.Session.SetString("StartPersonName", webTree.GetPersonName(Settings.StartPersonXref));

      trace.TraceData(TraceEventType.Information, 0, "AnalyzeSettingsModel.OnGet() end: start person sp:" + Settings.StartPersonXref + " name:" + Settings.StartPersonName);
      webTree.Dispose();
      return Page();
    }

    public async Task<IActionResult>  OnPost()
    {
      trace.TraceData(TraceEventType.Information, 0, "AnalyzeSettingsModel.OnPost() start gen:" + Settings.GenerationsBack + "/" + Settings.GenerationsForward + " dup:" + Settings.DuplicateCheck);

      //string geni_access_token2 = HttpContext.Session.GetString("geni_access_token");
      //string geni_refresh_token = HttpContext.Session.GetString("geni_refresh_token");
      //string token_expires_in2 = HttpContext.Session.GetString("token_expires_in");
      string GedcomFilename2 = HttpContext.Session.GetString("GedcomFilename");
      string OriginalFilename2 = HttpContext.Session.GetString("OriginalFilename");
      Settings.StartPersonXref = HttpContext.Session.GetString("StartPersonXref");
      Settings.StartPersonName = HttpContext.Session.GetString("StartPersonName");

      //trace.TraceData(TraceEventType.Information, 0, "gat:" + geni_access_token2 + ", grt:" + geni_refresh_token + ", gatei:" + token_expires_in2 + ", gfn:" + GedcomFilename2 + ", ofn:" + OriginalFilename2 + ", spn:" + Settings.StartPersonXref);

      if (this.User != null)
      {
        IdentityUser user = await _userManager.GetUserAsync(this.User);
        string userEmail = await _userManager.GetEmailAsync(user);
        string userId = _userManager.GetUserId(this.User);

        if(Settings.CheckWholeFile)
        {
          Settings.StartPersonName = "";
          Settings.StartPersonXref = "";
          Settings.GenerationsBack = -1;
          Settings.GenerationsForward = -1;
        }

        int jobId = FamilyDbContextClass.SaveJobDescription(_context, Settings, userId, userEmail, OriginalFilename2, GedcomFilename2);
        //FileAnalysis analysis = new FileAnalysis(Settings.StartPersonXref, userId, userEmail);
        //FamilyTreeContentClass contents = analysis.AnalyzeWebTree(_context, Settings, OriginalFilename2, GedcomFilename2, geni_access_token2, geni_refresh_token, token_expires_in2);

        if (jobId < 0)
        {
          trace.TraceData(TraceEventType.Error, 0, "AnalyzeSettingsModel.OnPost() end error loading tree");
          return Page();
        }
        else
        {
          FamilyDbContextClass.StartupCheck(_context, _appId, _emailSendSource);

          HttpContext.Session.SetString("OriginalFilename", "");
          HttpContext.Session.SetString("GedcomFilename", "");
        }
      }
      else
      {
        trace.TraceData(TraceEventType.Error, 0, "AnalyzeSettingsModel.OnPost() end no user or start person");
        return Page();
      }
      string redirectTarget = "../AnalysisResultView/Index";
      //trace.TraceData(TraceEventType.Information, 0, "AnalyzeSettingsModel.OnPost() end redirect to " + redirectTarget);
      return Redirect(redirectTarget);
    }
  }
}
