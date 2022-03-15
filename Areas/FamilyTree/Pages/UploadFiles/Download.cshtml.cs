using Ekmansoft.FamilyTree.WebApp.Data;
using Ekmansoft.FamilyTree.WebTools.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Threading.Tasks;

namespace FamilyTreeServices.Pages
{
  [Authorize]
  public class DownloadFileModel : PageModel
  {
    private readonly static TraceSource trace = new TraceSource("DownloadFileModel", SourceLevels.Information);
    private readonly IWebHostEnvironment _environment;
    private readonly FamilyTreeDbContext _context;

    public DownloadFileModel(IWebHostEnvironment environment, FamilyTreeDbContext context)
    {
      _environment = environment;
      _context = context;
    }

    [TempData]
    public string Filename { get; set; }

    [BindProperty]
    public Analysis Analysis { get; set; }


    //[RequestSizeLimit(200_000_000)]
    /*public FileResult Download()
    {
      byte[] fileBytes = System.IO.File.ReadAllBytes(@"/tmp/tmpRvajt6.tmp_export.ged");
      string fileName = "myfile.ext";
      return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
    }*/
    public async Task<FileResult> OnGetAsync(int? id, int? jsonId, int? kmlId)
    {
      if (jsonId != null)
      {
        string jsonFilename = "/tmp/job_" + jsonId + "_result.json";
        byte[] fileBytes = System.IO.File.ReadAllBytes(jsonFilename);
        return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, "AnalysisResults_" + jsonId + ".json");
      }
      if (kmlId != null)
      {
        string kmlFilename = "/tmp/job_" + kmlId + "_map.kml";
        byte[] fileBytes = System.IO.File.ReadAllBytes(kmlFilename);
        return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, "job_" + kmlId + "_map.kml");
      }
      if (id != null)
      {
        Analysis = await _context.Analyses.FirstOrDefaultAsync(m => m.Id == id);

        if (Analysis != null)
        {
          AnalysisResults results = AnalysisResults.FromJson(Analysis.Results);

          if (results != null)
          {
            string fileName = "job_" + id + "_export.ged";
            byte[] fileBytes = System.IO.File.ReadAllBytes(results.ExportedGedcomName);
            return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
          }
        }
      }
      return null;
    }
    public FileResult OnPost()
    {
      byte[] fileBytes = System.IO.File.ReadAllBytes(@"/tmp/tmpRvajt6.tmp_export.ged");
      string fileName = "myfile.ext";
      return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
    }
    public FileResult DownloadFile()
    {
      byte[] fileBytes = System.IO.File.ReadAllBytes(@"/tmp/tmpRvajt6.tmp_export.ged");
      string fileName = "myfile.ext";
      return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
    }
  }
}