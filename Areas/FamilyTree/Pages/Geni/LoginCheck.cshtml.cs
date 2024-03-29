using Ekmansoft.FamilyTree.WebApp.Data;
using Ekmansoft.FamilyTree.WebApp.Services;
using Ekmansoft.FamilyTree.WebTools.Data;
using Ekmansoft.FamilyTree.WebTools.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Diagnostics;

namespace FamilyTreeServices.Pages
{
  [Authorize]
  public class GeniLoginCheckModel : PageModel
  {
    private static readonly TraceSource trace = new TraceSource("GeniLoginCheck", SourceLevels.Information);
    private readonly WebAppIdentity _appId;
    private readonly FamilyTreeDbContext _context;
    private readonly UserManager<IdentityUser> _userManager;
    public string Message { get; set; }

    public GeniLoginCheckModel(FamilyTreeDbContext context, WebAppIdentity appId, UserManager<IdentityUser> userManager)
    {
      _context = context;
      _userManager = userManager;
      _appId = appId;
    }


    //[TempData]
    //public string Filename { get; set; }

    public IActionResult OnGet()
    {
      Message = "Check Login status to Geni.";

      string redirectTarget = "/FamilyTree/Geni/Login";

      if ((_userManager != null) && (_context != null))
      {
        string userId = _userManager.GetUserId(this.User);
        int secondsLeft = FamilyDbContextClass.CheckGeniLoginStatus(_context, userId);

        trace.TraceData(TraceEventType.Information, 0, ".OnGet() time left " + secondsLeft);

        if (secondsLeft > 100)
        {
          redirectTarget = "/FamilyTree/Analyze/Settings";

          UserInformation userInfo = FamilyDbContextClass.FindUserInformation(_context, userId);

          if (userInfo != null)
          {
            HttpContext.Session.SetString("geni_access_token", userInfo.GeniAccessToken);
            HttpContext.Session.SetString("geni_refresh_token", userInfo.GeniRefreshToken);
            HttpContext.Session.SetString("token_expires_in", userInfo.GeniExpiresIn.ToString());
          }
        }
      }
      else
      {
        trace.TraceData(TraceEventType.Warning, 0, ".OnGet() missing context or usermanager");
      }
      trace.TraceData(TraceEventType.Information, 0, ".OnGet() redirect to " + redirectTarget);
      return Redirect(redirectTarget);
    }
    public IActionResult OnPost()
    {
      Message = "Your Login to Geni. post";
      WebAuthentication appAuthentication = new WebAuthentication(_userManager.GetUserId(this.User), _appId.AppId, _appId.AppSecret,
        FamilyDbContextClass.UpdateGeniAuthentication);
      string redirectTarget = appAuthentication.getAuthorizeUrl();
      trace.TraceData(TraceEventType.Information, 0, ".OnPost() redirect to " + redirectTarget);
      return Redirect(redirectTarget);
    }
  }
}
