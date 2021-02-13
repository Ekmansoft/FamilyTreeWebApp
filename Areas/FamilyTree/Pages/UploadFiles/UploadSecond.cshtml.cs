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
  public class UploadSecondFileModel : PageModel
  {
    private static TraceSource trace = new TraceSource("UploadSecondFileModel", SourceLevels.Information);
    private readonly IWebHostEnvironment _environment;

    public UploadSecondFileModel(IWebHostEnvironment environment)
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
    public IFormFile UploadSecond { get; set; }

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
      trace.TraceData(TraceEventType.Information, 0, "Upload started of second file " + UploadSecond.FileName + " temp-name:" + gedcomFile + " size:" + UploadSecond.Length);
      //Filename = file;
      //OrigFilename = Upload.FileName;
      //OrigFileSize = (int)Upload.Length;


      using (var fileStream = new FileStream(gedcomFile, FileMode.Create))
      {
        await UploadSecond.CopyToAsync(fileStream);
      }
      TimeSpan delta = DateTime.Now - startTime;
      trace.TraceData(TraceEventType.Information, 0, "Upload done after " + delta.ToString());

      HttpContext.Session.SetString("GedcomFilename2", gedcomFile);
      HttpContext.Session.SetString("OriginalFilename2", UploadSecond.FileName);
      HttpContext.Session.SetInt32("Filesize2", (int)UploadSecond.Length);

      System.IO.File.Delete(file);

      return Redirect("/FamilyTree/UploadFiles/UploadSecondOk");
    }
  }
}