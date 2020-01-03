using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FamilyTreeServices.Pages
{
  [Authorize]
  public class UploadOkModel : PageModel
  {
    private readonly static TraceSource trace = new TraceSource("UploadOk", SourceLevels.Information);
    public string Message { get; set; }

    //[TempData]
    //public string Filename { get; set; }
    //[TempData]
    //public string OrigFilename { get; set; }
    //[TempData]
    //public int OrigFileSize { get; set; }

    public IActionResult OnGet()
    {
      string OrigFilename = HttpContext.Session.GetString("OriginalFilename");
      string Filename = HttpContext.Session.GetString("GedcomFilename");
      Message = "Your file " + OrigFilename +  " was successfully uploaded.";

      //HttpContext.Session.SetString("GedcomFilename", Filename);
      //HttpContext.Session.SetString("OriginalFilename", OrigFilename);

      return Redirect("/FamilyTree/Analyze/Settings");
    }
  }
}
