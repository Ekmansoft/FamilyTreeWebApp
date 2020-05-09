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
  public class AnalyzeModel : PageModel
  {
    private readonly FamilyTreeDbContext _context;
    private static TraceSource trace = new TraceSource("Analyze", SourceLevels.Verbose);

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
    public AnalyzeModel(FamilyTreeDbContext context)
    {
      _context = context;
    }

    public Issue Issue { get; set; }
    public Profile Profile1 { get; set; }

    public string ExternalLink { get; set; }

    public async Task<IActionResult> OnGetAsync(int? id, int? status)
    {
      if (id == null)
      {
        return NotFound();
      }
      if (status != null)
      {
        Issue = await _context.Issues.FirstOrDefaultAsync(m => m.Id == id);
        trace.TraceData(TraceEventType.Information, 0, "Analyze id update " + id + " " + status);

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

      trace.TraceData(TraceEventType.Information, 0, "Analyze id " + id);
      Issue = await _context.Issues.FirstOrDefaultAsync(m => m.Id == id);

      if (Issue == null)
      {
        return NotFound();
      }
      //trace.TraceData(TraceEventType.Information, 0, "MergeDup id-1 " + id);
      Profile1 = await _context.Profiles.FindAsync(Issue.ProfileId);

      if (Profile1 == null)
      {
        return NotFound();
      }

      if (Issue.Parameters != null)
      {
        string[] parameters = Issue.Parameters.Split(";");

        //trace.TraceData(TraceEventType.Information, 0, "MergeDup id-3 " + id);
        ExternalLink = null;
        foreach (string param in parameters)
        {
          if (param.Length > 0)
          {
            ExternalLink = CreateCompareLink(Profile1.Url, param);
          }
        }
        trace.TraceData(TraceEventType.Information, 0, "Analyze id-4 " + id);
      } 
      else
      {
        trace.TraceData(TraceEventType.Information, 0, "Analyze id-5 " + id);
        ExternalLink = Profile1.Url;
      }

      return Page();
    }
    private bool ProblemExists(int id)
    {
      return _context.Issues.Any(e => e.Id == id);
    }
  }
}
