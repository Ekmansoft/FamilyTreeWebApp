using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FamilyTreeServices.Pages
{
  [Authorize]
  public class ReanalyzeRunModel : PageModel
  {
    private static readonly TraceSource trace = new TraceSource("ReanalyzeRun", SourceLevels.Warning);
    public string Message { get; set; }

    //private readonly UserManager<IdentityUser> _userManager;

    public ReanalyzeRunModel()
    {
      //_userManager = userManager;
    }

    public ActionResult OnGet(int AnalysisId)
    {
      //Message = "ReanalyzeRunModel.OnGet()";
      if(AnalysisId > 0)
      {
      }
      trace.TraceData(TraceEventType.Information, 0, "ReanalyzeRunModel.OnGet()");
      return Page();
    }
    public static void OnPost()
    {
      //Message = "Analyzing.... post";
      trace.TraceData(TraceEventType.Information, 0, "ReanalyzeRunModel.OnPost()");
    }
  }
}
