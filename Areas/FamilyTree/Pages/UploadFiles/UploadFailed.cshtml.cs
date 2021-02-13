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
      Message = "Your file " + OrigFilename + " was was not successfully decoded. (Please note that cookies must be allowed)";
      if (OrigFilename.ToLower().IndexOf(".ged") < 0)
      {
        Message += " The filename does not seem to end with *.GED. Is it really a GEDCOM file?";
      }
      HttpContext.Session.SetString("GedcomFilename", "");
      HttpContext.Session.SetString("OriginalFilename", "");
    }
    public void OnPost()
    {
      Message = "Your file " + OrigFilename + " was was not successfully decoded. (Please note that cookies must be allowed)";
    }
  }
}
