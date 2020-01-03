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
using FamilyTreeWebApp.Data;

namespace FamilyTreeServices.Pages.AnalysisResultView
{
  [Authorize]
  public class DetailsModel : PageModel
  {
    private readonly FamilyTreeDbContext _context;

    public DetailsModel(FamilyTreeDbContext context)
    {
      _context = context;
    }

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
  }
}
