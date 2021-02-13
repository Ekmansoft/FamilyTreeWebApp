using FamilyTreeLibrary.FamilyTreeStore;
using FamilyTreeWebApp.Data;
using FamilyTreeWebApp.Services;
using FamilyTreeWebTools.Compare;
using FamilyTreeWebTools.Data;
using FamilyTreeWebTools.Email;
//using FamilyStudioData.Controllers;
using FamilyTreeWebTools.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace FamilyTreeServices.Pages
{
  [Authorize]
  public class CompareStartModel : PageModel
  {
    private static readonly TraceSource trace = new TraceSource("CompareStartModel ", SourceLevels.Information);

    private readonly FamilyTreeDbContext _context;
    private readonly WebAppIdentity _appId;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly EmailSendSource _emailSendSource;

    public CompareStartModel(FamilyTreeDbContext context, UserManager<IdentityUser> userManager, WebAppIdentity appId, EmailSendSource emailSendSource)
    {
      _userManager = userManager;
      _context = context;
      _appId = appId;
      _emailSendSource = emailSendSource;
      //PageLoading = true;
    }

    public string Message { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
      trace.TraceData(TraceEventType.Information, 0, "AnalyzeSettingsModel.OnGet() start:");
      //PageLoading = true;

      string GedcomFilename = HttpContext.Session.GetString("GedcomFilename");
      string OriginalFilename = HttpContext.Session.GetString("OriginalFilename");
      string GedcomFilename2 = HttpContext.Session.GetString("GedcomFilename2");
      string OriginalFilename2 = HttpContext.Session.GetString("OriginalFilename2");

      string userId = _userManager.GetUserId(this.User);

      WebAuthentication authenticationClass = null;
      if (string.IsNullOrEmpty(GedcomFilename) || (GedcomFilename == "Geni.com"))
      {
        authenticationClass = FamilyDbContextClass.CreateWebAuthentication(_context, userId, _appId);
      }


      FamilyWebTree webTree1 = new FamilyWebTree(GedcomFilename, authenticationClass);

      if (webTree1 == null)
      {
        trace.TraceData(TraceEventType.Error, 0, "AnalyzeSettingsModel.OnGet() error webtree=null: Gedcom info: " + GedcomFilename + " orig:" + OriginalFilename);
        Message = "No family tree found! Please check that you have enabled cookies!";
        return Page();
      }
      FamilyWebTree webTree2 = new FamilyWebTree(GedcomFilename2, authenticationClass);

      if (webTree2 == null)
      {
        trace.TraceData(TraceEventType.Error, 0, "AnalyzeSettingsModel.OnGet() error second webtree=null: Gedcom info: " + GedcomFilename2 + " orig:" + OriginalFilename2);
        Message = "No family tree found! Please check that you have enabled cookies!";
        return Page();
      }
      //PageLoading = false;

      FamilyTreeContentClass content1 = webTree1.GetFamilyTree().GetContents();
      if (content1.families > 0)
      {
        Message = "onget \nLoaded first family tree with " + content1.individuals + " profiles and " + content1.families + " families in.";
      }
      else
      {
        Message = "onget \nLoaded first family tree with " + content1.individuals + " profiles in.";
      }

      FamilyTreeContentClass content2 = webTree2.GetFamilyTree().GetContents();

      Message += "\nLoaded second family tree with " + content2.individuals + " profiles and " + content2.families + " families in.";

      if (!webTree1.IsValid())
      {
        if (!String.IsNullOrEmpty(GedcomFilename))
        {
          Message = "File 1 upload error (Are cookies enabled?)";
        }
        else
        {
          Message = "Geni login failed (Are cookies enabled?)";
        }
        webTree1.Dispose();
        webTree2.Dispose();
        return Page();
      }

      if (!webTree2.IsValid())
      {
        Message = "Second file upload error (Are cookies enabled?)";

        webTree1.Dispose();
        webTree2.Dispose();
        return Page();
      }

      IdentityUser user = await _userManager.GetUserAsync(this.User);
      string userEmail = await _userManager.GetEmailAsync(user);

      HttpContext.Session.SetString("GedcomFilename", "");
      HttpContext.Session.SetString("OriginalFilename", "");

      DateTime startTime = DateTime.Now;

      FileCompare compareMachine = new FileCompare();
      //compareMachine.CompareFiles(_context, webTree1, webTree2);
      IList<FileCompare.MatchingProfiles> list = compareMachine.CompareFiles(webTree2, webTree1);

      DateTime endTime = DateTime.Now;
      webTree1.Dispose();
      webTree2.Dispose();
      Message += "\nComparison done. Found " + list.Count + " matches";

      string emailString = EmailExportClass.ExportDuplicatesHtml(list, startTime, endTime, OriginalFilename, OriginalFilename2);
      SendMailClass.SendMail(_emailSendSource.Address, _emailSendSource.CredentialAddress, _emailSendSource.CredentialPassword,
         userEmail, "Comparison between " + OriginalFilename + " and " + OriginalFilename2, emailString);
      //Message += "\nComparison done!";

      return Page();
    }

  }
}
