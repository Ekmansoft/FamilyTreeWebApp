using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FamilyTreeServices.Pages
{
  [Authorize]
  public class AnalyzeRunModel : PageModel
  {
    private static readonly TraceSource trace = new TraceSource("AnalyzeRun", SourceLevels.Information);
    public string Message { get; set; }

    public void OnGet()
    {
      Message = "Analyzing.... get";
      trace.TraceData(TraceEventType.Information, 0, "AnalyzeRunModel.OnGet()" + Message);
    }
    public void OnPost()
    {
      Message = "Analyzing.... post";
      trace.TraceData(TraceEventType.Information, 0, "AnalyzeRunModel.OnPost()" + Message);
    }
  }
}
