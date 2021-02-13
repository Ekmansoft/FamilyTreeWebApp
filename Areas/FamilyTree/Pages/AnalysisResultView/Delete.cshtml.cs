using FamilyTreeWebApp.Data;
using FamilyTreeWebTools.Data;
using FamilyTreeWebTools.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Threading.Tasks;

namespace FamilyTreeServices.Pages.AnalysisResultView
{
  [Authorize]
  public class DeleteModel : PageModel
  {
    private readonly FamilyTreeDbContext _context;
    private readonly TraceSource trace = new TraceSource("DeleteModel", SourceLevels.Information);

    public DeleteModel(FamilyTreeDbContext context)
    {
      _context = context;
    }

    static public bool IsRunning(int jobId)
    {
      int progress = ProgressDbClass.Instance.GetProgress(jobId);

      if (progress >= 0)
      {
        return true;
      }
      return false;
    }

    [BindProperty]
    public Analysis Analysis { get; set; }

    public async Task<IActionResult> OnGetAsync(int? id)
    {
      if (id == null)
      {
        return NotFound();
      }

      Analysis = await _context.Analyses.FirstOrDefaultAsync(m => m.Id == id);

      if (Analysis == null)
      {
        return NotFound();
      }
      return Page();
    }

    public async Task<IActionResult> OnPostAsync(int? id, bool? Pause)
    {
      if (id == null)
      {
        return NotFound();
      }
      Analysis = await _context.Analyses.FindAsync(id);

      if (Analysis != null)
      {
        int progress = ProgressDbClass.Instance.GetProgress(Analysis.Id);
        if (progress >= 0)
        {
          if ((Pause != null) && (bool)Pause)
          {
            trace.TraceData(TraceEventType.Information, 0, "Pausing job number " + Analysis.Id + ", started " + Analysis.StartCount + " times");
            Analysis.StartCount = 1000;
            _context.Analyses.Update(Analysis);
            await _context.SaveChangesAsync();
          }

          ProgressDbClass.Instance.RequestStop(Analysis.Id);

          trace.TraceData(TraceEventType.Information, 0, "Request stop for job number " + Analysis.Id + ", started " + Analysis.StartCount + " times");
          return RedirectToPage("./Index");
        }


        _context.Analyses.Remove(Analysis);
        await _context.SaveChangesAsync();
      }

      return RedirectToPage("./Index");
    }
  }
}
