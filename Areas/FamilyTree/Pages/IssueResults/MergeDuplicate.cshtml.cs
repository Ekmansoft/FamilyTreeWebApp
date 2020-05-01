using System;
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

    public async Task<IActionResult> OnGetAsync(int? id)
    {
      if (id == null)
      {
        return NotFound();
      }

      Issue = await _context.Issues.FirstOrDefaultAsync(m => m.Id == id);

      if (Issue == null)
      {
        return NotFound();
      }
      if (Issue.Parameters == null)
      {
        return NotFound();
      }
      

      string[] parameters = Issue.Parameters.Split(";");

      Profile1 = await _context.Profiles.FindAsync(id);

      if (Profile1 == null)
      {
        return NotFound();
      }

      foreach (string param in parameters)
      {
        if (param.Length > 0)
        {
          CompareLink = CreateCompareLink(Profile1.Url, param);
        }
      }

      return Page();
    }
  }
}
