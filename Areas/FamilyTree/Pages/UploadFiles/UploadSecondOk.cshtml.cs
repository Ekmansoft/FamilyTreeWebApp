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
  public class UploadSecondOkModel : PageModel
  {
    private static TraceSource trace = new TraceSource("UploadSecondOk", SourceLevels.Information);
    public string Message { get; set; }

    //[TempData]
    //public string Filename { get; set; }
    //[TempData]
    //public string OrigFilename { get; set; }
    //[TempData]
    //public int OrigFileSize { get; set; }

    public IActionResult OnGet()
    {
      string OrigFilename2 = HttpContext.Session.GetString("OriginalFilename2");
      string Filename2 = HttpContext.Session.GetString("GedcomFilename2");
      Message = "Your file " + OrigFilename2 +  " was successfully uploaded.";

      //HttpContext.Session.SetString("GedcomFilename", Filename);
      //HttpContext.Session.SetString("OriginalFilename", OrigFilename);

      return Redirect("/FamilyTree/Compare/Start");
    }
  }
}
