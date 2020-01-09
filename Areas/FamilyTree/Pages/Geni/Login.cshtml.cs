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

namespace FamilyTreeServices.Pages
{
  //[Authorize]
  public class GeniLoginModel : PageModel
  {
    private static readonly TraceSource trace = new TraceSource("GeniLogin", SourceLevels.Verbose);
    private readonly WebAppIdentity _appId;
    private readonly UserManager<IdentityUser> _userManager;

    public GeniLoginModel(WebAppIdentity appId, UserManager<IdentityUser> userManager)
    {
      _userManager = userManager;
      _appId = appId;
    }

    public string Message { get; set; }


    public IActionResult OnGet()
    {
      trace.TraceData(TraceEventType.Information, 0, "GeniLoginModel.OnGet():" + _appId.AppId);
      Message = "Your Login to Geni.";
      string userId = _userManager.GetUserId(this.User);

      WebAuthentication appAuthentication = new WebAuthentication(userId, _appId.AppId, _appId.AppSecret, FamilyDbContextClass.UpdateGeniAuthentication);
      string redirectTarget = appAuthentication.getWebRedirect();

      trace.TraceData(TraceEventType.Information, 0, "GeniLoginModel.OnGet() redirect to " + redirectTarget);
      return Redirect(redirectTarget);
    }
    public IActionResult OnPost()
    {
      Message = "Your Login to Geni. post";
      WebAuthentication appAuthentication = new WebAuthentication(_userManager.GetUserId(this.User), _appId.AppId, _appId.AppSecret, 
        FamilyDbContextClass.UpdateGeniAuthentication);
      string redirectTarget = appAuthentication.getWebRedirect();
      trace.TraceData(TraceEventType.Information, 0, "GeniLoginModel.OnPost() " + _appId.AppId + " redirect to " + redirectTarget);
      return Redirect(redirectTarget);
    }
  }
}
