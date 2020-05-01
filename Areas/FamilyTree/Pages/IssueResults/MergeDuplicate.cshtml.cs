using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using FamilyTreeWebTools.Data;
using FamilyTreeWebApp.Data;

namespace FamilyTreeServices.Pages.IssueResults
{
  public class MergeDuplicateModel : PageModel
  {
    private readonly FamilyTreeDbContext _context;
    private static TraceSource trace = new TraceSource("MergeDuplicate", SourceLevels.Verbose);

    private string ExtractId(string url)
    {
      int ix = url.LastIndexOf('/');

      if ((ix > 0) && (ix < url.Length))
      {
        return url.Substring(ix + 1);
      }
      return null;
    }

    private string CreateCompareLink(string url1, string url2)
    {
      string id1 = ExtractId(url1);
      string id2 = ExtractId(url2);

      if ((id1 != null) && (id2 != null))
      {
        return "https://www.geni.com/merge/compare/" + id1 + "?return=match&to=" + id2;
      }
      return null;
    }
    public MergeDuplicateModel(FamilyTreeDbContext context)
    {
      _context = context;
    }

    public Issue Issue { get; set; }
    public Profile Profile1 { get; set; }

    public string CompareLink { get; set; }

    public async Task<IActionResult> OnGetAsync(int? id, int? status)
    {
      if (id == null)
      {
        return NotFound();
      }
      if (status != null)
      {
        Issue = await _context.Issues.FirstOrDefaultAsync(m => m.Id == id);
        trace.TraceData(TraceEventType.Information, 0, "MergeDup id update " + id + " " + status);

        Issue.Status = (Issue.IssueStatus)status;
        _context.Attach(Issue).State = EntityState.Modified;

        try
        {
          await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
          if (!ProblemExists(Issue.Id))
          {
            return NotFound();
          }
          else
          {
            throw;
          }
        }

      }

      trace.TraceData(TraceEventType.Information, 0, "MergeDup id " + id);
      Issue = await _context.Issues.FirstOrDefaultAsync(m => m.Id == id);

      if (Issue == null)
      {
        return NotFound();
      }
      trace.TraceData(TraceEventType.Information, 0, "MergeDup id-1 " + id);
      if (Issue.Parameters == null)
      {
        return NotFound();
      }
      trace.TraceData(TraceEventType.Information, 0, "MergeDup id-2 " + id);


      string[] parameters = Issue.Parameters.Split(";");

      Profile1 = await _context.Profiles.FindAsync(Issue.ProfileId);

      if (Profile1 == null)
      {
        return NotFound();
      }
      trace.TraceData(TraceEventType.Information, 0, "MergeDup id-3 " + id);

      foreach (string param in parameters)
      {
        if (param.Length > 0)
        {
          CompareLink = CreateCompareLink(Profile1.Url, param);
        }
      }
      trace.TraceData(TraceEventType.Information, 0, "MergeDup id-4 " + id);

      return Page();
    }
    private bool ProblemExists(int id)
    {
      return _context.Issues.Any(e => e.Id == id);
    }
  }
}
