using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace FamilyTreeServices.Pages
{
  [Authorize]
  public class UploadFileModel : PageModel
  {
    private static TraceSource trace = new TraceSource("UploadFileModel", SourceLevels.Verbose);
    private readonly IWebHostEnvironment _environment;

    public UploadFileModel(IWebHostEnvironment environment)
    {
      _environment = environment;
    }
    const string UploadDir = "~/tmp/";

    //[TempData]
    //public string Filename { get; set; }
    //[TempData]
    //public string OrigFilename { get; set; }
    //[TempData]
    //public int OrigFileSize { get; set; }


    [BindProperty]
    public IFormFile Upload { get; set; }

    //[RequestSizeLimit(200_000_000)]
    public async Task<IActionResult> OnPostAsync()
    {
      string file = Path.GetTempFileName();
      //string file = null; // Path.GetTempFileName();
      DateTime startTime = DateTime.Now;

      /*if(!Directory.Exists(UploadDir))
      {
        Directory.CreateDirectory(UploadDir);
      }
      file = UploadDir + Path.GetRandomFileName();*/


      string gedcomFile = file + ".ged";
      //var file = Path.Combine(_environment.ContentRootPath, "uploads", Upload.FileName);
      trace.TraceData(TraceEventType.Information, 0, "Upload started of file " + Upload.FileName + " temp-name:" + gedcomFile + " size:" + Upload.Length);
      //Filename = file;
      //OrigFilename = Upload.FileName;
      //OrigFileSize = (int)Upload.Length;


      using (var fileStream = new FileStream(gedcomFile, FileMode.Create))
      {
          await Upload.CopyToAsync(fileStream);
      }
      TimeSpan delta = DateTime.Now - startTime;
      trace.TraceData(TraceEventType.Information, 0, "Upload done after " + delta.ToString());

      HttpContext.Session.SetString("GedcomFilename", gedcomFile);
      HttpContext.Session.SetString("OriginalFilename", Upload.FileName);
      HttpContext.Session.SetInt32("Filesize", (int)Upload.Length);

      System.IO.File.Delete(file);

      return Redirect("/FamilyTree/UploadFiles/UploadOk");
    }
  }
}