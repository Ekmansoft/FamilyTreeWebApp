using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Diagnostics;

namespace FamilyTreeServices.Pages
{
  [Authorize]
  public class UploadFailedModel : PageModel
  {
    private static TraceSource trace = new TraceSource("UploadFailed", SourceLevels.Warning);
    public string Message { get; set; }

    [TempData]
    public string Filename { get; set; }
    [TempData]
    public string OrigFilename { get; set; }
    [TempData]
    public int OrigFileSize { get; set; }

    public void OnGet()
    {
      HttpContext.Session.SetString("GedcomFilename", "");
      HttpContext.Session.SetString("OriginalFilename", "");
      if (OrigFilename == null)
      {
        Message = " The filename is null. ";
        trace.TraceData(TraceEventType.Error, 0, "Error in file upload name is null");
        return;
      }
      Message = "Your file " + OrigFilename + " was not successfully decoded. (Please note that cookies must be allowed)";
      if (OrigFilename.ToLower().IndexOf(".ged") < 0)
      {
        Message += " The filename does not seem to end with *.GED. Is it really a GEDCOM file?";
      }
    }
    public void OnPost()
    {
      Message = "Your file " + OrigFilename + " was was not successfully decoded. (Please note that cookies must be allowed)";
    }
  }
}
