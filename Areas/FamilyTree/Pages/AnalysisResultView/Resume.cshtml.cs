using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using FamilyTreeWebTools.Data;
using FamilyTreeWebTools.Services;
using FamilyTreeWebApp.Data;
using System.Threading;
using System.Diagnostics;
using FamilyTreeWebApp.Services;

namespace FamilyTreeServices.Pages.AnalysisResultView
{
  [Authorize]
  public class ResumeModel : PageModel
  {
    private readonly FamilyTreeDbContext _context;
    private readonly WebAppIdentity _appId;
    private readonly EmailSendSource _emailSendSource;
    private TraceSource trace = new TraceSource("ResumeModel", SourceLevels.Information);

    public ResumeModel(FamilyTreeDbContext context, WebAppIdentity appId, EmailSendSource emailSendSource)
    {
      _context = context;
      _appId = appId;
      _emailSendSource = emailSendSource;
    }


    [BindProperty]
    public Analysis Analysis { get; set; }

    public async Task<IActionResult> OnGetAsync(int? id)
    {
      trace.TraceData(TraceEventType.Information, 0, "OnGet resuming job number " + id);
      if (id == null)
      {
        return NotFound();
      }

      Analysis = await _context.Analyses.FirstOrDefaultAsync(m => m.Id == id);

      if (Analysis == null)
      {
        return NotFound();
      }
      //Analysis = await _context.Analyses.FindAsync(id);

      if (Analysis != null)
      {
        trace.TraceData(TraceEventType.Information, 0, "OnGet resuming job number  step " + id);
        Analysis.StartCount = 0;
        _context.Analyses.Update(Analysis);
        await _context.SaveChangesAsync();

        FamilyDbContextClass.StartupCheck(_context, _appId, _emailSendSource);
      }
      return RedirectToPage("./Index");
      //return Page();
    }

    public async Task<IActionResult> OnPostAsync(int? id, bool? pause)
    {
      trace.TraceData(TraceEventType.Information, 0, "OnPost resuming job number " + id);
      if (id == null)
      {
        return NotFound();
      }
      Analysis = await _context.Analyses.FindAsync(id);

      if (Analysis != null)
      {
        Analysis.StartCount = 0;
        _context.Analyses.Update(Analysis);
        await _context.SaveChangesAsync();
      }

      return RedirectToPage("./Index");
    }
  }
}
